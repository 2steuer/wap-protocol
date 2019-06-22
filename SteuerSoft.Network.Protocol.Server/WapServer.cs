using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Communication.Material;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Payloads.Control;
using SteuerSoft.Network.Protocol.Payloads.Generic;
using SteuerSoft.Network.Protocol.Server.Material;
using SteuerSoft.Network.Protocol.Server.Util.MethodProxy;
using SteuerSoft.Network.Protocol.Util;
using SteuerSoft.Network.Protocol.Util.Logging;
using SteuerSoft.Network.Protocol.Util.Logging.Interfaces;
using String = System.String;

namespace SteuerSoft.Network.Protocol.Server
{
    public class WapServer
    {
        private ILogger _log = Log.Create("Server");

        private readonly WapEndPoint _baseEp;

        private TcpListener _listener;

        private HashSet<ClientConnection> _clients = new HashSet<ClientConnection>();

        private ServerMethodProxy _serverMethods = new ServerMethodProxy();
        private Dictionary<string, ClientMethodInfo> _clientMethods = new Dictionary<string, ClientMethodInfo>();
        private Dictionary<string, EventInfo> _events = new Dictionary<string, EventInfo>();

        public int Port { get; set; }

        public bool Running { get; set; } = false;

        public WapServer(int port, string baseEndpoint)
        {
            Port = port;
            _baseEp = WapEndPoint.Parse(baseEndpoint);
            InitMethods();
        }

        public WapServer(int port)
            :this(port, ":control")
        {
            
        }

        public WapServer()
            : this(0)
        {
            
        }

        private void InitMethods()
        {
            _serverMethods.AddMethod<Auth, Empty>(WapEndPoint.Parse(_baseEp, "auth"), AuthClient);
            _serverMethods.AddMethod<MethodInfo, Empty>(WapEndPoint.Parse(_baseEp, "method.register"),
                RegisterMethod);
            _serverMethods.AddMethod<Deregister, Empty>(WapEndPoint.Parse(_baseEp, "methods.deregister"),
                DeregisterMethod);
            _serverMethods.AddMethod<MethodInfo, Empty>(WapEndPoint.Parse(_baseEp, "method.check"),
                CheckMethod);

            _serverMethods.AddMethod<RegisterEvent, Empty>(WapEndPoint.Parse(_baseEp, "events.publisher.add"),
                AddPublisher);
            _serverMethods.AddMethod<Deregister, Empty>(WapEndPoint.Parse(_baseEp, "events.publisher.remove"),
                RemovePublisher);

            _serverMethods.AddMethod<RegisterEvent, Empty>(WapEndPoint.Parse(_baseEp, "events.subscribe"),
                SubscribeEvent);
            _serverMethods.AddMethod<Deregister, Empty>(WapEndPoint.Parse(_baseEp, "events.unsubscribe"),
                UnsubscribeEvent);

            _serverMethods.AddMethod<Empty, DictionaryData>(WapEndPoint.Parse(_baseEp, "events.list"), GetEvents);
            _serverMethods.AddMethod<Payloads.Generic.String, StringList>(
                WapEndPoint.Parse(_baseEp, "events.getsubscribers"), GetEventSubscribers);
            _serverMethods.AddMethod<Payloads.Generic.String, StringList>(
                WapEndPoint.Parse(_baseEp, "events.getpublishers"), GetEventPublishers);

            _serverMethods.AddMethod<Empty, MethodInfoList>(WapEndPoint.Parse(_baseEp, "methods.list"), GetMethods);
            _serverMethods.AddMethod<Payloads.Generic.String, MethodInfo>(WapEndPoint.Parse(_baseEp, "methods.getinfo"), GetMethodInfo);
        }

        #region Client Control Methods

        private Task<MethodResult<Empty>> AuthClient(ClientConnection clt, Auth auth)
        {
            if (EndpointRegistered(auth.EndPoint))
            {
                _log.Info($"A client wanted to register already occupied endpoint {auth.EndPoint}.");
                return GetEmptyErrorMessage("Endpoint already authenticated");
            }

            _log.Debug($"Authenticating {auth.EndPoint} ({auth.Name})");

            clt.EndPoint = WapEndPoint.Parse(auth.EndPoint);
            clt.Name = auth.Name;

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> RegisterMethod(ClientConnection clt, MethodInfo dat)
        {
            if (!clt.IsAuthenticated)
            {
                return GetNotAuthedMessage();
            }

            if (EndpointRegistered(dat.EndPoint))
            {
                return GetEmptyErrorMessage("Method endpoint already registered");
            }

            _clientMethods.Add(dat.EndPoint, new ClientMethodInfo()
            {
                Client = clt,
                ParamPayloadType = dat.ParamPayloadType,
                ResultPayloadType = dat.ResultPayloadType
            });

            clt.Methods.Add(dat.EndPoint);

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> DeregisterMethod(ClientConnection clt, Deregister dat)
        {
            if (!clt.IsAuthenticated)
            {
                return GetNotAuthedMessage();
            }

            if (!_clientMethods.ContainsKey(dat.EndPoint))
            {
                return GetEmptyErrorMessage("Method endpoint unknown");
            }

            if (!clt.Methods.Contains(dat.EndPoint))
            {
                return GetEmptyErrorMessage("Method not registered by this client!");
            }

            clt.Methods.Remove(dat.EndPoint);
            _clientMethods.Remove(dat.EndPoint);

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> CheckMethod(ClientConnection clt, MethodInfo dat)
        {
            if (!clt.IsAuthenticated)
            {
                return GetNotAuthedMessage();
            }

            if (!_clientMethods.ContainsKey(dat.EndPoint))
            {
                return GetEmptyErrorMessage("Method endpoint unknown");
            }

            if (_clientMethods[dat.EndPoint].ParamPayloadType != dat.ParamPayloadType ||
                _clientMethods[dat.EndPoint].ResultPayloadType != dat.ResultPayloadType)
            {
                return GetEmptyErrorMessage("Method signatured do not match");
            }

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> AddPublisher(ClientConnection clt, RegisterEvent dat)
        {
            if (!clt.IsAuthenticated)
            {
                return GetNotAuthedMessage();
            }

            EventInfo i = GetEventInfo(dat);

            if (i == null)
            {
                return GetEmptyErrorMessage("Event already registered and signature does not match");
            }

            i.Publishers.Add(clt);
            clt.PublishingEvents.Add(dat.EndPoint);

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> RemovePublisher(ClientConnection clt, Deregister dat)
        {
            if (!clt.IsAuthenticated)
            {
                return GetNotAuthedMessage();
            }

            var i = GetEventInfo(dat.EndPoint);

            if (i == null)
            {
                return GetEmptyErrorMessage("Endpoint not found");
            }

            i.Publishers.Remove(clt);
            clt.PublishingEvents.Remove(dat.EndPoint);

            TidyUpEventInfo(dat.EndPoint);

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> SubscribeEvent(ClientConnection clt, RegisterEvent dat)
        {
            if (!clt.IsAuthenticated)
            {
                return GetNotAuthedMessage();
            }

            EventInfo i = GetEventInfo(dat);

            if (i == null)
            {
                return GetEmptyErrorMessage("Event already registered and signature does not match");
            }

            i.Subscribtions.Add(clt);
            clt.SubscribedEvents.Add(dat.EndPoint);

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> UnsubscribeEvent(ClientConnection clt, Deregister dat)
        {
            if (!clt.IsAuthenticated)
            {
                return GetNotAuthedMessage();
            }

            var i = GetEventInfo(dat.EndPoint);

            if (i == null)
            {
                return GetEmptyErrorMessage("Endpoint not found");
            }

            i.Subscribtions.Remove(clt);
            clt.SubscribedEvents.Remove(dat.EndPoint);

            TidyUpEventInfo(dat.EndPoint);

            return GetEmptySuccessMessage();
        }

        private Task<MethodResult<Empty>> GetNotAuthedMessage()
        {
            return GetEmptyErrorMessage("Not authenticated");
        }

        private Task<MethodResult<Empty>> GetEmptyErrorMessage(string error)
        {
            return Task.FromResult(MethodResult<Empty>.FromError(error));
        }

        private Task<MethodResult<Empty>> GetEmptySuccessMessage()
        {
            return Task.FromResult(MethodResult<Empty>.FromResult(new Empty()));
        }

        private EventInfo GetEventInfo(string endpoint)
        {
            if (_events.ContainsKey(endpoint))
            {
                return _events[endpoint];
            }
            else
            {
                return null;
            }
        }

        private void TidyUpEventInfo(string endPoint)
        {
            var i = GetEventInfo(endPoint);
            if (i != null && i.Publishers.Count == 0 && i.Subscribtions.Count == 0)
            {
                _events.Remove(endPoint);
            }
        }

        private EventInfo GetEventInfo(RegisterEvent i)
        {
            if (_events.ContainsKey(i.EndPoint))
            {
                if (i.PayloadType == _events[i.EndPoint].PayloadType)
                {
                    return _events[i.EndPoint];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                _events.Add(i.EndPoint, new EventInfo() {PayloadType = i.PayloadType});
                return _events[i.EndPoint];
            }
        }

        private bool EndpointRegistered(string endpoint)
        {
            return _clientMethods.ContainsKey(endpoint) || _events.ContainsKey(endpoint) ||
                   _serverMethods.HasMethod(endpoint) || _clients.Any(c => c.EndPoint.ToString() == endpoint);
        }
        #endregion

        #region Information retrieval Methods
        public Task<MethodResult<DictionaryData>> GetEvents(ClientConnection clt, Empty empty)
        {
            DictionaryData dd = new DictionaryData();
            foreach (var info in _events)
            {
                dd.Data.Add(info.Key, info.Value.PayloadType);
            }

            return Task.FromResult(MethodResult<DictionaryData>.FromResult(dd));
        }

        public Task<MethodResult<StringList>> GetEventSubscribers(ClientConnection clt, Payloads.Generic.String ep)
        {
            StringList sl = new StringList();

            if (!_events.ContainsKey(ep.Value))
            {
                return Task.FromResult(MethodResult<StringList>.FromError("Event EndPoint unknown"));
            }

            foreach (ClientConnection connection in _events[ep.Value].Subscribtions)
            {
                sl.List.Add(connection.EndPoint.ToString());
            }

            return Task.FromResult(MethodResult<StringList>.FromResult(sl));
        }

        public Task<MethodResult<StringList>> GetEventPublishers(ClientConnection clt, Payloads.Generic.String ep)
        {
            StringList sl = new StringList();

            if (!_events.ContainsKey(ep.Value))
            {
                return Task.FromResult(MethodResult<StringList>.FromError("Event EndPoint unknown"));
            }

            foreach (ClientConnection connection in _events[ep.Value].Publishers)
            {
                sl.List.Add(connection.EndPoint.ToString());
            }

            return Task.FromResult(MethodResult<StringList>.FromResult(sl));
        }

        public Task<MethodResult<MethodInfo>> GetMethodInfo(ClientConnection clt, Payloads.Generic.String ep)
        {
            if (!_clientMethods.ContainsKey(ep.Value))
            {
                return Task.FromResult(MethodResult<MethodInfo>.FromError("Method endpoint unknown"));
            }

            return Task.FromResult(MethodResult<MethodInfo>.FromResult(new MethodInfo()
                    {
                        EndPoint = ep.Value,
                        ParamPayloadType = _clientMethods[ep.Value].ParamPayloadType,
                        ResultPayloadType = _clientMethods[ep.Value].ResultPayloadType
                    }
                ));
        }

        public Task<MethodResult<MethodInfoList>> GetMethods(ClientConnection clt, Empty e)
        {
            MethodInfoList l = new MethodInfoList();

            l.List.AddRange(_serverMethods.Methods.Select(i => new MethodInfo()
            {
                EndPoint = i.Key,
                ParamPayloadType = i.Value.ParameterPayloadType,
                ResultPayloadType = i.Value.ResultPayloadType
            }));

            l.List.AddRange(_clientMethods.Select(kvp => new MethodInfo()
            {
                EndPoint = kvp.Key,
                ParamPayloadType = kvp.Value.ParamPayloadType,
                ResultPayloadType = kvp.Value.ResultPayloadType
            }));

            return Task.FromResult(MethodResult<MethodInfoList>.FromResult(l));
        }
        #endregion

        public bool Start()
        {
            if (Running)
            {
                return false;
            }

            _log.Info($"Starting server on port {Port}...");
            _listener = new TcpListener(IPAddress.Any, Port);

            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                return false;
            }
            

            _log.Info("Server started!");

#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            HandleIncomingConnections();
#pragma warning restore CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist

            return true;
        }

        public bool Stop()
        {
            if (!Running)
            {
                return false;
            }

            _log.Info("Stopping server...");
            _listener.Stop();

            _log.Info("Stopping all client handlers...");
            foreach (ClientConnection client in _clients)
            {
                client.Stop();
            }

            _log.Info("Shutdown completed!");

            return true;
        }

        private async Task HandleIncomingConnections()
        {
            while (true)
            {
                try
                {
                    var newClient = await _listener.AcceptTcpClientAsync();

                    _log.Verbose($"New connection from {newClient.Client.RemoteEndPoint}");

                    var newConn = new ClientConnection(MethodCallHandler, EventMessageHandler, ClientStoppedHandler);
                    newConn.Start(newClient.GetStream());
                }
                catch (Exception ex)
                {
                    _log.Warning("Error while accepting new clients...");
                    _log.Warning(ex);
                    break;
                }
            }

            Running = false;
        }

        private void ClientStoppedHandler(object sender, Exception exception)
        {
            var clt = (ClientConnection)sender;

            _log.Debug($"Client {clt.EndPoint} exits");

            foreach (string method in clt.Methods)
            {
                _clientMethods.Remove(method);
            }

            foreach (string publishingEvent in clt.PublishingEvents)
            {
                _events[publishingEvent].Publishers.Remove(clt);
            }

            foreach (string subscribedEvent in clt.SubscribedEvents)
            {
                _events[subscribedEvent].Subscribtions.Remove(clt);
            }

            clt.EndPoint = WapEndPoint.Parse(":");
            _clients.Remove(clt);
        }

        private async Task<WapMessage> MethodCallHandler(ClientConnection client, ReceivedWapMessage message)
        {
            if (_serverMethods.HasMethod(message.EndPoint))
            {
                try
                {
                    return await _serverMethods.CallMethod(client, WapEndPoint.Parse(message.EndPoint), message);
                }
                catch (Exception ex)
                {
                    return new WapMessage<MethodResult<Empty>>(MessageType.MethodResponse, message.EndPoint, new MethodResult<Empty>()
                    {
                        Success = false,
                        Error = ex.Message
                    });
                }
            }
            else if (_clientMethods.ContainsKey(message.EndPoint))
            {
                return await _clientMethods[message.EndPoint].Client.CallMethod(message);
            }
            else
            {
                return new WapMessage<MethodResult<Empty>>(MessageType.MethodResponse, message.EndPoint, new MethodResult<Empty>()
                {
                    Success = false,
                    Error = "Method not found"
                });
            }
        }

        private Task EventMessageHandler(ClientConnection client, ReceivedWapMessage msg)
        {
            if (!client.IsAuthenticated || !client.PublishingEvents.Contains(msg.EndPoint))
            {
                return Task.FromResult(0); // just return a completed task
            }

            List<Task> sendTasks = new List<Task>();

            foreach (var subscr in _events[msg.EndPoint].Subscribtions)
            {
                sendTasks.Add(subscr.SendEventMessage(msg));
            }

            return Task.WhenAll(sendTasks);
        }
    }
}

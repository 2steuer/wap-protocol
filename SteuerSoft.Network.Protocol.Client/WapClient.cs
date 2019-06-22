using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Client.Exceptions;
using SteuerSoft.Network.Protocol.Client.Interfaces;
using SteuerSoft.Network.Protocol.Client.Material;
using SteuerSoft.Network.Protocol.Client.Provider;
using SteuerSoft.Network.Protocol.Client.Util.MethodProxy;
using SteuerSoft.Network.Protocol.Communication.Base;
using SteuerSoft.Network.Protocol.Communication.Material;
using SteuerSoft.Network.Protocol.ExtensionMethods;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Payloads.Control;
using SteuerSoft.Network.Protocol.Payloads.Generic;
using SteuerSoft.Network.Protocol.Startable;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Client
{
    [Export(typeof(IStartable))]
    public abstract class WapClient : IStartable
    {
        private readonly WapEndPoint _controllerEp;

        WapConnection _conn = null;
        TcpClient _tcp = null;

        private MethodProxy _methods = new MethodProxy();

        private Dictionary<IEventSubscriber, Action<ReceivedWapMessage>> _eventHandlers = new Dictionary<IEventSubscriber, Action<ReceivedWapMessage>>();

        public string ControllerAddress { get; set; }
        public int ControllerPort { get; set; }

        public WapEndPoint Endpoint { get; set; }

        public string Name { get; set; }

        protected WapClient(string controllerAddress, int port, string endPoint, string name, string controllerEndpoint = null)
        {
            ControllerAddress = controllerAddress;
            ControllerPort = port;
            Endpoint = WapEndPoint.Parse(endPoint);
            Name = name;

            _controllerEp = WapEndPoint.Parse(controllerEndpoint ?? ":control");
        }

        protected WapClient(string endPoint, string name)
            : this(string.Empty, 0, endPoint, name)
        {
            ControllerAddress = EnvironmentVariableHelper.Get("WAP_CONTROLLER_ADDRESS", "localhost");
            ControllerPort = int.Parse(EnvironmentVariableHelper.Get("WAP_CONTROLLER_PORT", "51234"));
            _controllerEp = WapEndPoint.Parse(EnvironmentVariableHelper.Get("WAP_CONTROLLER_ENDPOINT", ":control"));
        }

        public async Task Connect()
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(ControllerAddress, ControllerPort);
            _conn = new WapConnection(MethodCallHandler, EventHandler, StoppedHandler);
            _conn.Start(_tcp.GetStream());

            await CallControlMethod<Auth, Empty>("auth", new Auth() {EndPoint = Endpoint.ToString(), Name = Name});

            await OnStart();
        }

        public void Disconnect()
        {
            _conn.Stop();
            _tcp.Close();
        }

        private async Task<TResult> CallMethod<TParam, TResult>(WapEndPoint ep, TParam param)
        {
            WapMessage<TParam> msg = new WapMessage<TParam>(MessageType.MethodCall, ep.ToString(), param);
            var rMsg = await _conn.CallMethod(msg);
            WapMessage<MethodResult<TResult>> result = WapMessage<MethodResult<TResult>>.FromReceivedMessage(rMsg);

            if (!result.Payload.Success)
            {
                throw new MethodCallFailedException(result.Payload.Error);
            }

            return result.Payload.Result;
        }

        protected Task<TResult> CallControlMethod<TParam, TResult>(string command, TParam param)
        {
            return CallMethod<TParam, TResult>(WapEndPoint.Parse(_controllerEp, command), param);
        }

        private void StoppedHandler(object sender, Exception exception)
        {
            
        }

        private Task EventHandler(ReceivedWapMessage receivedWapMessage)
        {
            var hdlrs =
                _eventHandlers.Where(hdl => hdl.Key.Endpoint.ToString() == receivedWapMessage.EndPoint)
                    .Select(hdl => hdl.Value);

            var tsks = new List<Task>();
            foreach (var action in hdlrs)
            {
                tsks.Add(Task.Run(() => action(receivedWapMessage)));
            }

            return Task.WhenAll(tsks);
        }

        private Task<WapMessage> MethodCallHandler(ReceivedWapMessage receivedWapMessage)
        {
            if (!_methods.HasMethod(receivedWapMessage.EndPoint))
            {
                var mres = MethodResult<Empty>.FromError("Method not found in this endpoint!");
                var msg = new WapMessage<MethodResult<Empty>>(MessageType.MethodResponse, receivedWapMessage.EndPoint, mres);
                return Task.FromResult<WapMessage>(msg);
            }

            return _methods.CallMethod(WapEndPoint.Parse(receivedWapMessage.EndPoint), receivedWapMessage);
        }

        protected abstract Task OnStart();

        protected async Task<MethodCaller<TParam, TResult>> CreateMethodCaller<TParam, TResult>(string endPoint)
        {
            WapEndPoint ep = WapEndPoint.Parse(Endpoint, endPoint);
            // Call raises an exception if the method does not exist or has a different signature
            await CallControlMethod<MethodInfo, Empty>("method.check", new MethodInfo()
            {
                EndPoint = ep.ToString(),
                ParamPayloadType = typeof(TParam).GetPayloadTypeName(),
                ResultPayloadType = typeof(TResult).GetPayloadTypeName()
            });

            return new MethodCaller<TParam, TResult>(CallMethod<TParam, TResult>, ep);
        }

        protected async Task<MethodProvider<TParam, TResult>> RegisterMethod<TParam, TResult>(string endpoint,
            Func<TParam, Task<TResult>> func)
        {
            WapEndPoint ep = WapEndPoint.Parse(Endpoint, endpoint);

            await CallControlMethod<MethodInfo, Empty>("method.register", new MethodInfo()
            {
                EndPoint = ep.ToString(),
                ParamPayloadType = typeof(TParam).GetPayloadTypeName(),
                ResultPayloadType = typeof(TResult).GetPayloadTypeName()
            });

            // If no exception occured, method was successfully registered...
            _methods.AddMethod(ep, func);

            return new MethodProvider<TParam, TResult>(ep, func);
        }

        protected async Task DeregisterMethod<TParam, TResult>(MethodProvider<TParam, TResult> method)
        {
            await CallControlMethod<Deregister, Empty>("method.deregister", new Deregister()
            {
                EndPoint = method.Endpoint.ToString()
            });
        }

        protected async Task<EventPublisher<TEvent>> CreatePublisher<TEvent>(string endpoint) where TEvent:new()
        {
            WapEndPoint ep = WapEndPoint.Parse(Endpoint, endpoint);

            await CallControlMethod<RegisterEvent, Empty>("events.publisher.add", new RegisterEvent()
            {
                EndPoint = ep.ToString(),
                PayloadType = typeof(TEvent).GetPayloadTypeName()
            });

            return new EventPublisher<TEvent>(ep, PublishEventData);
        }

        protected async Task RemoveEventPublisher<TEvent>(EventPublisher<TEvent> pub) where TEvent : new()
        {
            await CallControlMethod<Deregister, Empty>("events.publisher.remove", new Deregister()
            {
                EndPoint = pub.Endpoint.ToString()
            });
        }

        private async Task PublishEventData<TEvent>(EventPublisher<TEvent> sender, TEvent data) where TEvent : new()
        {
            WapMessage<TEvent> msg = new WapMessage<TEvent>(MessageType.Event, sender.Endpoint.ToString(), data);

            await _conn.SendEventMessage(msg);
        }

        protected async Task<EventSubscriber<TEvent>> SubscribeEvent<TEvent>(string endpoint)
        {
            WapEndPoint ep = WapEndPoint.Parse(Endpoint, endpoint);

            await CallControlMethod<RegisterEvent, Empty>("events.subscribe", new RegisterEvent()
            {
                EndPoint = ep.ToString(),
                PayloadType = typeof(TEvent).GetPayloadTypeName()
            });

            var subscr = new EventSubscriber<TEvent>(ep);
            var hdl = new Action<ReceivedWapMessage>(msg =>
            {
                WapMessage<TEvent> wm = WapMessage<TEvent>.FromReceivedMessage(msg);
                subscr.PassData(wm.Payload);
            });

            _eventHandlers.Add(subscr, hdl);

            return subscr;
        }

        protected async Task UnsubscribeEvent<TEvent>(EventSubscriber<TEvent> subscr)
        {
            await CallControlMethod<Deregister, Empty>("events.unsubscribe", new Deregister()
            {
                EndPoint = subscr.Endpoint.ToString()
            });

            _eventHandlers.Remove(subscr);
        }

        public virtual Task Start(IEnumerable<string> args)
        {
            return Connect();
        }

        public virtual void Stop()
        {
            Disconnect();
        }
    }
}

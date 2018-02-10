using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Communication.Base.Material;
using SteuerSoft.Network.Protocol.Communication.Base.ValueTypes;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.Interfaces;

namespace SteuerSoft.Network.Protocol.Communication.Base
{
    public delegate void BrokenFrameDelegate(object sender, IEnumerable<byte> data, Exception frameException);

    public delegate void ProviderStoppedDelegate(object sender, Exception ex);

    public abstract class WapMessageExchangeProvider
    {
        private static readonly int ReadBlockSize = 64;

        private static readonly byte StartByte = 0x02;
        private static readonly byte EndByte = 0x03;
        private static readonly byte EscapeByte = 0x1B;
        private static readonly byte EscapeOffset = 0x20;

        private static readonly byte[] BytesForEscape = {StartByte, EndByte, EscapeByte};

        private Stream _stream;

        private SemaphoreSlim _txSem = new SemaphoreSlim(1);
        private SemaphoreSlim _rxSem = new SemaphoreSlim(1);
        private CancellationTokenSource _stopToken = new CancellationTokenSource();
        private List<byte> _bufBytes = new List<byte>();

        
        public event BrokenFrameDelegate OnBrokenFrame;
        public event ProviderStoppedDelegate OnStopped;

        public bool Running { get; private set; } = false;

        protected bool StartHandler(Stream stream)
        {
            if (Running)
            {
                return false;
            }

            _stream = stream;
            Running = true;
            
            // This call is running in the background until Stop() is called
            // i.e. the cancellationToken is set.
#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            ReceiveSpinner(_stopToken.Token);
#pragma warning restore CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            return true;
        }

        protected bool StopHandler()
        {
            if (!Running)
            {
                return false;
            }
            
            _stopToken.Cancel();
            _stream.Close();
            return true;
        }

        private async Task ReceiveSpinner(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                ReceivedWapMessage msg;
                try
                {
                    msg = await ReceiveMessage(ct);
                    await HandleMessage(msg);
                }
                catch (TaskCanceledException)
                {
                    // Continue, as the next check for the cancelled token is then
                    // performed and the loop is exited.
                    continue;
                }
                catch (BrokenFrameException ex)
                {
                    OnBrokenFrame?.Invoke(this, ex.BrokenFrame, ex.InnerException);
                }
                catch (Exception ex)
                {
                    OnStopped?.Invoke(this, ex);
                    break;
                }

            }

            Running = false;
            _stream = null;
        }

        private async Task<ReceivedWapMessage> ReceiveMessage(CancellationToken ct)
        {
            ReceiveStates state = ReceiveStates.Idle;
            List<byte> msgBytes = new List<byte>();

            await _rxSem.WaitAsync(ct);

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    _rxSem.Release();
                    ct.ThrowIfCancellationRequested();
                }

                byte[] bs;
                if (_bufBytes.Count > 0)
                {
                    bs = _bufBytes.ToArray();
                    _bufBytes.Clear();
                }
                else
                {
                    byte[] buf = new byte[ReadBlockSize];
                    int count = await _stream.ReadAsync(buf, 0, ReadBlockSize, ct).ConfigureAwait(false);

                    if (count == 0)
                    {
                        _rxSem.Release();
                        throw new OperationCanceledException("End of Stream reached during reading");
                    }
                    
                    bs = new byte[count];
                    Array.Copy(buf, bs, count);
                }

                for (int i = 0; i < bs.Length; i++)
                {
                    byte b = bs[i];

                    switch (state)
                    {
                        case ReceiveStates.Idle:
                            if (b == StartByte)
                            {
                                state = ReceiveStates.Reading;
                            }
                            break;

                        case ReceiveStates.Reading:
                            if (b == StartByte)
                            {
                                _rxSem.Release();
                                throw new BrokenFrameException("Unexpected start byte", msgBytes);
                            }
                            else if (b == EndByte)
                            {
                                ReceivedWapMessage msg;
                                try
                                {
                                    msg = new ReceivedWapMessage(msgBytes.ToArray());
                                }
                                catch (Exception ex)
                                {
                                    _rxSem.Release();
                                    throw new BrokenFrameException("Parsing failed", msgBytes, ex);
                                }
                                
                                _bufBytes.AddRange(bs.Skip(i + 1));
                                _rxSem.Release();
                                return msg;
                            }
                            else if (b == EscapeByte)
                            {
                                state = ReceiveStates.ReadingEscaped;
                            }
                            else
                            {
                                msgBytes.Add(b);
                            }
                            break;

                        case ReceiveStates.ReadingEscaped:
                            if (b < EscapeOffset)
                            {
                                msgBytes.Add(b);
                                _rxSem.Release();
                                throw new BrokenFrameException("Illegal escaped sequence", msgBytes);
                            }
                            else
                            {
                                b -= EscapeOffset;
                                msgBytes.Add(b);
                                state = ReceiveStates.Reading;
                            }
                            break;
                    }
                }
            }
        }

        protected async Task<bool> SendMesssage(IWapMessage msg)
        {
            if (!Running)
            {
                return false;
            }

            List<byte> sendList = new List<byte>();
            sendList.Add(StartByte);
            sendList.AddRange(EscapeBytes(msg.GetBytes()));
            sendList.Add(EndByte);

            await _txSem.WaitAsync().ConfigureAwait(false);
            await _stream.WriteAsync(sendList.ToArray(), 0, sendList.Count).ConfigureAwait(false);
            await _stream.FlushAsync().ConfigureAwait(false);
            _txSem.Release();

            return true;
        }

        private List<byte> EscapeBytes(IEnumerable<byte> bytes)
        {
            List<byte> ret = new List<byte>();

            foreach (byte b in bytes)
            {
                if (BytesForEscape.Contains(b))
                {
                    ret.Add(EscapeByte);
                    ret.Add((byte)(b + EscapeOffset));
                }
                else
                {
                    ret.Add(b);
                }
            }

            return ret;
        }

        protected abstract Task HandleMessage(ReceivedWapMessage msg);
    }
}

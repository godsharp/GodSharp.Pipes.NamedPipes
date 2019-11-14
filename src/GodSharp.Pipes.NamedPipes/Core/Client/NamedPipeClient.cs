using System;
using System.IO.Pipes;
using System.Security.Principal;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeClient : NamedPipe<NamedPipeClientStream,ClientConnectionArgs>
    {
        public int ConnectionTimeout { get; private set; } = 5000;
        public string PipeServer { get; private set; }

        public NamedPipeClient() : base()
        {
        }

        public NamedPipeClient(NamedPipeClientOptions options) : this() => Initialize(options);

        public void Initialize(NamedPipeClientOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Vaild();

            PipeServer = options.PipeServer;
            PipeName = options.PipeName;
            Instance = new NamedPipeClientStream(PipeServer, PipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);

            OnReadCompleted = options.OnReadCompleted;
            OnWaitForConnectionCompleted = options.OnWaitForConnectionCompleted;
            OnStopCompleted = options.OnStopCompleted;
            OnException = options.OnException;
            OutputLogger = options.OutputLogger;
            Initialized = true;
        }

        internal protected override string GetOutputPrefix() => $"{base.GetOutputPrefix()} / {PipeServer}";

        protected internal override void OnStopHandler()
        {
            OnStopCompleted?.Invoke(new ClientConnectionArgs(Guid, Instance, PipeName, PipeServer, buffer: null));
        }

        protected internal override void OnReadCompletedHandler(byte[] buffer)
        {
            OnReadCompleted?.Invoke(new ClientConnectionArgs(Guid, Instance, PipeName, PipeServer, buffer: buffer));
        }

        protected internal override void OnExceptionHandler(Exception exception)
        {
            OnException?.Invoke(new ClientConnectionArgs(Guid, Instance, PipeName, PipeServer, exception: exception));
        }

        internal protected override void WaitForConnection()
        {
            OutputLogger?.Invoke($"{GetOutputPrefix()} connecting to server.");

            Action action = () =>
            {
                if (!Instance.IsConnected) return;

                OutputLogger?.Invoke($"{GetOutputPrefix()} connected.");

                //WaitForRead();

                WaitInteraction();

                OnWaitForConnectionCompleted?.Invoke(new ClientConnectionArgs(Guid, Instance, PipeName, null, buffer: null));
            };

#if NET40 || NET45
            Instance.Connect(ConnectionTimeout);
            action();
#else
            Instance.ConnectAsync(ConnectionTimeout).ContinueWith((e) => action());
#endif
            OutputLogger?.Invoke($"{GetOutputPrefix()} connected to server.");
        }

        private void WaitInteraction()
        {
            Running = true;
            WaitForReadInternal(WaitInteractionReadCallback);
            WriteInternal(Guid.ToByteArray(), true);
        }

        private void WaitInteractionReadCallback(IAsyncResult result)
        {
            Guid guid = Guid.Empty;

            try
            {
                lock (myLock)
                {
                    if (Stopping) return;
                    int length = Instance.EndRead(result);

                    if (length < 1)
                    {
                        Stop();
                        return;
                    }

                    byte[] buffer = result.AsyncState as byte[];

                    byte[] tmp = new byte[length];

                    Buffer.BlockCopy(buffer, 0, tmp, 0, length);

                    OutputLogger?.Invoke($"{GetOutputPrefix()} interacted read data {length} bytes.");

                    try
                    {
                        guid = new Guid(tmp);
                    }
                    catch (Exception ex)
                    {
                        OnExceptionHandler(ex);
                    }
                }

                if (guid != Guid)
                {
                    WaitInteraction();
                }
                else
                {
                    Interacted = true;
                    WaitForRead();
                }
            }
            catch (Exception ex)
            {
                OnExceptionHandler(ex);
            }
        }
    }
}
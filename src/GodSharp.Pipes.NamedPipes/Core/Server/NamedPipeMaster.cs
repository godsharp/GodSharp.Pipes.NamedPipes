using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeMaster : NamedPipe<NamedPipeServerStream,MasterConnectionArgs>
    {
        public int MaxNumberOfServerInstances { get; private set; } = 128;
        public Guid ClientGuid { get; private set; } = Guid.Empty;

        public NamedPipeMaster() : base()
        {
        }

        public NamedPipeMaster(NamedPipeServerOptions options) => Initialize(options);

        public void Initialize(NamedPipeServerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Vaild();

            PipeName = options.PipeName;
            MaxNumberOfServerInstances = options.MaxNumberOfServerInstances;

            //Instance = new NamedPipeServerStream(PipeName, PipeDirection.InOut, MaxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Instance = new NamedPipeServerStream(PipeName, PipeDirection.InOut, MaxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, options.ReadBytesSize, options.ReadBytesSize);

            OnReadCompleted = options.OnReadCompleted;
            OnWaitForConnectionCompleted = options.OnWaitForConnectionCompleted;
            OnStopCompleted = options.OnStopCompleted;
            OnException = options.OnException;
            OutputLogger = options.OutputLogger;
            Initialized = true;
        }

        internal protected override string GetOutputPrefix() => $"{base.GetOutputPrefix()} / {ClientGuid}";

        protected internal override void OnStopHandler() => OnStopCompleted?.Invoke(new MasterConnectionArgs(Guid, ClientGuid, Instance, PipeName, buffer: null));

        protected internal override void OnReadCompletedHandler(byte[] buffer) => OnReadCompleted?.Invoke(new MasterConnectionArgs(Guid, ClientGuid, Instance, PipeName, buffer: buffer));

        protected internal override void OnExceptionHandler(Exception exception) => OnException?.Invoke(new MasterConnectionArgs(Guid, ClientGuid, Instance, PipeName, exception: exception));

        internal protected override void WaitForConnection()
        {
            Instance.BeginWaitForConnection(WaitForConnectionHandler, null);
            OutputLogger?.Invoke($"{GetOutputPrefix()} wait to connect.");
            Running = true;
        }

        private void WaitForConnectionHandler(IAsyncResult result)
        {
            try
            {
                Instance.EndWaitForConnection(result);

                if (Stopping) return;

                //WaitForRead();

                WaitInteraction();

                OutputLogger?.Invoke($"{GetOutputPrefix()} was connected.");
                OnWaitForConnectionCompleted?.Invoke(new MasterConnectionArgs(Guid, ClientGuid, Instance, PipeName, buffer: null));
            }
            catch (Exception ex)
            {
                OnExceptionHandler(ex);
            }
        }

        private void WaitInteraction()
        {
            Running = true;
            WaitForReadInternal(WaitInteractionReadCallback);
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

                if (guid == Guid.Empty)
                {
                    WaitInteraction();
                }
                else
                {
                    ClientGuid = guid;
                    WriteInternal(guid.ToByteArray(), true);

                    OutputLogger?.Invoke($"{GetOutputPrefix()} was interacted.");

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
using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    internal class NamedPipeMaster : NamedPipe<NamedPipeServerStream>
    {
        public int MaxNumberOfServerInstances { get; private set; } = 128;

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

        protected override void WaitForConnection()
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

                WaitForRead();

                OutputLogger?.Invoke($"{GetOutputPrefix()} was connected.");
                OnWaitForConnectionCompleted?.Invoke(new NamedPipeConnectionArgs(Guid, Instance, PipeName, null, buffer: null));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(new NamedPipeConnectionArgs(Guid, Instance, PipeName, null, exception: ex));
            }
        }
    }
}
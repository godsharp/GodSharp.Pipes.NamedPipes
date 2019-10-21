using System;
using System.IO.Pipes;
using System.Security.Principal;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeClient : NamedPipe<NamedPipeClientStream>
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

        protected override string GetOutputPrefix() => $"{base.GetOutputPrefix()} / {PipeServer}";

        protected override void WaitForConnection()
        {
            OutputLogger?.Invoke($"{GetOutputPrefix()} connecting to server.");

            Action action = () =>
            {
                if (!Instance.IsConnected) return;

                OutputLogger?.Invoke($"{GetOutputPrefix()} connected.");

                WaitForRead();
            };

#if NET40 || NET45
            Instance.Connect(ConnectionTimeout);
            action();
#else
            Instance.ConnectAsync(ConnectionTimeout).ContinueWith((e) => action());
#endif
            OutputLogger?.Invoke($"{GetOutputPrefix()} connected to server.");
        }
    }
}
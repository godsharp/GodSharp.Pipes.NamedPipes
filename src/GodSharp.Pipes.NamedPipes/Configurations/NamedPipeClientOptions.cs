using System;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeClientOptions : NamedPipeOptions<ClientConnectionArgs>
    {
        public int ConnectionTimeout { get; set; } = 5000;
        public string PipeServer { get; set; }

        public NamedPipeClientOptions()
        {
        }

        public NamedPipeClientOptions(string pipeName, Action<ClientConnectionArgs> onReadCompleted, Action<ClientConnectionArgs> onWaitForConnectionCompleted, Action<ClientConnectionArgs> onInteractionCompleted, Action<ClientConnectionArgs> onStopCompleted, Action<ClientConnectionArgs> onException, Action<string> outputLogger, int readBytesSize = 1024 * 1024, int connectionTimeout = 5000, string pipeServer = ".") : base(pipeName, onReadCompleted, onWaitForConnectionCompleted, onInteractionCompleted, onStopCompleted, onException, outputLogger, readBytesSize)
        {
            ConnectionTimeout = connectionTimeout;
            PipeServer = pipeServer ?? throw new ArgumentNullException(nameof(pipeServer));
        }

        public override void OnVaiding()
        {
            if (ConnectionTimeout < 1) throw new ArgumentOutOfRangeException(nameof(ConnectionTimeout));
            if (string.IsNullOrWhiteSpace(PipeServer) || string.IsNullOrEmpty(PipeServer)) throw new ArgumentNullException(nameof(PipeServer));
        }
    }
}
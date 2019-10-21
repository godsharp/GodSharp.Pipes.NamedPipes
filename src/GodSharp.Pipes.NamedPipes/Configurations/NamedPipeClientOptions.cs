using System;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeClientOptions : NamedPipeOptions
    {
        public int ConnectionTimeout { get; set; } = 5000;
        public string PipeServer { get; set; }

        public NamedPipeClientOptions()
        {
        }

        public NamedPipeClientOptions(string pipeName, Action<NamedPipeConnectionArgs> onReadCompleted, Action<NamedPipeConnectionArgs> onWaitForConnectionCompleted, Action<NamedPipeConnectionArgs> onStopCompleted, Action<NamedPipeConnectionArgs> onException, Action<string> outputLogger, int readBytesSize = 1024 * 1024, int connectionTimeout = 5000, string pipeServer = ".") : base(pipeName, onReadCompleted, onWaitForConnectionCompleted, onStopCompleted, onException,outputLogger, readBytesSize)
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
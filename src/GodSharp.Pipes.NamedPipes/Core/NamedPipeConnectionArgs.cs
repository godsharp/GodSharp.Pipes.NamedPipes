using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeConnectionArgs
    {
        public Guid Guid { get; internal set; }
        public PipeStream Instance { get; internal set; }
        public string PipeName { get; internal set; }
        public string PipeServer { get; internal set; }
        public byte[] Buffer { get; internal set; }
        public Exception Exception { get; internal set; }

        public NamedPipeConnectionArgs()
        {
        }

        public NamedPipeConnectionArgs(Guid guid, PipeStream instance, string pipeName, string pipeServer, byte[] buffer = null)
        {
            Guid = guid;
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            PipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            PipeServer = pipeServer;
            Buffer = buffer;
        }

        public NamedPipeConnectionArgs(Guid guid, PipeStream instance, string pipeName, string pipeServer, Exception exception = null)
        {
            Guid = guid;
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            PipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            PipeServer = pipeServer;// ?? throw new ArgumentNullException(nameof(pipeServer));
            Exception = exception;
        }
    }
}

using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    public class ConnectionArgs
    {
        public Guid Guid { get; internal set; }
        public PipeStream Instance { get; internal set; }
        public string PipeName { get; internal set; }
        public byte[] Buffer { get; internal set; }
        public Exception Exception { get; internal set; }

        public ConnectionArgs()
        {
        }

        public ConnectionArgs(Guid guid, PipeStream instance, string pipeName, byte[] buffer = null)
        {
            Guid = guid;
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            PipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            Buffer = buffer;
        }

        public ConnectionArgs(Guid guid, PipeStream instance, string pipeName, Exception exception = null)
        {
            Guid = guid;
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            PipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            Exception = exception;
        }
    }


}

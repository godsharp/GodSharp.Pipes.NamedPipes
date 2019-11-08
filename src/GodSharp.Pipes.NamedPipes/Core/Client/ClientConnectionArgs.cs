using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    public class ClientConnectionArgs : ConnectionArgs
    {
        public string PipeServer { get; internal set; }

        public ClientConnectionArgs()
        {
        }

        public ClientConnectionArgs(Guid guid, PipeStream instance, string pipeName, string pipeServer, byte[] buffer = null):base(guid,instance,pipeName,buffer)
        {
            PipeServer = pipeServer;
        }

        public ClientConnectionArgs(Guid guid, PipeStream instance, string pipeName, string pipeServer, Exception exception = null) : base(guid, instance, pipeName, exception)
        {
            PipeServer = pipeServer;
        }
    }
}

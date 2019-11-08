using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    public class MasterConnectionArgs: ConnectionArgs
    {
        public Guid ClientGuid { get; internal set; }

        public MasterConnectionArgs(Guid guid, Guid clientGuid, PipeStream instance, string pipeName, byte[] buffer = null) : base(guid, instance, pipeName, buffer)
        {
            ClientGuid = clientGuid;
        }

        public MasterConnectionArgs(Guid guid, Guid clientGuid, PipeStream instance, string pipeName, Exception exception = null) : base(guid, instance, pipeName, exception)
        {
            ClientGuid = clientGuid;
        }
    }
}

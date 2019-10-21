using System;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeServerOptions : NamedPipeOptions
    {
        public int MaxNumberOfServerInstances { get; private set; } = 128;

        public NamedPipeServerOptions()
        {
        }

        public NamedPipeServerOptions(string pipeName, Action<NamedPipeConnectionArgs> onReadCompleted, Action<NamedPipeConnectionArgs> onWaitForConnectionCompleted, Action<NamedPipeConnectionArgs> onStopCompleted, Action<NamedPipeConnectionArgs> onException, Action<string> outputLogger, int readBytesSize = 1024 * 1024, int maxNumberOfServerInstances = 128) : base(pipeName, onReadCompleted, onWaitForConnectionCompleted, onStopCompleted, onException,outputLogger, readBytesSize)
        {
            if (maxNumberOfServerInstances < 1 || maxNumberOfServerInstances > 254) throw new ArgumentOutOfRangeException(nameof(maxNumberOfServerInstances));
            MaxNumberOfServerInstances = maxNumberOfServerInstances;
        }

        public override void OnVaiding()
        {
            if (MaxNumberOfServerInstances < 1 || MaxNumberOfServerInstances > 254) throw new ArgumentOutOfRangeException(nameof(MaxNumberOfServerInstances));
        }
    }
}
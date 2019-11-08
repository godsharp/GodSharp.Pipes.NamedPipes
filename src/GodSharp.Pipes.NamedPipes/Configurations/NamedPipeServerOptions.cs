using System;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeServerOptions : NamedPipeOptions<MasterConnectionArgs>
    {
        public int MaxNumberOfServerInstances { get; private set; } = 254;

        public NamedPipeServerOptions()
        {
        }

        public NamedPipeServerOptions(string pipeName, Action<MasterConnectionArgs> onReadCompleted, Action<MasterConnectionArgs> onWaitForConnectionCompleted, Action<MasterConnectionArgs> onInteractionCompleted, Action<MasterConnectionArgs> onStopCompleted, Action<MasterConnectionArgs> onException, Action<string> outputLogger, int readBytesSize = 1024 * 1024, int maxNumberOfServerInstances = 254) : base(pipeName, onReadCompleted, onWaitForConnectionCompleted, onInteractionCompleted, onStopCompleted, onException, outputLogger, readBytesSize)
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
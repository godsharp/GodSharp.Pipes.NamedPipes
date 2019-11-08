using System;

namespace GodSharp.Pipes.NamedPipes
{
    public abstract class NamedPipeOptions<TArgs> where TArgs : ConnectionArgs
    {
        public string PipeName { get; set; }
        public Action<TArgs> OnWaitForConnectionCompleted { get; set; }
        public Action<TArgs> OnInteractionCompleted { get; set; }
        public Action<TArgs> OnStopCompleted { get; set; }
        public Action<TArgs> OnReadCompleted { get; set; }
        public Action<TArgs> OnException { get; set; }
        public Action<string> OutputLogger { get; set; }
        public int ReadBytesSize { get; set; } = 1024 * 1024;

        public NamedPipeOptions()
        {
        }

        public NamedPipeOptions(string pipeName, Action<TArgs> onReadCompleted, Action<TArgs> onWaitForConnectionCompleted, Action<TArgs> onInteractionCompleted, Action<TArgs> onStopCompleted, Action<TArgs> onException, Action<string> outputLogger, int readBytesSize = 1024 * 1024)
        {
            PipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            OnReadCompleted = onReadCompleted ?? throw new ArgumentNullException(nameof(onReadCompleted));
            OnWaitForConnectionCompleted = onWaitForConnectionCompleted;
            OnInteractionCompleted = onInteractionCompleted;
            OnStopCompleted = onStopCompleted;
            OnException = onException;
            OutputLogger = outputLogger;
            ReadBytesSize = readBytesSize;
        }

        public void Vaild()
        {
            if (string.IsNullOrWhiteSpace(PipeName) || string.IsNullOrEmpty(PipeName)) throw new ArgumentNullException(nameof(PipeName));
            if (OnReadCompleted == null) throw new ArgumentNullException(nameof(OnReadCompleted));
            if (ReadBytesSize < 1) throw new ArgumentOutOfRangeException(nameof(ReadBytesSize));

            OnVaiding();
        }

        public abstract void OnVaiding();
    }
}
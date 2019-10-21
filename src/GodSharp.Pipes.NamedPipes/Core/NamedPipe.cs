using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    public abstract class NamedPipe<TPipe> where TPipe : PipeStream
    {
        public int ReadBytesSize { get; protected set; } = 1024 * 1024;
        public string PipeName { get; protected set; }
        public Guid Guid { get; protected set; }
        public TPipe Instance { get; protected set; }
        public bool IsConnected => Instance.IsConnected;

        public Action<NamedPipeConnectionArgs> OnWaitForConnectionCompleted { get; protected set; }
        public Action<NamedPipeConnectionArgs> OnStopCompleted { get; protected set; }
        public Action<NamedPipeConnectionArgs> OnReadCompleted { get; protected set; }
        public Action<NamedPipeConnectionArgs> OnException { get; protected set; }
        public Action<string> OutputLogger { get; protected set; }

        protected bool Running = false;
        protected bool Stopping = false;
        protected bool Initialized = false;
        protected readonly string InstanceType = null;

        protected readonly object myLock = new object();

        public NamedPipe()
        {
            InstanceType = this.GetType().Name;
            Guid = Guid.NewGuid();
        }

        protected void VaildInitialized()
        {
            if (!Initialized) throw new InvalidOperationException("his instance not initialized, you should new instance with options paramters or call Initialize() first.");
        }

        public void Start()
        {
            lock (myLock)
            {
                VaildInitialized();

                WaitForConnection();

                OutputLogger?.Invoke($"{GetOutputPrefix()} started."); 
            }
        }

        public void Stop()
        {
            lock (myLock)
            {
                if (Stopping) return;

                Instance.Close();
                Stopping = true;
                OutputLogger?.Invoke($"{GetOutputPrefix()} stopped.");
                OnStopCompleted?.Invoke(new NamedPipeConnectionArgs(Guid, Instance, PipeName, null, buffer: null));
                Running = false; 
            }
        }

        protected virtual string GetOutputPrefix() => $"{InstanceType} {Guid} / {PipeName}";

        protected abstract void WaitForConnection();

        public void Write(byte[] buffer)
        {
            VaildInitialized();

            if (buffer == null || buffer.Length == 0) return;
            if (!Instance.IsConnected) return;

            try
            {
                Instance.Write(buffer, 0, buffer.Length);
                Instance.Flush();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(new NamedPipeConnectionArgs(Guid, Instance, PipeName, null, exception: ex));
            }
        }
        
        protected void WaitForRead()
        {
            Running = true;

            try
            {
                OutputLogger?.Invoke($"{GetOutputPrefix()} begin to read.");

                byte[] buffer = new byte[ReadBytesSize];
                Instance.BeginRead(buffer, 0, buffer.Length, WaitForReadHandler, buffer);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(new NamedPipeConnectionArgs(Guid, Instance, PipeName, null, exception: ex));
            }
        }

        protected void WaitForReadHandler(IAsyncResult result)
        {
            try
            {
                lock (myLock)
                {
                    if (Stopping) return;
                    int length = Instance.EndRead(result);

                    if (length < 1)
                    {
                        Stop();
                        return;
                    }

                    byte[] buffer = result.AsyncState as byte[];

                    byte[] tmp = new byte[length];

                    Buffer.BlockCopy(buffer, 0, tmp, 0, length);

                    OutputLogger?.Invoke($"{GetOutputPrefix()} read data {length} bytes.");

                    OnReadCompleted?.Invoke(new NamedPipeConnectionArgs(Guid, Instance, PipeName, null, buffer: tmp)); /**/
                }

                WaitForRead();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(new NamedPipeConnectionArgs(Guid, Instance, PipeName, null, exception: ex));
            }
        }
    }
}

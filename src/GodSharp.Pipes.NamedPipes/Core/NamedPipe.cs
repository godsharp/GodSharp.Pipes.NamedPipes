using System;
using System.IO.Pipes;

namespace GodSharp.Pipes.NamedPipes
{
    public abstract class NamedPipe<TPipe,TArgs>: IDisposable where TPipe : PipeStream
        where TArgs:ConnectionArgs
    {
        public int ReadBytesSize { get; protected set; } = 1024 * 1024;
        public string PipeName { get; protected set; }
        public Guid Guid { get; protected set; }
        public TPipe Instance { get; protected set; }
        public bool IsConnected => Instance?.IsConnected == true;

        public Action<TArgs> OnWaitForConnectionCompleted { get; protected set; }
        public Action<TArgs> OnStopCompleted { get; protected set; }
        public Action<TArgs> OnReadCompleted { get; protected set; }
        public Action<TArgs> OnException { get; protected set; }
        public Action<string> OutputLogger { get; protected set; }

        protected bool Interacted = false;
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
            if (!Initialized) throw new InvalidOperationException("this instance not initialized, you should new instance with options paramters or call Initialize() first.");
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
                Interacted = false;
                Stopping = true;
                OutputLogger?.Invoke($"{GetOutputPrefix()} stopped.");
                OnStopHandler();
                Running = false; 
            }
        }

        internal protected virtual string GetOutputPrefix() => $"{InstanceType} {Guid} / {PipeName}";

        internal protected abstract void WaitForConnection();
        internal protected abstract void OnStopHandler();
        internal protected abstract void OnReadCompletedHandler(byte[] buffer);
        internal protected abstract void OnExceptionHandler(Exception exception);

        public void Write(byte[] buffer) => WriteInternal(buffer, false);

        internal protected void WriteInternal(byte[] buffer,bool flag)
        {
            VaildInitialized();

            if (buffer == null || buffer.Length == 0) return;
            if (!Instance.IsConnected) return;
            if (!Interacted && !flag) return;

            try
            {
                Instance.Write(buffer, 0, buffer.Length);
                Instance.Flush();
            }
            catch (Exception ex)
            {
                OnExceptionHandler(ex);
            }
        }

        internal protected void WaitForReadInternal(AsyncCallback callback)
        {
            try
            {
                OutputLogger?.Invoke($"{GetOutputPrefix()} begin to read.");

                byte[] buffer = new byte[ReadBytesSize];
                Instance.BeginRead(buffer, 0, buffer.Length, callback, buffer);
            }
            catch (Exception ex)
            {
                OnExceptionHandler(ex);
            }
        }

        protected void WaitForRead() => WaitForReadInternal(WaitForReadHandler);

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

                    OnReadCompletedHandler(tmp);
                }

                WaitForRead();
            }
            catch (Exception ex)
            {
                OnExceptionHandler(ex);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Instance?.IsConnected == true && !Stopping) Stop();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NamedPipe()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

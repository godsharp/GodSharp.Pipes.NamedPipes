using System;
using System.Collections.Concurrent;
using System.Text;

namespace GodSharp.Pipes.NamedPipes
{
    public class NamedPipeServer:IDisposable
    {
        ConcurrentDictionary<Guid, NamedPipeMaster> masters = new ConcurrentDictionary<Guid, NamedPipeMaster>();
        NamedPipeMaster master = null;

        public string PipeName => Options.PipeName;
        public int MaxNumberOfServerInstances => Options.MaxNumberOfServerInstances;
        public ConcurrentDictionary<Guid, NamedPipeMaster> Masters => masters;

        public NamedPipeServerOptions Options { get; private set; }

        public NamedPipeServer()
        {
        }

        public NamedPipeServer(NamedPipeServerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Vaild();

            Options = options;

            if (Options.OnWaitForConnectionCompleted == null) Options.OnWaitForConnectionCompleted = NamedPipeConnected;
            else Options.OnWaitForConnectionCompleted += NamedPipeConnected;

            Action<MasterConnectionArgs> stopped = (args) => NamedPipeRemove(args.Guid);
            if (Options.OnStopCompleted == null) Options.OnStopCompleted = stopped;
            else Options.OnStopCompleted += stopped;
        }

        public void Start()
        {
            if (Options == null) throw new InvalidOperationException("This instance not initialized.");

            NamedPipeCreate();
        }

        public void Write(byte[] buffer)
        {
            try
            {
                foreach (var item in masters)
                {
                    if (!item.Value.IsConnected) continue;
                    item.Value.Write(buffer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Stop()
        {
            foreach (var item in masters)
            {
                try
                {
                    item.Value.Stop();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void NamedPipeConnected(MasterConnectionArgs args)
        {
            masters.TryAdd(master.Guid, master);

            master = null;

            PrintListeners();

            if (masters.Count >= Options.MaxNumberOfServerInstances) return;

            NamedPipeCreate();
        }

        private void NamedPipeCreate()
        {
            if (master == null)
            {
                master = new NamedPipeMaster(Options);
                master.Start();
            }
            else
            {
                if (!master.IsConnected) return;
            }
        }

        private void NamedPipeRemove(Guid guid)
        {
            masters.TryRemove(guid, out _);

            PrintListeners();
        }

        void PrintListeners()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"Listeners:{masters.Count}");
            int index = 1;
            foreach (var item in masters.Keys)
            {
                builder.AppendLine($"{index++,3}\t{item}");
            }

            Options.OutputLogger?.Invoke(builder.ToString());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NamedPipeServer()
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
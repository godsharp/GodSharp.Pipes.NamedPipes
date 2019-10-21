using GodSharp.Pipes.NamedPipes;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample
{
    public class Client
    {
        int instanceNumber = 254;
        ConcurrentDictionary<Guid, NamedPipeClient> clients = new ConcurrentDictionary<Guid, NamedPipeClient>();
        ConcurrentDictionary<Guid, int> counters = new ConcurrentDictionary<Guid, int>();

        public Client()
        {

            for (int i = 0; i < instanceNumber; i++)
            {
                NamedPipeClient client = new NamedPipeClient(new NamedPipeClientOptions("namedpipe.default", OnReadCompleted, OnConnectionCompleted, OnStopCompleted, OnException, OnOutputLogging,1024));

                client.Start();
                clients.TryAdd(client.Guid,client);
                counters.TryAdd(client.Guid, 0);
                Task.Delay(10).Wait();
            }

            //Task.Run(() =>
            //{
            //    Task.Delay(2000).Wait();
            //    foreach (var item in clients.Keys)
            //    {
            //        Write(item);
            //        Task.Delay(50).Wait();
            //    }
            //});
        }

        private void Write(Guid guid)
        {
            clients[guid].Write(Encoding.UTF8.GetBytes(DateTime.Now.ToString("HH:mm:ss.fff")));
        }

        private void OnReadCompleted(NamedPipeConnectionArgs args)
        {
            System.Console.WriteLine($"NamedPipeClient {args.Guid} / {args.PipeName} read data from server:{Encoding.UTF8.GetString(args.Buffer)}");
            Write(args.Guid);

            //counters[args.Guid] = counters[args.Guid]++;
            //if (counters[args.Guid] < 200) Write(args.Guid);
        }

        private void OnConnectionCompleted(NamedPipeConnectionArgs args)
        {
        }

        private void OnStopCompleted(NamedPipeConnectionArgs args)
        {

        }

        private void OnException(NamedPipeConnectionArgs args)
        {

        }
        private void OnOutputLogging(string log)
        {
            System.Console.WriteLine(log);
        }
    }
}

using GodSharp.Pipes.NamedPipes;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample
{
    public class Server
    {
        NamedPipeServer server = null;

        public Server()
        {
            server = new NamedPipeServer(new NamedPipeServerOptions("namedpipe.default", OnReadCompleted, OnConnectionCompleted, OnStopCompleted, OnException, OnOutputLogging, readBytesSize:1024,maxNumberOfServerInstances: 254));
            server.Start();

            Task.Run(() =>
            {
                Task.Delay(2000).Wait();
                RandomWrite();
            });
        }

        private void RandomWrite()
        {
            while (true)
            {
                try
                {
                    server.Write(Encoding.UTF8.GetBytes(DateTime.Now.ToString("HH:mm:ss.fff")));
                    Task.Delay(0).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void OnReadCompleted(NamedPipeConnectionArgs args)
        {
            System.Console.WriteLine($"NamedPipeMaster {args.Guid} / {args.PipeName} read data from client:{Encoding.UTF8.GetString(args.Buffer)}");
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

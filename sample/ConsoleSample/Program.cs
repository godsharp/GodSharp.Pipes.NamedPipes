using GodSharp.Pipes.NamedPipes;
using System;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello GodSharp.Pipes.NamedPipes!");

            Server server= new Server();
            Client client = new Client();

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}

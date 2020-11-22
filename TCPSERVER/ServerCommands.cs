using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MTConsole;
using TCPDll.Server;

namespace TCPServer
{
    public class ServerCommands
    {
        Server Server { get; set; }
        public ServerCommands(Server server)
        {
            Server = server;
        }

        public IPAddress Init(ref IPAddress[] ips)
        {
            MultithreadConsole.Init();
            Task serverCommandsTask = new Task(() => {               
                string cmd = "";
                while ((cmd = MultithreadConsole.ReadLine()) != "close")
                {
                    /*switch (cmd)
                    {

                    }*/
                }
                Server.CloseServer();
            });

            int index = 1;
            foreach (IPAddress ip in ips)
            {
                MultithreadConsole.WriteLine($"{index++}. {ip}");
            }
            while (!int.TryParse(MultithreadConsole.ReadLine(), out index) || index <= 0 || index > ips.Length) { }
            serverCommandsTask.Start();
            return ips[index - 1];          
        }
    }
}

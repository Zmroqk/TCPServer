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
            Server.ClientDisconnected += Server_ClientDisconnected;
            Server.OperationMessage += Server_OperationMessage;
            Server.ServerMessage += Server_ServerMessage;
            return ips[index - 1];          
        }

        private void Server_ServerMessage(object sender, TCPDll.Server.EventArgs.ServerMessageEventArgs e)
        {
            MultithreadConsole.WriteLine($"Server: {e.Message}");
        }

        private void Server_ClientDisconnected(object sender, TCPDll.Server.EventArgs.ClientDisconnectedEventArgs e)
        {
            MultithreadConsole.WriteLine($"Client: {e.User.Username} disconnected");
        }

        private void Server_OperationMessage(object sender, TCPDll.Server.EventArgs.OperationMessageEventArgs e)
        {
            MultithreadConsole.WriteLine($"Operation: {e.Message}");
        }
    }
}

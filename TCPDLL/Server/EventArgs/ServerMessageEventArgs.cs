using System;
using System.Collections.Generic;
using System.Text;

namespace TCPDll.Server.EventArgs
{
    public class ServerMessageEventArgs
    {
        public string Message { get; set; }
        public Server Server { get; set; }

        public ServerMessageEventArgs(Server server, string message)
        {
            Server = server;
            Message = message;
        }
    }
}

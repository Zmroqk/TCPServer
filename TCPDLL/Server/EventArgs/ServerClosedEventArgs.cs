using System;
using System.Collections.Generic;
using System.Text;

namespace TCPDll.Server.EventArgs
{
    public class ClientDisconnectedEventArgs
    {
        public string Message { get; set; }
        public User User { get; set; }

        public ClientDisconnectedEventArgs(User user, string message)
        {
            User = user;
            Message = message;
        }
    }
}

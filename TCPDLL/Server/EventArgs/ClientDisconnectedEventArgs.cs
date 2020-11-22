using System;
using System.Collections.Generic;
using System.Text;

namespace TCPDll.Server.EventArgs
{
    public class ServerClosedEventArgs
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }

        public ServerClosedEventArgs()
        {
            ErrorCode = 0;
            Message = "Server closed correctly";
        }

        public ServerClosedEventArgs(int errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TCPDll.Server.EventArgs
{
    public class OperationMessageEventArgs
    {
        public string Message { get; set; }
        public IOperation Operation { get; set; }

        public OperationMessageEventArgs(IOperation operation, string message)
        {
            Operation = operation;
            Message = message;
        }
    }
}

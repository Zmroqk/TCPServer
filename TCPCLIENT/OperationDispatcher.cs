using System;
using System.Collections.Generic;
using System.Text;
using TCPDll;
using TCPCLIENT.Operations;

namespace TCPCLIENT
{
    public class OperationDispatcher
    {
        User User;
        Dictionary<string, string> Headers { get; set; }

        public OperationDispatcher(User user, Dictionary<string, string> headers)
        {
            User = user;
            Headers = headers;
        }

        public void Dispatch()
        {
            if (Headers[TCPDll.Headers.HeaderContent] == TCPDll.Headers.TypeCreateOperation)
            {
                string operationType = Headers[TCPDll.Headers.HeaderOperationType];
                operationType = operationType.Replace("\0","");
                int operationId = int.Parse(Headers[TCPDll.Headers.HeaderOperationId]);
                IClientOperation clientOperation = null;
                switch (operationType)
                {
                    case TCPDll.Headers.OperationTypeGetUsername:
                        clientOperation = new SendUsernameOperation(User, operationId);
                        clientOperation.SendHeader();
                        clientOperation.SendData();
                        break;
                }
                User.Operations.Add(new Operation() { ID = operationId, OperationTask = clientOperation });
            }
        }
    }
}

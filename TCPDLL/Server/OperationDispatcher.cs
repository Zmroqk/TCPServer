using System;
using System.Collections.Generic;
using System.Text;
using TCPDll;
using TCPDll.Server.Operations;
using System.Threading.Tasks;

namespace TCPDll.Server
{
    public class OperationDispatcher
    {
        public event EventHandler<TaskCompletionSource<string>> DataRequired;
        User User;
        Dictionary<string, string> Headers { get; set; }

        public OperationDispatcher(User user, Dictionary<string, string> headers)
        {
            User = user;
            Headers = headers;
        }

        public void Dispatch()
        {
            if (!Headers.ContainsKey(TCPDll.Headers.HeaderContent))
                return;
            if (Headers[TCPDll.Headers.HeaderContent] == TCPDll.Headers.TypeCreateOperation)
            {
                string operationType = Headers[TCPDll.Headers.HeaderOperationType];
                operationType = operationType.Replace("\0","");
                int operationId = int.Parse(Headers[TCPDll.Headers.HeaderOperationId]);
                IClientOperation clientOperation = null;
                switch (operationType)
                {
                    case TCPDll.Headers.OperationTypeSendFile:
                        Task newOperation = new Task(() =>
                           {                              
                               clientOperation = new DownloadFileToServerOperation(User, operationId);
                               User.Operations.Add(new Operation() { ID = operationId, OperationTask = clientOperation });
                               clientOperation.Init();
                           });
                        newOperation.Start();
                        break;
                }               
            }
        }
    }
}

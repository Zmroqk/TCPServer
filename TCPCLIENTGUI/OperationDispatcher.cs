using System;
using System.Collections.Generic;
using System.Text;
using TCPDll;
using TCPClientGUI.Operations;
using System.Threading.Tasks;

namespace TCPClientGUI
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
            if (Headers[TCPDll.Headers.HeaderContent] == TCPDll.Headers.TypeCreateOperation)
            {
                string operationType = Headers[TCPDll.Headers.HeaderOperationType];
                operationType = operationType.Replace("\0","");
                int operationId = int.Parse(Headers[TCPDll.Headers.HeaderOperationId]);
                IClientOperation clientOperation = null;
                switch (operationType)
                {
                    case TCPDll.Headers.OperationTypeGetUsername:
                        Task newOperation = new Task(() =>
                           {
                               TaskCompletionSource<string> awaitingData = new TaskCompletionSource<string>();
                               DataRequired(this, awaitingData);
                               string username = awaitingData.Task.Result;
                               if (string.IsNullOrEmpty(username))
                               {
                                   User.Client.Close();
                                   return;
                               }                                  
                               clientOperation = new SendUsernameOperation(User, operationId, username);
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

using System;
using System.Collections.Generic;
using System.Text;
using TCPDll;
using TCPDll.Server.Operations;
using System.Threading.Tasks;
using TCPDll.Server.EventArgs;

namespace TCPDll.Server
{
    /// <summary>
    /// Dispatches operations accordingly to request header
    /// </summary>
    public class OperationDispatcher
    {
        /// <summary>
        /// Occurs when one of operations requires additional data
        /// </summary>
        public event EventHandler<TaskCompletionSource<string>> DataRequired;
        /// <summary>
        /// User that inited this dispatcher
        /// </summary>
        User User;
        /// <summary>
        /// Handler for operation messages
        /// </summary>
        EventHandler<OperationMessageEventArgs> MessageHandler { get; set; }
        /// <summary>
        /// Operation request headers
        /// </summary>
        Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Fill dispatcher with data
        /// </summary>
        /// <param name="user">User that inits this dispatcher</param>
        /// <param name="headers">Request headers</param>
        /// <param name="messageHandler">Handler for messages</param>
        public OperationDispatcher(User user, Dictionary<string, string> headers, EventHandler<OperationMessageEventArgs> messageHandler = null)
        {
            User = user;
            Headers = headers;
            MessageHandler = messageHandler;
        }

        /// <summary>
        /// Create new operation accordingly to what was requested
        /// </summary>
        public void Dispatch()
        {
            if (!Headers.ContainsKey(TCPDll.Headers.HeaderContent))
                return;
            if (Headers[TCPDll.Headers.HeaderContent] == TCPDll.Headers.TypeCreateOperation)
            {
                string operationType = Headers[TCPDll.Headers.HeaderOperationType];
                operationType = operationType.Replace("\0","");
                int operationId = int.Parse(Headers[TCPDll.Headers.HeaderOperationId]);
                IOperation clientOperation = null;
                switch (operationType)
                {
                    case TCPDll.Headers.OperationTypeSendFile:
                        Task newOperation = new Task(() =>
                           {                              
                               clientOperation = new DownloadFileToServerOperation(User, operationId, MessageHandler);
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

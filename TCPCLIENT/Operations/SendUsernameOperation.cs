using System;
using System.Collections.Generic;
using System.Text;
using TCPDll;

namespace TCPCLIENT.Operations
{
    public class SendUsernameOperation : IClientOperation
    {
        User User { get; set; }
        int OperationId { get; set; }

        public SendUsernameOperation(User user, int operationId)
        {
            User = user;
            OperationId = operationId;
        }

        public void SendData()
        {
            byte[] username = Encoding.UTF8.GetBytes("Username");
            byte[] data = new byte[username.Length + Headers.HeaderSize];
            Headers.Fill(ref data, Headers.PacketTypeData, OperationId, ref username);
            User.Client.Client.Send(data);
        }

        public void SendHeader()
        {
            string headerString = $"{Headers.HeaderContent}:{Headers.TypeString}\n" +
               $"{Headers.HeaderDataLength}:{Encoding.UTF8.GetBytes("Username").Length}";
            byte[] header = new byte[Headers.BufferSize];        
            byte[] headerStringBytes = Encoding.UTF8.GetBytes(headerString);
            Headers.Fill(ref header, Headers.PacketTypeHeader, OperationId, ref headerStringBytes);
            User.Client.Client.Send(header);
        }

        public void PutHeader(ref Dictionary<string, string> headers)
        {
            string content = headers[Headers.HeaderContent];
            if (string.IsNullOrEmpty(content) || content != Headers.TypeEndOperation)
                return;
            content = headers[Headers.HeaderOperationId];
            if (string.IsNullOrEmpty(content))
                return;
            int operationId;
            if (int.TryParse(content, out operationId) && operationId == OperationId)
            {
                User.Operations.RemoveAll((op) => op.OperationTask == this);
            }
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        public void Init()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        public void EndOperation()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        public void PutData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}

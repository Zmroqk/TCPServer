using System;
using System.Collections.Generic;
using System.Text;
using TCPDll;
using TCPDll.Tools.Extensions;

namespace TCPDll.Server.Operations
{
    /// <summary>
    /// GET user data (username) SERVER SIDE
    /// </summary>
    public class ClientDataOperation : IOperation
    {
        /// <summary>
        /// User for this operation
        /// </summary>
        User User { get; set; }
        /// <summary>
        /// Username string length
        /// </summary>
        int UsernameLength { get; set; }
        /// <summary>
        /// Operation id
        /// </summary>
        int OperationId { get; set; }

        /// <summary>
        /// String builder for this operation
        /// </summary>
        StringBuilder stringBuilder { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="user">User for this opearion</param>
        /// <param name="operationId">Operation id of this operation</param>
        public ClientDataOperation(User user, int operationId)
        {
            User = user;
            UsernameLength = 0;
            OperationId = operationId;
            stringBuilder = new StringBuilder();
        }

        /// <summary>
        /// Init this operation, Send header Create-Operation
        /// </summary>
        public void Init() {
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeCreateOperation}\n" +
                $"{Headers.HeaderOperationId}: {OperationId}\n" +
                $"{Headers.HeaderOperationType}: {Headers.OperationTypeGetUsername}";
            byte[] header = new byte[Headers.BufferSize];                     
            byte[] headerStringBytes = Encoding.UTF8.GetBytes(headerString);
            header.Fill(Headers.PacketTypeHeader, OperationId, ref headerStringBytes);
            //Headers.Fill(ref header, Headers.PacketTypeHeader, OperationId, ref headerStringBytes);
            User.ClientSocket.Send(header);
        }

        /// <summary>
        /// Get header with data
        /// </summary>
        /// <param name="headers">Headers for this operation</param>
        public void PutHeader(ref Dictionary<string, string> headers)
        {
            string content = headers[Headers.HeaderContent];
            if(string.IsNullOrEmpty(content) || content != Headers.TypeString)
                return;
            content = headers[Headers.HeaderDataLength];
            if (string.IsNullOrEmpty(content))
                return;
            int usernameLength;
            if(int.TryParse(content, out usernameLength))
            {
                UsernameLength = usernameLength;
            }
        }

        /// <summary>
        /// Get data
        /// </summary>
        /// <param name="data">Data for this operation</param>
        public void PutData(byte[] data)
        {
            if (UsernameLength == 0 || data.Length == 0)
                return;
            if(data.Length >= UsernameLength)
            {
                stringBuilder.Append(Encoding.UTF8.GetString(data, 0, UsernameLength));
                string username = stringBuilder.ToString();
                User.Username = username;
                EndOperation();
            }
            else
            {
                stringBuilder.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                UsernameLength -= data.Length;
            }
        }

        /// <summary>
        /// End opeartion by sending End-Operation
        /// </summary>
        public void EndOperation()
        {
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeEndOperation}\n" +
                $"{Headers.HeaderOperationId}: {OperationId}";
            byte[] header = new byte[Headers.BufferSize];
            byte[] headerStringBytes = Encoding.UTF8.GetBytes(headerString);
            Headers.Fill(ref header, Headers.PacketTypeHeader, OperationId, ref headerStringBytes);
            User.ClientSocket.Send(header);
            User.Operations.RemoveAll((op) => op.OperationTask == this);
        }


        public void SendHeader()
        {
            throw new NotImplementedException();
        }

        public void SendData()
        {
            throw new NotImplementedException();
        }
    }
}

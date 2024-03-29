﻿using System;
using System.Collections.Generic;
using System.Text;
using TCPDll;
using TCPDll.EventArgs;
using TCPDll.Tools.Extensions;
using System.Linq;


namespace TCPClientGUI.Operations
{
    public class SendUsernameOperation : IOperation
    {
        User User { get; set; }
        int OperationId { get; set; }
        string Username { get; set; }

        public SendUsernameOperation(User user, int operationId, string username)
        {
            User = user;
            OperationId = operationId;
            Username = username;
        }

        public event EventHandler<OperationStatusChangedEventArgs> StatusChanged;

        public void SendData()
        {
            byte[] username = Encoding.UTF8.GetBytes(Username);
            int dataAlreadySend = 0;
            byte[] data;
            if(username.Length > Headers.SizeDifferential)
            {
                while (dataAlreadySend < username.Length - Headers.SizeDifferential)
                {
                    data = new byte[Headers.BufferSize];
                    data.FillHeader(Headers.PacketTypeData, OperationId);
                    data.FillData(ref username, dataAlreadySend, Headers.SizeDifferential);
                    //Headers.FillHeader(ref data, Headers.PacketTypeData, OperationId);
                    //Headers.FillData(ref data, ref username, dataAlreadySend, Headers.SizeDifferential);
                    dataAlreadySend += Headers.SizeDifferential;
                    User.ClientSocket.Send(data);
                }
            }
            data = new byte[username.Length - dataAlreadySend + Headers.HeaderSize];
            data.FillHeader(Headers.PacketTypeData, OperationId);
            data.FillData(ref username, dataAlreadySend, username.Length - dataAlreadySend);
            //Headers.FillHeader(ref data, Headers.PacketTypeData, OperationId);
            //Headers.FillData(ref data, ref username, dataAlreadySend, username.Length-dataAlreadySend);
            User.ClientSocket.Send(data);         
        }
        
        public void SendHeader()
        {
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeString}\n" +
               $"{Headers.HeaderDataLength}: {Encoding.UTF8.GetBytes(Username).Length}";
            byte[] header = new byte[Headers.BufferSize];        
            byte[] headerStringBytes = Encoding.UTF8.GetBytes(headerString);
            header.Fill(Headers.PacketTypeHeader, OperationId, ref headerStringBytes);
            //Headers.Fill(ref header, Headers.PacketTypeHeader, OperationId, ref headerStringBytes);
            User.ClientSocket.Send(header);
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
                User.Operations.Remove(User.Operations.First((op) => op.OperationTask == this));
            }
        }

        /// <summary>
        /// Init this operation
        /// </summary>
        public void Init()
        {
            SendHeader();
            SendData();
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

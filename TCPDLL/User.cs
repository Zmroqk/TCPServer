﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace TCPDll
{
    /// <summary>
    /// TCP user class
    /// </summary>
    public class User
    {
        /// <summary>
        /// When new operation occurs
        /// </summary>
        public event EventHandler<Dictionary<string, string>> onNewOperation;
        /// <summary>
        /// When client disconnects
        /// </summary>
        public event EventHandler<string> onClientDisconnection;
        /// <summary>
        /// When client occurs on error
        /// </summary>
        public event EventHandler<Exception> ClientError;
        /// <summary>
        /// TCP client
        /// </summary>
        public TcpClient Client { get; set; }
        /// <summary>
        /// Tcp client socket
        /// </summary>
        public Socket ClientSocket { 
            get {
                return Client.Client;
            } 
        }
        /// <summary>
        /// User name
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// List of operations
        /// </summary>
        public List<Operation> Operations { get; set; }

        object lockState;
        /// <summary>
        /// Current downloaded data
        /// </summary>
        byte[] data;

        /// <summary>
        /// Default constructor
        /// </summary>
        public User()
        {
            Client = new TcpClient();
            Operations = new List<Operation>();
            lockState = new object();
        }

        /// <summary>
        /// Begin receiving data 
        /// </summary>
        public void StartReceiveData()
        {
            data = new byte[Headers.BufferSize];
            BeginReceive();
        }

        /// <summary>
        /// Generete new operation id for this user
        /// </summary>
        /// <returns>Available operation id</returns>
        public int GenerateOperationId()
        {
            int id;
            Random random = new Random();
            do
            {
                id = random.Next();
            } while (this.Operations.Find((op) => op.ID == id) != null);
            return id;
        }

        /// <summary>
        /// Clear buffer data
        /// </summary>
        void ClearBuffer()
        {          
            for (int i = 0; i < data.Length; i++) {
                data[i] = 0;
            }
        }

        /// <summary>
        /// Begin receiving data
        /// </summary>
        public void BeginReceive()
        {
            ClientSocket.BeginReceive(data, 0, Headers.BufferSize, SocketFlags.None, DataReceived, null);
        }

        /// <summary>
        /// Process data as raw data
        /// </summary>
        /// <param name="operation">Operation to which data belongs</param>
        void ProcessData(Operation operation)
        {
            if (operation != null)
            {
                byte[] rawData = new byte[Headers.BufferSize - Headers.HeaderSize];
                for (int i = 0; i < rawData.Length; i++)
                {
                    rawData[i] = data[i + Headers.HeaderSize];
                }
                try
                {
                    operation.OperationTask.PutData(rawData);
                }
                catch(Exception e)
                {
                    ClientError?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        /// Process datrra as header data
        /// </summary>
        /// <param name="dataReceived">Size of data received</param>
        /// <param name="operation">Operation to which data belongs</param>
        void ProcessHeaderData(int dataReceived, Operation operation)
        {
            string headerString = Encoding.UTF8.GetString(data, Headers.HeaderSize, dataReceived - Headers.HeaderSize);
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string[] headerPairs = headerString.Split('\n');
            foreach (string pair in headerPairs)
            {
                string[] pairSplited = pair.Split(": ");
                for(int i = 0; i < pairSplited.Length; i++)
                {
                    pairSplited[i] = pairSplited[i].Replace("\0", "");
                }
                if (pairSplited.Length >= 2)
                    headers.Add(pairSplited[0], pairSplited[1]);
            }
            if (operation != null)
            {
                try {
                    operation.OperationTask.PutHeader(ref headers);
                }
                catch (Exception e)
                {
                    ClientError?.Invoke(this, e);
                }
            }
            else
            {
                onNewOperation?.Invoke(this, headers);
            }
        }

        /// <summary>
        /// Process received data
        /// </summary>
        /// <param name="asyncResult">Result of BeginReceive()</param>
        void DataReceived(IAsyncResult asyncResult)
        {
            lock (lockState)
            {
                try
                {
                    if(ClientSocket == null)
                    {
                        onClientDisconnection?.Invoke(this, "Client is disconnected");
                        return;
                    }
                    int dataReceived = ClientSocket.EndReceive(asyncResult);
                    if (dataReceived < Headers.HeaderSize)
                    {
                        ClearBuffer();
                        BeginReceive();
                        return;
                    }
                    char typeOfPacket = (char)data[0];
                    byte[] Id = new byte[8];
                    for (int i = 1; i < Headers.HeaderSize; i++)
                    {
                        Id[i - 1] = data[i];
                    }
                    int operationId = BitConverter.ToInt32(Id, 0);
                    Operation operation = Operations.Find((op) => op.ID == operationId);
                    if (typeOfPacket == Headers.PacketTypeData)
                    {
                        ProcessData(operation);
                    }
                    else if (typeOfPacket == Headers.PacketTypeHeader)
                    {
                        ProcessHeaderData(dataReceived, operation);
                    }
                    ClearBuffer();
                    BeginReceive();
                }
                catch (Exception e)
                {
                    ClientError?.Invoke(this, e);
                }
            }
        }
    }
}

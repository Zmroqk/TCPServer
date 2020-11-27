using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TCPDll.Server.EventArgs;
using TCPDll.Server.Operations;

namespace TCPDll.Server
{
    /// <summary>
    /// Server class
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Tcp listener socket
        /// </summary>
        TcpListener ServerSocket { get; set; }

        /// <summary>
        /// Connected clients
        /// </summary>
        List<User> Clients { get; set; }

        /// <summary>
        /// Ocurrs when server closes
        /// </summary>
        public event EventHandler<ServerClosedEventArgs> ServerClosed;

        /// <summary>
        /// Ocurrs when client disconnects
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        /// Ocurrs when one of operations sends message
        /// </summary>
        public event EventHandler<OperationMessageEventArgs> OperationMessage;

        /// <summary>
        /// Ocurrs when one of operations sends message
        /// </summary>
        public event EventHandler<ServerMessageEventArgs> ServerMessage;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Server()
        {
            Clients = new List<User>();
        }

        /// <summary>
        /// Get all available IPAddresses
        /// </summary>
        /// <returns></returns>
        public IPAddress[] GetAvailableAddresses()
        {
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            List<IPAddress> availableAddresses = new List<IPAddress>();
            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    availableAddresses.Add(address);
            }
            return availableAddresses.ToArray();
        }

        /// <summary>
        /// Initialise server
        /// <error-code>1</error-code>
        /// </summary>
        public void InitServer(IPAddress ipAddress, int port)
        {
            try
            {
                ServerSocket = new TcpListener(ipAddress, port);
                ServerSocket.Start();
                ServerSocket.BeginAcceptTcpClient(ConnectUser, null);
                ServerMessage?.Invoke(this, new ServerMessageEventArgs(this, $"Server TCP started at: {ipAddress}:{port}\n" + $"Server version v0.1.1"));
            }
            catch(Exception e)
            {
                ServerClosed?.Invoke(this, new ServerClosedEventArgs(1, e.Message));
            }
        }

        /// <summary>
        /// Close server connections
        /// </summary>
        public void CloseServer()
        {
            foreach(User client in Clients)
                if(client.Client.Connected)
                {
                    client.onClientDisconnection -= OnClientDisconnect;
                    client.Client.Close();
                }
            if(ServerSocket.Server.Connected)
                ServerSocket.Stop();

            ServerClosed?.Invoke(this, new ServerClosedEventArgs());
        }

        /// <summary>
        /// On user connection
        /// </summary>
        /// <param name="asyncResult"></param>
        void ConnectUser(IAsyncResult asyncResult)
        {
            TcpClient client = ServerSocket.EndAcceptTcpClient(asyncResult);
            client.ReceiveBufferSize = Headers.BufferSize;
            client.SendBufferSize = Headers.BufferSize;
            User newUser = new User() { Client = client };
            ServerSocket.BeginAcceptTcpClient(ConnectUser, null);

            newUser.onClientDisconnection += OnClientDisconnect;
            newUser.onNewOperation += NewUser_onNewOperation;
            newUser.StartReceiveData();
            int Id = newUser.GenerateOperationId();
            Task newOperation = new Task(() =>
               {
                   IOperation operation = new ClientDataOperation(newUser, Id);
                   operation.Init();
                   newUser.Operations.Add(new Operation()
                   {
                       ID = Id,
                       OperationTask = operation
                   });
               });
            newOperation.Start();
            Clients.Add(newUser);
        }

        /// <summary>
        /// On client new operation request
        /// </summary>
        /// <param name="sender">User that has send request</param>
        /// <param name="e">Headers for request</param>
        private void NewUser_onNewOperation(object sender, Dictionary<string, string> e)
        {
            OperationDispatcher operationDispatcher = new OperationDispatcher((User)sender, e, OperationMessage);
            operationDispatcher.Dispatch();
        }

        /// <summary>
        /// Handle client disconnection
        /// </summary>
        /// <param name="sender">User that disconnected</param>
        /// <param name="message">Cause of disconnection</param>
        void OnClientDisconnect(object sender, string message)
        {
            User user = (User)sender;
            Clients.Remove(user);
            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(user, message));
        }
    }
}

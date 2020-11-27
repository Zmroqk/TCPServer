using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TCPDll;
using TCPClientGUI.Operations;

namespace TCPClientGUI.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// User socket
        /// </summary>
        User User { get; set; }

        /// <summary>
        /// Has user already connected to server
        /// </summary>
        bool AlreadyConnected { get; set; }

        bool _connectionStatus;
        string _connectionString;

        /// <summary>
        /// On property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Connection string for button
        /// </summary>
        public string ConnectionString { get { return _connectionString; } set { _connectionString = value; NotifyPropertyChanged(); } }

        /// <summary>
        /// Connection status true if connected
        /// </summary>
        public bool ConnectionStatus { get { return _connectionStatus; } set { _connectionStatus = value; NotifyPropertyChanged(); } }

        /// <summary>
        /// User selected username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindowViewModel()
        {
            User = new User();
            ConnectionString = "Connect";
            ConnectionStatus = false;
            AlreadyConnected = false;
            Username = "";
        }

        /// <summary>
        /// On new operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_newOperation(object sender, Dictionary<string, string> e)
        {
            OperationDispatcher operationDispatcher = new OperationDispatcher(User, e);
            operationDispatcher.DataRequired += OperationDispatcher_DataRequired;
            operationDispatcher.Dispatch();
        }

        /// <summary>
        /// On operation dispatcher data request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Task where data should be provided</param>
        private void OperationDispatcher_DataRequired(object sender, TaskCompletionSource<string> e)
        {
            e.SetResult(Username);
        }

        public void SendFileToServer(string file)
        {
            if (File.Exists(file))
            {
                string[] filenamePart = file.Split(new char[] { '\\', '/' });
                Task taskSendFile = new Task(() =>
                {
                    int operationId = User.GenerateOperationId();
                    IOperation clientOperation = new SendFileToServerOperation(User, operationId, file, filenamePart[filenamePart.Length - 1]);
                    User.Operations.Add(new Operation() { ID = operationId, OperationTask = clientOperation });
                    clientOperation.Init();
                });
                taskSendFile.Start();
            }
        }

        /// <summary>
        /// Try connecting to server
        /// </summary>
        /// <param name="ipToConnect">Ip to connect to</param>
        public void TryConnect(string ipToConnect)
        {
            if (User.Client.Connected)
                return;
            Task connectionTask = new Task(() =>
            {
                if (AlreadyConnected)
                    User = new User();
                if (!User.Client.Connected)
                {
                    User.Client.Connect(new IPEndPoint(IPAddress.Parse(ipToConnect), 20520));
                }                    
                if (User.Client.Connected)
                {
                    AlreadyConnected = true;
                    ConnectionString = "Connected";
                    ConnectionStatus = true;
                    User.onNewOperation += Client_newOperation;
                    User.onClientDisconnection += Client_onClientDisconnection;
                    User.StartReceiveData();
                }
            });
            connectionTask.Start();
        }

        /// <summary>
        /// On client disconnection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_onClientDisconnection(object sender, string e)
        {
            ConnectionStatus = false;
            ConnectionString = "Connect";
        }

        /// <summary>
        /// On property changed
        /// </summary>
        /// <param name="property">Property name</param>
        private void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TCPDll;

namespace TCPCLIENT
{
    /// <summary>
    /// Logika interakcji dla klasy ConnectTcpServer.xaml
    /// </summary>
    public partial class ConnectTcpServer : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        User Client { get; set; }
        Brush _connectionColor;
        public Brush ConnectionColor { get { return _connectionColor; } set { _connectionColor = value; OnPropertyChanged(); } }

        public ConnectTcpServer()
        {
            InitializeComponent();
            Client = new User();
            ConnectionColor = new SolidColorBrush(Colors.Gray);
            DataContext = this;
        }
      

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if(!Client.Client.Connected)
                Client.Client.Connect(new IPEndPoint(IPAddress.Parse(TxbIpAddress.Text), 20520));
            if (Client.Client.Connected)
            {
                Client.onNewOperation += Client_newOperation;
                Client.StartReceiveData();
                ConnectionColor = new SolidColorBrush(Colors.Green);
            }
        }

        private void Client_newOperation(object sender, Dictionary<string, string> e)
        {
            OperationDispatcher operationDispatcher = new OperationDispatcher(Client, e);
            operationDispatcher.Dispatch();
        }

        public User GetClient()
        {
            if (!Client.Client.Connected)
            {
                ConnectionColor = new SolidColorBrush(Colors.Gray);
            }
            return Client;
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

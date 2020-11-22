using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MTConsole;
using TCPDll.Server;
using Cfg;

namespace TCPServer
{
    class Program
    {
        static Task ServerThread;
        static Server Server;

        static void Main(string[] args)
        {          
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            ServerThread = new Task(() =>
            {
                Server = new Server();
                Server.ServerClosed += (sender, e) => { Console.WriteLine($"Code: {e.ErrorCode} :: {e.Message}"); Thread.Sleep(2000); taskCompletionSource.SetResult(true); };
                Config.ReadConfigFromJson<ConfigModel>();
                IPAddress[] ips = Server.GetAvailableAddresses();              
                ServerCommands serverCommands = new ServerCommands(Server);
                IPAddress selectedIp = serverCommands.Init(ref ips);
                Server.InitServer(selectedIp, (int)Config.Data["Port"]);
            });
            ServerThread.Start();
            
            taskCompletionSource.Task.Wait();
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCPClientGUI.ViewModel;

namespace TCPClientGUI
{
    public class MainWindow : Window
    {
        MainWindowViewModel viewModel;
        TextBox TxbAddress { get; set; }

        public MainWindow()
        {
            InitializeComponent();           
            TxbAddress = this.FindControl<TextBox>("TxbAddress");
            Icon = new WindowIcon("ClientIcon.ico");
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            viewModel.TryConnect(TxbAddress.Text);
        }
        private void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Directory = ".";
            fileDialog.Title = "Choose files";
            Task runDialog = new Task(() => { 
                Task<string[]> fileDialogTask = fileDialog.ShowAsync(this);
                string[] files = fileDialogTask.Result;
                viewModel.SendFileToServer(files[files.Length - 1]);
            });
            runDialog.Start();
        }     
    }
}

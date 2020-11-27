using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPDll;
using System.Timers;
using TCPDll.Tools;
using TCPDll.Server.EventArgs;

namespace TCPDll.Server.Operations
{
    public class DownloadFileToServerOperation : IOperation
    {
        User User { get; set; }
        int OperationId { get; set; }
        int CurrentDownload { get; set; }
        int ExpectedDownloadSize { get; set; }
        int ExpectedFilenameDownloadSize { get; set; }
        int OperationStep { get; set; }
        string Filename { get; set; }
        FileStream FileStream { get; set; }
        FileDownloader FileDownloader { get; set; }
        StringBuilder FilenameBuilder { get; set; }
        EventHandler<OperationMessageEventArgs> MessageHandler { get; set; }
        //Timer TimerForDebugInfo { get; set; }

        public DownloadFileToServerOperation(User user, int operationId, EventHandler<OperationMessageEventArgs> messageHandler = null)
        {
            User = user;
            OperationId = operationId;
            CurrentDownload = 0;
            ExpectedDownloadSize = 0;
            ExpectedFilenameDownloadSize = 0;
            OperationStep = 0;
            Filename = "";
            FilenameBuilder = new StringBuilder();
            MessageHandler = messageHandler;
        }

        public void EndOperation()
        {
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeEndOperation}";
            byte[] header = Headers.CreateHeader(OperationId, headerString);
            User.Client.Client.Send(header);
        }

        public void Init()
        {
            SendHeader();
        }

        public void PutData(byte[] data)
        {
            if(OperationStep == 0)
            {
                DownloadFilename(ref data);
            }
            else if(OperationStep == 1)
            {
                FileDownloader.PutNewData(ref data);
                SendHeader();
                //DownloadFile(ref data);
            }
        }

        public void PutHeader(ref Dictionary<string, string> headers)
        {
            string content = headers[Headers.HeaderContent];
            if (string.IsNullOrEmpty(content))
                return;
            switch (content)
            {
                case Headers.TypeString:
                    content = headers[Headers.HeaderDataLength];
                    ExpectedFilenameDownloadSize = int.Parse(content);
                    break;
                case Headers.TypeFile:
                    content = headers[Headers.HeaderDataLength];
                    ExpectedDownloadSize = int.Parse(content);
                    InitFileDownloader();
                    break;
            }
            SendHeader();
        }

        public void DownloadFile(ref byte[] data)
        {
            CurrentDownload += data.Length;
            if (CurrentDownload >= ExpectedDownloadSize)
            {
                FileStream.Write(data, 0, data.Length - (CurrentDownload - ExpectedDownloadSize));
                FileStream.Close();
                EndOperation();
                /*if(TimerForDebugInfo != null)
                {
                    TimerForDebugInfo.Stop();
                    TimerForDebugInfo.Dispose();
                    TimerForDebugInfo = null;
                }*/
                User.Operations.RemoveAll((op) => op.ID == OperationId);            
            }
            else
            {
                FileStream.Write(data, 0, data.Length);
            }
            SendHeader();                    
        }

        public void DownloadFilename(ref byte[] data)
        {
            CurrentDownload += data.Length;
            string stringData = Encoding.UTF8.GetString(data, 0, data.Length);
            stringData = stringData.Replace("\0", "");
            FilenameBuilder.Append(stringData);
            if(CurrentDownload >= ExpectedFilenameDownloadSize)
            {
                Filename = FilenameBuilder.ToString();
                //DateTime dateTime = DateTime.Now;
                //string directoryPath = $"./DataDownload/{dateTime.ToString("yyyy_MM_dd_HH_mm_ss")}_{OperationId}/";
                //DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                //if (!directoryInfo.Exists)
                //    directoryInfo.Create();
                //FileStream = new FileStream(directoryPath + Filename, FileMode.CreateNew, FileAccess.Write);              
                OperationStep = 1;
                CurrentDownload = 0;
                SendHeader();
            }
        }

        private void InitFileDownloader()
        {
            DateTime dateTime = DateTime.Now;
            string directoryPath = $"./DataDownload/{dateTime.ToString("yyyy_MM_dd_HH_mm_ss")}_{OperationId}/";
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            FileDownloader = new FileDownloader(directoryPath + Filename, ExpectedDownloadSize);
            FileDownloader.FileDownloaded += FileDownloader_FileDownloaded;
            FileDownloader.PropertyChanged += FileDownloader_PropertyChanged;
            FileDownloader.Init();
        }

        private void FileDownloader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MessageHandler?.Invoke(this, new OperationMessageEventArgs(this, FileDownloader.DownloadSpeed.ToString("F3")));
        }

        private void FileDownloader_FileDownloaded(object sender, FileDownloadEventArgs e)
        {
            FileDownloader.Dispose();
            EndOperation();
            User.Operations.RemoveAll((op) => op.ID == OperationId);
            FileDownloader = null;
        }

        public void SendHeader()
        {
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeProceed}";
            byte[] header = Headers.CreateHeader(OperationId, headerString);
            User.Client.Client.Send(header);
        }
    }
}

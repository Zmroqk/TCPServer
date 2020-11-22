using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using TCPDll.Server.EventArgs;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TCPDll.Tools
{
    public class FileDownloader : IDisposable, INotifyPropertyChanged
    {
        public event EventHandler<FileDownloadEventArgs> FileDownloaded;
        public event PropertyChangedEventHandler PropertyChanged;

        FileStream FileStream { get; set; }
        long DownloadSize { get; set; }
        long CurrentDownload { get; set; }
        Timer TimerDownloadSpeed { get; set; }

        int DownloadLastSecond { get; set; }
        public double DownloadSpeed { get => _downloadSpeed; set { _downloadSpeed = value; NotifyPropertyChanged(); } }

        double _downloadSpeed;

        object lockState;

        public FileDownloader(string path, long downloadSize)
        {
            FileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite);
            TimerDownloadSpeed = new Timer(1000);
            TimerDownloadSpeed.Elapsed += TimerDownloadSpeed_Elapsed;
            DownloadSize = downloadSize;
            lockState = new object();
        }

        public void Init()
        {
            CurrentDownload = 0;
            FileStream.Lock(0, DownloadSize);
            TimerDownloadSpeed.Start();
        }

        private void TimerDownloadSpeed_Elapsed(object sender, ElapsedEventArgs e)
        {
            DownloadSpeed = DownloadLastSecond / (double)(1024 * 1024);
            DownloadLastSecond = 0;
        }

        public void PutNewData(ref byte[] data)
        {
            CurrentDownload += data.Length;
            DownloadLastSecond += data.Length;
            lock (lockState)
            {
                if (CurrentDownload >= DownloadSize)
                {
                    FileStream.Write(data, 0, data.Length - (int)(CurrentDownload - DownloadSize));
                    FileDownloaded?.Invoke(this, new FileDownloadEventArgs(DownloadSize, FileStream.Name));
                }
                else
                {
                    FileStream.Write(data, 0, data.Length);
                }
            }
        }

        public void Dispose()
        {
            FileStream.Unlock(0, DownloadSize);
            FileStream.Close();
            TimerDownloadSpeed.Stop();

            FileStream.Dispose();
            TimerDownloadSpeed.Dispose();
        }

        private void NotifyPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using TCPDll.Server.EventArgs;
using System.ComponentModel;
using TCPDll.Tools.Extensions;

namespace TCPDll.Tools
{
    /// <summary>
    /// This class do not download file, it is a stream to a new file.
    /// It allows for easy checking if stream reached expected position
    /// </summary>
    public class FileDownloader : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Fires when file download is completed
        /// </summary>
        public event EventHandler<FileDownloadEventArgs> FileDownloaded;

        /// <summary>
        /// When property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// File stream to new file where we will save data
        /// </summary>
        FileStream FileStream { get; set; }

        /// <summary>
        /// Size of downloaded file
        /// </summary>
        long DownloadSize { get; set; }

        /// <summary>
        /// Current downloaded size
        /// </summary>
        long CurrentDownload { get; set; }

        /// <summary>
        /// Timer for checking speed of download
        /// </summary>
        Timer TimerDownloadSpeed { get; set; }

        /// <summary>
        /// Size of downloaded data in last second
        /// </summary>
        int DownloadLastSecond { get; set; }

        /// <summary>
        /// Download speed
        /// </summary>
        double _downloadSpeed;

        /// <summary>
        /// Download speed
        /// </summary>
        public double DownloadSpeed {
            get => _downloadSpeed; 
            private set { 
                if (_downloadSpeed != value) { 
                    _downloadSpeed = value; 
                    this.NotifyPropertyChanged(); 
                } 
            } 
        }      

        object lockState;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">Path where to create file</param>
        /// <param name="downloadSize">Size of file to download</param>
        public FileDownloader(string path, long downloadSize)
        {
            FileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite);
            TimerDownloadSpeed = new Timer(1000);
            TimerDownloadSpeed.Elapsed += TimerDownloadSpeed_Elapsed;
            DownloadSize = downloadSize;
            lockState = new object();
            CurrentDownload = 0;
        }

        /// <summary>
        /// Init FileDownloader
        /// </summary>
        public void Init()
        {            
            FileStream.Lock(0, DownloadSize);
            TimerDownloadSpeed.Start();
        }

        /// <summary>
        /// When 1 second of downloading elapses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDownloadSpeed_Elapsed(object sender, ElapsedEventArgs e)
        {
            DownloadSpeed = DownloadLastSecond / (double)(1024 * 1024);
            DownloadLastSecond = 0;
        }

        /// <summary>
        /// Put new data that has been downloaded
        /// </summary>
        /// <param name="data">Data that was downloaded</param>
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

        /// <summary>
        /// Dispose data used by FileDownloader
        /// </summary>
        public void Dispose()
        {
            FileStream.Unlock(0, DownloadSize);
            FileStream.Close();
            TimerDownloadSpeed.Stop();

            FileStream.Dispose();
            TimerDownloadSpeed.Dispose();            
        }
    }
}

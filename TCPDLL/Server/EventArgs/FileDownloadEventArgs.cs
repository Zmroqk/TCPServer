using System;
using System.Collections.Generic;
using System.Text;

namespace TCPDll.Server.EventArgs
{
    public class FileDownloadEventArgs
    {
        public string PathToFile { get; set; }
        public long DownloadSize { get; set; }

        public FileDownloadEventArgs(long downloadSize, string pathToFile)
        {
            DownloadSize = downloadSize;
            PathToFile = pathToFile;
        }
    }
}

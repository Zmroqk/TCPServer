using System;
using System.Collections.Generic;
using System.Text;

namespace TCPDll.EventArgs
{
    public class OperationStatusChangedEventArgs
    {
        public string Message { get; set; }
        public double? DownloadSpeed { get; set; }
        public short? OperationProgress { get; set; }

        public OperationStatusChangedEventArgs(string message, double? downloadSpeed = null, short? operationProgress = null)
        {
            Message = message;
            DownloadSpeed = downloadSpeed;
            OperationProgress = operationProgress;
        }
    }
}

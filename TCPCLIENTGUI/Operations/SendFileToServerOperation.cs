using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPDll;

namespace TCPClientGUI.Operations
{
    public class SendFileToServerOperation : IClientOperation
    {

        User User { get; set; }
        int OperationId { get; set; }
        string Filename { get; set; }
        string FilenameRelative { get; set; }
        FileStream FileStream { get; set; }
        TaskCompletionSource<bool> TaskContinue;

        public SendFileToServerOperation(User user, int operationId, string filename, string filenameRelative)
        {
            User = user;
            OperationId = operationId;
            Filename = filename;
            FilenameRelative = filenameRelative;
            TaskContinue = new TaskCompletionSource<bool>();
        }

        public void EndOperation()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeCreateOperation}\n" +
               $"{Headers.HeaderOperationId}: {OperationId}\n" +
               $"{Headers.HeaderOperationType}: {Headers.OperationTypeSendFile}";
            byte[] header = Headers.CreateHeader(OperationId, headerString);
            User.ClientSocket.Send(header);
            TaskContinue.Task.Wait();
            SendHeader();
            TaskContinue.Task.Wait();
            SendSecondHeader();
            TaskContinue.Task.Wait();
            SendData();
        }

        public void PutData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void PutHeader(ref Dictionary<string, string> headers)
        {
            string content = headers[Headers.HeaderContent];
            if (string.IsNullOrEmpty(content))
                return;
            switch (content)
            {
                case Headers.TypeProceed:
                    TaskContinue.SetResult(true);
                    TaskContinue = new TaskCompletionSource<bool>();
                    break;
                case Headers.TypeEndOperation:
                    User.Operations.RemoveAll((op) => op.ID == OperationId);
                    break;
            }
            
        }

        public void SendData()
        {
            byte[] data;
            while((data = Headers.CreateDataPacket(FileStream, OperationId)).Length != 0)
            {
                User.ClientSocket.Send(data);
                TaskContinue.Task.Wait();
            }           
        }

        public void SendHeader()
        {
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeString}\n" +
              $"{Headers.HeaderDataLength}: {Encoding.UTF8.GetBytes(FilenameRelative).Length}\n";
            byte[] header = Headers.CreateHeader(OperationId, headerString);
            User.ClientSocket.Send(header);

            TaskContinue.Task.Wait();

            byte[] filename = Encoding.UTF8.GetBytes(FilenameRelative);
            int dataAlreadySend = 0;
            byte[] data;
            if (filename.Length > Headers.SizeDifferential)
            {
                while (dataAlreadySend < filename.Length - Headers.SizeDifferential)
                {
                    data = new byte[Headers.BufferSize];
                    data.FillHeader(Headers.PacketTypeData, OperationId);
                    data.FillData(ref filename, dataAlreadySend, Headers.SizeDifferential);
                    dataAlreadySend += Headers.SizeDifferential;
                    User.ClientSocket.Send(data);
                }
            }
            data = new byte[filename.Length + Headers.HeaderSize];
            data.Fill(Headers.PacketTypeData, OperationId, ref filename);
            User.ClientSocket.Send(data);
        }
        public void SendSecondHeader()
        {
            FileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            string headerString = $"{Headers.HeaderContent}: {Headers.TypeFile}\n" +
              $"{Headers.HeaderDataLength}: {FileStream.Length}\n";
            byte[] header = Headers.CreateHeader(OperationId, headerString);
            User.ClientSocket.Send(header);
        }
    }
}

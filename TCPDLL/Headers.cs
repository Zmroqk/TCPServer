using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TCPDll.Exceptions;

namespace TCPDll
{
    /// <summary>
    /// Definitions for headers
    /// </summary>
    public class Headers
    {
        //HEADER-SIZE-DATA-------------------------------------------------------------------------------------------

        /// <summary>
        /// Every package header size
        /// </summary>
        public const int HeaderSize = 9;
        /// <summary>
        /// Package size
        /// </summary>
        public const int BufferSize = 1024;
        /// <summary>
        /// Difference in size: BufferSize - HeaderSize
        /// </summary>
        public const int SizeDifferential = BufferSize - HeaderSize;

        //HEADERS----------------------------------------------------------------------------------------------------

        /// <summary>
        /// Header, Content-type
        /// </summary>
        public const string HeaderContent = "Content-Type";
        /// <summary>
        /// Header, Data-Length
        /// </summary>
        public const string HeaderDataLength = "Data-Length";
        /// <summary>
        /// Header, Operation-ID
        /// </summary>
        public const string HeaderOperationId = "Operation-ID";
        /// <summary>
        /// Header, Operation-Type
        /// </summary>
        public const string HeaderOperationType = "Operation-Type";

        //CONTENT-TYPE-----------------------------------------------------------------------------------------------

        /// <summary>
        /// Content type, type username
        /// </summary>
        public const string TypeString = "String";
        /// <summary>
        /// Content type, type create-operation
        /// </summary>
        public const string TypeCreateOperation = "Create-Operation";
        /// <summary>
        /// Content type, type create-operation
        /// </summary>
        public const string TypeEndOperation = "End-Operation";
        /// <summary>
        /// Content type, type file
        /// </summary>
        public const string TypeFile = "File";
        /// <summary>
        /// Content type, proceed
        /// </summary>
        public const string TypeProceed = "Proceed";

        //OPERATION-TYPE---------------------------------------------------------------------------------------------

        /// <summary>
        /// Operation type, Get-Username
        /// </summary>
        public const string OperationTypeGetUsername = "Get-Username";

        /// <summary>
        /// Operation type, Send-File
        /// </summary>
        public const string OperationTypeSendFile = "Send-File";

        //PACKAGE-TYPE-----------------------------------------------------------------------------------------------

        /// <summary>
        /// Package type: Header
        /// </summary>
        public const char PacketTypeHeader = 'H';
        /// <summary>
        /// Package type: Data
        /// </summary>
        public const char PacketTypeData = 'D';

        //END--------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Create header data
        /// </summary>
        /// <param name="operationId">Operation id</param>
        /// <param name="headerString">Header string</param>
        /// <returns></returns>
        public static byte[] CreateHeader(int operationId, string headerString)
        {
            byte[] headerStringBytes = Encoding.UTF8.GetBytes(headerString);
            byte[] header = new byte[HeaderSize + headerStringBytes.Length];
            header.Fill(PacketTypeHeader, operationId, ref headerStringBytes);
            return header;
        }

        /// <summary>
        /// Create data packet from filestream
        /// </summary>
        /// <param name="fileStream">Filestream to use</param>
        /// <param name="OperationId">Operation id for header</param>
        /// <returns></returns>
        public static byte[] CreateDataPacket(FileStream fileStream, int OperationId)
        {
            if (fileStream.Position < fileStream.Length)
            {
                long dataAlreadySend = fileStream.Position;
                byte[] data;
                byte[] fileData;
                if (fileStream.Length - dataAlreadySend > SizeDifferential)
                {
                    data = new byte[BufferSize];
                    fileData = new byte[SizeDifferential];
                    fileStream.Read(fileData, 0, SizeDifferential);
                    data.FillHeader(PacketTypeData, OperationId);
                    data.FillData(ref fileData, 0, SizeDifferential);
                    return data;
                }
                data = new byte[fileStream.Length - dataAlreadySend + HeaderSize];
                fileData = new byte[fileStream.Length - dataAlreadySend];
                fileStream.Read(fileData, 0, (int)(fileStream.Length - dataAlreadySend));
                data.Fill(PacketTypeData, OperationId, ref fileData);
                return data;
            }
            else
            {
                return new byte[0];
            }
        }

        /// <summary>
        /// Fill array with necessary header data
        /// </summary>
        /// <param name="data">Data array to fill data with</param>
        /// <param name="PacketType">Package type to set</param>
        /// <param name="operationId">Operation id to set</param>
        [Obsolete("Use byte[] extension method instead")]
        public static void FillHeader(ref byte[] data, char PacketType, int operationId)
        {
            data[0] = (byte)PacketType;
            byte[] opId = BitConverter.GetBytes(operationId);
            for (int i = 1; i < opId.Length + 1; i++)
            {
                data[i] = opId[i - 1];
            }
        }

        /// <summary>
        /// Fill array with data
        /// </summary>
        /// <param name="data">Data array to fill data with</param>
        /// <param name="dataToFillWith">Data used to fill</param>
        [Obsolete("Use byte[] extension method instead")]
        public static void FillData(ref byte[] data, ref byte[] dataToFillWith)
        {
            for (int i = HeaderSize; i < dataToFillWith.Length + HeaderSize && i < BufferSize; i++)
            {
                data[i] = dataToFillWith[i - HeaderSize];
            }
        }

        /// <summary>
        /// Fill array with data with given offset and length
        /// </summary>
        /// <param name="data">Data array to fill data with</param>
        /// <param name="dataToFillWith">Data used to fill</param>
        /// <param name="offset">Offset to use when filling data</param>
        /// <param name="lengthToFill">Number of bytes to fill</param>
        [Obsolete("Use byte[] extension method instead")]
        public static void FillData(ref byte[] data, ref byte[] dataToFillWith, int offset, int lengthToFill)
        {
            if(lengthToFill > BufferSize - HeaderSize)
            {
                throw new LengthExceededException();
            }
            int index = HeaderSize;
            int indexForData = offset;
            int remainingDataToFill = lengthToFill;
            while(remainingDataToFill > 0 && indexForData < dataToFillWith.Length)
            {
                data[index++] = dataToFillWith[indexForData++];
                remainingDataToFill--;
            }
        }

        /// <summary>
        /// Fill array with data
        /// </summary>
        /// <param name="data">Array to fill</param>
        /// <param name="PacketType">Package type to set</param>
        /// <param name="operationId">Operation id to set</param>
        /// <param name="dataToFillWith">Data used to fill</param>
        [Obsolete("Use byte[] extension method instead")]
        public static void Fill(ref byte[] data, char PacketType, int operationId, ref byte[] dataToFillWith)
        {
            //FillHeader(ref data, PacketType, operationId);
            FillHeader(ref data, PacketType, operationId);
            FillData(ref data, ref dataToFillWith);
        }
    }
}

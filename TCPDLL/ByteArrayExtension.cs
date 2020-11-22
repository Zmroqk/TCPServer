using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TCPDll.Exceptions;

namespace TCPDll
{
    public static class ByteArrayExtension
    {
        /// <summary>
        /// Fill array with necessary header data
        /// </summary>
        /// <param name="data">Data array to fill data with</param>
        /// <param name="PacketType">Package type to set</param>
        /// <param name="operationId">Operation id to set</param>
        public static void FillHeader(this byte[] data, char PacketType, int operationId)
        {
            if (data.Length < Headers.HeaderSize)
                throw new LengthExceededException();
            data[0] = (byte)PacketType;
            byte[] opId = BitConverter.GetBytes(operationId);
            for (int i = 1; i < opId.Length + 1; i++)
            {
                data[i] = opId[i - 1];
            }
        }

        /// <summary>
        /// Fill array with data with given offset and length
        /// </summary>
        /// <param name="data">Data array to fill data with</param>
        /// <param name="dataToFillWith">Data used to fill</param>
        /// <param name="offset">Offset to use when filling data</param>
        /// <param name="lengthToFill">Number of bytes to fill</param>
        public static void FillData(this byte[] data, ref byte[] dataToFillWith, int offset, int lengthToFill)
        {
            if (data.Length < Headers.HeaderSize + lengthToFill)
                throw new LengthExceededException();
            if (lengthToFill > Headers.BufferSize - Headers.HeaderSize)
            {
                throw new LengthExceededException();
            }
            int index = Headers.HeaderSize;
            int indexForData = offset;
            int remainingDataToFill = lengthToFill;
            while (remainingDataToFill > 0 && indexForData < dataToFillWith.Length)
            {
                data[index++] = dataToFillWith[indexForData++];
                remainingDataToFill--;
            }
        }

        /// <summary>
        /// Fill array with data
        /// </summary>
        /// <param name="data">Data array to fill data with</param>
        /// <param name="dataToFillWith">Data used to fill</param>
        public static void FillData(this byte[] data, ref byte[] dataToFillWith)
        {
            for (int i = Headers.HeaderSize; i < dataToFillWith.Length + Headers.HeaderSize && i < Headers.BufferSize; i++)
            {
                data[i] = dataToFillWith[i - Headers.HeaderSize];
            }
        }

        /// <summary>
        /// Fill array with data
        /// </summary>
        /// <param name="data">Array to fill</param>
        /// <param name="PacketType">Package type to set</param>
        /// <param name="operationId">Operation id to set</param>
        /// <param name="dataToFillWith">Data used to fill</param>
        public static void Fill(this byte[] data, char PacketType, int operationId, ref byte[] dataToFillWith)
        {
            data.FillHeader(PacketType, operationId);
            data.FillData(ref dataToFillWith);
        }
    }
}

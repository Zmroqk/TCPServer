using System;
using System.Collections.Generic;
using TCPDll.EventArgs;

namespace TCPDll
{
    /// <summary>
    /// Incoming operations
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// Init operation
        /// </summary>
        void Init();
        /*
         * /// <summary>
        /// Send header to target
        /// </summary>
        void SendHeader();
        /// <summary>
        /// Send data to target
        /// </summary>
        void SendData();
        */

        /// <summary>
        /// Process header data
        /// </summary>
        /// <param name="headers">Incoming headers</param>
        void PutHeader(ref Dictionary<string, string> headers);

        /// <summary>
        /// Process data 
        /// </summary>
        /// <param name="data">Incoming data</param>
        void PutData(byte[] data);

        /// <summary>
        /// When operation status changes
        /// </summary>
        event EventHandler<OperationStatusChangedEventArgs> StatusChanged;
        /*
        /// <summary>
        /// End operation
        /// </summary>
        void EndOperation();
        */
    }
}

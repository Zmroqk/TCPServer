using System.Threading.Tasks;

namespace TCPDll
{

    /// <summary>
    /// Operation definition class
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Id of operation
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Operation incoming
        /// </summary>
        public IClientOperation OperationTask { get; set; }
    }
}
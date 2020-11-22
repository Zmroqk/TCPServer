using System;
using System.Collections.Generic;
using System.Text;

namespace TCPDll.Exceptions
{
    public class LengthExceededException : Exception
    {
        public LengthExceededException() : base("Length for data fill exceeded") { }
    }
}

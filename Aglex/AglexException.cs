using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aglex
{
    public class AglexException : Exception
    {
        public AglexException() { }

        public AglexException(string message) : base(message) { }

        public AglexException(string message, Exception innerException) : base(message, innerException) { }
    }
}

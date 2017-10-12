using System;

namespace DMS.Core.Exceptions
{
    public class DMSException : Exception
    {
        public DMSException(string message) : base(message)
        {
        }
    }
}

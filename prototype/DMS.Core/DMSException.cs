using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Core
{
    public class DMSException : Exception
    {
        public DMSException(string message) : base(message)
        {
        }
    }
}

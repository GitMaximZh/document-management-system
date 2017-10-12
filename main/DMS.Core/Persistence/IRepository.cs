using System;
using System.Collections.Generic;
using System.Text;

namespace DMS.Core.Persistence
{
    public interface IRepository
    {
        void Initialize();
        void CleanUp();
    }
}

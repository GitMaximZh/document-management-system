using System;

namespace DMS.Core.Persistence
{
    public class DMSTransactionScope : IDisposable
    {
        protected bool isCompleted = false;

        public virtual void Complete()
        {
            isCompleted = true;
        }

        protected virtual void Rollback()
        {
            
        }

        public void Dispose()
        {
            if(!isCompleted)
                Rollback();
        }
    }
}

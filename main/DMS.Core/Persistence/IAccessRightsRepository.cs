using DMS.Core.Security;

namespace DMS.Core.Persistence
{
    public interface IAccessRightsRepository : IRepository
    {
        void AddUser(User user);
    }
}

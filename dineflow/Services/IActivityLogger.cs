// IActivityLogger.cs
namespace dineflow.Services
{
    public interface IActivityLogger
    {
        void ActivityLog(string action, string userId, string detail);
    }
}
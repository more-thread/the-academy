using TRS.Models;

namespace TRS.Interfaces
{
    public interface IJobClassService
    {
        Task<List<JobClass>> GetJobClassByID(string ID);
        Task<List<JobClass>> GetHRJobClassList();
    }
}
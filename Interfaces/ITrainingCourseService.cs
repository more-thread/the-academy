using TRS.Models;

namespace TRS.Interfaces
{
    public interface ITrainingCourseService
    {
        Task<TrainingCourse> GetTrainingCourseByCode(string code);
        Task<List<TrainingCourse>> GetTrainingCourseList();
        bool AddCourse(TrainingCourse model);
        bool UpdateCourse();
    }
}
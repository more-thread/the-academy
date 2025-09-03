using Microsoft.EntityFrameworkCore;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingCourseService : ITrainingCourseService
    {
        //todo add database context
        private readonly AppDBContext _context;
        public TrainingCourseService(AppDBContext context)
        {
            _context = context;
        }
        public async Task<TrainingCourse> GetTrainingCourseByCode(string code)
        {
            var result = _context.mTrainingCourse.Where(w => w.CourseCode == code).Include(c => c.Program).FirstOrDefaultAsync();
            return await result;
        }

        public async Task<List<TrainingCourse>> GetTrainingCourseList()
        {
            var result = _context.mTrainingCourse.Include(c => c.Program.JobClasses).Include(c => c.Program).ToListAsync();            
            return await result;
        }
        public bool AddCourse(TrainingCourse model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        public bool UpdateCourse()
        {       
            return _context.SaveChanges() > 0;
        }
    }   
}
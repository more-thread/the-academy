using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingProgramService : ITrainingProgramService 
    {
        //todo add database context
        private readonly AppDBContext _context;
        public TrainingProgramService(AppDBContext context)
        {
            _context = context;
        }
        public async Task<TrainingProgram> GetTrainingProgramByCode(string code)
        {
            var result = _context.mTrainingProgram.Where(c=>c.ProgramCode == code).Include(c=>c.Courses).Include(c=> c.JobClasses).FirstOrDefaultAsync();
            return await result;
        }
        public async Task<List<TrainingProgram>> GetTrainingProgramList()
        {
            var result = _context.mTrainingProgram.Include(c=>c.Courses).Include(c => c.JobClasses).ToListAsync();            
            return await result;
        }
        
        public bool AddProgram(TrainingProgram model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        
        public bool UpdateProgram()
        {       
            return _context.SaveChanges() > 0;
        }
    }   
}
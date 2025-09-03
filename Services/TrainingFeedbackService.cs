

using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingFeedbackService : ITrainingFeedbackService
    {        
        private readonly AppDBContext _context;
        public TrainingFeedbackService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<TrainingFeedbackQuestions>> GetTrainingFeedbackQuestionsList()
        {
            var result = _context.mTrainingFeedbackQuestions.Where(w=>w.Status == true).ToListAsync();
            return await result;            
        }

        public async Task<TrainingFeedbackQuestions> GetTrainingFeedbackQuestionsByID(string id)
        {
            var result = _context.mTrainingFeedbackQuestions.Where(w=>w.QuestionID == id).FirstOrDefaultAsync();
            return await result;            
        }
        
        public async Task<List<TrainingFeedback>> GetTrainingFeedbackByRegistrationCode(string code)
        {
            var result = await _context.tTrainingFeedback.Where(w=>w.TrainingRegistration.RegistrationCode == code).Include(c => c.TrainingFeedbackQuestions)
            .Include(c => c.TrainingRegistration)            
            .ToListAsync();
            return result;            
        }

        public bool AddFeeback(TrainingFeedback model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        
    }
}
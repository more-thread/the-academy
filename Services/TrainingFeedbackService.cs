

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

        public async Task<List<TrainingRegistration>> GetTrainingFeedbackListByScheduleCode(string code)
        {
            var result = await _context.tTrainingRegistration
                .Where(w => w.TrainingSchedule.TrainingCode == code && w.TrainingRegistrationStatus == "REGISTERED")
                .Include(r => r.EmployeeInfo)
                .Include(r => r.TrainingSchedule)
                .Select(r => new TrainingRegistration
                {
                    RegistrationCode = r.RegistrationCode,
                    EmployeeNo = r.EmployeeNo,
                    TrainingFeedbackStatus = r.TrainingFeedbackStatus,
                    EmployeeInfo = r.EmployeeInfo,
                    TrainingSchedule = r.TrainingSchedule
                })
                .OrderBy(r => r.EmployeeInfo.EmployeeName)
                .ToListAsync();

            return result;
        }

        public async Task<Dictionary<string, Dictionary<string, string>>> GetTrainingFeedbackAnswersByScheduleCode(string code)
        {
            var feedbacks = await _context.tTrainingFeedback
                .Where(w => w.TrainingRegistration.TrainingSchedule.TrainingCode == code)
                .Include(f => f.TrainingFeedbackQuestions)
                .Include(f => f.TrainingRegistration)
                .ToListAsync();

            feedbacks = feedbacks.Where(w => w.TrainingFeedbackQuestions.Category != "Comments").ToList();

            // Group by employee and create a dictionary of answers with text mapping
            try
            {
                var answersByEmployee = feedbacks
                .GroupBy(f => f.EmployeeNo)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .GroupBy(f => f.TrainingFeedbackQuestions.QuestionID)
                        .ToDictionary(
                            qg => qg.Key,
                            qg => MapAnswerToText(
                                qg.OrderByDescending(f => f.DateModified ?? f.DateCreated)
                                  .First().Answer ?? "")
                        )
                );


                return answersByEmployee;
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
            
        }

        /// <summary>
        /// Maps numeric answer values to their corresponding text representations
        /// </summary>
        /// <param name="answer">The numeric answer value</param>
        /// <returns>Text representation of the answer</returns>
        private static string MapAnswerToText(string answer)
        {
            return answer switch
            {
                "1" => "Strongly Disagree",
                "2" => "Disagree",
                "3" => "Agree",
                "4" => "Strongly Agree",
                "0" => "NA",
                _ => answer // Return as-is for text answers or other values
            };
        }

        public bool AddFeeback(TrainingFeedback model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        
    }
}
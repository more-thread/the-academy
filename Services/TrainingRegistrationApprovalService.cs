
using Microsoft.EntityFrameworkCore;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingRegistrationApprovalService : ITrainingRegistrationApprovalService
    {
        //todo add database context
        private readonly AppDBContext _context;
        public TrainingRegistrationApprovalService(AppDBContext context)
        {
            _context = context;
        }
        public bool AddRegistration(TrainingRegistration model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        
        public bool UpdateRegistration()
        {       
            return _context.SaveChanges() > 0;
        }
    }   
}
using Microsoft.EntityFrameworkCore;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingCoordinatorService : ITrainingCoordinatorService
    {        
        private readonly AppDBContext _context;
        public TrainingCoordinatorService(AppDBContext context)
        {
            _context = context;
        }
        
        
        public async Task<TrainingCoordinator> GetTrainingCoordinatorByEmployeeNo(string paramEmployeeNo)
        {            
            var result = _context.mTrainingCoordinator.Where(w => w.EmployeeNo == paramEmployeeNo).FirstOrDefaultAsync();
            return await result;
        }


        public async Task<List<VwHrEmployeeInfo>> GetActiveEmployeeList()
        {
            var _coordinators = _context.mTrainingCoordinator.Select(c => c.EmployeeNo).ToList();
            var _list = _context.VwHrEmployeeInfos.Where(w => w.EmployeeStatus == "A" && !_coordinators.Contains(w.EmployeeNo)).ToListAsync();           
            return await _list;
        }

        public async Task<VwHrEmployeeInfo> GetActiveEmployeeByEmployeeNo(string paramEmployee)
        {
           var result = _context.VwHrEmployeeInfos.Where(w => w.EmployeeNo == paramEmployee);

           return await result.FirstOrDefaultAsync();
        }

        public async Task<List<TrainingCoordinator>> GetTrainingCoordinatorList()
        {                       
            var result = _context.mTrainingCoordinator
            .Join(_context.VwHrEmployeeInfos, tc => tc.EmployeeNo, ei => ei.EmployeeNo, (tc, ei) => new TrainingCoordinator
            {
                EmployeeNo = tc.EmployeeNo,
                EmployeeName = ei.EmployeeName,
                PositionName = ei.PositionName,
                Status = tc.Status
            });
            return await result.ToListAsync();
        }
        
        public bool AddTrainingCoordinator(TrainingCoordinator model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        public bool UpdateStatus()
        {       
            return _context.SaveChanges() > 0;
        }
    }   
}
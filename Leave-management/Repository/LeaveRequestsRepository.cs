using Leave_management.Contract;
using Leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_management.Repository
{
    public class LeaveRequestsRepository : ILeaveRequestsRepository
    {
        private readonly ApplicationDbContext _db;
        public LeaveRequestsRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(LeaveRequests entity)
        {
            await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequests entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequests>> FindAll()
        {
            var LeaveRequests = await _db.LeaveRequests
               .Include(q => q.RequestingEmployee)
               .Include(q => q.ApprovedBy)
               .Include(q => q.LeaveType)
               .ToListAsync();
            return LeaveRequests;
        }

        public async Task<LeaveRequests> FindById(int id)
        {
            var LeaveRequests = await _db.LeaveRequests
                 .Include(q => q.RequestingEmployee)
                 .Include(q => q.ApprovedBy)
                 .Include(q => q.LeaveType)
                 .FirstOrDefaultAsync(q => q.Id == id);
            return LeaveRequests;
        }

        public async Task<ICollection<LeaveRequests>> GetLeaveRequestsByEmployee(string employeeid)
        {
            var LeaveRequests = await FindAll();
            return LeaveRequests.Where(q => q.RequestingEmployeeId == employeeid)
                .ToList();
        }

        public async Task<bool> isExists(int id)
        {
            var exists = await _db.LeaveRequests.AnyAsync(q => q.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var saveChanges = await _db.SaveChangesAsync();
            return saveChanges > 0;
        }

        public async Task<bool> Update(LeaveRequests entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}

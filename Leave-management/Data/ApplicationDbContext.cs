using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Leave_management.Models;

namespace Leave_management.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequests> LeaveRequests { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveAllocation> LeaveAllocations { get; set; }
        public DbSet<Leave_management.Models.LeaveTypeVM> DetailsLeaveTypeVM { get; set; }
        public DbSet<Leave_management.Models.EmployeeVM> EmployeeVM { get; set; }
        public DbSet<Leave_management.Models.LeaveAllocationVM> LeaveAllocationVM { get; set; }
        public DbSet<Leave_management.Models.EditLeaveAllocationVM> EditLeaveAllocationVM { get; set; }
        public DbSet<Leave_management.Models.LeaveRequestsVM> LeaveRequestsVM { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_management.Models
{
    public class LeaveTypeVM
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        
        [Required]
        [Range(1,25, ErrorMessage = "Please Enter Valid Number")]
        [Display(Name = "Default Number Of Days")]
        public int DefaultDays { get; set; }
        [Required]
        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }
    }
    public class DetailsLeaveTypeVM { }
   
}

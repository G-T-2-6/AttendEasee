using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AttendEase.Models
{
    public class Project
    {
        public int ProjectId { get; set; }  
        public string ProjectCode { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        
        // Navigation property: Project can have multiple Users
        public ICollection<User> Users { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspections.Domain
{
    public class FollowUp
    {
        public int Id {get;set;}
        [Required]
        public int InspectionId { get; set; }
        public Inspection? Inspection { get; set; }

        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }
    }
}

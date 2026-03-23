using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspections.Domain
{
    internal class FollowUp
    {
        public int Id {get;set;}
        public int InspectionId { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get;set }
    }
}

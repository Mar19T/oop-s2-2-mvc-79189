using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspections.Domain
{
    public enum FollowUpStatus { Open, Closed }

    public class FollowUp
    {
        public int Id { get; set; }
        public int InspectionId { get; set; }
        public DateTime DueDate { get; set; }
        public FollowUpStatus Status { get; set; }
        public DateTime? ClosedDate { get; set; }
        public Inspection Inspection { get; set; } = null!;
    }

}

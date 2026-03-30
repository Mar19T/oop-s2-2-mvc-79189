using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Inspections.Domain
{
    public enum Outcome { Pass, Fail, PassWithExeptions }

    public class Inspection
    {
        public int Id { get; set; }
        public int PremisesId { get; set; }
        public DateTime InspectionDate { get; set; }
        public int Score { get; set; }
        public Outcome Outcome { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Premises Premises { get; set; } = null!;
        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}
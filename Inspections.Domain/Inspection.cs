using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspections.Domain
{
    public class Inspection
    {
            public int Id { get; set; }
            [Required]
            public int PremisesId { get; set; }
            public Premises? Premises { get; set; }
            public DateTime InspectionDate { get; set; }  
            public int Score { get; set; }
            public string Outcome { get; set; }
            public string Notes { get; set; }
            //relationships:
            public List<FollowUp>? FollowUps { get; set; }
    }
}

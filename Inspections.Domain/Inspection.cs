using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspections.Domain
{
    internal class Inspection
    {
            public int Id { get; set; }
            public int PremisesId { get; set; }
            public DateTime InspectionDate { get; set; }
            public string Score { get; set; }
            public string Outcome { get; set; }
            public string Notes { get; set; }
    }
}

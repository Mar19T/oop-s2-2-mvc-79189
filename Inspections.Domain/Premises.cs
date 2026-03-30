using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspections.Domain
{
    public class Premises
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string RiskRating{ get; set; }
        //relationships:
        public List<Inspection>? Inspections { get; set; }
    }
    public enum RiskRating {Low, Medium, High}
}

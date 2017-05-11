using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Db.Objects
{
    public class Trip
    {
        [Key]
        public int TripID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<TripSection> Sections { get; set; }
    }
}

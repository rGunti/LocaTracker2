using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Db.Objects
{
    public class TripSection
    {
        [Key]
        public int TripSectionID { get; set; }

        public int TripID { get; set; }
        public Trip Trip { get; set; }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocaTracker2.Db.Objects
{
    public class Trip : BaseNotifyPropertyChange
    {
        private int tripId;
        private string name;
        private string description;
        private List<TripSection> sections;

        [Key]
        public int TripID { get { return tripId; } set { tripId = value; RaisePropertyChanged(); } }
        public string Name { get { return name; } set { name = value; RaisePropertyChanged(); } }
        public string Description { get { return description; } set { description = value; RaisePropertyChanged(); } }

        public List<TripSection> Sections { get { return sections; } set { sections = value; RaisePropertyChanged(); } }
    }
}

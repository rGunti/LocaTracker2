using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Settings
{
    public class UnitSettingsReader : SettingsReader<UnitSettingsReader>
    {
        private const string KEY_USE_IMPERIAL_UNITS = "LOCATRACK2.UNITS.UseImperialUnits";

        protected override void InitializeSettingsValues()
        {
            InitSettingsValue(KEY_USE_IMPERIAL_UNITS, false);
        }

        public bool UseImperialUnits { get { return GetBool(KEY_USE_IMPERIAL_UNITS); } set { SetBool(KEY_USE_IMPERIAL_UNITS, value); } }
    }
}

using Verse;

namespace WeaponsSorter
{
    /// <summary>
    /// Definition of the settings for the mod
    /// </summary>
    internal class WeaponsSorterSettings : ModSettings
    {
        public bool SortByTech = true;
        public bool SortByMod = false;
        public int SortSetting = 0;
        public bool RangedSeparate = false;
        public bool MeleeSeparate = false;
        public bool GrenadesSeparate = false;

        /// <summary>
        /// Saving and loading the values
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref SortByTech, "SortByTech", true, false);
            Scribe_Values.Look(ref SortByMod, "SortByMod", false, false);
            Scribe_Values.Look(ref SortSetting, "SortSetting", 0, false);
            Scribe_Values.Look(ref RangedSeparate, "RangedSeparate", false, false);
            Scribe_Values.Look(ref MeleeSeparate, "MeleeSeparate", false, false);
            Scribe_Values.Look(ref GrenadesSeparate, "GrenadesSeparate", false, false);
        }
    }
}
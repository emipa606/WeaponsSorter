﻿using Verse;

namespace WeaponsSorter;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class WeaponsSorterSettings : ModSettings
{
    public bool BladeLinkSeparate;
    public bool GrenadesSeparate;
    public bool MeleeSeparate;
    public bool OneHandedSeparate;
    public bool RangedSeparate;
    public bool SortByMod;
    public bool SortByTag;
    public bool SortByTech = true;
    public int SortSetting;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref SortByTech, "SortByTech", true);
        Scribe_Values.Look(ref SortByMod, "SortByMod");
        Scribe_Values.Look(ref SortByTag, "SortByTag");
        Scribe_Values.Look(ref SortSetting, "SortSetting");
        Scribe_Values.Look(ref RangedSeparate, "RangedSeparate");
        Scribe_Values.Look(ref MeleeSeparate, "MeleeSeparate");
        Scribe_Values.Look(ref OneHandedSeparate, "OneHandedSeparate");
        Scribe_Values.Look(ref GrenadesSeparate, "GrenadesSeparate");
        Scribe_Values.Look(ref BladeLinkSeparate, "BladeLinkSeparate");
    }
}
using System.Collections.Generic;
using Mlie;
using UnityEngine;
using Verse;

namespace WeaponsSorter;

[StaticConstructorOnStartup]
internal class WeaponsSorterMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static WeaponsSorterMod Instance;

    private static string currentVersion;

    public static bool CeLoaded;

    /// <summary>
    ///     The private settings
    /// </summary>
    public readonly WeaponsSorterSettings Settings;

    /// <summary>
    ///     Cunstructor
    /// </summary>
    /// <param name="content"></param>
    public WeaponsSorterMod(ModContentPack content) : base(content)
    {
        Instance = this;
        CeLoaded = ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended", true) != null;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings = GetSettings<WeaponsSorterSettings>();
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Weapons Sorter";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        GUI.contentColor = Color.yellow;
        GUI.contentColor = Color.white;
        if (!(Settings.SortByTech || Settings.SortByMod || Settings.SortByTag || Settings.SortByAmmo))
        {
            Settings.SortByTech = true;
        }

        var enabledSettings = new List<bool>
            { Settings.SortByTech, Settings.SortByMod, Settings.SortByTag, Settings.SortByAmmo };
        if (enabledSettings.Count(b => b.Equals(true)) == 2)
        {
            var categories = new string[2];
            var i = 0;
            for (var j = 0; j < enabledSettings.Count; j++)
            {
                if (!enabledSettings[j])
                {
                    continue;
                }

                switch (j)
                {
                    case 0:
                        listingStandard.CheckboxLabeled("WS_SettingTechCategories".Translate(),
                            ref Settings.SortByTech,
                            "WS_SettingTechCategoriesDescription".Translate());
                        categories[i] = "WS_SettingTech".Translate();
                        break;
                    case 1:
                        listingStandard.CheckboxLabeled("WS_SettingModCategories".Translate(), ref Settings.SortByMod,
                            "WS_SettingModCategoriesDescription".Translate());
                        categories[i] = "WS_SettingMod".Translate();
                        break;
                    case 2:
                        listingStandard.CheckboxLabeled("WS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                            "WS_SettingTagCategoriesDescription".Translate());
                        categories[i] = "WS_SettingTag".Translate();
                        break;
                    case 3 when CeLoaded:
                        listingStandard.CheckboxLabeled("WS_SettingAmmoCategories".Translate(), ref Settings.SortByAmmo,
                            "WS_SettingAmmoCategoriesDescription".Translate());
                        categories[i] = "WS_SettingAmmo".Translate();
                        break;
                    default:
                        Settings.SortByAmmo = false;
                        break;
                }

                i++;
            }

            listingStandard.Gap();
            listingStandard.Label("WS_SettingSortOrder".Translate());
            if (listingStandard.RadioButton($"{categories[0]} / {categories[1]}",
                    Settings.SortSetting == 0))
            {
                Settings.SortSetting = 0;
            }

            if (listingStandard.RadioButton($"{categories[1]} / {categories[0]}",
                    Settings.SortSetting == 1))
            {
                Settings.SortSetting = 1;
            }
        }
        else
        {
            listingStandard.CheckboxLabeled("WS_SettingTechCategories".Translate(), ref Settings.SortByTech,
                "WS_SettingTechCategoriesDescription".Translate());
            listingStandard.CheckboxLabeled("WS_SettingModCategories".Translate(), ref Settings.SortByMod,
                "WS_SettingModCategoriesDescription".Translate());
            listingStandard.CheckboxLabeled("WS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                "WS_SettingTagCategoriesDescription".Translate());
            if (CeLoaded)
            {
                listingStandard.CheckboxLabeled("WS_SettingAmmoCategories".Translate(), ref Settings.SortByAmmo,
                    "WS_SettingAmmoCategoriesDescription".Translate());
            }

            GUI.contentColor = Color.grey;
            listingStandard.Gap();
            listingStandard.Label("WS_SettingSortOrder".Translate());
            listingStandard.Label("/");
            listingStandard.Label("/");
            GUI.contentColor = Color.white;
        }

        listingStandard.GapLine();
        listingStandard.CheckboxLabeled("WS_SettingGrenadesCategories".Translate(), ref Settings.GrenadesSeparate,
            "WS_SettingGrenadesCategoriesDescription".Translate());
        if (ModLister.RoyaltyInstalled)
        {
            listingStandard.CheckboxLabeled("WS_SettingBladeLinkCategories".Translate(),
                ref Settings.BladeLinkSeparate, "WS_SettingBladeLinkCategoriesDescription".Translate());
        }

        listingStandard.CheckboxLabeled("WS_SettingRangedCategories".Translate(), ref Settings.RangedSeparate,
            "WS_SettingRangedCategoriesDescription".Translate());
        listingStandard.CheckboxLabeled("WS_SettingMeleeCategories".Translate(), ref Settings.MeleeSeparate,
            "WS_SettingMeleeCategoriesDescription".Translate());
        if (WeaponsSorter.CeAmmoCategoryDef != null)
        {
            listingStandard.CheckboxLabeled("WS_SettingOneHandedCategories".Translate(),
                ref Settings.OneHandedSeparate,
                "WS_SettingOneHandedCategoriesDescription".Translate());
        }

        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("WS_CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();

        Settings.Write();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        WeaponsSorter.SortWeapons();
    }
}
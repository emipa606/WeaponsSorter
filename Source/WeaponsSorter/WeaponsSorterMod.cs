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
    public static WeaponsSorterMod instance;

    private static string currentVersion;

    /// <summary>
    ///     The private settings
    /// </summary>
    private WeaponsSorterSettings settings;

    /// <summary>
    ///     Cunstructor
    /// </summary>
    /// <param name="content"></param>
    public WeaponsSorterMod(ModContentPack content) : base(content)
    {
        instance = this;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.WeaponsSorter"));
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal WeaponsSorterSettings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GetSettings<WeaponsSorterSettings>();
            }

            return settings;
        }
        set => settings = value;
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
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        GUI.contentColor = Color.yellow;
        //listing_Standard.Label("WS_SettingDeselectOptions".Translate());
        GUI.contentColor = Color.white;
        if (!(Settings.SortByTech || Settings.SortByMod || Settings.SortByTag))
        {
            Settings.SortByTech = true;
        }

        var enabledSettings = new List<bool> { Settings.SortByTech, Settings.SortByMod, Settings.SortByTag };
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
                        listing_Standard.CheckboxLabeled("WS_SettingTechCategories".Translate(),
                            ref Settings.SortByTech,
                            "WS_SettingTechCategoriesDescription".Translate());
                        categories[i] = "WS_SettingTech".Translate();
                        break;
                    case 1:
                        listing_Standard.CheckboxLabeled("WS_SettingModCategories".Translate(), ref Settings.SortByMod,
                            "WS_SettingModCategoriesDescription".Translate());
                        categories[i] = "WS_SettingMod".Translate();
                        break;
                    case 2:
                        listing_Standard.CheckboxLabeled("WS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                            "WS_SettingTagCategoriesDescription".Translate());
                        categories[i] = "WS_SettingTag".Translate();
                        break;
                }

                i++;
            }

            listing_Standard.Gap();
            listing_Standard.Label("WS_SettingSortOrder".Translate());
            if (listing_Standard.RadioButton($"{categories[0]} / {categories[1]}",
                    Settings.SortSetting == 0))
            {
                Settings.SortSetting = 0;
            }

            if (listing_Standard.RadioButton($"{categories[1]} / {categories[0]}",
                    Settings.SortSetting == 1))
            {
                Settings.SortSetting = 1;
            }
        }
        else
        {
            listing_Standard.CheckboxLabeled("WS_SettingTechCategories".Translate(), ref Settings.SortByTech,
                "WS_SettingTechCategoriesDescription".Translate());
            listing_Standard.CheckboxLabeled("WS_SettingModCategories".Translate(), ref Settings.SortByMod,
                "WS_SettingModCategoriesDescription".Translate());
            listing_Standard.CheckboxLabeled("WS_SettingTagCategories".Translate(), ref Settings.SortByTag,
                "WS_SettingTagCategoriesDescription".Translate());

            GUI.contentColor = Color.grey;
            listing_Standard.Gap();
            listing_Standard.Label("WS_SettingSortOrder".Translate());
            listing_Standard.Label("/");
            listing_Standard.Label("/");
            GUI.contentColor = Color.white;
        }

        listing_Standard.GapLine();
        listing_Standard.CheckboxLabeled("WS_SettingGrenadesCategories".Translate(), ref Settings.GrenadesSeparate,
            "WS_SettingGrenadesCategoriesDescription".Translate());
        if (ModLister.RoyaltyInstalled)
        {
            listing_Standard.CheckboxLabeled("WS_SettingBladeLinkCategories".Translate(),
                ref Settings.BladeLinkSeparate, "WS_SettingBladeLinkCategoriesDescription".Translate());
        }

        listing_Standard.CheckboxLabeled("WS_SettingRangedCategories".Translate(), ref Settings.RangedSeparate,
            "WS_SettingRangedCategoriesDescription".Translate());
        listing_Standard.CheckboxLabeled("WS_SettingMeleeCategories".Translate(), ref Settings.MeleeSeparate,
            "WS_SettingMeleeCategoriesDescription".Translate());
        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("WS_CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();

        Settings.Write();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        WeaponsSorter.SortWeapons();
    }
}
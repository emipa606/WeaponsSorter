using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WeaponsSorter
{
    [StaticConstructorOnStartup]
    internal class WeaponsSorterMod : Mod
    {
        /// <summary>
        ///     The instance of the settings to be read by the mod
        /// </summary>
        public static WeaponsSorterMod instance;

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
            if (!(Settings.SortByTech || Settings.SortByMod))
            {
                Settings.SortByTech = true;
            }

            if (AtLeastTwo(new List<bool> {Settings.SortByTech, Settings.SortByMod}))
            {
                var categories = new string[2];
                listing_Standard.CheckboxLabeled("WS_SettingTechCategories".Translate(), ref Settings.SortByTech,
                    "WS_SettingTechCategoriesDescription".Translate());
                categories[0] = "WS_SettingTech".Translate();
                listing_Standard.CheckboxLabeled("WS_SettingModCategories".Translate(), ref Settings.SortByMod,
                    "WS_SettingModCategoriesDescription".Translate());
                categories[1] = "WS_SettingMod".Translate();
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
            listing_Standard.End();

            Settings.Write();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            WeaponsSorter.SortWeapons();
        }

        public static bool AtLeastTwo(List<bool> listOfBool)
        {
            switch (listOfBool.Count)
            {
                case 1:
                    listOfBool.Add(false);
                    listOfBool.Add(false);
                    break;
                case 2:
                    listOfBool.Add(false);
                    break;
                case 3:
                    break;
                default:
                    return false;
            }

            return AtLeastTwo(listOfBool[0], listOfBool[1], listOfBool[2]);
        }

        private static bool AtLeastTwo(bool a, bool b, bool c)
        {
            return a && (b || c) || b && c;
        }
    }
}
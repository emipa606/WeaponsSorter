﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WeaponsSorter;

[StaticConstructorOnStartup]
public class WeaponsSorter
{
    private static readonly ThingCategoryDef ceAmmoCategoryDef;
    private static readonly IEnumerable<ThingCategoryDef> categoriesToIgnore = new List<ThingCategoryDef>();
    private static Dictionary<string, List<ThingDef>> WeaponTagDictionary;

    static WeaponsSorter()
    {
        ceAmmoCategoryDef = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("Ammo");
        if (ceAmmoCategoryDef != null)
        {
            categoriesToIgnore = ceAmmoCategoryDef.ThisAndChildCategoryDefs;
            Log.Message($"[WeaponsSorter]: CE is loaded, ignoring {categoriesToIgnore.Count()} thingCategories");
        }

        UpdateTags();
        SortWeapons();
    }

    public static void UpdateTags()
    {
        WeaponTagDictionary = new Dictionary<string, List<ThingDef>>();
        foreach (var weapon in ThingCategoryDefOf.Weapons.DescendantThingDefs)
        {
            if (weapon.weaponTags == null || !weapon.weaponTags.Any())
            {
                continue;
            }

            foreach (var weaponTag in weapon.weaponTags)
            {
                if (!WeaponTagDictionary.ContainsKey(weaponTag))
                {
                    WeaponTagDictionary[weaponTag] = new List<ThingDef> { weapon };
                    continue;
                }

                WeaponTagDictionary[weaponTag].Add(weapon);
            }
        }
    }

    public static void SortWeapons()
    {
        var weaponsInGame = ThingCategoryDefOf.Weapons.DescendantThingDefs
            .Where(def => def.thingCategories.SharesElementWith(categoriesToIgnore) == false)
            .ToHashSet();

        Log.Message($"Weapons Sorter: Updating {weaponsInGame.Count} weapon categories.");

        foreach (var category in ThingCategoryDefOf.Weapons.ThisAndChildCategoryDefs.Where(def =>
                     !categoriesToIgnore.Contains(def)))
        {
            if (category == ceAmmoCategoryDef)
            {
                continue;
            }

            category.childThingDefs.Clear();
            if (category == ThingCategoryDefOf.Weapons && ceAmmoCategoryDef != null)
            {
                category.childCategories = new List<ThingCategoryDef> { ceAmmoCategoryDef };
            }
            else
            {
                category.childCategories.Clear();
            }

            if (category.parent != ThingCategoryDefOf.Root)
            {
                category.parent = null;
            }

            category.ClearCachedData();
        }

        foreach (var category in from categories in DefDatabase<ThingCategoryDef>.AllDefsListForReading
                 where categories.defName.StartsWith("WS_")
                 select categories)
        {
            category.childThingDefs.Clear();
            category.childCategories.Clear();
            if (category.parent != ThingCategoryDefOf.Root)
            {
                category.parent = null;
            }

            category.ClearCachedData();
        }

        // Clean current tags and categories
        foreach (var weapon in weaponsInGame)
        {
            weapon.thingCategories.Clear();
        }

        var allSortOptions = new List<bool>
        {
            WeaponsSorterMod.instance.Settings.SortByTech, WeaponsSorterMod.instance.Settings.SortByMod,
            WeaponsSorterMod.instance.Settings.SortByTag
        };
        if (allSortOptions.Count(b => b.Equals(true)) == 2)
        {
            var firstOption = NextSortOption.None;
            var secondOption = NextSortOption.None;
            for (var j = 0; j < allSortOptions.Count; j++)
            {
                if (!allSortOptions[j])
                {
                    continue;
                }

                firstOption = (NextSortOption)j;
                break;
            }

            for (var j = allSortOptions.Count - 1; j > -1; j--)
            {
                if (!allSortOptions[j])
                {
                    continue;
                }

                secondOption = (NextSortOption)j;
                break;
            }

            if (WeaponsSorterMod.instance.Settings.SortSetting == 1)
            {
                (firstOption, secondOption) = (secondOption, firstOption);
            }

            switch (firstOption)
            {
                case NextSortOption.Tech:
                    SortByTech(weaponsInGame, ThingCategoryDefOf.Weapons, secondOption);
                    break;
                case NextSortOption.Mod:
                    SortByMod(weaponsInGame, ThingCategoryDefOf.Weapons, secondOption);
                    break;
                case NextSortOption.Tag:
                    SortByTag(weaponsInGame, ThingCategoryDefOf.Weapons, secondOption);
                    break;
            }
        }
        else
        {
            if (WeaponsSorterMod.instance.Settings.SortByTech)
            {
                SortByTech(weaponsInGame, ThingCategoryDefOf.Weapons);
            }

            if (WeaponsSorterMod.instance.Settings.SortByMod)
            {
                SortByMod(weaponsInGame, ThingCategoryDefOf.Weapons);
            }

            if (WeaponsSorterMod.instance.Settings.SortByTag)
            {
                SortByTag(weaponsInGame, ThingCategoryDefOf.Weapons);
            }
        }

        ThingCategoryDefOf.Weapons.ResolveReferences();
        Log.Message("Weapons Sorter: Update done.");
    }

    private static void SortByTech(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"Sorting by tech, then by {nextSortOption}");
        foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
        {
            var weaponToCheck =
                (from weaponDef in weaponToSort where weaponDef.techLevel == techLevel select weaponDef)
                .ToHashSet();
            var techLevelDefName = $"{thingCategoryDef.defName}_{techLevel}";
            if (thingCategoryDef == ThingCategoryDefOf.Weapons)
            {
                techLevelDefName = $"WS_{techLevel}";
            }

            var techLevelThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(techLevelDefName);
            if (techLevelThingCategory == null)
            {
                techLevelThingCategory = new ThingCategoryDef
                    { defName = techLevelDefName, label = techLevel.ToStringHuman() };
                DefGenerator.AddImpliedDef(techLevelThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                AddweaponToCategory(weaponToCheck, techLevelThingCategory);
                if (techLevelThingCategory.childThingDefs.Count <= 0 &&
                    techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(techLevelThingCategory);
                techLevelThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Mod:
                        SortByMod(weaponToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Tag:
                        SortByTag(weaponToCheck, techLevelThingCategory);
                        break;
                }

                if (techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(techLevelThingCategory);
                techLevelThingCategory.parent = thingCategoryDef;
            }

            thingCategoryDef.ResolveReferences();
        }
    }

    private static void SortByTag(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"Sorting by tag, then by {nextSortOption}");

        foreach (var tag in WeaponTagDictionary.Keys.OrderBy(s => s))
        {
            if (!weaponToSort.SharesElementWith(WeaponTagDictionary[tag]))
            {
                continue;
            }

            var tagCategoryDefName = $"{thingCategoryDef.defName}_tag_{tag}";
            if (thingCategoryDef == ThingCategoryDefOf.Weapons)
            {
                tagCategoryDefName = $"WS_tag_{tag}";
            }

            var tagThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(tagCategoryDefName);
            if (tagThingCategory == null)
            {
                tagThingCategory = new ThingCategoryDef
                    { defName = tagCategoryDefName, label = tag.CapitalizeFirst() };
                DefGenerator.AddImpliedDef(tagThingCategory);
            }

            var weaponToCheck = weaponToSort.Intersect(WeaponTagDictionary[tag]).ToHashSet();

            if (nextSortOption == NextSortOption.None)
            {
                AddweaponToCategory(weaponToCheck, tagThingCategory);
                if (tagThingCategory.childThingDefs.Count <= 0 &&
                    tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(tagThingCategory);
                tagThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Mod:
                        SortByMod(weaponToCheck, tagThingCategory);
                        break;
                    case NextSortOption.Tech:
                        SortByTech(weaponToCheck, tagThingCategory);
                        break;
                }

                if (tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(tagThingCategory);
                tagThingCategory.parent = thingCategoryDef;
            }

            thingCategoryDef.ResolveReferences();
        }

        foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
        {
            var weaponToCheck =
                (from weaponDef in weaponToSort where weaponDef.techLevel == techLevel select weaponDef)
                .ToHashSet();
            var techLevelDefName = $"{thingCategoryDef.defName}_{techLevel}";
            if (thingCategoryDef == ThingCategoryDefOf.Weapons)
            {
                techLevelDefName = $"WS_{techLevel}";
            }

            var techLevelThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(techLevelDefName);
            if (techLevelThingCategory == null)
            {
                techLevelThingCategory = new ThingCategoryDef
                    { defName = techLevelDefName, label = techLevel.ToStringHuman() };
                DefGenerator.AddImpliedDef(techLevelThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                AddweaponToCategory(weaponToCheck, techLevelThingCategory);
                if (techLevelThingCategory.childThingDefs.Count <= 0 &&
                    techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(techLevelThingCategory);
                techLevelThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Mod:
                        SortByMod(weaponToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Tech:
                        SortByTech(weaponToCheck, techLevelThingCategory);
                        break;
                }

                if (techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(techLevelThingCategory);
                techLevelThingCategory.parent = thingCategoryDef;
            }

            thingCategoryDef.ResolveReferences();
        }
    }

    private static void SortByMod(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"Sorting by mod, then by {nextSortOption}");
        foreach (var modData in from modData in ModLister.AllInstalledMods where modData.Active select modData)
        {
            var weaponToCheck =
                (from weaponDef in weaponToSort
                    where weaponDef.modContentPack is { PackageId: { } } &&
                          weaponDef.modContentPack.PackageId == modData.PackageId
                    select weaponDef).ToHashSet();
            var modDefName = $"{thingCategoryDef.defName}_{modData.PackageId}";
            if (thingCategoryDef == ThingCategoryDefOf.Weapons)
            {
                modDefName = $"WS_{modData.PackageId}";
            }

            var modThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(modDefName);
            if (modThingCategory == null)
            {
                modThingCategory = new ThingCategoryDef { defName = modDefName, label = modData.Name };
                DefGenerator.AddImpliedDef(modThingCategory);
            }

            if (nextSortOption == NextSortOption.None)
            {
                AddweaponToCategory(weaponToCheck, modThingCategory);
                if (modThingCategory.childThingDefs.Count <= 0 && modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(modThingCategory);
                modThingCategory.parent = thingCategoryDef;
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Tech:
                        SortByTech(weaponToCheck, modThingCategory);
                        break;
                    case NextSortOption.Mod:
                        SortByMod(weaponToCheck, modThingCategory);
                        break;
                }

                if (modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }

                thingCategoryDef.childCategories.Add(modThingCategory);
                modThingCategory.parent = thingCategoryDef;
            }

            thingCategoryDef.ResolveReferences();
        }

        var missingweaponToCheck =
            (from weaponDef in weaponToSort
                where weaponDef.weaponTags == null || !weaponDef.weaponTags.Any()
                select weaponDef).ToHashSet();
        if (missingweaponToCheck.Count == 0)
        {
            return;
        }

        var missingTagDefName = $"{thingCategoryDef.defName}_Tag_None";
        if (thingCategoryDef == ThingCategoryDefOf.Weapons)
        {
            missingTagDefName = "WS_Tag_None";
        }

        var missingTagThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(missingTagDefName);
        if (missingTagThingCategory == null)
        {
            missingTagThingCategory = new ThingCategoryDef
                { defName = missingTagDefName, label = "WS_None".Translate() };
            DefGenerator.AddImpliedDef(missingTagThingCategory);
        }

        if (nextSortOption == NextSortOption.None)
        {
            AddweaponToCategory(missingweaponToCheck, missingTagThingCategory);
            if (missingTagThingCategory.childThingDefs.Count <= 0 &&
                missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }

            thingCategoryDef.childCategories.Add(missingTagThingCategory);
            missingTagThingCategory.parent = thingCategoryDef;
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Tech:
                    SortByTech(missingweaponToCheck, missingTagThingCategory);
                    break;
                case NextSortOption.Mod:
                    SortByMod(missingweaponToCheck, missingTagThingCategory);
                    break;
            }

            if (missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }

            thingCategoryDef.childCategories.Add(missingTagThingCategory);
            missingTagThingCategory.parent = thingCategoryDef;
        }
    }


    /// <summary>
    ///     The sorting of weapon into categories
    /// </summary>
    /// <param name="weaponToSort"></param>
    /// <param name="thingCategoryDef"></param>
    private static void AddweaponToCategory(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef)
    {
        var grenadeDefName = $"{thingCategoryDef.defName}_Grenade";
        var grenadeThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(grenadeDefName);
        if (grenadeThingCategory == null)
        {
            grenadeThingCategory = new ThingCategoryDef
                { defName = grenadeDefName, label = "WS_Grenade".Translate() };
            DefGenerator.AddImpliedDef(grenadeThingCategory);
        }

        grenadeThingCategory.childCategories.Clear();
        grenadeThingCategory.childThingDefs.Clear();
        var bladeLinkDefName = $"{thingCategoryDef.defName}_BladeLink";
        var bladeLinkThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(bladeLinkDefName);
        if (bladeLinkThingCategory == null)
        {
            bladeLinkThingCategory = new ThingCategoryDef
                { defName = bladeLinkDefName, label = "WS_BladeLink".Translate() };
            DefGenerator.AddImpliedDef(bladeLinkThingCategory);
        }

        bladeLinkThingCategory.childCategories.Clear();
        bladeLinkThingCategory.childThingDefs.Clear();
        var rangedDefName = $"{thingCategoryDef.defName}_Ranged";
        var rangedThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(rangedDefName);
        if (rangedThingCategory == null)
        {
            rangedThingCategory = new ThingCategoryDef { defName = rangedDefName, label = "WS_Ranged".Translate() };
            DefGenerator.AddImpliedDef(rangedThingCategory);
        }

        rangedThingCategory.childCategories.Clear();
        rangedThingCategory.childThingDefs.Clear();
        var meleeDefName = $"{thingCategoryDef.defName}_Melee";
        var meleeThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(meleeDefName);
        if (meleeThingCategory == null)
        {
            meleeThingCategory = new ThingCategoryDef { defName = meleeDefName, label = "WS_Melee".Translate() };
            DefGenerator.AddImpliedDef(meleeThingCategory);
        }

        meleeThingCategory.childCategories.Clear();
        meleeThingCategory.childThingDefs.Clear();
        thingCategoryDef.childCategories.Clear();
        thingCategoryDef.childThingDefs.Clear();
        foreach (var weapon in weaponToSort)
        {
            if (WeaponsSorterMod.instance.Settings.GrenadesSeparate)
            {
                if (weapon.GetCompProperties<CompProperties_Explosive>() != null && weapon.tools == null)
                {
                    weapon.thingCategories.Add(grenadeThingCategory);
                    grenadeThingCategory.childThingDefs.Add(weapon);
                    continue;
                }
            }

            if (ModLister.RoyaltyInstalled && WeaponsSorterMod.instance.Settings.BladeLinkSeparate)
            {
                if (weapon.weaponTags?.Contains("Bladelink") == true)
                {
                    weapon.thingCategories.Add(bladeLinkThingCategory);
                    bladeLinkThingCategory.childThingDefs.Add(weapon);
                    continue;
                }
            }

            if (WeaponsSorterMod.instance.Settings.RangedSeparate)
            {
                if (weapon.IsRangedWeapon)
                {
                    weapon.thingCategories.Add(rangedThingCategory);
                    rangedThingCategory.childThingDefs.Add(weapon);
                    continue;
                }
            }

            if (WeaponsSorterMod.instance.Settings.MeleeSeparate)
            {
                if (!weapon.IsRangedWeapon)
                {
                    weapon.thingCategories.Add(meleeThingCategory);
                    meleeThingCategory.childThingDefs.Add(weapon);
                    continue;
                }
            }

            weapon.thingCategories.Add(thingCategoryDef);
            thingCategoryDef.childThingDefs.Add(weapon);
        }

        if (WeaponsSorterMod.instance.Settings.GrenadesSeparate && grenadeThingCategory.childThingDefs.Count > 0)
        {
            grenadeThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(grenadeThingCategory);
            grenadeThingCategory.ResolveReferences();
        }

        if (ModLister.RoyaltyInstalled && WeaponsSorterMod.instance.Settings.BladeLinkSeparate &&
            bladeLinkThingCategory.childThingDefs.Count > 0)
        {
            bladeLinkThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(bladeLinkThingCategory);
            bladeLinkThingCategory.ResolveReferences();
        }

        if (WeaponsSorterMod.instance.Settings.RangedSeparate && rangedThingCategory.childThingDefs.Count > 0)
        {
            rangedThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(rangedThingCategory);
            rangedThingCategory.ResolveReferences();
        }

        if (!WeaponsSorterMod.instance.Settings.MeleeSeparate || meleeThingCategory.childThingDefs.Count <= 0)
        {
            thingCategoryDef.ResolveReferences();
            return;
        }

        meleeThingCategory.parent = thingCategoryDef;
        thingCategoryDef.childCategories.Add(meleeThingCategory);
        meleeThingCategory.ResolveReferences();
        thingCategoryDef.ResolveReferences();
    }

    private enum NextSortOption
    {
        Tech = 0,
        Mod = 1,
        Tag = 2,
        None = 3
    }
}
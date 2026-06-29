using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace WeaponsSorter;

[StaticConstructorOnStartup]
public class WeaponsSorter
{
    public static readonly ThingCategoryDef CeAmmoCategoryDef;
    private static readonly IEnumerable<ThingCategoryDef> categoriesToIgnore = new List<ThingCategoryDef>();
    private static Dictionary<string, List<ThingDef>> weaponTagDictionary;

    static WeaponsSorter()
    {
        if (WeaponsSorterMod.CeLoaded)
        {
            CeAmmoCategoryDef = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("Ammo");
            if (CeAmmoCategoryDef != null)
            {
                categoriesToIgnore = CeAmmoCategoryDef.ThisAndChildCategoryDefs;
                Log.Message($"[WeaponsSorter]: CE is loaded, ignoring {categoriesToIgnore.Count()} thingCategories");
            }
        }
        else
        {
            WeaponsSorterMod.Instance.Settings.SortByAmmo = false;
            if (!WeaponsSorterMod.Instance.Settings.SortByMod && !WeaponsSorterMod.Instance.Settings.SortByTag &&
                !WeaponsSorterMod.Instance.Settings.SortByTech)
            {
                WeaponsSorterMod.Instance.Settings.SortByTech = true;
            }
        }

        updateTags();
        SortWeapons();
    }

    private static void updateTags()
    {
        weaponTagDictionary = new Dictionary<string, List<ThingDef>>();
        foreach (var weapon in ThingCategoryDefOf.Weapons.DescendantThingDefs)
        {
            if (weapon.weaponTags == null || !weapon.weaponTags.Any())
            {
                continue;
            }

            foreach (var weaponTag in weapon.weaponTags)
            {
                if (!weaponTagDictionary.ContainsKey(weaponTag))
                {
                    weaponTagDictionary[weaponTag] = [weapon];
                    continue;
                }

                weaponTagDictionary[weaponTag].Add(weapon);
            }
        }
    }

    public static void SortWeapons()
    {
        var weaponsInGame = ThingCategoryDefOf.Weapons.DescendantThingDefs
            .Where(def => !def.thingCategories.SharesElementWith(categoriesToIgnore))
            .ToHashSet();

        Log.Message($"Weapons Sorter: Updating {weaponsInGame.Count} weapon categories.");

        foreach (var category in ThingCategoryDefOf.Weapons.ThisAndChildCategoryDefs.Where(def =>
                     !categoriesToIgnore.Contains(def)))
        {
            if (category == CeAmmoCategoryDef)
            {
                continue;
            }

            category.childThingDefs.Clear();
            if (category == ThingCategoryDefOf.Weapons && CeAmmoCategoryDef != null)
            {
                category.childCategories = [CeAmmoCategoryDef];
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

        var enabledSortOptions = new List<NextSortOption>();
        if (WeaponsSorterMod.Instance.Settings.SortByTech)
        {
            enabledSortOptions.Add(NextSortOption.Tech);
        }

        if (WeaponsSorterMod.Instance.Settings.SortByMod)
        {
            enabledSortOptions.Add(NextSortOption.Mod);
        }

        if (WeaponsSorterMod.Instance.Settings.SortByTag)
        {
            enabledSortOptions.Add(NextSortOption.Tag);
        }

        if (WeaponsSorterMod.Instance.Settings.SortByAmmo)
        {
            enabledSortOptions.Add(NextSortOption.Ammo);
        }

        if (enabledSortOptions.Count == 2)
        {
            var firstOption = enabledSortOptions[0];
            var secondOption = enabledSortOptions[1];

            if (WeaponsSorterMod.Instance.Settings.SortSetting == 1)
            {
                (firstOption, secondOption) = (secondOption, firstOption);
            }

            switch (firstOption)
            {
                case NextSortOption.Tech:
                    sortByTech(weaponsInGame, ThingCategoryDefOf.Weapons, secondOption);
                    break;
                case NextSortOption.Mod:
                    sortByMod(weaponsInGame, ThingCategoryDefOf.Weapons, secondOption);
                    break;
                case NextSortOption.Tag:
                    sortByTag(weaponsInGame, ThingCategoryDefOf.Weapons, secondOption);
                    break;
                case NextSortOption.Ammo:
                    sortByAmmo(weaponsInGame, ThingCategoryDefOf.Weapons, secondOption);
                    break;
            }
        }
        else
        {
            if (WeaponsSorterMod.Instance.Settings.SortByTech)
            {
                sortByTech(weaponsInGame, ThingCategoryDefOf.Weapons);
            }

            if (WeaponsSorterMod.Instance.Settings.SortByMod)
            {
                sortByMod(weaponsInGame, ThingCategoryDefOf.Weapons);
            }

            if (WeaponsSorterMod.Instance.Settings.SortByTag)
            {
                sortByTag(weaponsInGame, ThingCategoryDefOf.Weapons);
            }

            if (WeaponsSorterMod.Instance.Settings.SortByAmmo)
            {
                sortByAmmo(weaponsInGame, ThingCategoryDefOf.Weapons);
            }
        }

        ThingCategoryDefOf.Weapons.ResolveReferences();
        Log.Message("[Weapons Sorter]: Update done.");
    }

    private static void sortByTech(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"[Weapons Sorter]: Sorting by tech, then by {nextSortOption}");
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
                addWeaponToCategory(weaponToCheck, techLevelThingCategory);
                if (techLevelThingCategory.childThingDefs.Count <= 0 &&
                    techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Mod:
                        sortByMod(weaponToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Tag:
                        sortByTag(weaponToCheck, techLevelThingCategory);
                        break;
                    case NextSortOption.Ammo:
                        sortByAmmo(weaponToCheck, techLevelThingCategory);
                        break;
                }

                if (techLevelThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(techLevelThingCategory);
            techLevelThingCategory.parent = thingCategoryDef;

            thingCategoryDef.ResolveReferences();
        }
    }

    private static void sortByTag(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"[Weapons Sorter]: Sorting by tag, then by {nextSortOption}");

        foreach (var tag in weaponTagDictionary.Keys.OrderBy(s => s))
        {
            if (!weaponToSort.SharesElementWith(weaponTagDictionary[tag]))
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

            var weaponToCheck = weaponToSort.Intersect(weaponTagDictionary[tag]).ToHashSet();

            if (nextSortOption == NextSortOption.None)
            {
                addWeaponToCategory(weaponToCheck, tagThingCategory);
                if (tagThingCategory.childThingDefs.Count <= 0 &&
                    tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Mod:
                        sortByMod(weaponToCheck, tagThingCategory);
                        break;
                    case NextSortOption.Tech:
                        sortByTech(weaponToCheck, tagThingCategory);
                        break;
                    case NextSortOption.Ammo:
                        sortByAmmo(weaponToCheck, tagThingCategory);
                        break;
                }

                if (tagThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(tagThingCategory);
            tagThingCategory.parent = thingCategoryDef;

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
            addWeaponToCategory(missingweaponToCheck, missingTagThingCategory);
            if (missingTagThingCategory.childThingDefs.Count <= 0 &&
                missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Tech:
                    sortByTech(missingweaponToCheck, missingTagThingCategory);
                    break;
                case NextSortOption.Mod:
                    sortByMod(missingweaponToCheck, missingTagThingCategory);
                    break;
                case NextSortOption.Ammo:
                    sortByAmmo(missingweaponToCheck, missingTagThingCategory);
                    break;
            }

            if (missingTagThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }

        thingCategoryDef.childCategories.Add(missingTagThingCategory);
        missingTagThingCategory.parent = thingCategoryDef;

        thingCategoryDef.ResolveReferences();
    }

    private static void sortByMod(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"[Weapons Sorter]: Sorting by mod, then by {nextSortOption}");
        foreach (var modData in from modData in ModLister.AllInstalledMods where modData.Active select modData)
        {
            var weaponToCheck =
                (from weaponDef in weaponToSort
                    where weaponDef.modContentPack is { PackageId: not null } &&
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
                addWeaponToCategory(weaponToCheck, modThingCategory);
                if (modThingCategory.childThingDefs.Count <= 0 && modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Tech:
                        sortByTech(weaponToCheck, modThingCategory);
                        break;
                    case NextSortOption.Tag:
                        sortByTag(weaponToCheck, modThingCategory);
                        break;
                    case NextSortOption.Ammo:
                        sortByAmmo(weaponToCheck, modThingCategory);
                        break;
                }

                if (modThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(modThingCategory);
            modThingCategory.parent = thingCategoryDef;

            thingCategoryDef.ResolveReferences();
        }

        var missingweaponToCheck =
            (from weaponDef in weaponToSort
                where weaponDef.modContentPack?.PackageId == null
                select weaponDef).ToHashSet();
        if (missingweaponToCheck.Count == 0)
        {
            return;
        }

        var missingModDefName = $"{thingCategoryDef.defName}_Mod_None";
        if (thingCategoryDef == ThingCategoryDefOf.Weapons)
        {
            missingModDefName = "WS_Mod_None";
        }

        var missingModThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(missingModDefName);
        if (missingModThingCategory == null)
        {
            missingModThingCategory = new ThingCategoryDef
                { defName = missingModDefName, label = "WS_None".Translate() };
            DefGenerator.AddImpliedDef(missingModThingCategory);
        }

        if (nextSortOption == NextSortOption.None)
        {
            addWeaponToCategory(missingweaponToCheck, missingModThingCategory);
            if (missingModThingCategory.childThingDefs.Count <= 0 &&
                missingModThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Tech:
                    sortByTech(missingweaponToCheck, missingModThingCategory);
                    break;
                case NextSortOption.Tag:
                    sortByTag(missingweaponToCheck, missingModThingCategory);
                    break;
                case NextSortOption.Ammo:
                    sortByAmmo(missingweaponToCheck, missingModThingCategory);
                    break;
            }

            if (missingModThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }

        thingCategoryDef.childCategories.Add(missingModThingCategory);
        missingModThingCategory.parent = thingCategoryDef;

        thingCategoryDef.ResolveReferences();
    }

    private static void sortByAmmo(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef,
        NextSortOption nextSortOption = NextSortOption.None)
    {
        Log.Message($"[Weapons Sorter]: Sorting by ammo, then by {nextSortOption}");
        if (!WeaponsSorterMod.CeLoaded)
        {
            return;
        }

        var ammoSetDictionary = new Dictionary<string, HashSet<ThingDef>>();
        var ammoSetLabels = new Dictionary<string, string>();

        foreach (var weapon in weaponToSort)
        {
            if (!tryGetAmmoSetData(weapon, out var ammoSetDefName, out var ammoSetLabel))
            {
                continue;
            }

            if (!ammoSetDictionary.TryGetValue(ammoSetDefName, out var weaponsWithAmmoSet))
            {
                weaponsWithAmmoSet = [];
                ammoSetDictionary[ammoSetDefName] = weaponsWithAmmoSet;
                ammoSetLabels[ammoSetDefName] = ammoSetLabel;
            }

            weaponsWithAmmoSet.Add(weapon);
        }

        foreach (var ammoSetDefName in ammoSetDictionary.Keys.OrderBy(key => ammoSetLabels[key]))
        {
            var ammoCategoryDefName = $"{thingCategoryDef.defName}_Ammo_{ammoSetDefName}";
            if (thingCategoryDef == ThingCategoryDefOf.Weapons)
            {
                ammoCategoryDefName = $"WS_Ammo_{ammoSetDefName}";
            }

            var ammoThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(ammoCategoryDefName);
            if (ammoThingCategory == null)
            {
                ammoThingCategory = new ThingCategoryDef
                    { defName = ammoCategoryDefName, label = ammoSetLabels[ammoSetDefName].CapitalizeFirst() };
                DefGenerator.AddImpliedDef(ammoThingCategory);
            }

            var weaponsWithAmmoSet = ammoSetDictionary[ammoSetDefName];

            if (nextSortOption == NextSortOption.None)
            {
                addWeaponToCategory(weaponsWithAmmoSet, ammoThingCategory);
                if (ammoThingCategory.childThingDefs.Count <= 0 && ammoThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }
            else
            {
                switch (nextSortOption)
                {
                    case NextSortOption.Tech:
                        sortByTech(weaponsWithAmmoSet, ammoThingCategory);
                        break;
                    case NextSortOption.Mod:
                        sortByMod(weaponsWithAmmoSet, ammoThingCategory);
                        break;
                    case NextSortOption.Tag:
                        sortByTag(weaponsWithAmmoSet, ammoThingCategory);
                        break;
                }

                if (ammoThingCategory.childCategories.Count <= 0)
                {
                    continue;
                }
            }

            thingCategoryDef.childCategories.Add(ammoThingCategory);
            ammoThingCategory.parent = thingCategoryDef;

            thingCategoryDef.ResolveReferences();
        }

        var missingweaponToCheck = weaponToSort.Where(weapon => !tryGetAmmoSetData(weapon, out _, out _)).ToHashSet();
        if (missingweaponToCheck.Count == 0)
        {
            return;
        }

        var missingAmmoDefName = $"{thingCategoryDef.defName}_Ammo_None";
        if (thingCategoryDef == ThingCategoryDefOf.Weapons)
        {
            missingAmmoDefName = "WS_Ammo_None";
        }

        var missingAmmoThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(missingAmmoDefName);
        if (missingAmmoThingCategory == null)
        {
            missingAmmoThingCategory = new ThingCategoryDef
                { defName = missingAmmoDefName, label = "WS_None".Translate() };
            DefGenerator.AddImpliedDef(missingAmmoThingCategory);
        }

        if (nextSortOption == NextSortOption.None)
        {
            addWeaponToCategory(missingweaponToCheck, missingAmmoThingCategory);
            if (missingAmmoThingCategory.childThingDefs.Count <= 0 &&
                missingAmmoThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }
        else
        {
            switch (nextSortOption)
            {
                case NextSortOption.Tech:
                    sortByTech(missingweaponToCheck, missingAmmoThingCategory);
                    break;
                case NextSortOption.Mod:
                    sortByMod(missingweaponToCheck, missingAmmoThingCategory);
                    break;
                case NextSortOption.Tag:
                    sortByTag(missingweaponToCheck, missingAmmoThingCategory);
                    break;
            }

            if (missingAmmoThingCategory.childCategories.Count <= 0)
            {
                return;
            }
        }

        thingCategoryDef.childCategories.Add(missingAmmoThingCategory);
        missingAmmoThingCategory.parent = thingCategoryDef;

        thingCategoryDef.ResolveReferences();

        return;

        static bool tryGetAmmoSetData(ThingDef weapon, out string ammoSetDefName, out string ammoSetLabel)
        {
            ammoSetDefName = null;
            ammoSetLabel = null;

            if (weapon.comps == null)
            {
                return false;
            }

            foreach (var compProperties in weapon.comps)
            {
                var compPropertiesType = compProperties.GetType();
                if (compPropertiesType.FullName != "CombatExtended.CompProperties_AmmoUser")
                {
                    continue;
                }

                var ammoSetField = compPropertiesType.GetField("ammoSet",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var ammoSetObject = ammoSetField?.GetValue(compProperties);
                if (ammoSetObject == null)
                {
                    var ammoSetProperty = compPropertiesType.GetProperty("ammoSet",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    ammoSetObject = ammoSetProperty?.GetValue(compProperties, null);
                }

                if (ammoSetObject is Def ammoSetDef && !ammoSetDef.defName.NullOrEmpty())
                {
                    ammoSetDefName = ammoSetDef.defName;
                    ammoSetLabel = ammoSetDef.label.NullOrEmpty() ? ammoSetDef.defName : ammoSetDef.label;
                    return true;
                }

                if (ammoSetObject == null)
                {
                    return false;
                }

                var ammoSetText = ammoSetObject.ToString();
                if (ammoSetText.NullOrEmpty())
                {
                    return false;
                }

                ammoSetDefName = ammoSetText;
                ammoSetLabel = ammoSetText;
                return true;
            }

            return false;
        }
    }


    /// <summary>
    ///     The sorting of weapon into categories
    /// </summary>
    /// <param name="weaponToSort"></param>
    /// <param name="thingCategoryDef"></param>
    private static void addWeaponToCategory(HashSet<ThingDef> weaponToSort, ThingCategoryDef thingCategoryDef)
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


        var oneHandedDefName = $"{thingCategoryDef.defName}_OneHanded";
        var oneHandedThingCategory = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(oneHandedDefName);
        if (oneHandedThingCategory == null)
        {
            oneHandedThingCategory = new ThingCategoryDef
                { defName = oneHandedDefName, label = "WS_OneHanded".Translate() };
            DefGenerator.AddImpliedDef(oneHandedThingCategory);
        }

        oneHandedThingCategory.childCategories.Clear();
        oneHandedThingCategory.childThingDefs.Clear();

        thingCategoryDef.childCategories.Clear();
        thingCategoryDef.childThingDefs.Clear();
        foreach (var weapon in weaponToSort)
        {
            var somethingChosen = false;
            var isGrenade = false;

            if (WeaponsSorterMod.Instance.Settings.GrenadesSeparate)
            {
                if (weapon.GetCompProperties<CompProperties_Explosive>() != null && weapon.tools == null ||
                    weapon.weaponTags?.Any(tag => tag.ToLower().Contains("grenade")) == true)
                {
                    weapon.thingCategories.Add(grenadeThingCategory);
                    grenadeThingCategory.childThingDefs.Add(weapon);
                    somethingChosen = true;
                    isGrenade = true;
                }
            }

            if (ModLister.RoyaltyInstalled && WeaponsSorterMod.Instance.Settings.BladeLinkSeparate)
            {
                if (weapon.weaponTags?.Contains("Bladelink") == true)
                {
                    weapon.thingCategories.Add(bladeLinkThingCategory);
                    bladeLinkThingCategory.childThingDefs.Add(weapon);
                    somethingChosen = true;
                }
            }

            if (WeaponsSorterMod.Instance.Settings.RangedSeparate)
            {
                if (weapon.IsRangedWeapon && !isGrenade)
                {
                    weapon.thingCategories.Add(rangedThingCategory);
                    rangedThingCategory.childThingDefs.Add(weapon);
                    somethingChosen = true;
                }
            }

            if (WeaponsSorterMod.Instance.Settings.MeleeSeparate)
            {
                if (!weapon.IsRangedWeapon)
                {
                    weapon.thingCategories.Add(meleeThingCategory);
                    meleeThingCategory.childThingDefs.Add(weapon);
                    somethingChosen = true;
                }
            }

            if (WeaponsSorterMod.Instance.Settings.OneHandedSeparate)
            {
                if (weapon.weaponTags?.Contains("CE_OneHandedWeapon") == true)
                {
                    weapon.thingCategories.Add(oneHandedThingCategory);
                    oneHandedThingCategory.childThingDefs.Add(weapon);
                    somethingChosen = true;
                }
            }

            if (somethingChosen)
            {
                continue;
            }

            weapon.thingCategories.Add(thingCategoryDef);
            thingCategoryDef.childThingDefs.Add(weapon);
        }

        if (WeaponsSorterMod.Instance.Settings.GrenadesSeparate && grenadeThingCategory.childThingDefs.Count > 0)
        {
            grenadeThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(grenadeThingCategory);
            grenadeThingCategory.ResolveReferences();
        }

        if (ModLister.RoyaltyInstalled && WeaponsSorterMod.Instance.Settings.BladeLinkSeparate &&
            bladeLinkThingCategory.childThingDefs.Count > 0)
        {
            bladeLinkThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(bladeLinkThingCategory);
            bladeLinkThingCategory.ResolveReferences();
        }

        if (WeaponsSorterMod.Instance.Settings.RangedSeparate && rangedThingCategory.childThingDefs.Count > 0)
        {
            rangedThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(rangedThingCategory);
            rangedThingCategory.ResolveReferences();
        }

        if (WeaponsSorterMod.Instance.Settings.MeleeSeparate && meleeThingCategory.childThingDefs.Count > 0)
        {
            meleeThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(meleeThingCategory);
            meleeThingCategory.ResolveReferences();
        }

        if (WeaponsSorterMod.Instance.Settings.OneHandedSeparate && oneHandedThingCategory.childThingDefs.Count > 0)
        {
            oneHandedThingCategory.parent = thingCategoryDef;
            thingCategoryDef.childCategories.Add(oneHandedThingCategory);
            oneHandedThingCategory.ResolveReferences();
        }

        thingCategoryDef.ResolveReferences();
    }

    private enum NextSortOption
    {
        Tech = 0,
        Mod = 1,
        Tag = 2,
        None = 3,
        Ammo
    }
}
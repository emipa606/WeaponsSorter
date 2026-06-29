# GitHub Copilot Instructions for RimWorld Modding - Weapons Sorter

## Mod Overview and Purpose
**Weapons Sorter** is a user-friendly mod designed to help players efficiently manage and organize their weapons in RimWorld. The mod introduces enhanced sorting capabilities, allowing items to be categorized based on various properties. This mod isn’t limited to vanilla content; it seamlessly integrates with all weapons, including those added by other mods, ensuring a consistent and customizable experience for players.

## Key Features and Systems
- **Dynamic Sorting Options:**
  - **Tech Level Sorting**: Categorize weapons based on their technological advancement levels, from neolithic to spacer tech.
  - **Mod Name Sorting**: Group weapons according to the mod that introduced them, aiding in organization for mod-heavy games.
  - **Weapon Tag Sorting**: Classify weapons using specific tags, providing flexibility in how categories are defined.
  - **Ammo Set Sorting (Combat Extended)**: Group CE-compatible weapons by their configured `ammoSet` when Combat Extended is active.
  - **Subcategories for Weapon Types**: Further granularity is offered with subcategories for:
    - **Grenades**
    - **Ranged Weapons**
    - **Melee Weapons**

- **Compatibility and Stability**:
  - Compatible with both modded and vanilla weapons.
  - Confirms functionality alongside Combat Extended (CE).
  - Safe to be added or removed from save files without causing issues.

- **Multilingual Support**: Available in multiple languages including Russian, Chinese, French, and Korean, ensuring wide accessibility.

## Coding Patterns and Conventions
- **C# Structure**:
  - Follow object-oriented principles.
  - Maintain clear and descriptive naming for methods and variables to improve readability and maintainability.
  - Use PascalCase for class and method names (e.g., `WeaponsSorterMod`, `DoSettingsWindowContents`).
  - Adopt camelCase for local variables and parameters (e.g., `addWeaponToCategory`, `sortByMod`).

- **File Organization**:
  - Group related classes and types within the same namespace and file where appropriate.
  - Separate files logically (e.g., settings management in `WeaponsSorterSettings.cs`).

## XML Integration
- **ThingCategoryDefs**:
  - Utilize the `ThingCategoryDef` in XML files for defining custom weapon categories.
  - Ensure XML structure is consistent and follows RimWorld's XML schema standards for easy integration and error-free functionality.

## Harmony Patching
- Consider using Harmony for non-destructive patching to maintain compatibility with other mods.
- Implement postfix and prefix Harmony patches to safely expand or modify game behaviors without altering original game code.

## Suggestions for Copilot
- **Enhance functionality with automated suggestions**:
  - Automate the creation of custom categories based on player preferences using snippets.
  - Generate pattern-based sorting logic, leveraging keywords like `sort`, `move`, and `group`.

- **Encourage code reusability**:
  - Suggest reusable methods and patterns for sorting functionalities across different parts of the mod.

- **Facilitate multilingual support**:
  - Enable automatic translation entries based on existing pattern recognition within language files.

- **Promote efficient XML handling**:
  - Recommend XML snippets that adhere to RimWorld’s modding standards, especially for handling `ThingCategoryDefs`.

By adhering to these structured guidelines and leveraging the power of GitHub Copilot, mod developers can streamline their development process, improve code quality and consistency, and enhance user experience within RimWorld modding projects.


This file encapsulates the objectives, technical details, and coding norms related to the Weapons Sorter mod, offering clear guidance for any developer involved in the project, utilizing GitHub Copilot as a supportive tool.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).


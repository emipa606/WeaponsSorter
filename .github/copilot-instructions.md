# Copilot Instructions for RimWorld Mod: Weapons Sorter

## Mod Overview and Purpose

The Weapons Sorter mod for RimWorld is designed to enhance the game's inventory management by allowing players to efficiently sort, categorize, and manage weapons within their colonies. This mod introduces a systematic approach to organizing weapons based on specific criteria, helping players streamline their gameplay and ensure their colonies are always combat-ready.

## Key Features and Systems

- **Weapon Categorization**: Automatically categorizes weapons based on damage, type, and range, providing a clear overview of the colony's arsenal.
- **Custom Sorting Options**: Players can define sorting preferences to prioritize certain weapons over others.
- **Integration with Vanilla Weapon System**: Seamlessly integrates with RimWorld's existing weapon mechanics without altering base game functionality.
- **User-Friendly UI**: Offers an intuitive interface within the mod settings for user customization.

## Coding Patterns and Conventions

- **Class Design**: The project follows a standardized internal class structure for mod components, with `WeaponsSorterMod` and `WeaponsSorterSettings` as key classes.
- **Code Style**: Variable names use camelCase and class names use PascalCase. Methods are concise and adhere to the single responsibility principle.
- **Comments**: Important sections of the code are well-commented to explain the logic and flow, assisting other developers and contributors in understanding the code base.

## XML Integration

- **XML Configuration**: The mod uses XML files to define weapon categories and sorting rules. Ensure XML files are correctly formatted and placed in the appropriate directory.
- **Error Handling**: Given the "Error parsing XML" note, verify to make sure that all XML configurations are valid to avoid any runtime errors.

## Harmony Patching

- **Method Patching**: Utilizes Harmony library to apply patches selectively on methods related to inventory and weapon handling without altering core game files.
- **Prefix and Postfix Methods**: Methods are patched using both prefix and postfix to ensure custom logic is applied before and after the original functionality.

## Suggestions for Copilot

- **Automate XML Error Checking**: Implement automated XML validation checks to prevent errors during game loading.
- **Enhanced Sorting Algorithms**: Suggest enhancements in sorting algorithms to optimize for both performance and usability.
- **User Interface Improvements**: Explore UI suggestions that Copilot might provide for a more intuitive settings menu and real-time updates.
- **Expand Compatibility**: Leverage Copilot to identify potential compatibility issues with other popular mods and suggest code solutions.

By following these instructions and suggestions, contributors using GitHub Copilot for this project can maintain a consistent codebase, integrate seamlessly with RimWorld's systems, and propose creative solutions using AI-powered insights.

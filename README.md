# DFU-ModSettingsChecker
The `ModSettingsChecker.cs` script allows you to easily check other mod's settings and show an error in the start menu, if the mod is installed and one or more setting values interfer with your own mod.

# How to use
## Installation
Simply copy the [ModSettingsChecker.cs](https://github.com/AncientGrief/DFU-ModSettingsChecker/blob/main/ModSettingsChecker.cs) file to your project/mod.

## CSV
You can add a comma separated `.csv` file to your mod (don't forget to add it to the .dfmod with the Mod Builder). Mod Settings Checker can then read it's contents and automatically show it's results.

Example code, add this to your `Awake()`-method of your mod:
```csharp
ModSettingsChecker.CheckFromCsv(mod, "SettingsToCheck.csv"); //mod is the instance of your own mod
```
*You can see a complete mod example here: [Complete Mod Example](https://github.com/AncientGrief/DFU-ModSettingsChecker/blob/main/ExampleMod/ExampleMod.cs)*


That's it; you only need the correct format for the comma-separated .csv file, which looks like this:
`ModGuid,Section,Key,Type,ExpectedValue,ErrorMessage`

- **`ModGuid`**: The GUID or Name of the mod you want to check (if you use a name, it must be the name of the .dfmod file (case sensitive), e.g: `tome of battle` or `DaggerBlood`
- **`Section`**: The Settings section, which is the blue name in the mod settings windows of DFU
- **`Key`**: The Settings Key (or name), which is the yellow name, next to the actual widgets like checkboses, sliders and so on.
- **`Type`**: The type of the setting and check to perform (see below for a complete list).
- **`ExpectedValue`**: The value a setting must have to show the error message.
- **`ErrorMessage`**: The message to show to the user, if the condition is met.

  Here is an example CSV:
```
ModGuid, Section, Key, Type, ExpectedValue, ErrorMessage
parallaxdungeondoors, Settings, BiggerDoors, bool,false, My Mod needs big doors enabled!!!
parallaxdungeondoors, Settings, BiggerDoorScale, int.less, 40, Doors must be at least 40% bigger!
```

You can also edit `.csv` files with Excel or you use an online tool like [Online CSV Editor and Viewer](https://www.convertcsv.com/csv-viewer-editor.htm). Simply paste the example on their page and edit as you see fit.

### Possible conditions:
- **`exists`**: Checks if the target mod exists. GUID or name as ExpectedValue. This can be used to double-check missing mods. Example: `modname, null, null, exists, 0, Please also install mod 'modname'!`
- **`bool`**: Checks if the boolean value matches `true` or `false`.
- **`int`**: Validates that the integer value is exactly equal to the expected value.
- **`int.less`**: Validates that the integer value is less than the expected value.
- **`int.greater`**: Validates that the integer value is greater than the expected value.
- **`int.tuple.first.equal`**: Checks if the first value in an integer tuple equals the expected value.
- **`int.tuple.second.equal`**: Checks if the second value in an integer tuple equals the expected value.
- **`int.tuple.first.less`**: Checks if the first value in an integer tuple is less than the expected value.
- **`int.tuple.second.less`**: Checks if the second value in an integer tuple is less than the expected value.
- **`int.tuple.first.greater`**: Checks if the first value in an integer tuple is greater than the expected value.
- **`int.tuple.second.greater`**: Checks if the second value in an integer tuple is greater than the expected value.
- **`float`**: Validates that the float value is exactly equal to the expected value.
- **`float.less`**: Validates that the float value is less than the expected value.
- **`float.greater`**: Validates that the float value is greater than the expected value.
- **`float.tuple.first.equal`**: Checks if the first value in a float tuple equals the expected value.
- **`float.tuple.second.equal`**: Checks if the second value in a float tuple equals the expected value.
- **`float.tuple.first.less`**: Checks if the first value in a float tuple is less than the expected value.
- **`float.tuple.second.less`**: Checks if the second value in a float tuple is less than the expected value.
- **`float.tuple.first.greater`**: Checks if the first value in a float tuple is greater than the expected value.
- **`float.tuple.second.greater`**: Checks if the second value in a float tuple is greater than the expected value.
- **`choice.equal`**: Checks if the selected choice index matches the expected value.
- **`choice.not.equal`**: Checks if the selected choice index does *not* match the expected value.
- **`string.equal`**: Checks if the string value matches the expected string exactly.
- **`string.not.equal`**: Checks if the string value does *not* match the expected string.
- **`string.lower`**: Compares the lowercase version of the string with the expected lowercase value.
- **`string.lower.not`**: Ensures the lowercase version of the string does *not* match the expected value.
- **`string.contains`**: Checks if the string contains the expected substring.
- **`string.contains.not`**: Checks if the string does *not* contain the expected substring.
- **`string.contains.lower`**: Checks if the lowercase string contains the expected lowercase substring.
- **`string.contains.lower.not`**: Checks if the lowercase string does *not* contain the expected lowercase substring.

## Code
You can also use C# code to check for your mods. Here is an example you can put in your `Awake()`-method:
```csharp
ModSettingsChecker modChecker = ModSettingsChecker.Create().Init(mod.Title);

//Check Parallax Dungeon Doors by GUID
modChecker.Check("5fc82cd7-6b86-4060-a777-b597f900a6b9", settings =>
{
    settings
        .Toggle("Settings", "BiggerDoors", false, "My Mod needs big doors enabled!!!")
        .SliderIntLess("Settings", "BiggerDoorScale", 40, "Doors must be at least 40% bigger!");
});

//Check another mod by name
modChecker.Check("tome of battle", settings =>
{
    settings.xxx
});
```

## How to find the settings of a mod?
There are two ways to figure out the correct Section and Key values of a mod:

### 1st option: Extracting the mod settings with the Mod Manager
You can Extract the mod files and go to the folder (DFU data folder, it's shown to you in the Startup Screen, you can double-click it to open it).

On Windows it's here:
`C:\Users\USERNAME\AppData\LocalLow\Daggerfall Workshop\Daggerfall Unity\Mods\ExtractedFiles`

Then navigate to `/Mods/ExtractedFiles/MODNAME` and open the `modsettings.json`.
Here you'll find all possible settings.

### 2nd option: Use a piece of debug code
Mod Settings Checker has a static helper method to print all settings of a mod to the Unity debug console:
Simply call:

`ModSettingsChecker.ShowAllModSettings("5fc82cd7-6b86-4060-a777-b597f900a6b9");`

or

`ModSettingsChecker.ShowAllModSettings("tome of battle");`

Start DFU and go to the Start screen, then filter your Unity log to `[Mod Settings Checker]`.
You should see all settings in the log now :)

Example output:
```
[Mod Settings Checker] Settings for mod Tome of Battle:
[Mod Settings Checker] Section = 'Controls', Key = 'AttackInput', Type = 'Text' | Value = 'Mouse1'.
[Mod Settings Checker] Section = 'Controls', Key = 'MeleeMode', Type = 'MultipleChoice' | Value = '2'.
[Mod Settings Checker] Section = 'Controls', Key = 'VanillaMeleeViewDampenStrength', Type = 'SliderFloat' | Value = '0.5'.
[Mod Settings Checker] Section = 'HitDetection', Key = 'RadialDistance', Type = 'Toggle' | Value = 'True'.
[Mod Settings Checker] Section = 'HitDetection', Key = 'RequireLineOfSight', Type = 'Toggle' | Value = 'True'.
...
```

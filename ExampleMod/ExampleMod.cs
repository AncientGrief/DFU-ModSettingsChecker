using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using Game.Mods.ModSettingsChecker;
using UnityEngine;

public class ExampleMod : MonoBehaviour
{
    private static Mod mod;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        _ = new GameObject(mod.Title, typeof(ExampleMod));
    }

    private void Awake()
    {
        ModSettingsChecker modChecker = ModSettingsChecker.Create().Init(mod.Title);

        ModSettingsChecker.ShowAllModSettings("tome of battle");

        var settingsToCheck = mod.GetAsset<TextAsset>("SettingsToCheck.csv");
        if(settingsToCheck)
            modChecker.CheckFromCsv(settingsToCheck);
        else
            Debug.LogError("SettingsToCheck.csv not found in mod assets!");

        //Check Parallax Dungeon Doors
        /*ModSettingsChecker.Check("5fc82cd7-6b86-4060-a777-b597f900a6b9", settings =>
        {
            settings
                .Toggle("Settings", "BiggerDoors", false, "My Mod needs big doors enabled!!!")
                .SliderIntLess("Settings", "BiggerDoorScale", 40, "Doors must be at least 40% bigger!");
        });*/
    }
}

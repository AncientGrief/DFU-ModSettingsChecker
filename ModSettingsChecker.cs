using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine;
// ReSharper disable UnusedMethodReturnValue.Global

namespace Game.Mods.ModSettingsChecker
{
    public class ModSettingsChecker
    {
        private static class ParseHelper
        {
            public static bool TryInt(string s, out int value, string type, string section, string key)
            {
                if (int.TryParse(s, out value))
                    return true;

                Debug.LogWarning($"[ModSettingsChecker] Failed to parse int '{s}' for '{section}/{key}' (type: {type}).");
                return false;
            }

            public static bool TryFloat(string s, out float value, string type, string section, string key)
            {
                if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    return true;

                Debug.LogWarning(
                    $"[Mod Settings Checker] Failed to parse float '{s}' for '{section}/{key}' (type: {type}).");
                return false;
            }
        }

        private string _modName;
        private readonly Dictionary<string, Mod> _cachedModsByGuid = new Dictionary<string, Mod>();
        private readonly Dictionary<string, Mod> _cachedModsByName = new Dictionary<string, Mod>();
        private readonly Dictionary<string, string> _existErrorMessages = new Dictionary<string, string>();
        private readonly List<string> _errorMessages = new List<string>();

        private static FieldInfo fieldInfo;

        public static ModSettingsChecker Create()
        {
            return new ModSettingsChecker();
        }

        public ModSettingsChecker Init(string nameOfMod)
        {
            _modName = nameOfMod;
            _cachedModsByGuid.Clear();
            _cachedModsByName.Clear();
            _errorMessages.Clear();
            _existErrorMessages.Clear();

            DaggerfallStartWindow.OnStartFirstVisible += DaggerfallStartWindow_OnStartFirstVisible;

            return this;
        }

        private void DaggerfallStartWindow_OnStartFirstVisible()
        {
            foreach (var lines in _errorMessages.Select(errors => WrapText(errors)))
            {
                lines.Insert(0, $"Warning from {_modName} mod:");
                ShowMessageBox(lines.ToArray());
            }

            foreach (var lines in _existErrorMessages.Select(errors => WrapText(errors.Value)))
            {
                lines.Insert(0, $"Warning from {_modName} mod:");
                ShowMessageBox(lines.ToArray());
            }

            DaggerfallStartWindow.OnStartFirstVisible -= DaggerfallStartWindow_OnStartFirstVisible;
        }

        public void CheckFromCsv(TextAsset csvAsset)
        {
            string[] lines = csvAsset.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines.Skip(1)) // Skip header
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                string[] fields = line.Split(',');

                if (fields.Length < 6)
                {
                    continue;
                }

                string modGuid = fields[0].Trim();
                string section = fields[1].Trim();
                string key = fields[2].Trim();
                string type = fields[3].Trim().ToLower();
                string expectedValue = fields[4].Trim();
                string errorMessage = fields[5].Trim();

                bool modExists = Check(modGuid, settings =>
                {
                    switch (type)
                    {
                        case "bool":
                            settings.Toggle(section, key, expectedValue.ToLower() == "true", errorMessage);
                            break;

                        case "int":
                            if (ParseHelper.TryInt(expectedValue, out int intVal1, type, section, key))
                                settings.SliderIntEqual(section, key, intVal1, errorMessage);
                            break;

                        case "int.less":
                            if (ParseHelper.TryInt(expectedValue, out int intVal2, type, section, key))
                                settings.SliderIntLess(section, key, intVal2, errorMessage);
                            break;

                        case "int.greater":
                            if (ParseHelper.TryInt(expectedValue, out int intVal3, type, section, key))
                                settings.SliderIntGreater(section, key, intVal3, errorMessage);
                            break;

                        case "int.tuple.first.equal":
                            if (ParseHelper.TryInt(expectedValue, out int intVal4, type, section, key))
                                settings.TupleIntFirstEqual(section, key, intVal4, errorMessage);
                            break;

                        case "int.tuple.second.equal":
                            if (ParseHelper.TryInt(expectedValue, out int intVal5, type, section, key))
                                settings.TupleIntSecondEqual(section, key, intVal5, errorMessage);
                            break;

                        case "int.tuple.first.less":
                            if (ParseHelper.TryInt(expectedValue, out int intVal6, type, section, key))
                                settings.TupleIntFirstLess(section, key, intVal6, errorMessage);
                            break;

                        case "int.tuple.second.less":
                            if (ParseHelper.TryInt(expectedValue, out int intVal7, type, section, key))
                                settings.TupleIntSecondLess(section, key, intVal7, errorMessage);
                            break;

                        case "int.tuple.first.greater":
                            if (ParseHelper.TryInt(expectedValue, out int intVal8, type, section, key))
                                settings.TupleIntFirstGreater(section, key, intVal8, errorMessage);
                            break;

                        case "int.tuple.second.greater":
                            if (ParseHelper.TryInt(expectedValue, out int intVal9, type, section, key))
                                settings.TupleIntSecondGreater(section, key, intVal9, errorMessage);
                            break;

                        case "float":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal1, type, section, key))
                                settings.SliderFloatEqual(section, key, floatVal1, errorMessage);
                            break;

                        case "float.less":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal2, type, section, key))
                                settings.SliderFloatLess(section, key, floatVal2, errorMessage);
                            break;

                        case "float.greater":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal3, type, section, key))
                                settings.SliderFloatGreater(section, key, floatVal3, errorMessage);
                            break;

                        case "float.tuple.first.equal":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal4, type, section, key))
                                settings.TupleFloatFirstEqual(section, key, floatVal4, errorMessage);
                            break;

                        case "float.tuple.second.equal":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal5, type, section, key))
                                settings.TupleFloatSecondEqual(section, key, floatVal5, errorMessage);
                            break;

                        case "float.tuple.first.less":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal6, type, section, key))
                                settings.TupleFloatFirstLess(section, key, floatVal6, errorMessage);
                            break;

                        case "float.tuple.second.less":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal7, type, section, key))
                                settings.TupleFloatSecondLess(section, key, floatVal7, errorMessage);
                            break;

                        case "float.tuple.first.greater":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal8, type, section, key))
                                settings.TupleFloatFirstGreater(section, key, floatVal8, errorMessage);
                            break;

                        case "float.tuple.second.greater":
                            if (ParseHelper.TryFloat(expectedValue, out float floatVal9, type, section, key))
                                settings.TupleFloatSecondGreater(section, key, floatVal9, errorMessage);
                            break;

                        case "choice.equal":
                            if (ParseHelper.TryInt(expectedValue, out int intVal10, type, section, key))
                                settings.MultipleChoiceEqual(section, key, intVal10, errorMessage);
                            break;

                        case "choice.not.equal":
                            if (ParseHelper.TryInt(expectedValue, out int intVal11, type, section, key))
                                settings.MultipleChoiceNotEqual(section, key, intVal11, errorMessage);
                            break;

                        case "string.equal":
                            settings.TextEqual(section, key, expectedValue, errorMessage);
                            break;

                        case "string.not.equal":
                            settings.TextNotEqual(section, key, expectedValue, errorMessage);
                            break;

                        case "string.lower":
                            settings.TextEqualToLower(section, key, expectedValue.ToLower(), errorMessage);
                            break;

                        case "string.lower.not":
                            settings.TextNotEqualToLower(section, key, expectedValue.ToLower(), errorMessage);
                            break;

                        case "string.contains":
                            settings.TextContains(section, key, expectedValue, errorMessage);
                            break;

                        case "string.contains.not":
                            settings.TextNotContains(section, key, expectedValue, errorMessage);
                            break;

                        case "string.contains.lower":
                            settings.TextContainsToLower(section, key, expectedValue.ToLower(), errorMessage);
                            break;

                        case "string.contains.lower.not":
                            settings.TextNotContainsToLower(section, key, expectedValue.ToLower(), errorMessage);
                            break;
                        case "exists":
                            //Ignore it, it is checked differently
                            break;

                        default:
                            Debug.LogWarning(
                                $"[Mod Settings Checker] Unknown setting type '{type}' in CSV for setting '{section}/{key}'.");
                            break;
                    }
                });

                if (!modExists && !_existErrorMessages.ContainsKey(modGuid) && type == "exists")
                    _existErrorMessages.Add(modGuid,errorMessage);
            }
        }

        public bool Check(Mod mod, Action<ModSettingsRequirement> action)
        {
            if(!_cachedModsByGuid.ContainsKey(mod.GUID))
                _cachedModsByGuid.Add(mod.GUID, mod);

            return CheckInternal(mod, action);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool Check(string modGuidOrName, Action<ModSettingsRequirement> action)
        {
            Mod mod = TryGetMod(modGuidOrName);

            return mod != null && CheckInternal(mod, action);
        }

        private bool CheckInternal(Mod mod, Action<ModSettingsRequirement> action)
        {
            var req = new ModSettingsRequirement(this, mod.GetSettings());

            action(req);
            return true;
        }

        private Mod TryGetMod(string modGuidOrName)
        {
            if (!_cachedModsByGuid.TryGetValue(modGuidOrName, out Mod mod))
            {
                _cachedModsByName.TryGetValue(modGuidOrName, out mod);
            }

            if (mod != null)
                return mod;

            mod = ModManager.Instance.GetModFromGUID(modGuidOrName);

            if (mod != null)
            {
                _cachedModsByGuid.Add(modGuidOrName, mod);
                return mod;
            }

            mod = ModManager.Instance.GetModFromName(modGuidOrName);

            if (mod != null)
            {
                _cachedModsByName.Add(modGuidOrName, mod);
                return mod;
            }

            return null;
        }

        public void AddUiErrorMessage(string message)
        {
            _errorMessages.Add(message);
        }

        private static List<string> WrapText(string input, int maxLineLength = 50)
        {
            var lines = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return lines;

            string[] rawLines = input.Split('\n');

            foreach (string rawLine in rawLines)
            {
                string[] words = rawLine.Trim().Split(' ');
                var currentLine = new StringBuilder();

                foreach (string word in words)
                {
                    if (currentLine.Length + word.Length + 1 > maxLineLength)
                    {
                        lines.Add(currentLine.ToString().TrimEnd());
                        currentLine.Clear();
                    }

                    currentLine.Append(word + " ");
                }

                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine.ToString().TrimEnd());
                }
            }

            return lines;
        }

        private static void ShowMessageBox(string[] lines)
        {
            var messageBox = new DaggerfallMessageBox(DaggerfallUI.UIManager);
            messageBox.SetText(lines);
            messageBox.ParentPanel.BackgroundColor = Color.clear;
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Accept);

            messageBox.OnButtonClick += (sender, button) => { messageBox.CloseWindow(); };

            messageBox.Show();
        }

        [Obsolete("This method is intended for debugging purposes only. Remove in production code.")]
        public static void ShowAllModSettings(string modGuidOrName)
        {
            Mod mod = ModManager.Instance.GetModFromGUID(modGuidOrName);
            if (mod == null)
            {
                mod = ModManager.Instance.GetModFromName(modGuidOrName);
                if (mod == null)
                    return;
            }

            ModSettings modSettings = mod.GetSettings();

            if (fieldInfo == null)
                fieldInfo = typeof(ModSettings).GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);

            var data = (ModSettingsData)fieldInfo?.GetValue(modSettings);

            if (data != null)
            {
                Debug.Log($"[Mod Settings Checker] Settings for mod {mod.Title}:");
                foreach (Section s in data.Sections)
                {
                    foreach (Key k in s.Keys)
                    {
                        string value = null;
                        switch (k.KeyType)
                        {
                            case KeyType.Color:
                                value = modSettings.GetColor(s.Name, k.Name).ToString();
                                break;
                            case KeyType.MultipleChoice:
                                value = modSettings.GetValue<int>(s.Name, k.Name).ToString();
                                break;
                            case KeyType.SliderFloat:
                                value = modSettings.GetValue<float>(s.Name, k.Name)
                                    .ToString(CultureInfo.InvariantCulture);
                                break;
                            case KeyType.SliderInt:
                                value = modSettings.GetValue<int>(s.Name, k.Name).ToString();
                                break;
                            case KeyType.Text:
                                value = modSettings.GetValue<string>(s.Name, k.Name);
                                break;
                            case KeyType.Toggle:
                                value = modSettings.GetValue<bool>(s.Name, k.Name).ToString();
                                break;
                            case KeyType.TupleFloat:
                                value = modSettings.GetTupleFloat(s.Name, k.Name).ToString();
                                break;
                            case KeyType.TupleInt:
                                value = modSettings.GetTupleInt(s.Name, k.Name).ToString();
                                break;
                            default:
                                Debug.LogWarning($"[Mod Settings Checker] Unknown key type '{k.KeyType}' for '{s.Name}/{k.Name}'.");
                                break;
                        }

                        Debug.Log($"[Mod Settings Checker] Section = '{s.Name}', Key = '{k.Name}', Type = '{k.KeyType}' | Value = '{value}'.");
                    }
                }
            }
            else
            {
                Debug.LogError($"[Mod Settings Checker] Failed to get ModSettingsData for mod {mod.Title}.");
            }
        }
    }

    public class ModSettingsRequirement
    {
        private readonly ModSettings _settings;
        private readonly ModSettingsChecker _instance;

        public ModSettingsRequirement(ModSettingsChecker instance, ModSettings settings)
        {
            _settings = settings;
            _instance = instance;
        }

        public ModSettingsRequirement Toggle(string section, string key, bool expected, string errorMessage)
        {
            if (_settings.GetBool(section, key) == expected)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement SliderIntLess(string section, string key, int maxValue, string errorMessage)
        {
            if (_settings.GetInt(section, key) < maxValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement SliderIntGreater(string section, string key, int minValue, string errorMessage)
        {
            if (_settings.GetInt(section, key) > minValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement SliderIntEqual(string section, string key, int expectedValue, string errorMessage)
        {
            if (_settings.GetInt(section, key) == expectedValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement SliderFloatLess(string section, string key, float maxValue, string errorMessage)
        {
            if (_settings.GetFloat(section, key) < maxValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement SliderFloatGreater(string section, string key, float minValue,
            string errorMessage)
        {
            if (_settings.GetFloat(section, key) > minValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement SliderFloatEqual(string section, string key, float expectedValue,
            string errorMessage)
        {
            if (Mathf.Approximately(_settings.GetFloat(section, key), expectedValue))
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleIntFirstEqual(string section, string key, int expectedValue,
            string errorMessage)
        {
            if (_settings.GetTupleInt(section, key).First == expectedValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleIntSecondEqual(string section, string key, int expectedValue,
            string errorMessage)
        {
            if (_settings.GetTupleInt(section, key).Second == expectedValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleIntFirstLess(string section, string key, int maxValue, string errorMessage)
        {
            if (_settings.GetTupleInt(section, key).First < maxValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleIntSecondLess(string section, string key, int maxValue, string errorMessage)
        {
            if (_settings.GetTupleInt(section, key).Second < maxValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleIntFirstGreater(string section, string key, int minValue,
            string errorMessage)
        {
            if (_settings.GetTupleInt(section, key).First > minValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleIntSecondGreater(string section, string key, int minValue,
            string errorMessage)
        {
            if (_settings.GetTupleInt(section, key).Second > minValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleFloatFirstEqual(string section, string key, float expectedValue,
            string errorMessage)
        {
            if (Mathf.Approximately(_settings.GetTupleFloat(section, key).First, expectedValue))
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleFloatSecondEqual(string section, string key, float expectedValue,
            string errorMessage)
        {
            if (Mathf.Approximately(_settings.GetTupleFloat(section, key).Second, expectedValue))
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleFloatFirstLess(string section, string key, float maxValue,
            string errorMessage)
        {
            if (_settings.GetTupleFloat(section, key).First < maxValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleFloatSecondLess(string section, string key, float maxValue,
            string errorMessage)
        {
            if (_settings.GetTupleFloat(section, key).Second < maxValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleFloatFirstGreater(string section, string key, float minValue,
            string errorMessage)
        {
            if (_settings.GetTupleFloat(section, key).First > minValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TupleFloatSecondGreater(string section, string key, float minValue,
            string errorMessage)
        {
            if (_settings.GetTupleFloat(section, key).Second > minValue)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement MultipleChoiceEqual(string section, string key, int expectedChoice,
            string errorMessage)
        {
            if (_settings.GetValue<int>(section, key) == expectedChoice)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement MultipleChoiceNotEqual(string section, string key, int notExpectedChoice,
            string errorMessage)
        {
            if (_settings.GetValue<int>(section, key) != notExpectedChoice)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextEqual(string section, string key, string text, string errorMessage)
        {
            if (_settings.GetString(section, key) == text)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextNotEqual(string section, string key, string text, string errorMessage)
        {
            if (_settings.GetString(section, key) != text)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextEqualToLower(string section, string key, string text, string errorMessage)
        {
            if (_settings.GetString(section, key)?.ToLower() == text)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextNotEqualToLower(string section, string key, string text, string errorMessage)
        {
            if (_settings.GetString(section, key)?.ToLower() != text)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextContains(string section, string key, string text, string errorMessage)
        {
            if (_settings.GetString(section, key)?.Contains(text) == true)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextNotContains(string section, string key, string text, string errorMessage)
        {
            if (_settings.GetString(section, key)?.ToLower().Contains(text) == false)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextContainsToLower(string section, string key, string text, string errorMessage)
        {
            if (_settings.GetString(section, key)?.ToLower().Contains(text) == true)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement TextNotContainsToLower(string section, string key, string text,
            string errorMessage)
        {
            if (_settings.GetString(section, key)?.ToLower().Contains(text) == false)
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }

        public ModSettingsRequirement Custom(Func<ModSettings, bool> check, string errorMessage)
        {
            if (check(_settings))
            {
                _instance.AddUiErrorMessage(errorMessage);
            }

            return this;
        }
    }
}
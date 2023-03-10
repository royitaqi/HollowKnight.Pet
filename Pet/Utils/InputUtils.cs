using System;
using System.Collections.Generic;
using InControl;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Pet.Utils
{
    internal static class InputUtils
    {
        internal static void Load()
        {
            if (InputHandler == null)
            {
                InputHandler = typeof(HeroController).GetField("inputHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(HeroController.instance) as InputHandler;
                if (InputHandler == null)
                {
                    return;
                }
            }

            // Get all the input actions in HeroActions. Create a map from action instance to string name. The hook will print the name of the actions being queried.
            if (Hooks.Count == 0)
            {
                var oneAxisFieldNames = GetFieldNamesGeneric<OneAxisInputControl>();
                var twoAxisFieldNames = GetFieldNamesGeneric<TwoAxisInputControl>();
                HookPropertyGeneric<OneAxisInputControl, bool>(oneAxisFieldNames, ApplyControllerOverride);
                HookPropertyGeneric<OneAxisInputControl, int>(oneAxisFieldNames, ApplyControllerOverride);
                HookPropertyGeneric<OneAxisInputControl, float>(oneAxisFieldNames, ApplyControllerOverride);
                HookPropertyGeneric<TwoAxisInputControl, bool>(twoAxisFieldNames, ApplyControllerOverride);
                HookPropertyGeneric<TwoAxisInputControl, int>(twoAxisFieldNames, ApplyControllerOverride);
                HookPropertyGeneric<TwoAxisInputControl, float>(twoAxisFieldNames, ApplyControllerOverride);
                //HookPropertyGeneric<TwoAxisInputControl, Vector2>(twoAxisFieldNames, ApplyControllerOverride);
            }
        }

        internal static void Unload()
        {
            InputHandler = null;

            foreach (var hook in Hooks)
            {
                hook.Dispose();
            }
            Hooks.Clear();

            ControllerBoolOverrides.Clear();
            ControllerIntOverrides.Clear();
            ControllerFloatOverrides.Clear();
        }

        #region Controller Overrides
        private static Dictionary<object, string> GetFieldNamesGeneric<AxisInputControl>()
        {
            var fieldNames = new Dictionary<object, string>();
            foreach (var field in typeof(HeroActions).GetFields())
            {
                if (field.FieldType.IsSubclassOf(typeof(AxisInputControl)))
                {
                    fieldNames[field.GetValue(InputHandler.inputActions)] = field.Name;
                    typeof(InputUtils).LogModTEMP($"Found {typeof(AxisInputControl).Name.Substring(0, 7)} field: {field.Name}");
                }
            }
            return fieldNames;
        }

        private static void HookPropertyGeneric<AxisInputControl, T>(Dictionary<object, string> fieldNames, Func<string, T, T> applyOverride) where AxisInputControl : class
        {
            foreach (var prop in typeof(AxisInputControl).GetProperties())
            {
                if (prop.PropertyType == typeof(T))
                {
                    var getter = typeof(AxisInputControl).GetMethod($"get_{prop.Name}");
                    ModAssert.AllBuilds(getter != null, $"getter for {prop.Name} should be nonnull");
                    var genericHook = (Func<AxisInputControl, T> orig, AxisInputControl self) =>
                    {
                        // original value
                        var ret = orig(self);

                        // if not one of the interesting actions, return original value
                        if (!fieldNames.ContainsKey(self))
                        {
                            return ret;
                        }

                        // override value
                        var key = $"{fieldNames[self]}.{prop.Name}";
                        ret = applyOverride(key, ret);
                        //typeof(InputUtils).LogModTEMP($"~ {typeof(AxisInputControl).Name.Substring(0, 7)} ~ {key}: {ret}");
                        return ret;
                    };
                    Hooks.Add(new Hook(getter, genericHook));
                    typeof(InputUtils).LogModTEMP($"Hooked {typeof(AxisInputControl).Name.Substring(0, 7)} ~ {prop.Name}");
                }
            }
        }

        internal static void PressDirection(string key)
        {
            Load();
            typeof(InputUtils).LogMod($"Pressing {key}");
            ControllerFloatOverrides.Add(key + ".Value", 1f);
        }
        internal static void ReleaseDirection(string key)
        {
            Load();
            typeof(InputUtils).LogMod($"Releasing {key}");
            ControllerFloatOverrides.Remove(key + ".Value");
        }
        internal static void PressButton(string key)
        {
            Load();
            typeof(InputUtils).LogMod($"Pressing {key}");
            ControllerBoolOverrides.Add(key + ".WasPressed", true);
        }
        internal static void ReleaseButton(string key)
        {
            Load();
            typeof(InputUtils).LogMod($"Releasing {key}");
            ControllerBoolOverrides.Remove(key + ".WasPressed");
        }
        private static bool ApplyControllerOverride(string key, bool dft)
        {
            if (ControllerBoolOverrides.ContainsKey(key))
            {
                return ControllerBoolOverrides[key];
            }
            return dft;
        }
        private static int ApplyControllerOverride(string key, int dft)
        {
            if (ControllerIntOverrides.ContainsKey(key))
            {
                return ControllerIntOverrides[key];
            }
            return dft;
        }
        private static float ApplyControllerOverride(string key, float dft)
        {
            if (ControllerFloatOverrides.ContainsKey(key))
            {
                typeof(InputUtils).LogModTEMP($"Overriding {key}: {ControllerFloatOverrides[key]}");
                return ControllerFloatOverrides[key];
            }
            return dft;
        }
        private static InputHandler InputHandler;
        private static readonly List<Hook> Hooks = new();
        private static readonly Dictionary<string, bool> ControllerBoolOverrides = new();
        private static readonly Dictionary<string, int> ControllerIntOverrides = new();
        private static readonly Dictionary<string, float> ControllerFloatOverrides = new();
        #endregion
    }

    internal static class KeyboardOverride
    {
        internal static void Load()
        {
        }

        internal static void Unload()
        {
            KeyboardOverrides.Clear();
        }

        internal static void PressKey(KeyCode key, int count = 1)
        {
            typeof(InputUtils).LogMod($"Pressing {key} ({count} times)");
            KeyboardOverrides.Add(key, count);
        }

        internal static void ReleaseKey(KeyCode key)
        {
            if (KeyboardOverrides.ContainsKey(key))
            {
                typeof(InputUtils).LogMod($"Releasing {key}");
                KeyboardOverrides.Remove(key);
            }
        }

        internal static bool GetKeyDown(KeyCode key)
        {
            if (KeyboardOverrides.ContainsKey(key))
            {
                if (--KeyboardOverrides[key] == 0)
                {
                    typeof(InputUtils).LogMod($"Auto-releasing {key}");
                    KeyboardOverrides.Remove(key);
                }
                return true;
            }
            return Input.GetKeyDown(key);
        }
        private static readonly Dictionary<KeyCode, int> KeyboardOverrides = new();
    }
}

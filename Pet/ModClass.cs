using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Pet.Utils;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using Pet.Modules;

namespace Pet
{
    public sealed partial class Pet : Mod, ITogglableMod, IGlobalSettings<GlobalData>
    {
        internal static Pet Instance;
        internal GlobalData GlobalData { get; set; } = new GlobalData();

        ///
        /// Mod
        ///
#if (DEBUG)
        public override string GetVersion() => version.Value + "-DEBUG";
#else
        public override string GetVersion() => version.Value;
#endif
        private static readonly Lazy<string> version = new(() =>
        {
            Assembly asm = typeof(Pet).Assembly;
            string ver = asm.GetName().Version.ToString();
            using var sha = SHA256.Create();
            using FileStream stream = File.OpenRead(asm.Location);
            byte[] hashBytes = sha.ComputeHash(stream);
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            return $"{ver}-{hash.Substring(0, 6)}";
        });

        public override List<ValueTuple<string, string>> GetPreloadNames()
        {
            return new List<ValueTuple<string, string>>
            {
                //("Deepnest_East_07","Hollow Shade"),
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log($"Initializing mod {GetVersion()}");

            if (preloadedObjects != null)
            {
                this.LogModDebug("Preloaded objects:");
                foreach (var scene in preloadedObjects.Keys)
                {
                    this.LogModDebug($"    - {scene}");
                    foreach (var name in preloadedObjects[scene].Keys)
                    {
                        this.LogModDebug($"        - {name}");
                    }
                }
            }

            // logger
            Instance = this;
            LoggingUtils.LoggingFunction = this.Log;
            LoggingUtils.LogLevel = LogLevel.Debug;
            LoggingUtils.FilterFunction = LoggingUtils.DontRepeatWithin1s;

            // display
            ModDisplay.Instance = new ModDisplay();

            // input overrides
            KeyboardOverride.Load();

            // objects
            _shade = new();
            _lantern = new();
            _kingsoul = new();

            // hooks
            GameUtils.Load();
            GameUtils.GameLoaded += _shade.Load;
            GameUtils.GameLoaded += _lantern.Load;
            GameUtils.GameLoaded += _kingsoul.Load;
            GameUtils.OnGameQuit += _shade.Unload;
            GameUtils.OnGameQuit += _lantern.Unload;
            GameUtils.OnGameQuit += _kingsoul.Load;
            GameUtils.CatchUpEvents();

#if (DEBUG)
            // debugger
            Debugger.Instance = new Debugger();
            Debugger.Instance.Load();
#endif

            Log("Initialized mod");
        }

        ///
        /// ITogglableMod
        ///
        public void Unload()
        {
#if (DEBUG)
            // debugger
            if (Debugger.Instance != null)
            {
                Debugger.Instance.Unload();
                Debugger.Instance = null;
            }
#endif

            // hooks
            GameUtils.Unload();
            GameUtils.GameLoaded -= _shade.Load;
            GameUtils.GameLoaded -= _lantern.Load;
            GameUtils.GameLoaded -= _kingsoul.Load;
            GameUtils.OnGameQuit -= _shade.Unload;
            GameUtils.OnGameQuit -= _lantern.Unload;
            GameUtils.OnGameQuit -= _kingsoul.Load;

            // objects
            _shade.Unload();
            _shade = null;
            _lantern.Unload();
            _lantern = null;
            _kingsoul.Unload();
            _kingsoul = null;

            // input overrides
            KeyboardOverride.Unload();

            // display
            if (ModDisplay.Instance != null)
            {
                ModDisplay.Instance.Destroy();
                ModDisplay.Instance = null;
            }
        }

        private Shade _shade;
        private Lantern _lantern;
        private Kingsoul _kingsoul;

        ///
        /// IGlobalSettings<GlobalData>
        ///
        public void OnLoadGlobal(GlobalData data) => GlobalData = data;
        public GlobalData OnSaveGlobal() => GlobalData;
    }
}

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

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log($"Initializing mod {GetVersion()}");

            // logger
            Instance = this;
            LoggingUtils.LoggingFunction = this.Log;
            LoggingUtils.LogLevel = LogLevel.Fine;
            LoggingUtils.FilterFunction = LoggingUtils.DontRepeatWithin1s;

            // display
            ModDisplay.Instance = new ModDisplay();

            // hooks

            // input overrides
            KeyboardOverride.Load();

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

            // input overrides
            KeyboardOverride.Unload();

            // hooks

            // display
            if (ModDisplay.Instance != null)
            {
                ModDisplay.Instance.Destroy();
                ModDisplay.Instance = null;
            }

            // objects
        }

        ///
        /// IGlobalSettings<GlobalData>
        ///
        public void OnLoadGlobal(GlobalData data) => GlobalData = data;
        public GlobalData OnSaveGlobal() => GlobalData;
    }
}

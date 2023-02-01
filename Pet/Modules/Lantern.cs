using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Pet.Utils;
using SFCore.Utils;
using static HutongGames.PlayMaker.FsmEventTarget;

namespace Pet.Modules;

internal class Lantern
{
    internal bool IsLoaded { get; set; }

    internal void Load()
    {
        this.LogMod("Load()");
        IsLoaded = true;

        // set up as if hero has lantern
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
    }

    internal void Unload()
    {
        if (!IsLoaded) return;

        this.LogMod("Unload()");
        IsLoaded = false;

        // set up as if hero has lantern
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == "hasLantern")
        {
            return IsLoaded;
        }
        return orig;
    }
}

using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Pet.Utils;
using SFCore.Utils;
using UnityEngine;
using static HutongGames.PlayMaker.FsmEventTarget;

namespace Pet.Modules;

internal class Lantern
{
    internal bool IsLoaded { get; set; }
    const string PdHasLanternForGame = "hasLantern";
    const string PdHasLanternForInventory = "Pet hasLantern For Inventory";

    internal void Load()
    {
        this.LogMod("Load()");
        IsLoaded = true;

        // set up as if hero has lantern
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModInventoryFsm(true);
    }

    internal void Unload()
    {
        if (!IsLoaded) return;

        this.LogMod("Unload()");
        IsLoaded = false;

        // recover
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        ModInventoryFsm(false);
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == PdHasLanternForGame)
        {
            return orig || IsLoaded;
        }
        if (name == PdHasLanternForInventory)
        {
            return PlayerData.instance.hasLantern;
        }
        return orig;
    }

    private void ModInventoryFsm(bool hook)
    {
        this.LogModDebug("Modding inventory FSM");
        var fsm = GoUtils.GetFsmFromResources("Equipment", "Build Equipment List");
        var state = fsm.GetState("Lantern");
        var actionIndex = state.FindActionIndexByType(typeof(PlayerDataBoolTest));
        var action = state.Actions[actionIndex] as PlayerDataBoolTest;
        action.boolName = hook ? PdHasLanternForInventory : PdHasLanternForGame;
    }
}

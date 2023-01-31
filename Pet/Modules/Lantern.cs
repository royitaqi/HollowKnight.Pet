using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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
        var fsm = VignetteFsm;
        fsm.ChangeTransition("Dark Lev Check", "DARK 1", "Lantern");
        fsm.ChangeTransition("Dark Lev Check", "DARK 2", "Lantern");
        fsm.ChangeTransition("Scene Reset", "DARK 1", "Lantern 2");
        fsm.ChangeTransition("Scene Reset", "DARK 2", "Lantern 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 1", "Lantern 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 2", "Lantern 2");

        GameUtils.OnFsmStart += GameUtils_OnFsmStart;
    }

    private void GameUtils_OnFsmStart(PlayMakerFSM fsm)
    {
        if (IsLoaded && fsm.FsmName == "Disable if No Lantern")
        {
            this.LogMod($"Unlocking toll gate machine in dark room");
            fsm.GetState("Check").Insert(new FsmUtils.InsertParam
            {
                Action = new SendEvent
                {
                    eventTarget = Self,
                    sendEvent = FsmEvent.Finished,
                },
                Named = "Pet Unlock Toll Gate Machine",
                Before = typeof(PlayerDataBoolTest),
            });
        }
    }

    internal void Unload()
    {
        this.LogMod("Unload()");
        IsLoaded = false;

        // recover setup
        var fsm = VignetteFsm;
        fsm.ChangeTransition("Dark Lev Check", "DARK 1", "Dark 1");
        fsm.ChangeTransition("Dark Lev Check", "DARK 2", "Dark 2");
        fsm.ChangeTransition("Scene Reset", "DARK 1", "Dark 1 2");
        fsm.ChangeTransition("Scene Reset", "DARK 2", "Dark 2 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 1", "Dark 1 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 2", "Dark 2 2");

        GameUtils.OnFsmStart -= GameUtils_OnFsmStart;
    }

    private PlayMakerFSM VignetteFsm => HeroController.instance.gameObject.Find("Vignette").GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Darkness Control");
}

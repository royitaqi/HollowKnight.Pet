using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Pet.Utils;
using SFCore.Utils;
using static HutongGames.PlayMaker.FsmEventTarget;

namespace Pet.Modules;

internal class Kingsoul
{
    internal bool IsLoaded { get; set; }

    internal void Load()
    {
        this.LogMod("Load()");
        IsLoaded = true;

        // set up as if hero has lantern
        KingsoulCharmFsm.GetState("Check").Insert(new FsmUtils.InsertParam
        {
            Action = new SendEvent
            {
                eventTarget = Self,
                sendEvent = FsmEvent.GetFsmEvent("ACTIVE"),
                delay = 0f,
            },
            Named = "Pet Has Kingsoul",
            Before = typeof(PlayerDataBoolTest),
        });
        KingsoulCharmFsm.SetState("Check");
    }

    internal void Unload()
    {
        if (!IsLoaded) return;

        this.LogMod("Unload()");
        IsLoaded = false;

        // recover setup
        KingsoulCharmFsm.GetState("Check").RemoveActionByName("Pet Has Kingsoul");
        KingsoulCharmFsm.SetState("Check");
    }

    private PlayMakerFSM KingsoulCharmFsm => HeroController.instance.gameObject.Find("Charm Effects").GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "White Charm");
}

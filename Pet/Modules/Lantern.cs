using Pet.Utils;
using Satchel;

namespace Pet.Modules;

internal class Lantern
{
    internal bool IsLoaded { get; set; }

    internal void Load()
    {
        this.LogMod("Load()");
        IsLoaded = true;

        // set up as if hero has lantern
        var fsm = LocateVignetteFsm();
        fsm.ChangeTransition("Dark Lev Check", "DARK 1", "Lantern");
        fsm.ChangeTransition("Dark Lev Check", "DARK 2", "Lantern");
        fsm.ChangeTransition("Scene Reset", "DARK 1", "Lantern 2");
        fsm.ChangeTransition("Scene Reset", "DARK 2", "Lantern 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 1", "Lantern 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 2", "Lantern 2");
    }

    internal void Unload()
    {
        this.LogMod("Unload()");
        IsLoaded = false;

        // recover setup
        var fsm = LocateVignetteFsm();
        fsm.ChangeTransition("Dark Lev Check", "DARK 1", "Dark 1");
        fsm.ChangeTransition("Dark Lev Check", "DARK 2", "Dark 2");
        fsm.ChangeTransition("Scene Reset", "DARK 1", "Dark 1 2");
        fsm.ChangeTransition("Scene Reset", "DARK 2", "Dark 2 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 1", "Dark 1 2");
        fsm.ChangeTransition("Scene Reset 2", "DARK 2", "Dark 2 2");
    }

    private PlayMakerFSM LocateVignetteFsm()
    {
        return HeroController.instance.gameObject.Find("Vignette").GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Darkness Control");
    }
}

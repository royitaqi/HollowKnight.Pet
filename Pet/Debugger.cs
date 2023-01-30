using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Pet.Modules;
using Pet.Utils;
using SFCore.Utils;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Pet
{
#if (DEBUG)
    internal class Debugger
    {
        public static Debugger Instance;

        public void Load()
        {
            ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
            On.PlayMakerFSM.Start += PlayMakerFSM_Start;
        }

        public void Unload()
        {
            ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
            On.PlayMakerFSM.Start -= PlayMakerFSM_Start;
        }

        private void ModHooks_HeroUpdateHook()
        {
            EachHeroUpdate();
        }

        private void PlayMakerFSM_Start(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            orig(self);

            EachFsmAtStart(self);
        }

        #region Playground
        private void EachFsmAtStart(PlayMakerFSM fsm)
        {
            var isKnightDreamNail = (PlayMakerFSM fsm) => fsm is
            {
                name: "Knight",
                FsmName: "Dream Nail",
            };

            var isShadeControl = (PlayMakerFSM fsm) =>
                (fsm.name == "Hollow Shade" || fsm.name == "Hollow Shade(Clone)" || fsm.name == "Grimmchild" || fsm.name == "Grimmchild(Clone)")
                && (fsm.FsmName == "Shade Control" || fsm.FsmName == "Control");

            if (isKnightDreamNail(fsm) || isShadeControl(fsm))
            {
                this.LogModDebug($"Hooked FSM: {fsm.name}-{fsm.FsmName}");
                fsm.MakeLog(true);
            }
        }

        private void EachHeroUpdate()
        {
            if (KeyboardOverride.GetKeyDown(KeyCode.Alpha9))
            {
                if (!_shade.IsLoaded)
                {
                    _shade.Load();
                }
                else
                {
                    _shade.Unload();
                }
            }
        }

        private Shade _shade = new();
        #endregion Playground
    }
#endif
}

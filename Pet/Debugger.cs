using Modding;
using Pet.Utils;
using System.Collections;
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
        private void EachFsmAtStart(PlayMakerFSM self)
        {
            // custom logic
            if (self is
                {
                    name: "Knight",
                    FsmName: "Dream Nail"
                })
            {
                // do something for this FSM
            }
        }

        private void EachHeroUpdate()
        {
        }
        #endregion Playground
    }
#endif
}

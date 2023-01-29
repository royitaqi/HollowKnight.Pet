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
                this.LogModDebug("Creating shade");
                _shade = Shade.Create(HeroController.instance.transform.position + new Vector3(4, 4, 0));

                var fsm = _shade.GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Shade Control");
                var fly = fsm.GetState("Friendly Idle");
                fly.Actions = new FsmStateAction[] { };

                var ownerDefaultFsmOwner = new FsmOwnerDefault { OwnerOption = OwnerDefaultOption.UseOwner, GameObject = _shade };
                var heroOD = new FsmOwnerDefault { OwnerOption = OwnerDefaultOption.UseOwner, GameObject = HeroController.instance.gameObject };
                var heroGO = new FsmGameObject { Value = HeroController.instance.gameObject };
                var distance = fsm.GetFloatVariable("Distance");
                var speed = fsm.GetFloatVariable("Speed");
                var away = fsm.GetBoolVariable("Away");
                var notAway = fsm.GetBoolVariable("Not Away");
                var awayTimer = fsm.GetFloatVariable("Away Timer");
                var radius = fsm.GetFloatVariable("Radius");
                var offset = fsm.GetVector3Variable("Offset");

                // --- Change

                fly.AddMethod(() =>
                {
                    // 1
                    var offsetX = 2f;
                    // 2
                    var heroScale = HeroController.instance.gameObject.transform.GetScaleX();
                    // 3
                    offsetX *= heroScale;
                    // 4
                    var offsetY = UnityEngine.Random.Range(1.85f, 2f);
                    // 5
                    offset.Value = new Vector3(offsetX, offsetY, 0);
                    // 6
                    radius.Value = 0.25f;
                });

                // --- Follow

                // 1
                fly.AddAction(new SetRotation
                {
                    gameObject = ownerDefaultFsmOwner,
                    quaternion = new Quaternion(0, 0, 0, 0),
                    vector = new Vector3(0, 0, 0),
                    xAngle = 0,
                    yAngle = 0,
                    zAngle = 0,
                    space = 0,
                    everyFrame = false,
                    lateUpdate = false,
                });

                // 4
                fly.AddAction(new SetCircleCollider
                {
                    gameObject = ownerDefaultFsmOwner,
                    active = true,
                });

                // 5
                fly.AddAction(new GrimmChildFly
                {
                    objectA = new FsmGameObject { Value = _shade },
                    objectB = heroGO,
                    spriteFacesRight = false, // shade is different than grimmchild
                    playNewAnimation = true,
                    newAnimationClip = "Fly",
                    resetFrame = true,
                    fastAnimSpeed = 10,
                    fastAnimationClip = "Fly",
                    normalAnimationClip = "Fly",
                    flyingFast = false,
                });

                // 6
                fly.AddAction(new GetDistance
                {
                    gameObject = ownerDefaultFsmOwner,
                    target = heroGO,
                    storeResult = distance,
                    everyFrame = true,
                });

                // 7
                fly.AddAction(new FloatMultiply
                {
                    floatVariable = distance,
                    multiplyBy = 1.15f,
                    everyFrame = true,
                });

                // 8
                fly.AddAction(new FloatTestToBool
                {
                    float1 = distance,
                    float2 = 10f,
                    tolerance = 0f,
                    equalBool = false,
                    lessThanBool = false,
                    greaterThanBool = away,
                    everyFrame = true,
                });

                // 9
                fly.AddAction(new FloatOperator
                {
                    float1 = distance,
                    float2 = 4f,
                    operation = 0,
                    storeResult = speed,
                    everyFrame = true,
                });

                // 10
                fly.AddAction(new FloatClamp
                {
                    floatVariable = speed,
                    minValue = 4f,
                    maxValue = 18f,
                    everyFrame = true,
                });

                // 11
                fly.AddAction(new DistanceFlySmooth
                {
                    gameObject = ownerDefaultFsmOwner,
                    target = heroGO,
                    distance = 0,
                    speedMax = speed,
                    accelerationForce = 50f,
                    targetRadius = radius,
                    deceleration = 0.9f,
                    offset = offset,
                });

                // 12
                fly.AddAction(new GetPosition
                {
                    gameObject = ownerDefaultFsmOwner,
                    vector = fsm.GetVector3Variable("Self Pos"),
                    x = 0f,
                    y = 0f,
                    z = 0f,
                    space = 0f,
                    everyFrame = true,
                });

                // 13
                fly.AddAction(new FloatAddV2
                {
                    floatVariable = awayTimer,
                    add = 1f,
                    everyFrame = true,
                    perSecond = true,
                    fixedUpdate = false,
                    activeBool = away,
                });

                // 14
                fly.AddAction(new SetBoolValue
                {
                    boolVariable = notAway,
                    boolValue = away,
                    everyFrame = true,
                });

                // 15
                fly.AddAction(new BoolFlipEveryFrame
                {
                    boolVariable = notAway,
                    everyFrame = true,
                });

                // 16
                fly.AddAction(new SetFloatValueV2
                {
                    floatVariable = awayTimer,
                    floatValue = 0f,
                    everyFrame = true,
                    activeBool = notAway,
                });

                //// 17
                //fly.AddAction(new FloatCompare
                //{
                //    float1 = awayTimer,
                //    float2 = 2f,
                //    tolerance = 0f,
                //    greaterThan = fsm.FsmEvents.First(e => e.Name == "TELE"),
                //    everyFrame = true,
                //});

                this.LogModDebug("Shade created and actions added");
            }
        }

        GameObject _shade;
        #endregion Playground
    }
#endif
}

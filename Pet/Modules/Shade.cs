using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Pet.Utils;
using SFCore.Utils;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Pet.Modules;

internal class Shade
{
    internal bool IsLoaded { get; set; }
    private GameObject _shade;
    private float? _spawnCooldown;

    internal void Load()
    {
        this.LogMod("Load()");
        IsLoaded = true;

        // per-game hooks
        GameUtils.OnHeroUpdate += OnHeroUpdate;
        HookKnightFocus();
    }

    internal void Unload()
    {
        if (!IsLoaded) return;

        this.LogMod("Unload()");
        IsLoaded = false;

        // per-game hooks
        GameUtils.OnHeroUpdate -= OnHeroUpdate;
        UnhookKnightFocus();

        // objects
        _spawnCooldown = null;
        if (_shade != null)
        {
            UObject.DestroyImmediate(_shade);
            _shade = null;
        }
    }

    private void OnHeroUpdate()
    {
        // some update event will still come after unload
        if (!IsLoaded) return;

        // not in a game
        if (GameManager.instance == null || HeroController.instance == null)
        {
            Unload();
            return;
        }

        this.LogModFine($"OnUpdate(): _shade = {_shade?.GetInstanceID().ToString() ?? "null"}, _spawnCooldown = {(_spawnCooldown == null ? "null" : (((int)(_spawnCooldown.Value - GameManager.instance.PlayTime)).ToString() + "->0"))}");

        // we have a shade running
        if (_shade != null) return;

        // no shade and cooldown is met
        if (_spawnCooldown == null || GameManager.instance.PlayTime >= _spawnCooldown.Value)
        {
            _shade = Create(HeroController.instance.transform.position);
            _spawnCooldown = null;
        }
    }

    private GameObject Create(Vector3 position)
    {
        typeof(Shade).LogModDebug("Creating shade");

        var go = GoUtils.GetGameObjectFromResources("Hollow Shade");
        var shade = GameObject.Instantiate(go);
        GameObject.DontDestroyOnLoad(shade);
        shade.Find("Shade Particles").GetComponent<ParticleSystem>().scalingMode = ParticleSystemScalingMode.Hierarchy;
        shade.GetComponent<AudioSource>().enabled = false;
        shade.RemoveComponents<HealthManager>();
        shade.RemoveComponents<DamageHero>();
        shade.transform.SetScaleMatching(0.5f);
        shade.transform.position = position;

        ModifyShadeFsm(shade);

        typeof(Shade).LogModDebug("Shade created and actions added");

        return shade;
    }

    private void ModifyShadeFsm(GameObject shade)
    {
        var fsm = shade.GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Shade Control");
        var change = fsm.AddState("Pet Change");
        var follow = fsm.AddState("Pet Follow");
        var tele = fsm.AddState("Pet Tele");
        var killed = fsm.GetState("Killed");
        fsm.AddTransition("Friendly Idle", "FINISHED", "Pet Change");
        fsm.AddTransition("Pet Change", "FINISHED", "Pet Follow");
        fsm.AddTransition("Pet Follow", "FINISHED", "Pet Change");
        fsm.AddTransition("Pet Follow", "TELE", "Pet Tele");
        fsm.AddTransition("Pet Tele", "FINISHED", "Pet Change");

        var shadeOD = new FsmOwnerDefault { OwnerOption = OwnerDefaultOption.UseOwner, GameObject = shade };
        var shadeGO = new FsmGameObject { Value = shade };
        var heroGO = new FsmGameObject { Value = HeroController.instance.gameObject };
        var distance = fsm.GetFloatVariable("Distance");
        var speed = fsm.GetFloatVariable("Speed");
        var away = fsm.GetBoolVariable("Away");
        var notAway = fsm.GetBoolVariable("Not Away");
        var awayTimer = fsm.GetFloatVariable("Away Timer");
        var radius = fsm.GetFloatVariable("Radius");
        var offset = fsm.GetVector3Variable("Offset");

        // --- Friendly?

        var friendly = fsm.GetState("Friendly?");
        friendly.Insert(new FsmUtils.InsertParam
        {
            Action = new SetIntValue
            {
                intVariable = fsm.GetIntVariable("Royal Charm State"),
                intValue = 4,
                everyFrame = false,
            },
            Named = "Pet Be Friendly",
            After = typeof(GetPlayerDataInt),
        });

        // --- Friendly Idle

        fsm.GetState("Friendly Idle").AddAction(new SendEventByName
        {
            eventTarget = FsmEventTarget.Self,
            sendEvent = "FINISHED",
            delay = 0f,
            everyFrame = false,
        });

        // --- Pet Change

        //change.AddAction(new FaceObject
        //{
        //    objectA = shadeGO,
        //    objectB = heroGO,
        //    spriteFacesRight = false,
        //    playNewAnimation = true,
        //    newAnimationClip = "Fly",
        //    resetFrame = true,
        //    everyFrame = false,
        //});

        change.AddMethod(() =>
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

        // --- Pet Follow

        // 1
        follow.AddAction(new SetRotation
        {
            gameObject = shadeOD,
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
        follow.AddAction(new SetCircleCollider
        {
            gameObject = shadeOD,
            active = true,
        });

        // 5
        follow.AddAction(new GrimmChildFly
        {
            objectA = shadeGO,
            objectB = heroGO,
            spriteFacesRight = false, // shade is different than grimmchild
            playNewAnimation = true,
            newAnimationClip = "Fly",
            resetFrame = true,
            fastAnimSpeed = 10f,
            fastAnimationClip = "Fly",
            normalAnimationClip = "Fly",
            pauseBetweenAnimChange = 0.3f,
            flyingFast = false,
        });

        // 6
        follow.AddAction(new GetDistance
        {
            gameObject = shadeOD,
            target = heroGO,
            storeResult = distance,
            everyFrame = true,
        });

        // 7
        follow.AddAction(new FloatMultiply
        {
            floatVariable = distance,
            multiplyBy = 1.15f,
            everyFrame = true,
        });

        // 8
        follow.AddAction(new FloatTestToBool
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
        follow.AddAction(new FloatOperator
        {
            float1 = distance,
            float2 = 4f,
            operation = 0,
            storeResult = speed,
            everyFrame = true,
        });

        // 10
        follow.AddAction(new FloatClamp
        {
            floatVariable = speed,
            minValue = 4f,
            maxValue = 18f,
            everyFrame = true,
        });

        // 11
        follow.AddAction(new DistanceFlySmooth
        {
            gameObject = shadeOD,
            target = heroGO,
            distance = 0,
            speedMax = speed,
            accelerationForce = 50f,
            targetRadius = radius,
            deceleration = 0.9f,
            offset = offset,
        });

        // 12
        follow.AddAction(new GetPosition
        {
            gameObject = shadeOD,
            vector = fsm.GetVector3Variable("Self Pos"),
            x = 0f,
            y = 0f,
            z = 0f,
            space = 0f,
            everyFrame = true,
        });

        // 13
        follow.AddAction(new FloatAddV2
        {
            floatVariable = awayTimer,
            add = 1f,
            everyFrame = true,
            perSecond = true,
            fixedUpdate = false,
            activeBool = away,
        });

        // 14
        follow.AddAction(new SetBoolValue
        {
            boolVariable = notAway,
            boolValue = away,
            everyFrame = true,
        });

        // 15
        follow.AddAction(new BoolFlipEveryFrame
        {
            boolVariable = notAway,
            everyFrame = true,
        });

        // 16
        follow.AddAction(new SetFloatValueV2
        {
            floatVariable = awayTimer,
            floatValue = 0f,
            everyFrame = true,
            activeBool = notAway,
        });

        // 17
        follow.AddAction(new FloatCompare
        {
            float1 = awayTimer,
            float2 = 2f,
            tolerance = 0f,
            //greaterThan = FsmEvent.GetFsmEvent("ZERO HP"),
            greaterThan = FsmEvent.GetFsmEvent("TELE"),
            everyFrame = true,
        });

        // 18
        follow.AddAction(new Wait
        {
            time = 0.25f,
            realTime = false,
            finishEvent = FsmEvent.Finished,
        });

        // --- Pet Tele

        tele.AddMethod(() =>
        {
            this.LogModDebug($"Shade is too far away from hero. Teleporting ...");
            var pos = HeroController.instance.transform.position;
            shade.transform.position = pos;
        });

        tele.AddAction(new NextFrameEvent());

        // --- Killed

        killed.Insert(new FsmUtils.InsertParam
        {
            Method = () =>
            {
                _shade = null;
                if (GameManager.instance != null)
                {
                    _spawnCooldown = GameManager.instance.PlayTime + 2f;
                    this.LogModDebug($"Shade dying. Planned respawn in 2 seconds.");
                }
                else
                {
                    _spawnCooldown = null;
                    this.LogModDebug($"Shade dying. No respawn is planned (GameManager.instance == null).");
                }
            },
            Named = "Pet Plan Respawn",
            Before = typeof(DestroySelf),
        });
    }

    private PlayMakerFSM GetKnightFocusFsm()
    {
        return HeroController.instance.gameObject.GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Spell Control");
    }

    private void HookKnightFocus()
    {
        var fsm = GetKnightFocusFsm();
        var focus = fsm.AddState("Pet Focus");
        focus.AddMethod(() =>
        {
            _shade?.GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Shade Control")?.SendEvent("ZERO HP");
        });
        fsm.ChangeTransition("Focus", "FOCUS COMPLETED", "Pet Focus");
        fsm.AddTransition("Pet Focus", "FINISHED", "Spore Cloud");
    }

    private void UnhookKnightFocus()
    {
        var fsm = GetKnightFocusFsm();
        fsm.ChangeTransition("Focus", "FOCUS COMPLETED", "Spore Cloud");
        fsm.RemoveState("Pet Focus");
    }
}

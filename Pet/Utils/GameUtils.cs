using Modding;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Pet.Utils;

internal static class GameUtils
{
    internal static void Load()
    {
        USceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
        On.PlayMakerFSM.Start += PlayMakerFSM_Start;
        ModHooks.BeforeSceneLoadHook += ModHooks_BeforeSceneLoadHook;
        On.HeroController.Awake += HeroController_Awake;
    }

    internal static void Unload()
    {
        USceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
        On.PlayMakerFSM.Start -= PlayMakerFSM_Start;
        ModHooks.BeforeSceneLoadHook -= ModHooks_BeforeSceneLoadHook;
        On.HeroController.Awake -= HeroController_Awake;
    }

    internal static void CatchUpEvents()
    {
        if (IsInGame)
        {
            typeof(GameUtils).LogMod("Game has been loaded (this is a catch up event)");
            _gameLoaded?.Invoke();

            typeof(GameUtils).LogMod("Scene has been changed (this is a catch up event)");
            _sceneChanged?.Invoke(null, USceneManager.GetActiveScene());
        }
    }

    #region system hooks
    private static void SceneManager_activeSceneChanged(Scene from, Scene to)
    {
        if (from.name == "Menu_Title")
        {
            _gameLoadedScene = true;
            MaybeRaiseGameLoaded();
        }

        typeof(GameUtils).LogModDebug($"Scene has been changed from \"{from.name}\" to \"{to.name}\"");
        _sceneChanged?.Invoke(from, to);

        // This triggers about 5ms earlier than `On.QuitToMenu.Start`.
        if (to.name == "Quit_To_Menu")
        {
            typeof(GameUtils).LogMod("Game is quiting");
            _onGameQuit?.Invoke();
        }
    }

    private static void ModHooks_HeroUpdateHook()
    {
        typeof(GameUtils).LogModFine("Hero is being updated");
        _onHeroUpdate?.Invoke();
    }

    private static void PlayMakerFSM_Start(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM fsm)
    {
        typeof(GameUtils).LogModFine($"FSM \"{fsm.name}-{fsm.FsmName}\" is being started");
        _onFsmStart?.Invoke(fsm);
    }

    private static string ModHooks_BeforeSceneLoadHook(string maybeScene)
    {
        typeof(GameUtils).LogModDebug($"Scene is being changed");
        _onSceneChange?.Invoke();
        return maybeScene;
    }

    private static void HeroController_Awake(On.HeroController.orig_Awake orig, HeroController self)
    {
        typeof(GameUtils).LogModDebug($"Hero controller is awaken");
        orig(self);

        _gameLoadedHero = true;
        MaybeRaiseGameLoaded();
    }
    #endregion system hooks

    // Both events can come in different order in different scenarios:
    //   - When starting a new game, scene is activated before hero controller instance is created.
    //   - When loading an existing game, hero controller instance is created before scene is activated.
    // So here we log both's readiness and only consider the game loaded when both are ready.
    private static void MaybeRaiseGameLoaded()
    {
        if (_gameLoadedScene && _gameLoadedHero)
        {
            _gameLoadedHero = false;
            _gameLoadedScene = false;
            typeof(GameUtils).LogMod("Game has been loaded");
            _gameLoaded?.Invoke();
        }
    }
    private static bool _gameLoadedScene;
    private static bool _gameLoadedHero;

    #region my hooks
    internal static event Action GameLoaded
    {
        add
        {
            _gameLoaded -= value;
            _gameLoaded += value;
        }
        remove
        {
            _gameLoaded -= value;
        }
    }
    private static event Action _gameLoaded;

    internal static event Action OnGameQuit
    {
        add
        {
            _onGameQuit -= value;
            _onGameQuit += value;
        }
        remove
        {
            _onGameQuit -= value;
        }
    }
    private static event Action _onGameQuit;

    internal static event Action OnSceneChange
    {
        add
        {
            _onSceneChange -= value;
            _onSceneChange += value;
        }
        remove
        {
            _onSceneChange -= value;
        }
    }
    private static event Action _onSceneChange;

    internal static event Action<Scene?, Scene> SceneChanged
    {
        add
        {
            _sceneChanged -= value;
            _sceneChanged += value;
        }
        remove
        {
            _sceneChanged -= value;
        }
    }
    private static event Action<Scene?, Scene> _sceneChanged;

    internal static event Action OnHeroUpdate
    {
        add
        {
            _onHeroUpdate -= value;
            _onHeroUpdate += value;
        }
        remove
        {
            _onHeroUpdate -= value;
        }
    }
    private static event Action _onHeroUpdate;

    internal static event Action<PlayMakerFSM> OnFsmStart
    {
        add
        {
            _onFsmStart -= value;
            _onFsmStart += value;
        }
        remove
        {
            _onFsmStart -= value;
        }
    }
    private static event Action<PlayMakerFSM> _onFsmStart;
    #endregion my hooks

    internal static bool IsInGame => GameManager.instance.IsGameplayScene();
}

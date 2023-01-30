using Modding;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Pet.Utils;

internal static class HookUtils
{
    public static void Load()
    {
        USceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
    }

    public static void Unload()
    {
        USceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
    }

    #region system hooks
    private static void SceneManager_activeSceneChanged(Scene from, Scene to)
    {
        typeof(HookUtils).LogModDebug($"SceneManager_activeSceneChanged(from = {from.name}, to = {to.name})");

        if (from.name == "Menu_Title")
        {
            typeof(HookUtils).LogMod("Game has been loaded");
            _gameLoaded?.Invoke();
        }

        // This triggers about 5ms earlier than `On.QuitToMenu.Start`.
        if (to.name == "Quit_To_Menu")
        {
            typeof(HookUtils).LogMod("Game is quiting");
            _onGameQuit?.Invoke();
        }
    }

    private static void ModHooks_HeroUpdateHook()
    {
        typeof(HookUtils).LogModFine("Hero is being updated");
        _onHeroUpdate?.Invoke();
    }
    #endregion system hooks

    #region my hooks
    public static event Action GameLoaded
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

    public static event Action OnGameQuit
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

    public static event Action OnHeroUpdate
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
    #endregion my hooks
}

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
    }

    internal static void Unload()
    {
        USceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
    }

    #region system hooks
    private static void SceneManager_activeSceneChanged(Scene from, Scene to)
    {
        typeof(GameUtils).LogModDebug($"SceneManager_activeSceneChanged(from = {from.name}, to = {to.name})");

        if (from.name == "Menu_Title")
        {
            typeof(GameUtils).LogMod("Game has been loaded");
            _gameLoaded?.Invoke();
        }

        typeof(GameUtils).LogModDebug("Game has been loaded");
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
    #endregion system hooks

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

    internal static event Action<Scene, Scene> SceneChanged
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
    private static event Action<Scene, Scene> _sceneChanged;

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
    #endregion my hooks

    internal static bool IsInGame => GameManager.instance.IsGameplayScene();
}

using Pet.Utils;
using Modding;
using Satchel.BetterMenus;
using MenuButton = Satchel.BetterMenus.MenuButton;

namespace Pet
{
    public sealed partial class Pet : ICustomMenuMod
    {
        public bool ToggleButtonInsideMenu => true;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggle)
        {
            _menuRef = new Menu(Instance.Name, new Element[]
            {
                toggle!.Value.CreateToggle(
                    "Shade",
                    "A small pet, usually given to new players ..."
                ),
#if DEBUG
                new HorizontalOption(
                    "Fade Bottom Left Display",
                    "Press \"0\" on main keyboard to bring up the display",
                    new []{ "Off", "On" },
                    selectedIndex => {
                        Pet.Instance.GlobalData.FadeDisplay = selectedIndex == 1;
                        ModDisplay.Instance?.Update();
                    },
                    () => Pet.Instance.GlobalData.FadeDisplay ? 1 : 0
                ),
                new MenuButton(
                    "DEBUG: Move & Resize Display",
                    "",
                    _ => ModDisplay.Instance?.EnableDebugger()
                ),
                new MenuButton(
                    "DEBUG: Change Logger Settings",
                    "",
                    _ => LoggingUtils.EnableDebugger()
                ),
#endif
            });

            _menuRef.SetMenuButtonNameAndDesc(Instance, Instance.Name);
            return _menuRef.GetMenuScreen(modListMenu);
        }

        private Menu _menuRef;
    }
}

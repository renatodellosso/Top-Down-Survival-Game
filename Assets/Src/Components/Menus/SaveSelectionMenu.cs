namespace Assets.Src.Components.Menus
{
    public class SaveSelectionMenu : ExitableMenu
    {

        public NewGameMenu newGameMenu;

        public bool multiplayer = false;

        // Start is called before the first frame update
        void Start()
        {
            newGameMenu = transform.parent.GetComponentInChildren<NewGameMenu>(includeInactive: true);
            newGameMenu.previousMenu = this;
        }

        public void NewGame()
        {
            newGameMenu.multiplayer = multiplayer;
            FadeOut(onFadeComplete: () => newGameMenu.FadeIn());
        }
    }
}
using Assets.Src.Components.Menus;
using Assets.Src.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameMenu : ExitableMenu
{

    public StartGameLoadingScreen loadingScreen;

    TMP_InputField inputField;
    Button confirmButton, backButton;

    // Start is called before the first frame update
    void Start()
    {
        //Find components
        inputField = GetComponentInChildren<TMP_InputField>();

        IEnumerable<Button> buttons = GetComponentsInChildren<Button>();
        backButton = buttons.First();
        confirmButton = buttons.Last();

        //Add listeners
        confirmButton.onClick.AddListener(Confirm);
        backButton.onClick.AddListener(Back);
    }

    void Confirm()
    {
        string joinCode = inputField.text;
        
        print("Joining game... Join Code: " + joinCode);

        FadeOut(onFadeComplete: () => StartGameLoadingScreen.StartGame(loadSaveFile: false, multiplayer: true, joinCode: joinCode));
    }

}

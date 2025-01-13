using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{

    private UIDocument _document;
    private Button _startButton;
    private Button _exitButton;
    private List<Button> _menuButtons;

    private SceneManager _sceneManager;

    private void Awake(){
        _document = GetComponent<UIDocument>();
        _startButton = _document.rootVisualElement.Q<Button>("StartButton") as Button;
        _startButton.RegisterCallback<ClickEvent>(OnStartGameClick);
        _exitButton = _document.rootVisualElement.Q<Button>("ExitButton") as Button;
        _exitButton.RegisterCallback<ClickEvent>(onGameExitClick);
        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();
        for (int i = 0; i < _menuButtons.Count; i++){
            _menuButtons[i].RegisterCallback<ClickEvent>(OnMenuButtonClick);
        }
    }

    private void OnDisable(){
        _startButton.UnregisterCallback<ClickEvent>(OnStartGameClick);
    }

    private void OnStartGameClick(ClickEvent evt){
        Debug.Log("Start Game Clicked");
        SceneManager.LoadScene("ChatRoom");
    }

    private void onGameExitClick(ClickEvent evt){
        Debug.Log("Exit Game Clicked");
        Application.Quit();
    }

    private void OnMenuButtonClick(ClickEvent evt){
        Debug.Log("Menu Button Clicked");
    }
}

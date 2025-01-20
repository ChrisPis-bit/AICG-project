using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuEvents : MonoBehaviour
{
    private UIDocument _document;
    private Button _continueButton;
    private Button _mainMenuButton;

    private void Awake(){

        _document = GetComponent<UIDocument>();
        _document.enabled = false;

        _continueButton = _document.rootVisualElement.Q<Button>("ContinueButton");
        _continueButton.RegisterCallback<ClickEvent>(OnContinueButtonClick);
        _mainMenuButton = _document.rootVisualElement.Q<Button>("MainMenuButton");
        _mainMenuButton.RegisterCallback<ClickEvent>(OnMainMenuButtonClick);
    }

    private void OnContinueButtonClick(ClickEvent evt){
        Debug.Log("Continue Button Clicked");
        _document.enabled = false;
    }

    private void OnMainMenuButtonClick(ClickEvent evt){
        Debug.Log("Main Menu Button Clicked");
        // _document.enabled = false;
        SceneManager.LoadScene("MainMenu");
    }
}

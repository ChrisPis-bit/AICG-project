using FMODUnity;
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
    private Slider _slider;

    private void Awake(){

        _document = GetComponent<UIDocument>();

        _continueButton = _document.rootVisualElement.Q<Button>("ContinueButton");
        _mainMenuButton = _document.rootVisualElement.Q<Button>("MainMenuButton");
        _slider = _document.rootVisualElement.Q<Slider>("VolumeSlider");
        _slider.RegisterValueChangedCallback(OnSliderChange);
        RuntimeManager.StudioSystem.getParameterByName("MasterVolume", out float volume);
        _slider.value = volume;

        _document.rootVisualElement.transform.position = Vector3.one * 10000;
    }

    private void OnEnable()
    {

        _continueButton.clicked += OnContinueButtonClick;
        _mainMenuButton.clicked += OnMainMenuButtonClick;
    }

    private void OnDisable()
    {
        _continueButton.clicked -= OnContinueButtonClick;
        _mainMenuButton.clicked -= OnMainMenuButtonClick;
    }

    private void OnContinueButtonClick(){
        Debug.Log("Continue Button Clicked");
        _document.rootVisualElement.transform.position = Vector3.one * 10000;
    }

    private void OnMainMenuButtonClick(){
        Debug.Log("Main Menu Button Clicked");
        // _document.enabled = false;
        SceneManager.LoadScene("MainMenu");
    }

    private void OnSliderChange(ChangeEvent<float> evt)
    {
        Debug.Log("Slider Value Changed: " + evt.newValue);
        // change fmod parameter 'MasterVolume'
        var log = RuntimeManager.StudioSystem.setParameterByName("MasterVolume", evt.newValue);
        Debug.Log("FMOD set parameter: " + log);
    }
}

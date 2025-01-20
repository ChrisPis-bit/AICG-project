using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FMODUnity;

public class MainMenuEvents : MonoBehaviour
{
    //
    private UIDocument _document;
    private Button _startButton;
    private Button _exitButton;
    private List<Button> _menuButtons;

    // slider
    [SerializeField] private float _volume = 0.8f;
    private Slider _slider;


    private void Awake(){
        _document = GetComponent<UIDocument>();
        _startButton = _document.rootVisualElement.Q<Button>("StartButton");
        _startButton?.RegisterCallback<ClickEvent>(OnStartGameClick);
        _exitButton = _document.rootVisualElement.Q<Button>("ExitButton");
        _exitButton?.RegisterCallback<ClickEvent>(onGameExitClick);

        // slider
        _slider = _document.rootVisualElement.Q<Slider>("VolumeSlider");

        _slider.RegisterValueChangedCallback(OnSliderChange);
        RuntimeManager.StudioSystem.getParameterByName("MasterVolume", out _volume);
        _slider.value = _volume;


        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();
        for (int i = 0; i < _menuButtons.Count; i++){
            _menuButtons[i].RegisterCallback<ClickEvent>(OnMenuButtonClick);
        }
    }

    private void OnDisable(){
        _startButton?.UnregisterCallback<ClickEvent>(OnStartGameClick);
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

    private void OnSliderChange(ChangeEvent<float> evt){
        Debug.Log("Slider Value Changed: " + evt.newValue);
        // change fmod parameter 'MasterVolume'
        var log = RuntimeManager.StudioSystem.setParameterByName("MasterVolume", evt.newValue);
        Debug.Log("FMOD set parameter: " + log);
    }
}

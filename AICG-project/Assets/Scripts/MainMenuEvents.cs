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

    // sliders
    private Slider _musicVolumeSlider;
    private Slider _talkingVolumeSlider;
    [SerializeField] private MouseClickSound mouseClickSound;


    private void Awake(){
        _document = GetComponent<UIDocument>();
        _startButton = _document.rootVisualElement.Q<Button>("StartButton");
        _startButton?.RegisterCallback<ClickEvent>(OnStartGameClick);
        _exitButton = _document.rootVisualElement.Q<Button>("ExitButton");
        _exitButton?.RegisterCallback<ClickEvent>(onGameExitClick);

        // sliders
        _musicVolumeSlider = _document.rootVisualElement.Q<Slider>("MusicVolumeSlider");
        _musicVolumeSlider.RegisterValueChangedCallback(OnMusicVolumeSliderChange);
        RuntimeManager.StudioSystem.getParameterByName("MusicVolume", out float _volume);
        _musicVolumeSlider.value = _volume;

        _talkingVolumeSlider = _document.rootVisualElement.Q<Slider>("TalkingVolumeSlider");
        _talkingVolumeSlider.RegisterValueChangedCallback(OnTalkingVolumeSliderChange);
        RuntimeManager.StudioSystem.getParameterByName("TalkingVolume", out _volume);
        _talkingVolumeSlider.value = _volume;


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
        mouseClickSound.PlayClickSound();
    }

    private void OnMusicVolumeSliderChange(ChangeEvent<float> evt){
        Debug.Log("MusicSlider Value Changed: " + evt.newValue);
        var log = RuntimeManager.StudioSystem.setParameterByName("MusicVolume", evt.newValue);
        Debug.Log("FMOD set parameter: " + log);
    }

    private void OnTalkingVolumeSliderChange(ChangeEvent<float> evt){
        Debug.Log("TalkingSlider Value Changed: " + evt.newValue);
        var log = RuntimeManager.StudioSystem.setParameterByName("TalkingVolume", evt.newValue);
        Debug.Log("FMOD set parameter: " + log);
    }
}

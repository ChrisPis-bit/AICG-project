using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatCanvasEventHandler : MonoBehaviour
{
    private UIDocument _document;
    [SerializeField] private MouseClickSound _mouseClickSound;
    [SerializeField] private string _pauseMenuButtonLabel = "PauseMenuButton";
    private Button _pauseMenuButton;
    [SerializeField] private UIDocument _pauseMenuDocument;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        foreach (var button in _document.rootVisualElement.Query<Button>().ToList())
        {
            button.RegisterCallback<ClickEvent>(evt => _mouseClickSound.PlayClickSound());
        }

        _pauseMenuButton = _document.rootVisualElement.Q<Button>(_pauseMenuButtonLabel);
        _pauseMenuButton.clicked += OnPauseMenuButtonClick;
    }

    private void OnPauseMenuButtonClick(){
        Debug.Log("Pause Menu Button Clicked");
        _pauseMenuDocument.enabled = true;
    }
}

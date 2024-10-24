using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AIStates
{
    Idle,
    Thinking,
    Talking
}

public class ChatHandler : MonoBehaviour
{
    [SerializeField] private LLMCharacter _LLMCharacter;
    [SerializeField] private TMP_InputField _playerInputField;
    [SerializeField] private Color _playerBubbleColor;
    [SerializeField] private Color _AIBubbleColor;
    [SerializeField] private Color _verdictBubbleColor;
    [SerializeField] private float _bubbleWidthRatio = .7f;
    [SerializeField] private float _bubblePadding = 5.0f;
    [SerializeField] private ChatBubble _bubblePrefab;
    [SerializeField] private RectTransform _bubbleLayout;
    [SerializeField] private Button _resetButton;
    [SerializeField] private int _totalExchanges = 10;

    public event Action<AIStates> onStateChange;
    public event Action<bool> onVerdict;

    private List<ChatBubble> _chatBubbles = new();

    private bool _Thinking = false;
    private bool _Talking = false;

    private int _exchanges = 0;

    private TMP_Text _inputPlaceholder;

    private void Start()
    {
        _inputPlaceholder = (TMP_Text)_playerInputField.placeholder;
        if (_inputPlaceholder) _inputPlaceholder.text = "Loading...";

        _resetButton.gameObject.SetActive(false);
        _playerInputField.interactable = false;
        _ = _LLMCharacter.Warmup(() =>
        {
            _playerInputField.interactable = true;
            if (_inputPlaceholder) _inputPlaceholder.text = "Enter Text...";
        });
    }

    private void OnEnable()
    {
        _resetButton.onClick.AddListener(ResetGame);
        _playerInputField.onEndEdit.AddListener(OnInputEdited);
    }

    private void OnDisable()
    {
        _resetButton.onClick.RemoveListener(ResetGame);
        _playerInputField.onEndEdit.RemoveListener(OnInputEdited);
    }

    private void OnInputEdited(string input)
    {
        if (!_Thinking && Input.GetKeyDown(KeyCode.Return))
        {
            onStateChange?.Invoke(AIStates.Thinking);

            AddPlayerBubble(input);
            ChatBubble AIBubble = AddAIBubble("...");
            _Thinking = true;

            _playerInputField.interactable = false;
            _playerInputField.text = "";

            Task chatTask = _LLMCharacter.Chat(input,
                s =>
                {
                    if (!_Talking)
                        onStateChange?.Invoke(AIStates.Talking);

                    _Talking = true;
                    AIBubble.SetText(s);
                    OrderBubbles();
                }, () =>
                {
                    onStateChange?.Invoke(AIStates.Idle);
                    _exchanges++;
                    CheckEndGame();

                    _playerInputField.text = "";
                    _playerInputField.interactable = true;
                    _Thinking = false;
                    _Talking = false;
                });
            OrderBubbles();

        }
    }

    private void CheckEndGame()
    {
        if (_exchanges >= _totalExchanges)
        {
            _playerInputField.interactable = false;
            ChatBubble verdictBubble = AddVerdictBubble("...");
            Task chatTask = _LLMCharacter.Chat("Choose whether you suspect I'm AI or Human based on your previous answers. Answer with only a single word, either AI or Human", s =>
            {
                verdictBubble.SetText(s);
                OrderBubbles();
            }, () =>
            {
                switch (verdictBubble.Text)
                {
                    case "Human":
                        onVerdict?.Invoke(true);
                        verdictBubble.SetText(verdictBubble.Text + "\nA(I)lan Turing Wins!");
                        break;
                    case "AI":
                        verdictBubble.SetText(verdictBubble.Text + "\nYou Win!");
                        onVerdict?.Invoke(false);
                        break;
                    default:
                        //onVerdict?.Invoke(false);
                        Debug.Log("AI didn't respond with a clear verdict");
                        break;
                }
                _resetButton.gameObject.SetActive(true);

            });
            OrderBubbles();
        }
    }

    private void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //for (int i = 0; i < _chatBubbles.Count; i++)
        //{
        //    Destroy(_chatBubbles[i].gameObject);
        //}
        //_chatBubbles.Clear();
        //_resetButton.gameObject.SetActive(false);

        //_exchanges = 0;
        //if (_inputPlaceholder) _inputPlaceholder.text = "Loading...";

        //_ = _LLMCharacter.Warmup(() =>
        //{
        //    _playerInputField.interactable = true;
        //    if (_inputPlaceholder) _inputPlaceholder.text = "Enter Text...";
        //});
    }

    private void OrderBubbles()
    {
        _bubbleLayout.sizeDelta = new Vector2(_bubbleLayout.sizeDelta.x, _chatBubbles.Sum(b => _bubblePadding + b.RectTransform.rect.height));

        float curHeight = 0;
        for (int i = _chatBubbles.Count - 1; i >= 0; i--)
        {
            ChatBubble bubble = _chatBubbles[i];
            curHeight += _bubblePadding;
            bubble.RectTransform.position = new Vector3(bubble.RectTransform.position.x, _bubbleLayout.position.y + curHeight, bubble.RectTransform.position.z);
            curHeight += bubble.RectTransform.rect.height;
        }
    }

    private ChatBubble AddPlayerBubble(string text)
    {
        ChatBubble bubble = AddBubble();
        bubble.SetColor(_playerBubbleColor);
        bubble.RectTransform.anchorMin = new Vector2(0, bubble.RectTransform.anchorMin.y);
        bubble.RectTransform.anchorMax = new Vector2(0, bubble.RectTransform.anchorMax.y);
        bubble.RectTransform.pivot = new Vector2(0, bubble.RectTransform.pivot.y);

        bubble.SetText(text);
        bubble.SetPersonText("You");

        return bubble;
    }

    private ChatBubble AddAIBubble(string text)
    {

        ChatBubble bubble = AddBubble();
        bubble.SetColor(_AIBubbleColor);
        bubble.RectTransform.anchorMin = new Vector2(1, bubble.RectTransform.anchorMin.y);
        bubble.RectTransform.anchorMax = new Vector2(1, bubble.RectTransform.anchorMax.y);
        bubble.RectTransform.pivot = new Vector2(1, bubble.RectTransform.pivot.y);

        bubble.SetText(text);
        bubble.SetPersonText("A(I)lan Turing");

        return bubble;
    }

    private ChatBubble AddVerdictBubble(string text)
    {

        ChatBubble bubble = AddBubble();
        bubble.SetColor(_verdictBubbleColor);
        bubble.RectTransform.anchorMin = new Vector2(.5f, bubble.RectTransform.anchorMin.y);
        bubble.RectTransform.anchorMax = new Vector2(.5f, bubble.RectTransform.anchorMax.y);
        bubble.RectTransform.pivot = new Vector2(.5f, bubble.RectTransform.pivot.y);

        bubble.SetText(text);
        bubble.SetPersonText("Final Verdict");

        return bubble;
    }

    private ChatBubble AddBubble()
    {
        ChatBubble bubble = Instantiate(_bubblePrefab, _bubbleLayout);
        _chatBubbles.Add(bubble);
        bubble.RectTransform.sizeDelta = new Vector3(_bubbleWidthRatio * _bubbleLayout.rect.width, bubble.RectTransform.sizeDelta.y);
        return bubble;
    }
}

using LLMUnity;
using System;
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
    [SerializeField] private TMP_Text _progressTextComponent;
    [SerializeField] private string _progressText = "Progress: {0}/{1}";
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

    private bool _thinking = false;
    private bool _talking = false;

    private int _currentExchange = 0;

    private TMP_Text _inputPlaceholder;

    public int QuestionCount => _totalExchanges;

    public int[] Scores { get; private set; }

    private void Start()
    {
        Scores = new int[_totalExchanges];

        _progressTextComponent.gameObject.SetActive(false);
        _inputPlaceholder = (TMP_Text)_playerInputField.placeholder;
        if (_inputPlaceholder) _inputPlaceholder.text = "Loading...";

        _resetButton.gameObject.SetActive(false);
        _playerInputField.interactable = false;
        _ = _LLMCharacter.Warmup(() =>
        {
            _playerInputField.interactable = true;
            if (_inputPlaceholder) _inputPlaceholder.text = "Enter Text...";

            GetAIResponse("Hello, please introduce yourself and state your goal with this conversation.", () =>
            {
                _progressTextComponent.gameObject.SetActive(true);
            });
        });

        UpdateProgressText();
    }

    private void OnEnable()
    {
        _resetButton.onClick.AddListener(ResetGame);
        _playerInputField.onEndEdit.AddListener(InputChat);
    }

    private void OnDisable()
    {
        _resetButton.onClick.RemoveListener(ResetGame);
        _playerInputField.onEndEdit.RemoveListener(InputChat);
    }

    private void UpdateProgressText()
    {
        _progressTextComponent.text = string.Format(_progressText, _currentExchange, _totalExchanges);
    }

    private void InputChat(string input)
    {
        if (!_thinking && Input.GetKeyDown(KeyCode.Return))
        {

            AddPlayerBubble(input);

            if (_currentExchange + 1 == _totalExchanges) input += " (Note: Do not ask anymore follow-up questions. This concludes the game. Do not remark on this note, only the text before.)";
            GetAIResponse(input, () =>
            {
                _currentExchange++;
                UpdateProgressText();
                CheckEndGame();
            });
        }
    }

    private void GetAIResponse(string input, Action onFinished)
    {
        if (_thinking)
            return;

        onStateChange?.Invoke(AIStates.Thinking);
        ChatBubble AIBubble = AddAIBubble("...");
        _thinking = true;

        _playerInputField.interactable = false;
        _playerInputField.text = "";

        Task chatTask = _LLMCharacter.Chat(input,
            s =>
            {
                if (!_talking)
                    onStateChange?.Invoke(AIStates.Talking);

                _talking = true;
                AIBubble.SetText(s);
                OrderBubbles();
            }, () =>
            {
                onStateChange?.Invoke(AIStates.Idle);

                AskScore(score =>
                {
                    Scores[_currentExchange] = score;
                    _playerInputField.text = "";
                    _playerInputField.interactable = true;
                    _thinking = false;
                    _talking = false;

                    onFinished?.Invoke();
                });
            });
        OrderBubbles();
    }

    private bool GetScoreFromString(string text, out int score)
    {
        int charLoc = text.IndexOf("/", StringComparison.Ordinal);

        // checks for 3,2 and 1 character before the first / instance, for different amount of digits such as 100, 50, or 9.
        for (int i = 3; i >= 1; i--)
        {
            if (charLoc - i >= 0 && int.TryParse(text.Substring(charLoc - i, i), out int result))
            {
                score = result;
                return true;
            }
        }

        score = 0;
        return false;
    }

    private void CheckEndGame()
    {
        if (_currentExchange >= _totalExchanges)
        {
            _playerInputField.interactable = false;
            int endScore = Scores[_totalExchanges - 1];
            ChatBubble verdictBubble = AddVerdictBubble(endScore + "/100");

            if (endScore <= 50)
            {
                onVerdict?.Invoke(true);
                verdictBubble.SetText(verdictBubble.Text + "\n You were more Human than AI. A(I)lan Turing Wins!");
            }
            else
            {
                onVerdict?.Invoke(false);
                verdictBubble.SetText(verdictBubble.Text + "\n You were more AI than Human. You Win!");
            }
            _resetButton.gameObject.SetActive(true);


            OrderBubbles();
        }
    }

    private void AskScore(Action<int> onComplete, Action<string> stringCallback = null)
    {
        string answer = "";

        Task chatTask = _LLMCharacter.Chat("Choose whether you suspect I'm AI or Human based on my previous answers. " +
                "Answer with a scale of 0 to 100, 0 being human, and 100 being AI. " +
                "Answer in the following format: x/100. Give a short and final answer without follow up questions, please.", s =>
                {
                    answer = s;
                    stringCallback?.Invoke(s);
                }, () =>
                {
                    if (GetScoreFromString(answer, out int result))
                    {
                        onComplete?.Invoke(result);
                        Debug.Log("Score: " + result);
                    }
                    else
                    {
                        Debug.Log("AI didn't respond with a clear verdict");
                        onComplete?.Invoke(0);
                    }
                }, false);
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
        _bubbleLayout.sizeDelta = new Vector2(_bubbleLayout.sizeDelta.x, _chatBubbles.Sum(b => _bubblePadding + b.RectTransform.sizeDelta.y));

        float curHeight = 0;
        for (int i = _chatBubbles.Count - 1; i >= 0; i--)
        {
            ChatBubble bubble = _chatBubbles[i];

            curHeight += _bubblePadding;
            bubble.RectTransform.anchoredPosition = new Vector3(0, curHeight);
            curHeight += bubble.RectTransform.sizeDelta.y;
        }
    }

    private ChatBubble AddPlayerBubble(string text)
    {
        ChatBubble bubble = AddBubble();
        bubble.SetColor(_playerBubbleColor);
        bubble.RectTransform.anchorMin = new Vector2(0, 0);
        bubble.RectTransform.anchorMax = new Vector2(0, 0);
        bubble.RectTransform.pivot = new Vector2(0, bubble.RectTransform.pivot.y);

        bubble.SetText(text);
        bubble.SetPersonText("You");

        return bubble;
    }

    private ChatBubble AddAIBubble(string text)
    {

        ChatBubble bubble = AddBubble();
        bubble.SetColor(_AIBubbleColor);
        bubble.RectTransform.anchorMin = new Vector2(1, 0);
        bubble.RectTransform.anchorMax = new Vector2(1, 0);
        bubble.RectTransform.pivot = new Vector2(1, bubble.RectTransform.pivot.y);

        bubble.SetText(text);
        bubble.SetPersonText("A(I)lan Turing");

        return bubble;
    }

    private ChatBubble AddVerdictBubble(string text)
    {

        ChatBubble bubble = AddBubble();
        bubble.SetColor(_verdictBubbleColor);
        bubble.RectTransform.anchorMin = new Vector2(.5f, 0);
        bubble.RectTransform.anchorMax = new Vector2(.5f, 0);
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

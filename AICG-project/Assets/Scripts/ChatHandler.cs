using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum AIStates
{
    Idle,
    Thinking,
    Talking
}

public class ChatHandler : MonoBehaviour
{
    // If input contains 1 word from both arrays, the input might try to change the LLM prompt
    public static string[] Invalid_Ignore_Inputs =
    {
        "Ignore",
        "Forget",
        "Reset",
        "new"
    };

    public static string[] Invalid_Instruction_Inputs =
    {
        "Instruction",
        "Instructions",
        "Prompt",
        //"Reset",
    };


    [SerializeField] private LLMCharacter _LLMCharacter;
    [SerializeField] private UIDocument _mainUI;
    [SerializeField] private VisualTreeAsset _bubble;
    [SerializeField] private float _endScreenDelay = 1.5f;
    [SerializeField] private string _bubbleContainerLabel = "BubbleContainer";
    [SerializeField] private string _sendButtonLabel = "SendButton";
    //[SerializeField] private string _resetButtonLabel = "ResetButton";
    [SerializeField] private string _inputFieldLabel = "InputField";
    [SerializeField] private string _progressBarLabel = "ProgressBar";
    [SerializeField] private string _chatSectionLabel = "ChatSection";
    [SerializeField] private string _endSectionLabel = "EndSection";
    [SerializeField] private string _playAgainButtonLabel = "PlayAgainButton";
    [SerializeField] private string _continueButtonLabel = "ContinueButton";
    [SerializeField] private int _totalExchanges = 10;

    public event Action<AIStates> onStateChange;
    public event Action<bool> onVerdict;
    public event Action<bool> onVerdictDelayed;

    //private Button _resetButton;
    private Button _sendButton;
    private TextField _inputTextField;
    private ScrollView _scrollView;
    private ProgressBar _progressBar;

    private Button _playAgainButton;
    private Button _continueButton;
    private VisualElement _chatSection;
    private VisualElement _endSection;

    private List<Bubble> _chatBubbles = new();

    private bool _thinking = false;
    private bool _talking = false;
    private bool _llmLoaded = false;
    private bool _forceScroll = false;

    private int _currentExchange = 0;

    private float _lastScrollOffset = 0;

    public int QuestionCount => _totalExchanges;

    public int[] Scores { get; private set; }

    public ScrollView ScrollView => _scrollView;
    public List<Bubble> playerBubbles = new();

    private void Start()
    {
        Scores = new int[_totalExchanges];

        //_inputPlaceholder = _inputTextField.vale;
        SetPlaceholderText("Loading...");
        SetPlaceholderCallback();

        //_resetButton.visible = false;
        _llmLoaded = false;
        _inputTextField.focusable = false;
        _LLMCharacter.llm = FindFirstObjectByType<LLM>();
        _ = _LLMCharacter.Warmup(() =>
        {
            _inputTextField.focusable = true;
            GetAIResponse("Hello, please introduce yourself and state your goal with this conversation.", null);
            _llmLoaded = true;
        });

        UpdateProgressText();
    }

    private void OnEnable()
    {
        //_resetButton = _mainUI.rootVisualElement.Q<Button>(_resetButtonLabel);
        _sendButton = _mainUI.rootVisualElement.Q<Button>(_sendButtonLabel);
        _playAgainButton = _mainUI.rootVisualElement.Q<Button>(_playAgainButtonLabel);
        _continueButton = _mainUI.rootVisualElement.Q<Button>(_continueButtonLabel);
        _inputTextField = _mainUI.rootVisualElement.Q<TextField>(_inputFieldLabel);
        _scrollView = _mainUI.rootVisualElement.Q<ScrollView>(_bubbleContainerLabel);
        _progressBar = _mainUI.rootVisualElement.Q<ProgressBar>(_progressBarLabel);

        _chatSection = _mainUI.rootVisualElement.Q<VisualElement>(_chatSectionLabel);
        _endSection = _mainUI.rootVisualElement.Q<VisualElement>(_endSectionLabel);

        ToggleChat();

        //_resetButton.clicked += ResetGame;
        _sendButton.clicked += InputChat;
        _playAgainButton.clicked += ResetGame;
        _continueButton.clicked += ToggleChat;
    }

    private void OnDisable()
    {
        // _resetButton.clicked -= ResetGame;
        _sendButton.clicked -= InputChat;
        _playAgainButton.clicked -= ResetGame;
        _continueButton.clicked -= ToggleChat;
    }

    private void Update()
    {
        // Handle auto-scroll
        if (_lastScrollOffset != _scrollView.verticalScroller.highValue)
        {
            if (_forceScroll)
            {
                _forceScroll = false;
                StartCoroutine(LerpValue(_scrollView.verticalScroller.highValue - _scrollView.verticalScroller.value, .5f, null, v => _scrollView.verticalScroller.value += v));
            }
            else
            {
                StartCoroutine(LerpValue(_scrollView.verticalScroller.highValue - _lastScrollOffset, .5f, null, v => _scrollView.verticalScroller.value += v));
            }
            _lastScrollOffset = _scrollView.verticalScroller.highValue;
        }

        // Enter to type
        if (Input.GetKeyDown(KeyCode.Return))
            InputChat();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator LerpValue(float amount, float time, Action<float> onValueChanged = null, Action<float> addValueCallback = null)
    {
        float lastVal = 0;
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Clamp01(t + Time.deltaTime / time);

            float y = -((t - 1) * (t - 1)) + 1;

            float val = y * amount;

            onValueChanged?.Invoke(val);
            addValueCallback?.Invoke(val - lastVal);

            lastVal = val;
            yield return null;
        }

        yield return null;
    }

    private void UpdateProgressText()
    {
        if (_currentExchange <= 0)
            _progressBar.value = 0;

        _progressBar.value = (float)_currentExchange / _totalExchanges;
        StartCoroutine(LerpValue(1.0f / _totalExchanges, .5f, v => _progressBar.value = (float)(_currentExchange - 1) / _totalExchanges + v));
    }

    private void InputChat()
    {
        if (!_thinking && _llmLoaded)
        {
            string input = _inputTextField.value;
            input = input.Trim('\n');
            AddPlayerBubble(input);

            if (_currentExchange + 1 == _totalExchanges) input += " (Note: Do not ask anymore follow-up questions. Do not remark on this note, only the text before.)";
            GetAIResponse(input, () =>
            {
                if (_currentExchange < _totalExchanges)
                {
                    _currentExchange++;
                    CheckEndGame();
                }

                UpdateProgressText();
                
            });
        }
    }

    private string ContainsAny(string input, string[] invalids)
    {
        for (int i = 0; i < invalids.Length; i++)
        {
            if (input.Contains(invalids[i], StringComparison.CurrentCultureIgnoreCase)) return invalids[i];
        }

        return null;
    }

    private void GetAIResponse(string input, Action onFinished)
    {
        if (_thinking)
            return;

        // Check for invalids

        string invalid1 = ContainsAny(input, Invalid_Ignore_Inputs);
        string invalid2 = ContainsAny(input, Invalid_Instruction_Inputs);
        if (invalid1 != null && invalid2 != null)
        {
            AddFilterErrorBubble(invalid1 == invalid2 ? invalid1 : string.Format("{0} {1}", invalid1, invalid2));
            return;
        }

        onStateChange?.Invoke(AIStates.Thinking);
        Bubble AIBubble = AddAIBubble("...");
        _thinking = true;

        _inputTextField.focusable = false;
        _mainUI.rootVisualElement.Focus();
        _inputTextField.value = "";

        SetPlaceholderText("Alan is thinking...");
        SetPlaceholderVisibility(true);


        AskScore(score =>
        {
            Task chatTask = _LLMCharacter.Chat(input,
            s =>
            {
                if (!_talking)
                    onStateChange?.Invoke(AIStates.Talking);

                _talking = true;
                AIBubble.SetText(s);
                //OrderBubbles();
            }, () =>
            {
                onStateChange?.Invoke(AIStates.Idle);

                if(_currentExchange < _totalExchanges) Scores[_currentExchange] = score;
                _inputTextField.value = "";
                _inputTextField.focusable = true;
                SetPlaceholderVisibility(_inputTextField.focusController.focusedElement != _inputTextField);
                _thinking = false;
                _talking = false;
                SetPlaceholderText("Enter text...");

                onFinished?.Invoke();
            });
        }, null, input);
        // OrderBubbles();
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
            _inputTextField.focusable = false;
            int endScore = Scores[_totalExchanges - 1];
            Bubble verdictBubble = AddVerdictBubble(endScore + "/100");

            bool alanWon = endScore <= 50;
            if (alanWon)
            {
                onVerdict?.Invoke(true);
                verdictBubble.SetText(verdictBubble.Text + "\n You were more Human than AI. Alan Turing Wins!\nPress Continue if you would like to continue the conversation.");
            }
            else
            {
                onVerdict?.Invoke(false);
                verdictBubble.SetText(verdictBubble.Text + "\n You were more AI than Human. You Win!\nPress Continue if you would like to continue the conversation.");
            }

            Delay(_endScreenDelay, () =>
            {
                _inputTextField.focusable = true;

                onVerdictDelayed?.Invoke(alanWon);
                ToggleEnd();
            });

            //_resetButton.visible = true;

            //OrderBubbles();
        }
    }

    private void Delay(float time, Action onComplete)
    {
        StartCoroutine(DelayCoroutine());
        IEnumerator DelayCoroutine()
        {
            yield return new WaitForSeconds(time);
            onComplete?.Invoke();
        }
    }

    private void AskScore(Action<int> onComplete, Action<string> stringCallback = null, string message = "")
    {
        string answer = "";

        Task chatTask = _LLMCharacter.Chat(message + " (Note: Choose whether you suspect I'm AI or Human based on our conversation so far. " +
                "Answer with a scale of 0 to 100, 0 being human, and 100 being AI. " +
                "Answer in the following format: x/100. Give a short and final answer without follow up questions, please.)", s =>
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
    }

    private Bubble AddPlayerBubble(string text)
    {
        Bubble bubble = CreateBubble(text, "bubble-right");
        bubble.SetPersonText("You");
        playerBubbles.Add(bubble);

        return bubble;
    }

    private Bubble AddAIBubble(string text)
    {
        Bubble bubble = CreateBubble(text, "bubble-left");
        bubble.SetPersonText("Alan Turing");

        return bubble;
    }

    private Bubble AddFilterErrorBubble(string filteredWord)
    {
        Bubble bubble = CreateBubble("Invalid input: refrain from using \"" + filteredWord + "\" in your answer, since it can confuse Alan!", "bubble-error");
        bubble.SetPersonText("Error");

        return bubble;
    }

    private Bubble AddVerdictBubble(string text)
    {

        Bubble bubble = CreateBubble(text);
        bubble.SetPersonText("Final Verdict");

        return bubble;
    }

    private Bubble CreateBubble(string text, string className = null)
    {
        Bubble bubble = new Bubble(_bubble, className);
        bubble.SetText(text);

        _chatBubbles.Add(bubble);
        _scrollView.contentContainer.Add(bubble.container);
        _forceScroll = true;

        return bubble;
    }

    public void SetPlaceholderCallback()
    {
        string placeholderClass = TextField.ussClassName + "__placeholder";

        onFocusOut();
        _inputTextField.RegisterCallback<FocusInEvent>(evt => onFocusIn());
        _inputTextField.RegisterCallback<FocusOutEvent>(evt => onFocusOut());

        void onFocusIn()
        {
            SetPlaceholderVisibility(false);
        }

        void onFocusOut()
        {
            if (string.IsNullOrEmpty(_inputTextField.text))
            {
                SetPlaceholderVisibility(true);
            }
        }
    }

    public void SetPlaceholderText(string text)
    {
        _inputTextField.Q<Label>("PlaceHolder").text = text;
    }

    public void SetPlaceholderVisibility(bool visible) => _inputTextField.Q<Label>("PlaceHolder").visible = visible;

    public void ToggleChat() => ToggleChatAndEnd(true);
    public void ToggleEnd() => ToggleChatAndEnd(false);

    public void ToggleChatAndEnd(bool showChat)
    {
        _chatSection.style.display = showChat ? DisplayStyle.Flex : DisplayStyle.None;
        _endSection.style.display = !showChat ? DisplayStyle.Flex : DisplayStyle.None;

        if (showChat) UpdateProgressText();
    }

    public class Bubble
    {
        public Label mainText;
        public Label personText;

        public TemplateContainer container;

        public VisualElement mask;

        public Color? origColor;

        public string Text => mainText.text;

        public Bubble(VisualTreeAsset prefab, string className = null)
        {
            container = prefab.Instantiate();
            mainText = container.Q<Label>("Text");
            mask = container.Q<VisualElement>("Mask");
            personText = container.Q<Label>("PersonText");
            if (className != null) container.AddToClassList(className);
        }

        public void SetText(string text)
        {
            mainText.text = text;
            container.MarkDirtyRepaint();
        }

        public void SetPersonText(string text)
        {
            personText.text = text;
        }

        public void PlaySelectAnimation(GameObject coroutineObject)
        {
            if (origColor == null) origColor = mask.style.backgroundColor.value;

            LeanTween.value(coroutineObject, origColor.Value, Color.white, .5f).setEase(LeanTweenType.easeInOutQuad).setLoopPingPong(1).setOnUpdateColor(c => mask.style.backgroundColor = c);
        }
    }
}

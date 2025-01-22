using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatTimeline : MonoBehaviour
{
    [SerializeField] private ChatHandler _chatHandler;
    [SerializeField] private VisualTreeAsset _scoreBar;
    [SerializeField] private UIDocument _mainUI;
    [SerializeField] private string _timelineParentLabel = "TimelineParent";
    [SerializeField] private string _scoreBarHolderLabel = "ScoreBarHolder";
    [SerializeField] private bool _isTimelineVisible = false;

    private ScoreBar[] _bars;

    private VisualElement _scoreBarHolder;
    private VisualElement _timelineParent;

    private void Start()
    {
        _timelineParent = _mainUI.rootVisualElement.Q<VisualElement>(_timelineParentLabel);
        _scoreBarHolder = _mainUI.rootVisualElement.Q<VisualElement>(_scoreBarHolderLabel);

        _timelineParent.visible = _isTimelineVisible;

        _bars = new ScoreBar[_chatHandler.QuestionCount];

        for (int i = 0; i < _chatHandler.QuestionCount; i++)
        {
            ScoreBar bar = new ScoreBar(_scoreBar, i + 1, _scoreBarHolder);
            _bars[i] = bar;
        }

        for (int i = 0; i < _chatHandler.QuestionCount; i++)
        {
            _bars[i].SetBarHeight(0);
        }
    }

    private void OnEnable()
    {
        _chatHandler.onVerdictDelayed += OnVerdictGiven;
    }

    private void OnDisable()
    {
        _chatHandler.onVerdictDelayed -= OnVerdictGiven;
    }

    private void OnVerdictGiven(bool _)
    {
        for (int i = 0; i < _chatHandler.QuestionCount; i++)
        {
            _bars[i].BindQuestion(_chatHandler.playerBubbles[i], _chatHandler.ScrollView, gameObject);
            _bars[i].SetBarHeight(_chatHandler.Scores[i]);
        }

        _timelineParent.visible = true;
    }

    public class ScoreBar
    {
        public TemplateContainer container;
        public Label text;
        public ChatHandler.Bubble bubble;

        public ScoreBar(VisualTreeAsset prefab, int question, VisualElement parent)
        {
            container = prefab.Instantiate();
            container.AddToClassList("bar-parent");
            text = container.Q<Label>("Text");

            text.text = "Q" + question.ToString();

            parent.Add(container);
        }

        public void SetBarHeight(int score)
        {
            container.style.height = Length.Percent(score);
        }

        public void BindQuestion(ChatHandler.Bubble bubble, ScrollView scrollView, GameObject obj)
        {
            this.bubble = bubble;
            container.AddManipulator(new Clickable(() =>
            {
                scrollView.ScrollTo(this.bubble.container);
                this.bubble.PlaySelectAnimation(obj);
            }));

        }
    }
}

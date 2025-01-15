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

    private ScoreBar[] _bars;

    private VisualElement _scoreBarHolder;
    private VisualElement _timelineParent;

    private void Start()
    {
        _timelineParent = _mainUI.rootVisualElement.Q<VisualElement>(_timelineParentLabel);
        _scoreBarHolder = _mainUI.rootVisualElement.Q<VisualElement>(_scoreBarHolderLabel);

        _timelineParent.visible = false;

        _bars = new ScoreBar[_chatHandler.QuestionCount];

        for (int i = 0; i < _chatHandler.QuestionCount; i++)
        {
            _bars[i] = new ScoreBar(_scoreBar, i + 1, _scoreBarHolder);
        }

        for (int i = 0; i < _chatHandler.QuestionCount; i++)
        {
            _bars[i].SetBarHeight(50);
        }
    }

    private void OnEnable()
    {
        _chatHandler.onVerdict += OnVerdictGiven;
    }

    private void OnDisable()
    {
        _chatHandler.onVerdict -= OnVerdictGiven;
    }

    private void OnVerdictGiven(bool _)
    {
        for (int i = 0; i < _chatHandler.QuestionCount; i++)
        {
            _bars[i].SetBarHeight(_chatHandler.Scores[i]);
        }

        _timelineParent.visible = true;
    }

    public class ScoreBar
    {
        public TemplateContainer container;
        public Label text;

        public ScoreBar(VisualTreeAsset prefab, int question, VisualElement parent)
        {
            container = prefab.Instantiate();
            container.style.marginRight = Length.Auto();
            container.style.marginLeft = Length.Auto();
            text = container.Q<Label>("Text");

            text.text = "Q" + question.ToString();

            parent.Add(container);
        }

        public void SetBarHeight(int score)
        {
            container.style.height = Length.Percent(score);
        }
    }
}

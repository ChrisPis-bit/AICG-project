using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatTimeline : MonoBehaviour
{
    [SerializeField] private ChatHandler _chatHandler;
    [SerializeField] private GameObject _timelineParent;
    [SerializeField] private GameObject _scoreBar;
    [SerializeField] private RectTransform _scoreBarHolder;
    [SerializeField] private TMP_Text _scoreSidebarText;
    [SerializeField] private RectTransform _scoreSidebarTextHolder;

    private RectTransform[] _bars;

    private void Start()
    {
        _timelineParent.SetActive(false);
        _bars = new RectTransform[_chatHandler.QuestionCount];

        for (int i = 0; i < _chatHandler.QuestionCount; i++)
        {
            GameObject newScoreBar = Instantiate(_scoreBar, _scoreBarHolder);
            TMP_Text newText = Instantiate(_scoreSidebarText, _scoreSidebarTextHolder);

            RectTransform barTransform = (RectTransform)newScoreBar.transform;
            _bars[i] = barTransform;

            newText.text = "Q" + (i + 1).ToString();
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
            RectTransform barTransform = _bars[i];
            barTransform.sizeDelta = new Vector2(barTransform.sizeDelta.x, _scoreBarHolder.rect.height * (_chatHandler.Scores[i] / 100.0f));
        }

        _timelineParent.SetActive(true);
    }
}

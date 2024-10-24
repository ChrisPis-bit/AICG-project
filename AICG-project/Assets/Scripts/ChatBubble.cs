using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_Text _personText;
    [SerializeField] private Image _image;
    [SerializeField] private float _heightPadding = 5;

    public RectTransform RectTransform => (RectTransform)transform;
    public string Text => _text.text;
    public void SetText(string text)
    {
        _text.text = text;
        RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, _text.preferredHeight + _heightPadding * 2);
    }
    public void SetPersonText(string text)
    {
        _personText.text = text;
    }

    public void SetColor(Color color)
    {
        _image.color = color;
    }
}

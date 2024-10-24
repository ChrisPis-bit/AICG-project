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

    public RectTransform RectTransform => (RectTransform)transform;
    public string Text => _text.text;
    public void SetText(string text)
    {
        _text.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }
    public void SetPersonText(string text)
    {
        _personText.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }

    public void SetColor(Color color)
    {
        _image.color = color;
    }
}

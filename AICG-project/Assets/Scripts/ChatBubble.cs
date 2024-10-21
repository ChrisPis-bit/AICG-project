using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image _image;

    public RectTransform RectTransform => (RectTransform)transform;

    public void SetText(string text)
    {
        _text.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    }

    public void SetColor(Color color)
    {
        _image.color = color;
    }
}

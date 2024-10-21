using LLMUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatHandler : MonoBehaviour
{
    [SerializeField] private LLMCharacter _LLMCharacter;
    [SerializeField] private TMP_InputField _playerInputField;
    [SerializeField] private Color _playerBubbleColor;
    [SerializeField] private Color _AIBubbleColor;
    [SerializeField] private float _bubbleWidthRatio = .7f;
    [SerializeField] private float _bubblePadding = 5.0f;
    [SerializeField] private ChatBubble _bubblePrefab;
    [SerializeField] private RectTransform _bubbleLayout;

    private List<ChatBubble> _chatBubbles = new();

    private bool _AIThinking = false;

    // Start is called before the first frame update
    async void Start()
    {
        _playerInputField.DeactivateInputField();
        await _LLMCharacter.Warmup();
        _playerInputField.ActivateInputField();
    }

    private void OnEnable()
    {
        _playerInputField.onEndEdit.AddListener(OnInputEdited);
    }

    private void OnDisable()
    {
        _playerInputField.onEndEdit.RemoveListener(OnInputEdited);
    }

    private void OnInputEdited(string input)
    {
        if (!_AIThinking && Input.GetKeyDown(KeyCode.Return))
        {
            _playerInputField.text = "";
            AddPlayerBubble(input);
            ChatBubble AIBubble = AddAIBubble("...");
            _AIThinking = true;
            _playerInputField.DeactivateInputField();
            Task chatTask = _LLMCharacter.Chat(input, s =>
            {
                AIBubble.SetText(s);
                OrderBubbles();
            }, () =>
            {
                _AIThinking = false;
                _playerInputField.ActivateInputField();
            });
            OrderBubbles();

        }
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

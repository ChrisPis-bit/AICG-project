using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private int _animationCount;
    [SerializeField] private ChatHandler _chatHandler;

    private void OnEnable()
    {
        _chatHandler.onVerdict += OnVerdict;
        _chatHandler.onStateChange += OnAIStateChange;
    }

    private void OnDisable()
    {
        _chatHandler.onVerdict -= OnVerdict;
        _chatHandler.onStateChange -= OnAIStateChange;
    }

    private void OnVerdict(bool correct)
    {
        _animator.SetTrigger(correct ? "Win" : "Lose");
    }

    private void OnAIStateChange(AIStates state)
    {
        switch (state)
        {
            case AIStates.Thinking:
                break;
            case AIStates.Talking:
                _animator.SetTrigger("Talk");
                _animator.SetInteger("Animation", Random.Range(1, _animationCount + 1));
                break;
            default:
                _animator.SetInteger("Animation", 0);
                break;
        }
    }
}

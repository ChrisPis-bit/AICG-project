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

    private void FixedUpdate()
    {
        _animator.SetInteger("Animation", Random.Range(1, _animationCount + 1));

    }

    private void OnAIStateChange(AIStates state)
    {
        switch (state)
        {
            case AIStates.Thinking:
                _animator.SetBool("WhileTalk", false);
                break;
            case AIStates.Talking:
                _animator.SetBool("WhileTalk", true);
                _animator.SetTrigger("Talk");
                _animator.SetInteger("Animation", Random.Range(1, _animationCount + 1));
                break;
            default:
                _animator.SetBool("WhileTalk", false);
                _animator.SetInteger("Animation", 0);
                break;
        }
    }
}

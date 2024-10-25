using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private int _animationCount;
    [SerializeField] private ChatHandler _chatHandler;
    [SerializeField] private Transform _start;
    [SerializeField] private Transform _end;
    [SerializeField] private float _time;

    private float curT;
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

    private void Awake()
    {
        OnAIStateChange(AIStates.Talking);
    }

    private void Update()
    {
        curT += Time.deltaTime;
        if (curT / _time > 1)
            return;
        Camera.main.transform.position = _start.position + (_end.position - _start.position) * (curT / _time);
        Camera.main.transform.rotation = Quaternion.Slerp(_start.rotation, _end.rotation, (curT / _time));
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

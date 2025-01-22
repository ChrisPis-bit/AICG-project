using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundPlayer : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference _talkEvent;
    [SerializeField] private ChatHandler _chatHandler;

    private FMOD.Studio.EventInstance instance;

    private void OnEnable()
    {
        instance = RuntimeManager.CreateInstance(_talkEvent);
        _chatHandler.onStateChange += OnStateChange;
    }

    private void OnDisable()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
        _chatHandler.onStateChange -= OnStateChange;
    }

    private void OnStateChange(AIStates state)
    {
        switch (state)
        {
            case AIStates.Talking:
                instance.start();
                break;
            default:
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
        }
    }
}

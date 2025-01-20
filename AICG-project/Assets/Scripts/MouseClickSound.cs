using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;

public class MouseClickSound : MonoBehaviour
{
    [SerializeField] private EventReference _clickEvent;
    private FMOD.Studio.EventInstance instance;

    private void OnEnable()
    {
        instance = RuntimeManager.CreateInstance(_clickEvent);
    }

    private void OnDisable()
    {
        instance.release();
    }   

    public void PlayClickSound()
    {
        Debug.Log("Click Sound Played");
        instance.start();
    }


}

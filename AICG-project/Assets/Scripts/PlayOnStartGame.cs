using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Delays the play call of the <see cref="PlayableDirector"/> until the
/// first frame (I.E. After UIDocument is initialized).
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class DelayedPlayOnAwake : MonoBehaviour
{
    private static bool _introSequencePlayOnce = true;
    public Animator alanAnimator;

    private void Start()
    {
        if (_introSequencePlayOnce)
        {
            alanAnimator.SetTrigger("Talk");
            GetComponent<PlayableDirector>().Play();
            _introSequencePlayOnce = false;
        }
    }
}
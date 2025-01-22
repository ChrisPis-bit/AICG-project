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

    [SerializeField] private bool _playOnce = true;
    private void Start()
    {
        if (_playOnce) { GetComponent<PlayableDirector>().Play(); _playOnce = false; }

    }
}
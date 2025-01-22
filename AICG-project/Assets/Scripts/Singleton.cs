using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class Singleton : MonoBehaviour
{
    public static Singleton instance { get; private set; }
    void Awake(){
        if (!instance) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            Debug.LogWarning("Instance already exists, destroying duplicate");
        }
    }
}

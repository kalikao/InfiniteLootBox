//Just an initial idea, not finished.
//Attach this component to a child of a Light and it will make that light glimmer like a lightning.
//Potential improvements might be: affecting the skybox, adding sprite raindrops, adding wet texture overlay on the screen. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{

    Light mLight;
    void Awake()
    {
        mLight = transform.parent.GetComponent<Light>();
    }

    void Update()
    {
        var t = Time.time;
        var i0 = Mathf.Sin(t * 1.1f) < 0 ? 0 : 1;
        var i1 = Mathf.Sin(t * 1.7f) < 0 ? 0 : 1;
        var s1 = Mathf.Sin(t * 51f);
        var s2 = Mathf.Sin(t * 37f);
        var e = 10 * Mathf.Max(0, i0 * i1 * s1 * s2);
        mLight.intensity = 0.3f + e;
    }
}

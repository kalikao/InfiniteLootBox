using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Used to smoothly fade-in a looped AudioSource.
/// </summary>
/// <remarks>
/// Good for background music.
/// </remarks>
public class AudioFadeIn : MonoBehaviour
{
    private AudioSource mAudio;
    public float Volume;
    void Awake()
    {
        mAudio = GetComponent<AudioSource>();
        mAudio.volume = 0;
		StartCoroutine(Fade());
    }
    IEnumerator Fade()
    {
		float mVol=0;
        while (mVol < Volume)
        {
			mVol += 0.005f;
            mAudio.volume = mVol;
			yield return new WaitForSeconds(0.1f);
		}
		mAudio.volume = Volume;
    }

}

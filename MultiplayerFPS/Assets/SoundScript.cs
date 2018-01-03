using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundScript : MonoBehaviour {

    public AudioClip[] audioClip;

    public void playSound(int clip)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClip[clip];
        audio.Play();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHolder : MonoBehaviour
{
  public AudioClip[] audioClips;

  public AudioClip GetAudioClip()
  {
    // return audioClips[Mathf.RoundToInt(Random.value * (audioClips.Length - 1))];
    return audioClips[6];
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAgent : MonoBehaviour
{
  private AudioClip clip;
  public AudioSource audioSource;
  public Vector3Int position;

  private void Start()
  {
    audioSource = GetComponent<AudioSource>();
    audioSource.volume = 0.5f;
  }

  private void OnEnable()
  {
    if(GameObject.Find("GameManager").GetComponent<GameManager>().IsPlaying && audioSource != null)
    {
      audioSource.clip = GameObject.Find("SoundHolder").GetComponent<SoundHolder>().GetAudioClip();

      audioSource.PlayDelayed(0f);
      
    }
  }

  private void OnDisable()
  {
    if(audioSource != null)
      if (audioSource.isPlaying)
        audioSource.Stop();
  }
}

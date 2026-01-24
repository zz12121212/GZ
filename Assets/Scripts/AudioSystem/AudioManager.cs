using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioClip clip;
    // Start is called before the first frame update
    void Start()
    {
        clip = this.GetComponent<AudioClip>();
    }


}

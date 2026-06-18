using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    //public bool inOrOut;
    [Header("Ä¿±ê³¡¾°")]
    public string nextScene;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            SceneManager.LoadScene(nextScene);
        }
    }
}

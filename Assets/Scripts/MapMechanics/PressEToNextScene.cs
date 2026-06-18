using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressEToNextScene : MonoBehaviour
{
    public string NextSceneName;
    private GameObject PromptMessage;

    private void Start()
    {
        PromptMessage = GameObject.Find("UI").transform.Find("dialogs").Find("PromptMessage_1").gameObject;
    }

    private void OnTriggerEnter2D (Collider2D collision)
    {
        if (collision.CompareTag("Player")) {

            if (Input.GetKeyDown(KeyCode.E))
            {
                SceneManage.Instance.LoadToScene(NextSceneName);
            }
        }
    }
}

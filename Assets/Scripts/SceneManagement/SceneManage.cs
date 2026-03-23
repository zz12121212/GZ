using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManage : MonoBehaviour
{
    public static SceneManage Instance;
    private GameObject LoadUI;
    private bool isLoading = false;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }
        UIPanelData[] panels = FindObjectsOfType<UIPanelData>();
        foreach (UIPanelData data in panels) { 
        if(data.panelType == UIPanels.LoadPanel)
            {
                LoadUI = data.gameObject;
            }
        }
    }

    public void LoadToScene(string SceneName) { 
        if (isLoading == true || SceneName == null) { return; }
        StartCoroutine(LoadScene(SceneName));
    }

    IEnumerator LoadScene(string sceneName )
    {
        isLoading = true;
        if (LoadUI != null) {
            LoadUI.SetActive(true);
        }
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone) {
            if (asyncOperation.progress >= 0.9f) {
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        if (LoadUI != null) LoadUI.SetActive(false); 
        isLoading = false;

    }
}

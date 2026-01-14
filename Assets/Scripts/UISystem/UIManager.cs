using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;

public enum UIPanels {
    StartUI,
    EndUI,
    PauseUI
}



public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private Dictionary<UIPanels,GameObject> UIObjects =  new Dictionary<UIPanels,GameObject>();

    private void Start()
    {
        Instance = this;
        UIPanelData[] panels = FindObjectsOfType<UIPanelData>();
        foreach (UIPanelData data in panels) {
            UIObjects.Add(data.panelType, data.gameObject);
        }
    }

    public void OpenThePanel(UIPanels panel) {
        if (!UIObjects[panel]) { return; }
        bool NeedPause = UIObjects[panel].GetComponent<UIPanelData>().NeedPause;
        if (NeedPause) {
            Time.timeScale = 0;
        }
        UIObjects[panel].SetActive(true);
    }

    public void CloseThePanel(UIPanels panel) {
        if (!UIObjects[panel]) { return; }
        bool NeedPause = UIObjects[panel].GetComponent<UIPanelData>().NeedPause;
        UIObjects[panel].SetActive(true);
        if (NeedPause)
        {
            Time.timeScale = 1;
        }
    }
}

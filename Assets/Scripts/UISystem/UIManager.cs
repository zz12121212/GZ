using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

    public enum UIPanels
    {
        StartUI,
        EndUI,
        PauseUI,
        EndingPanel,
        emotionalKnot,
        LoadPanel,
        dialogUI
}

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private Dictionary<UIPanels, GameObject> UIObjects = new Dictionary<UIPanels, GameObject>();

        private void Start()
        {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        UIPanelData[] panels = FindObjectsOfType<UIPanelData>();
            foreach (UIPanelData data in panels)
            {
            if (!UIObjects.ContainsKey(data.panelType))
            {
                UIObjects.Add(data.panelType, data.gameObject);
            }
        }
        }

        public void OpenThePanel(int panelInt)
        {
            UIPanels panel = (UIPanels)panelInt;
            if (!UIObjects[panel]) { return; }
            bool NeedPause = UIObjects[panel].GetComponent<UIPanelData>().NeedPause;
            if (NeedPause)
            {
                Time.timeScale = 0;
            }
            UIObjects[panel].SetActive(true);
        }

        public void CloseThePanel(int panelInt)
        {
            UIPanels panel = (UIPanels)panelInt;
            if (!UIObjects[panel]) { return; }
            bool NeedPause = UIObjects[panel].GetComponent<UIPanelData>().NeedPause;
            UIObjects[panel].SetActive(false);
            if (NeedPause)
            {
                Time.timeScale = 1;
            }
        }
    }


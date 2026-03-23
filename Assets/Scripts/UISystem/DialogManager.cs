using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct aDialog
{
    public Sprite personImage;
    public string name;
    public string dialog;
}

public class DialogManager : MonoBehaviour
{  
    [Header("UI References")]
    [SerializeField] private Button next;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField]private Text Name;
    [SerializeField]private Text Dialog;
    [SerializeField]private Image PersonImage;
    [Header("Dialog Data")]
    public aDialog[] dialogs;
    private int currentDialogIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        dialogPanel.SetActive(false);
        next.onClick.AddListener(ShowNextDialog);
    }

    private void OnEnable()
    {
        Dialog.text = dialogs[currentDialogIndex].dialog;
        Name.text = dialogs[currentDialogIndex].name;
        PersonImage.sprite = dialogs[currentDialogIndex].personImage;
    }

    private void ShowNextDialog() {
        if (currentDialogIndex >= dialogs.Length) {
            currentDialogIndex = 0;
            UIManager.Instance.CloseThePanel((int)dialogPanel.GetComponent<UIPanelData>().panelType);
        }
        else {
            currentDialogIndex++;
            Dialog.text = dialogs[currentDialogIndex].dialog;
            Name.text = dialogs[currentDialogIndex].name;
            PersonImage.sprite = dialogs[currentDialogIndex].personImage;
        }
    }
}

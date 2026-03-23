using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    public Image bar;

    public void ClickToChange() {
        bar.GetComponent<BarController>().UpdateValue(100,50);
    }
}

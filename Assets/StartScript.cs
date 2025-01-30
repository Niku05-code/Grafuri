using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartScript : MonoBehaviour
{
    public Button startButton;
    public GameObject selected;
    public Button generateGraphButton;

    public Toggle OrientedToggle;
    public Toggle NeorientedToggle;

    void Start()
    {
        OrientedToggle.onValueChanged.AddListener(OnOrientedToggleChanged);
        NeorientedToggle.onValueChanged.AddListener(OnNeorientedToggleChanged);
    }

    void OnOrientedToggleChanged(bool isChecked)
    {
        if (isChecked)
            NeorientedToggle.isOn = false;
    }

    void OnNeorientedToggleChanged(bool isChecked)
    {
        if (isChecked)
            OrientedToggle.isOn = false;
    }

    public void startFunc()
    {
        RectTransform rt1 = startButton.GetComponent<RectTransform>();
        rt1.anchoredPosition = new Vector2(10000, 0);
        RectTransform rt2 = selected.GetComponent<RectTransform>();
        rt2.anchoredPosition = new Vector2(-960, -540);
        RectTransform rt3 = generateGraphButton.GetComponent<RectTransform>();
        rt3.anchoredPosition = new Vector2(0, -200);
    }

}

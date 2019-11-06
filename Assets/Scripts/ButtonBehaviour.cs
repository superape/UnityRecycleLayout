using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RecycleLayout;

public class ButtonBehaviour : MonoBehaviour
{
    public InputField inputField;
    public Button gotoBtn;
    public Button currentIndexBtn;
    public RecycleLayout.RecycleLayout recycleLayout;
    public static ButtonBehaviour instance;
    // Use this for initialization
    void Start()
    {
        instance = this;
        gotoBtn.onClick.RemoveAllListeners();
        gotoBtn.onClick.AddListener(GotoIndex);

        currentIndexBtn.onClick.RemoveAllListeners();
        currentIndexBtn.onClick.AddListener(GetCurrentIndex);

    }

    void GotoIndex()
    {
        int index = int.Parse(inputField.text);
        recycleLayout.GotoIndex(index, onComplete: () =>
            {
                Debug.LogFormat("goto Index:[{0}]", index);
            });
    }

    void GetCurrentIndex()
    {
        int index = recycleLayout.GetCurrentIndex();
        Debug.LogFormat("current Index:[{0}]", index);
    }
}

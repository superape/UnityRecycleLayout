using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RecycleLayout;

public class ElementTest : RecycleLayoutElement
{
    [SerializeField]
    private Text indexText;
	// Use this for initialization
	void Start () {
		
	}

    public void FillData(int data)
    {
        int index = data;
        indexText.text = index.ToString();
    }
}

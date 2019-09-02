using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RecycleLayout;

public class ElementTest : RecycleLayoutElement
{
    [SerializeField]
    private Text indexText;
    [SerializeField]
    private Button elementBtn;

    private bool selected = false;

    private Vector2 selectedSize = new Vector2(1080, 300);
	// Use this for initialization
	void Start () {
        elementBtn.onClick.RemoveAllListeners();
        elementBtn.onClick.AddListener(OnElementClick);
	}

    private void OnElementClick()
    {
        selected = !selected;
        ButtonBehaviour.instance.recycleLayout.RefreshData();
    }

    public void FillData(int data)
    {
        int index = data;
        indexText.text = index.ToString();
    }

    public override bool TryGetElementSize(out Vector2 size)
    {
        size = selectedSize;
        return selected;
    }
}

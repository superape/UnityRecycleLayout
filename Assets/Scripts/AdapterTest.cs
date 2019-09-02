using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecycleLayout;

public class AdapterTest : RecycleLayoutAdapter
{
    private int dataCount = 100000;

    public override int ElementIndex2PrefabIndex(int elementIndex)
    {
        if (elementIndex < 5)
            return 0;
        else
            return 1;
    }

    public override void Initialize()
    {
        
    }

    public override int GetCount()
    {
        return dataCount;
    }

    public override void FillElementData(ElementBuffer elementBuffer, int elementIndex)
    {
        ((ElementTest)elementBuffer.Element).FillData(elementIndex);
    }

    public override Vector2 GetElementSize(int elementIndex)
    {
        if (elementIndex == 4)
        {
            return new Vector2(1080f, 470f);
        }
        if (elementIndex % 7 == 0)
        {
            return new Vector2(500f, 500f);
        }
        return base.GetElementSize(elementIndex);
    }

    public override void GetAdditionalData(Action onGetExtraDataDone)
    {
        onRequestAdditionalDataDone = onGetExtraDataDone;
        Invoke("RequestAdditionalData", 1f);
    }

    private Action onRequestAdditionalDataDone;

    private void RequestAdditionalData()
    {
        dataCount += 10;
        if (onRequestAdditionalDataDone != null)
        {
            onRequestAdditionalDataDone();
        }
    }
}

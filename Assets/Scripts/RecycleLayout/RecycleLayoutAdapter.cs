using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecycleLayout
{
    public abstract class RecycleLayoutAdapter : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] m_elementPrefabArray;

        private RecycleLayoutElement[] m_prefabElementArray;

        [HideInInspector]
        public RecycleLayout recycleLayout;

        public abstract void Initialize();

        public abstract int GetCount();

        public abstract int ElementIndex2PrefabIndex(int elementIndex);

        public abstract void FillElementData(ElementBuffer elementBuffer, int elementIndex);

        public virtual GameObject GetElementPrefab(int elementIndex)
        { 
            int prefabIndex = ElementIndex2PrefabIndex(elementIndex);
            return m_elementPrefabArray[prefabIndex];
        }

        public virtual Vector2 GetElementSize(int elementIndex)
        {
            int prefabIndex = ElementIndex2PrefabIndex(elementIndex);

            if (m_prefabElementArray == null || m_prefabElementArray.Length == 0)
            {
                m_prefabElementArray = new RecycleLayoutElement[m_elementPrefabArray.Length];
                for (int i = 0; i < m_elementPrefabArray.Length; i++)
                {
                    m_prefabElementArray[i] = m_elementPrefabArray[i].GetComponent<RecycleLayoutElement>();
                    if (m_prefabElementArray[i] == null)
                    {
                        m_prefabElementArray[i] = m_elementPrefabArray[i].AddComponent<RecycleLayoutElement>();
                    }
                }
            }

            return m_prefabElementArray[prefabIndex].elementSize;
        }

        public virtual void ResizeElementBeforeShowing(int elementIndex)
        {
            
        }

        public virtual int ElementPrefabArrayLength
        {
            get
            {
                return m_elementPrefabArray.Length;
            }
        }

        public virtual void GetAdditionalData(Action onGetExtraDataDone)
        {
            
        }

        public virtual void SetPrefabAnchorAndPivot(Vector2 vec)
        {
            for (int i = 0; i < m_elementPrefabArray.Length; i++)
            {
                GameObject go = m_elementPrefabArray[i];
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.anchorMax = vec;
                rect.anchorMin = vec;
                rect.pivot = vec;
            }
        }
    }
}
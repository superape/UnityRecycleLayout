using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecycleLayout
{
    public class RecycleLayoutElement : MonoBehaviour
    {
        [SerializeField]
        public Vector2 elementSize;

        public virtual bool TryGetElementSize(out Vector2 size)
        {
            size = Vector2.zero;
            return false;
        }
    }
}
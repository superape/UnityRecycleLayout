using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace RecycleLayout
{
    public class DynamicRecycleLayout : RecycleLayout
    {
        private bool m_touchingBottom = false;

        public bool TouchingBottom
        {
            get { return m_touchingBottom; }
            set 
            { 
                m_touchingBottom = value; 
            }
        }

        protected override void Awake()
        {
            if (this.Adapter != null && Application.isPlaying)
            {
                Initialize();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            base.ScrollRect.onValueChanged.AddListener(DetectScroll2End);
        }

        private void DetectScroll2End(Vector2 vec2)
        {
            if (base.Vertical)
            {
                if (content.localPosition.y > (content.rect.size.y - ScrollRectTransform.rect.size.y))
                {
                    TouchBottom();
                }
            }
            else if (base.Horizontal)
            {
                if (base.Left2Right)
                {
                    if (content.localPosition.x < -(content.rect.size.x - ScrollRectTransform.rect.size.x))
                    {
                        TouchBottom();
                    }
                }
                else
                {
                    if (content.localPosition.x > (content.rect.size.x - ScrollRectTransform.rect.size.x))
                    {
                        TouchBottom();
                    }
                }
            }
        }

        private void TouchBottom()
        {
            if (!TouchingBottom)
            {
                Debug.LogError("touch bottom!");
                TouchingBottom = true;
                base.Adapter.GetAdditionalData(() =>
                {
                    base.RefreshData(ContentPositionSetting.keep);
                    TouchingBottom = false;
                });
            }
        }
    }
}

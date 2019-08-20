using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace RecycleLayout
{
    public class RecycleLayout : MonoBehaviour
    {
        public Action OnFillDataCompleted;

        [SerializeField]
        protected Vector2 m_spacing;

        public Vector2 Spacing
        {
            get { return m_spacing; }
        }

        [SerializeField]
        protected RectTransform content;

        [SerializeField]
        protected float bufferFactor = 0.5f;

        [SerializeField]
        private bool left2Right = true;

        public bool Left2Right
        {
            get { return left2Right; }
        }

        [SerializeField]
        private RecycleLayoutDirection layoutDirection = RecycleLayoutDirection.Vertical;
        protected bool Horizontal
        {
            get
            {
                return layoutDirection == RecycleLayoutDirection.Horizontal;
            }
        }
        protected bool Vertical
        {
            get
            {
                return layoutDirection == RecycleLayoutDirection.Vertical;
            }
        }

        private ScrollRect m_scrollRect = null;
        protected ScrollRect ScrollRect
        {
            get 
            {
                if (m_scrollRect == null)
                {
                    Image image = gameObject.GetComponent<Image>();
                    if (image == null)
                    {
                        image = gameObject.AddComponent<Image>();
                        image.sprite = null;
                        image.material = null;
                        image.raycastTarget = true;
                    }

                    Mask mask = gameObject.GetComponent<Mask>();
                    if (mask == null)
                    {
                        mask = gameObject.AddComponent<Mask>();
                        mask.showMaskGraphic = false;
                    }

                    m_scrollRect = gameObject.GetComponent<ScrollRect>();
                    if (m_scrollRect == null)
                    {
                        m_scrollRect = gameObject.AddComponent<ScrollRect>();
                    }
                    m_scrollRect.horizontal = this.Horizontal;
                    m_scrollRect.vertical = this.Vertical;
                    m_scrollRect.content = this.content;
                }
                return m_scrollRect; 
            }
        }

        private RectTransform m_scrollRectTransform = null;
        protected RectTransform ScrollRectTransform
        {
            get
            {
                if (m_scrollRectTransform == null)
                {
                    m_scrollRectTransform = ScrollRect.GetComponent<RectTransform>();
                }
                return m_scrollRectTransform; 
            }
        }

        [SerializeField]
        private RecycleLayoutAdapter m_adapter;
        public RecycleLayoutAdapter Adapter
        {
            get
            {
                if (m_adapter == null)
                {
                    m_adapter = gameObject.GetComponent<RecycleLayoutAdapter>();
                }
                return m_adapter;
            }
        }

        private Vector2 viewportSize;

        private Vector2 visualRange;

        private List<ElementBuffer>[] bufferPool;

        protected List<ElementPositionInfo> elementPositionInfos = new List<ElementPositionInfo>();

        private List<int> showingCells = new List<int>();//element index

        private int CellsCount
        {
            get
            {
                return this.Adapter.GetCount();
            }
        }
        
        #region Initialize
        protected virtual void Awake()
        {
            if (this.Adapter != null && Application.isPlaying)
            {
                Initialize();
            }
        }

        protected virtual void Initialize()
        {
            m_adapter.recycleLayout = this;

            Adapter.Initialize();

            SetAnchorAndPivot();

            this.viewportSize = this.ScrollRectTransform.rect.size;

            if (this.Vertical)
            {
                this.visualRange = new Vector2((0f - this.bufferFactor) * this.viewportSize.y, (1f + this.bufferFactor) * this.viewportSize.y);
            }
            else if (this.Horizontal)
            {
                if (this.Left2Right)
                {
                    this.visualRange = new Vector2((0f - this.bufferFactor) * this.viewportSize.x, (1f + this.bufferFactor) * this.viewportSize.x);
                }
                else
                {
                    this.visualRange = new Vector2((-1f - this.bufferFactor) * this.viewportSize.x, (this.bufferFactor) * this.viewportSize.x);
                }
            }

            GenerateCellBuffers();

            CalculateSizeAndCellPos(ContentPositionSetting.gotoBegin);

            this.ScrollRect.onValueChanged.AddListener(FillData);

            FillData(Vector2.zero);

            if (OnFillDataCompleted != null)
            {
                OnFillDataCompleted();
            }
        }
        #endregion

        #region implementation
        private void SetAnchorAndPivot()
        {
            Vector2 vec = Vector2.zero;
            if (this.Left2Right)
            {
                vec = new Vector2(0f, 1f);
            }
            else
            {
                vec = new Vector2(1f, 1f);
            }

            SetScrollRectAnchorAndPivot(vec);
            SetContentAnchorAndPivot(vec);
            this.Adapter.SetPrefabAnchorAndPivot(vec);
        }

        private void SetScrollRectAnchorAndPivot(Vector2 vec)
        {
            ScrollRectTransform.pivot = vec;
            ScrollRectTransform.anchorMin = vec;
            ScrollRectTransform.anchorMax = vec;
        }

        private void SetContentAnchorAndPivot(Vector2 vec)
        {
            content.pivot = vec;
            content.anchorMin = vec;
            content.anchorMax = vec;
        }

        private void GenerateCellBuffers()
        {
            this.bufferPool = new List<ElementBuffer>[Adapter.ElementPrefabArrayLength];
            for (int i = 0; i < Adapter.ElementPrefabArrayLength; i++)
            {
                this.bufferPool[i] = new List<ElementBuffer>();
            }
        }

        public float CalculateSizeAndCellPos(ContentPositionSetting contentPositionSetting = ContentPositionSetting.none)
        {
            this.elementPositionInfos.Clear();
            float length = 0;
            float offsetX = 0;
            float offsetY = 0;

            if (this.Vertical)
            {
                offsetY = 0;
                for (int i = 0; i < this.CellsCount; )
                {
                    //row head size
                    Vector2 rowHeadSize = GetElementSize(i);
                    float cellWidth = rowHeadSize.x;
                    float maxHeightInRow = rowHeadSize.y;
                    float currentHeight = rowHeadSize.y;

                    offsetX = 0;

                    int j = 0;
                    for (j = 0; Mathf.Abs(offsetX) + cellWidth <= this.viewportSize.x && i + j < this.CellsCount; j++)
                    {
                        int elementIndex = i + j;
                        Vector3 localPos = new Vector3(offsetX, -offsetY, 0.0f);
                        this.elementPositionInfos.Add(new ElementPositionInfo(elementIndex, localPos));
                        if (this.Left2Right)
                        {
                            offsetX += cellWidth + Spacing.x;
                        }
                        else
                        {
                            offsetX -= cellWidth + Spacing.x;
                        }
                        if (currentHeight > maxHeightInRow)
                        {
                            maxHeightInRow = currentHeight;
                        }

                        Vector2 nextElementSize = GetElementSize(elementIndex + 1);
                        cellWidth = nextElementSize.x;
                        currentHeight = nextElementSize.y;
                    }
                    offsetY += maxHeightInRow + Spacing.y;
                    i += j;
                }

                length = offsetY;
                if (length < ScrollRectTransform.rect.size.y)
                {
                    length = ScrollRectTransform.rect.size.y;
                }

                content.sizeDelta = new Vector2(this.viewportSize.x, length);

                if (contentPositionSetting == ContentPositionSetting.gotoBegin)
                {
                    content.localPosition = Vector3.zero;
                }
            }
            else if (this.Horizontal)
            {
                offsetX = 0;
                for (int i = 0; i < this.CellsCount;)
                {
                    //colunm head size
                    Vector2 columnHeadSize = GetElementSize(i);
                    float cellHeight = columnHeadSize.y;
                    float maxWidthInColunm = columnHeadSize.x;
                    float currentWidth = columnHeadSize.x;

                    offsetY = 0;
                    int j = 0;
                    for (j = 0; Mathf.Abs(offsetY) + cellHeight <= this.viewportSize.y && i + j < this.CellsCount; j++)
                    {
                        int elementIndex = i + j;
                        Vector3 localPos = new Vector3(offsetX, offsetY, 0.0f);
                        this.elementPositionInfos.Add(new ElementPositionInfo(elementIndex, localPos));
                        offsetY -= cellHeight + Spacing.y;
                        if (currentWidth > maxWidthInColunm)
                        {
                            maxWidthInColunm = currentWidth;
                        }

                        Vector2 nextElementSize = GetElementSize(elementIndex + 1);
                        cellHeight = nextElementSize.y;
                        currentWidth = nextElementSize.x;
                    }
                    if (this.Left2Right)
                    {
                        offsetX += maxWidthInColunm + Spacing.x;
                    }
                    else
                    {
                        offsetX -= maxWidthInColunm + Spacing.x;
                    }
                    i += j;
                }

                length = Mathf.Abs(offsetX);
                if (length < ScrollRectTransform.rect.size.x)
                {
                    length = ScrollRectTransform.rect.size.x;
                }

                content.sizeDelta = new Vector2(length, this.viewportSize.y);

                if (contentPositionSetting == ContentPositionSetting.gotoBegin)
                {
                    content.localPosition = Vector3.zero;
                }
            }

            return length;
        }


        private void FillData(Vector2 vec2)
        {
            RecycleBuffers();
            if (this.Vertical)
            {
                float currentPos = content.localPosition.y;

                for (int i = 0; i < this.elementPositionInfos.Count; i++)
                {
                    ElementPositionInfo elementPositionInfo = elementPositionInfos[i];
                    float distance2Top = -elementPositionInfo.LocalPos.y - currentPos;
                    SetElement(distance2Top, elementPositionInfo);
                }
            }
            else if (this.Horizontal)
            {
                float currentPos = content.localPosition.x;

                for (int i = 0; i < this.elementPositionInfos.Count; i++)
                {
                    ElementPositionInfo elementPositionInfo = elementPositionInfos[i];
                    float distance2Top = elementPositionInfo.LocalPos.x + currentPos;
                    SetElement(distance2Top, elementPositionInfo);
                }
            }
        }

        private void SetElement(float distance2Top, ElementPositionInfo elementPositionInfo)
        {
            if (distance2Top >= this.visualRange.x && distance2Top <= this.visualRange.y && !this.showingCells.Contains(elementPositionInfo.ElementIndex))
            {
                ElementBuffer buffer = GetUsableBuffer(elementPositionInfo.ElementIndex);
                if (buffer != null)
                {
                    buffer.Use(elementPositionInfo);
                    buffer.SetSize(this.GetElementSize(elementPositionInfo.ElementIndex));
                    this.Adapter.FillElementData(buffer, elementPositionInfo.ElementIndex);
                }
            }
        }

        private ElementBuffer GetUsableBuffer(int index)
        {
            int prefabIndex = this.Adapter.ElementIndex2PrefabIndex(index);
            List<ElementBuffer> buffers = this.bufferPool[prefabIndex];
            for (int i = 0; i < buffers.Count; i++)
            {
                if (!buffers[i].IsUsing)
                {
                    this.showingCells.Add(index);
                    return buffers[i];
                }
            }

            GameObject go = GameObject.Instantiate<GameObject>(this.Adapter.GetElementPrefab(index));
            if (gameObject.activeInHierarchy == false)
            {
                go.SetActive(true);
            }
            go.transform.SetParent(content);
            go.transform.localScale = Vector3.one;
            go.SetActive(false);
            ElementBuffer buffer = new ElementBuffer(go);
            this.bufferPool[prefabIndex].Add(buffer);

            this.showingCells.Add(index);
            return buffer;
        }

        private Vector2 GetElementSize(int index)
        {
            return Adapter.GetElementSize(index);
        }

        private void ForceRecycleAllBuffers()
        {
            for (int i = 0; i < this.bufferPool.Length; i++)
            {
                List<ElementBuffer> buffers = this.bufferPool[i];
                for (int j = 0; j < buffers.Count; j++)
                {
                    ElementBuffer buffer = buffers[j];
                    buffer.Recycle();
                    this.showingCells.Remove(buffer.ElementIndex);
                }
            }
        }

        private void RecycleBuffers()
        {
            float currentPos = 0f;
            float distance2Top = 0f;

            if(this.Vertical)
            {
                currentPos = content.localPosition.y;
            }
            else if (this.Horizontal)
            {
                currentPos = content.localPosition.x;
            }

            for (int i = 0; i < this.bufferPool.Length; i++)
            {
                List<ElementBuffer> buffers = this.bufferPool[i];
                for (int j = 0; j < buffers.Count; j++)
                {
                    ElementBuffer buffer = buffers[j];

                    if (this.Vertical)
                    {
                        distance2Top = -buffer.LocalPosition.y - currentPos;
                    }
                    else if (this.Horizontal)
                    {
                        distance2Top = buffer.LocalPosition.x + currentPos;
                    }

                    if (buffer.IsUsing && (distance2Top < this.visualRange.x || distance2Top > this.visualRange.y))
                    {
                        buffer.Recycle();
                        this.showingCells.Remove(buffer.ElementIndex);
                    }
                }
            }
        }
        #endregion

        #region interface
        public virtual GameObject GameobjectInIndex(int index)
        {
            GameObject obj = null;
            for (int i = 0; i < bufferPool.Length; i++)
            {
                List<ElementBuffer> buffers = bufferPool[i];
                for (int j = 0; j < buffers.Count; j++)
                {
                    if (buffers[j].ElementIndex == index && buffers[j].IsUsing)
                    {
                        obj = buffers[j].BufferGameObject;
                        break;
                    }
                }
            }

            return obj;
        }

        public virtual void GotoIndex(int index, float duration = 0.5f, Action onComplete = null)
        {
            if (index > this.CellsCount)
            {
                return;
            }

            ElementPositionInfo element = elementPositionInfos[index];

            if (this.Vertical)
            {
                float height = -element.LocalPos.y;

                if (duration > 0)
                {
                    DOTween.Sequence().Append(content.DOLocalMoveY(height, duration))
                        .AppendCallback(() =>
                        {
                            if (onComplete != null)
                            {
                                onComplete();
                            }
                        }).Play();
                }
                else
                {
                    content.localPosition = new Vector3(content.localPosition.x, height, content.localPosition.z);
                    this.FillData(Vector2.zero);
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                }
            }
            else if (this.Horizontal)
            {
                float width = -element.LocalPos.x;
                if (duration > 0)
                {
                    DOTween.Sequence().Append(content.DOLocalMoveX(width, duration))
                        .AppendCallback(() =>
                        {
                            if (onComplete != null)
                            {
                                onComplete();
                            }
                        }).Play();
                }
                else
                {
                    content.localPosition = new Vector3(width, content.localPosition.y, content.localPosition.z);
                    this.FillData(Vector2.zero);
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                }
            }
        }

        public void RefreshData(ContentPositionSetting contentPositionSetting = ContentPositionSetting.keep, Action onComplete = null)
        {
            Adapter.Initialize();

            ForceRecycleAllBuffers();
            CalculateSizeAndCellPos(contentPositionSetting);
            FillData(Vector2.zero);

            if (onComplete != null)
            {
                onComplete();
            }
        }

        public int GetCurrentIndex()
        {
            int index = int.MaxValue;
            if (this.Vertical)
            {
                float currentPos = content.localPosition.y;

                for (int i = 0; i < bufferPool.Length; i++)
                {
                    List<ElementBuffer> buffers = bufferPool[i];
                    for (int j = 0; j < buffers.Count; j++)
                    {
                        ElementBuffer buffer = buffers[j];
                        if (buffer.IsUsing && buffer.LocalPosition.y <= -currentPos && buffer.ElementIndex < index)
                        {
                            index = buffer.ElementIndex;
                        }
                    }
                }
            }
            else if (this.Horizontal)
            {
                float currentPos = content.localPosition.x;
                for (int i = 0; i < bufferPool.Length; i++)
                {
                    List<ElementBuffer> buffers = bufferPool[i];
                    for (int j = 0; j < buffers.Count; j++)
                    {
                        ElementBuffer buffer = buffers[j];
                        if (buffer.IsUsing)
                        {
                            if (this.Left2Right && buffer.LocalPosition.x >= -currentPos && buffer.ElementIndex < index)
                            {
                                index = buffer.ElementIndex;
                            }
                            else if (!this.Left2Right && buffer.LocalPosition.x <= -currentPos && buffer.ElementIndex < index)
                            {
                                index = buffer.ElementIndex;
                            }
                        }
                    }
                }
            }
            return index;
        }
        #endregion
    }

    public class ElementPositionInfo
    {
        private int elementIndex;

        public int ElementIndex
        {
            get { return elementIndex; }
        }

        private Vector3 localPos;

        public Vector3 LocalPos
        {
            get { return localPos; }
        }

        public ElementPositionInfo(int index, Vector3 localPos)
        {
            this.elementIndex = index;

            this.localPos = localPos;
        }
    }

    public class ElementBuffer
    {
        private int elementIndex;

        public int ElementIndex
        {
            get { return elementIndex; }
        }

        private GameObject bufferGameObject;

        public GameObject BufferGameObject
        {
            get { return bufferGameObject; }
        }

        private RectTransform rectTransform;

        public Vector3 LocalPosition
        {
            get
            {
                return rectTransform.localPosition;
            }
        }

        private RecycleLayoutElement element;

        public RecycleLayoutElement Element
        {
            get { return element; }
        }

        private bool isUsing;

        public bool IsUsing
        {
            get { return isUsing; }
        }

        public ElementBuffer(GameObject bufferGameObject)
        {
            this.elementIndex = -1;
            this.isUsing = false;

            this.bufferGameObject = bufferGameObject;

            this.element = bufferGameObject.GetComponent<RecycleLayoutElement>();
            if (this.element == null)
            {
                this.element = bufferGameObject.AddComponent<RecycleLayoutElement>();
            }

            this.rectTransform = bufferGameObject.GetComponent<RectTransform>();
        }

        public void Use(ElementPositionInfo elementPositionInfo)
        {
            this.isUsing = true;
            this.elementIndex = elementPositionInfo.ElementIndex;
            this.bufferGameObject.SetActive(true);
            this.rectTransform.localPosition = elementPositionInfo.LocalPos;
            this.bufferGameObject.name = elementPositionInfo.ElementIndex.ToString();
            this.rectTransform.SetSiblingIndex(elementPositionInfo.ElementIndex);
        }

        public void Recycle()
        {
            this.isUsing = false;
            this.bufferGameObject.SetActive(false);
        }

        public void SetSize(Vector2 size)
        {
            this.rectTransform.sizeDelta = size;
        }
    }

    public enum RecycleLayoutDirection
    { 
        Horizontal = 0,
        Vertical = 1,
    }

    public enum ContentPositionSetting
    { 
        none = 0,//do nothing
        gotoBegin = 1,//goto begin
        keep = 2,//keep current showing element
    }
}
# UnityRecycleLayout
# 简介
Unity里带缓冲的自动布局控件，支持Horizontal/Vertical/Left2Right/Right2Left，支持动态填充功能。
# 接口
public void GotoIndex(int index, float duration = 0.5f, Action onComplete = null)
在时间duration内滚动到给定的index对应的element。

public int GetCurrentIndex()
获取当前视口内完整展示的最前的element的index。

public ElementBuffer ElementBufferInIndex(int index)
根据给定的index，获取当前视口内相应的ElementBuffer对象。

public void RefreshData(ContentPositionSetting contentPositionSetting = ContentPositionSetting.keep, Action onComplete = null)
根据contentPositionSetting，按照不同的方式刷新布局内的所有element。
    public enum ContentPositionSetting
    { 
        none = 0,//do nothing
        gotoBegin = 1,//goto begin
        keep = 2,//keep current showing element
    }

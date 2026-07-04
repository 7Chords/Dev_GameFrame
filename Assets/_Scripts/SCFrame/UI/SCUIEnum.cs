using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame.UI
{

    /// <summary>
    /// UI 显示层级（全屏 / 叠加 / 顶层 / 节点内部等）。
    /// </summary>
    public enum SCUIShowType
    {
        NONE,
        FULL,// 全屏
        ADDITION,// 叠加在当前栈顶之上
        TOP,// 最顶层展示
        INTERNAL,// UINode 内部的子层级 / 小块
    }

}

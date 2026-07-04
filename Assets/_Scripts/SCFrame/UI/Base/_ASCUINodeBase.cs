using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame.UI
{
    /// <summary>
    /// UI节点抽象基类 - 单纯是逻辑层的东西 view表现都在panel里
    /// </summary>
    public abstract class _ASCUINodeBase
    {
        private bool _m_hasEnterNode;
        private bool _m_hasHideNode;
        private bool _m_hasExitNode;

        protected SCUIShowType _m_showType;
        public bool hasEnterNode { get => _m_hasEnterNode; }
        public bool hasHideNode { get => _m_hasHideNode; }
        public bool hasExitNode { get => _m_hasExitNode; }
        public SCUIShowType showType { get => _m_showType; }

        //进入同类型（normal/addition）节点时是否隐藏当前节点
        public abstract bool needHideWhenEnterNewSameTypeNode { get; }

        //退出同类型（normal/addition）节点时是否显示当前节点
        public abstract bool needShowWhenQuitNewSameTypeNode { get; }

        //该节点是否可以被esc关闭
        public abstract bool canQuitByEsc { get; }

        //该节点是否可以被鼠标右键关闭
        public abstract bool canQuitByMouseRight { get; }

        //该节点在UI列表里是否忽略
        public abstract bool ignoreOnUIList { get; }

        //该节点隐藏是否需要移动到队列底部
        public abstract bool needMoveToBottomWhenHide { get; }

        public _ASCUINodeBase(SCUIShowType _showType)
        {
            _m_showType = _showType;
        }

        public bool EnterNode()
        {
            if (_m_hasEnterNode)
            {
                Debug.LogError(GetNodeName() + "已经Enter,无法再次Enter!!!");
                return false;
            }
            _m_hasEnterNode = true;
            _m_hasHideNode = true;
            OnEnterNode();
            return true;
        }
        public abstract void OnEnterNode();


        public void HideNode()
        {
            if (!_m_hasEnterNode)
            {
                Debug.LogError(GetNodeName() + "没有Enter,无法Hide!!!");
                return;
            }
            _m_hasHideNode = true;
            OnHideNode();
        }
        public abstract void OnHideNode();


        public void ShowNode()
        {
            if (!_m_hasEnterNode)
            {
                Debug.LogError(GetNodeName() + "没有Enter,无法Show!!!");
                return;
            }
            if(!_m_hasHideNode)
            {
                Debug.LogWarning(GetNodeName() + "当前正在show,无法再次Show!!!");
                return;
            }

            _m_hasHideNode = false;
            OnShowNode();
        }

        public abstract void OnShowNode();


        public void QuitNode()
        {
            if (!_m_hasEnterNode)
            {
                Debug.LogError(GetNodeName() + "没有Enter,无法Quit!!!");
                return;
            }
            if (_m_hasExitNode)
                return;
            _m_hasExitNode = true;
            OnQuitNode();
        }
        public abstract void OnQuitNode();


        public Transform GetRootTransform()
        {
            switch(_m_showType)
            {
                case SCUIShowType.FULL:
                    return SCGameMono.instance.fullLayerRoot.transform;
                case SCUIShowType.ADDITION:
                    return SCGameMono.instance.additionLayerRoot.transform;
                case SCUIShowType.TOP:
                    return SCGameMono.instance.topLayerRoot.transform;
                default:
                    Debug.LogError(GetNodeName() + "找不到可以挂载的Canvas节点！");
                    return SCGameMono.instance.mainCanvas.transform;
            }
        }

        //获取节点的名字 全局唯一
        public abstract string GetNodeName();
        //addressable里面的资源名
        public abstract string GetResName();
        //拷贝数据 如果为了方便使用同一个node 但是构造函数传递的数据来源不一样要重新赋值
        //比如我为了方便就写了一个牌堆node 第一次打开传递的是手牌列表 第二次打开的是背包牌列表 要重新覆盖一下数据 因为第二次打开不会重新初始化 只是show 要更新数据来源
        public abstract void CopyData(_ASCUINodeBase _anotherNode);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    public class SCInputListener : Singleton<SCInputListener>
    {

        private int _m_tbsFrameChecker;

        private int _m_tbsFrameInterval;

        /// <summary>为 false 时：玩法层封装输入不响应，且 SCEventListener 不派发指针类订阅（与面板淡入淡出一致）。</summary>
        private bool _m_canInput = true;

        /// <summary>是否应向 SCEventListener 等派发 UI 指针回调（与 SetCanInput 同步）。</summary>
        public bool canInput => _m_canInput;

        public override void OnInitialize()
        {
            SCTaskHelper.instance.AddUpdateListener(updateInput);
            _m_canInput = true;
        }

        public override void OnDiscard()
        {
            SCTaskHelper.instance.RemoveUpdateListener(updateInput);
        }


        private void updateInput()
        {
            if (!_m_canInput)
                return;

            if (_m_tbsFrameChecker < _m_tbsFrameInterval)
            {
                _m_tbsFrameChecker += 1;
                return;
            }
            if (Input.anyKeyDown)
                _m_tbsFrameChecker = 0;

            if (Input.GetKeyDown(KeyCode.Escape))
                UINodeMgr.instance.CloseNodeByEsc();
            if (Input.GetMouseButtonDown(1))
                UINodeMgr.instance.CloseNodeByMouseRight();
        }

        public float GetHorizontalInput()
        {
            if (!_m_canInput)
                return 0;
            return Input.GetAxis("Horizontal");
        }
        public float GetVerticalInput()
        {
            if (!_m_canInput)
                return 0;
            return Input.GetAxis("Vertical");
        }

        public bool GetKeyCodeDown(KeyCode _code)
        {
            if (!_m_canInput)
                return false;
            return Input.GetKeyDown(_code);
        }
        public bool GetKeyCode(KeyCode _code)
        {
            if (!_m_canInput)
                return false;
            return Input.GetKey(_code);
        }

        public void SetCanInput(bool _canInput)
        {
            _m_canInput = _canInput;
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 协程条目：封装一次 Unity 协程的运行与回收。
    /// </summary>
    public class CoroutineItem
    {
        public IEnumerator enumerator { get; private set; }
        public bool isRunning { get; private set; }

        private Coroutine _m_unityCoroutine;
        private Action<string> _m_onComplete;
        private string _m_name;

        public CoroutineItem(IEnumerator _enumerator)
        {
            enumerator = _enumerator;
            isRunning = false;
        }

        /// <summary>由 <see cref="CoroutineContainer"/> 启动包装协程。</summary>
        public void Start(Action<string> _onComplete, string _name)
        {
            if (isRunning || SCTaskHelper.instance == null)
                return;

            _m_onComplete = _onComplete;
            _m_name = _name;
            isRunning = true;
            _m_unityCoroutine = SCTaskHelper.instance.StartCoroutine(RunWrapper());
        }

        private IEnumerator RunWrapper()
        {
            yield return enumerator;
            isRunning = false;
            _m_onComplete?.Invoke(_m_name);
        }

        /// <summary>停止底层 Unity 协程。</summary>
        public void Stop()
        {
            isRunning = false;
            if (_m_unityCoroutine != null && SCTaskHelper.instance != null)
                SCTaskHelper.instance.StopCoroutine(_m_unityCoroutine);

            _m_unityCoroutine = null;
            enumerator = null;
            _m_onComplete = null;
            _m_name = null;
        }
    }
}

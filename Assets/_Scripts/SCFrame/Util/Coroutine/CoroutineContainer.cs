using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 协程容器：同一容器可并行多条协程，通过 <paramref name="_name"/> 区分；
    /// 同名再次 <see cref="Run"/> 会先停止旧协程。销毁时调用 <see cref="KillAll"/>。
    /// </summary>
    public class CoroutineContainer
    {
        private const string DefaultExclusiveName = "exclusive";

        private readonly Dictionary<string, CoroutineItem> _m_items = new Dictionary<string, CoroutineItem>();

        /// <summary>按名称启动协程；同名已存在则先停止再启动。</summary>
        public void Run(IEnumerator _enumerator, string _name)
        {
            if (!validateRunArgs(_enumerator, _name))
                return;

            if (_m_items.ContainsKey(_name))
                Kill(_name);

            startCoroutine(_enumerator, _name);
        }

        /// <summary>按名称启动协程，等价于 <see cref="Run"/>，语义表示「同通道只保留一条」。</summary>
        public void RunExclusive(IEnumerator _enumerator, string _name = DefaultExclusiveName)
        {
            Run(_enumerator, _name);
        }

        /// <summary>按名称停止单条协程。</summary>
        public void Kill(string _name)
        {
            if (string.IsNullOrEmpty(_name) || !_m_items.TryGetValue(_name, out CoroutineItem item))
                return;

            item.Stop();
            _m_items.Remove(_name);
        }

        /// <summary>停止并清空容器内全部协程。</summary>
        public void KillAll()
        {
            foreach (CoroutineItem item in _m_items.Values)
                item.Stop();

            _m_items.Clear();
        }

        /// <summary>指定名称的协程是否仍在容器中。</summary>
        public bool IsRunning(string _name)
        {
            return !string.IsNullOrEmpty(_name) && _m_items.ContainsKey(_name);
        }

        private void startCoroutine(IEnumerator _enumerator, string _name)
        {
            var item = new CoroutineItem(_enumerator);
            _m_items[_name] = item;
            item.Start(onCoroutineComplete, _name);
        }

        private void onCoroutineComplete(string _name)
        {
            if (!string.IsNullOrEmpty(_name))
                _m_items.Remove(_name);
        }

        private bool validateRunArgs(IEnumerator _enumerator, string _name)
        {
            if (string.IsNullOrEmpty(_name))
            {
                Debug.LogError("CoroutineContainer: coroutine name is null or empty!");
                return false;
            }

            if (_enumerator == null)
            {
                Debug.LogError($"CoroutineContainer: enumerator is null! name={_name}");
                return false;
            }

            if (SCTaskHelper.instance == null)
            {
                Debug.LogError($"CoroutineContainer: SCTaskHelper is not ready! name={_name}");
                return false;
            }

            return true;
        }
    }
}

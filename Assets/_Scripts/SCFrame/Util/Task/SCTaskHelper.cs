using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// SCFrame 任务助手：为非 Mono 提供 Update / LateUpdate / FixedUpdate、延迟到下一帧执行；
    /// Unity 协程由本 Mono 承载，业务侧请通过 <see cref="CoroutineContainer"/> 管理。
    /// </summary>
    public class SCTaskHelper : SingletonPersistent<SCTaskHelper>
    {
        private Action _m_updateEvent;
        private Action _m_lateUpdateEvent;
        private Action _m_fixedUpdateEvent;

        /// <summary>延迟到下一帧再执行的委托队列。</summary>
        private Queue<Action> _m_nextUpdateActionQueue;
        private Queue<Action> _m_nextLateUpdateActionQueue;
        private Queue<Action> _m_nextFixedUpdateActionQueue;

        private TweenContainer _m_tweenContainer;

        public override void OnInitialize()
        {
            _m_nextUpdateActionQueue = new Queue<Action>();
            _m_nextLateUpdateActionQueue = new Queue<Action>();
            _m_nextFixedUpdateActionQueue = new Queue<Action>();
            _m_tweenContainer = new TweenContainer();
        }

        public override void OnDiscard()
        {
            _m_updateEvent = null;
            _m_lateUpdateEvent = null;
            _m_fixedUpdateEvent = null;
            _m_nextUpdateActionQueue.Clear();
            _m_nextLateUpdateActionQueue.Clear();
            _m_nextFixedUpdateActionQueue.Clear();
            _m_nextUpdateActionQueue = null;
            _m_nextLateUpdateActionQueue = null;
            _m_nextFixedUpdateActionQueue = null;

            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;

            StopAllCoroutines();
        }


        // =============== 帧回调监听 ===============

        /// <summary>注册 Update 回调。</summary>
        public void AddUpdateListener(Action _action)
        {
            _m_updateEvent += _action;
        }
        /// <summary>移除 Update 回调。</summary>
        public void RemoveUpdateListener(Action _action)
        {
            _m_updateEvent -= _action;
        }

        /// <summary>注册 LateUpdate 回调。</summary>
        public void AddLateUpdateListener(Action _action)
        {
            _m_lateUpdateEvent += _action;
        }
        /// <summary>移除 LateUpdate 回调。</summary>
        public void RemoveLateUpdateListener(Action _action)
        {
            _m_lateUpdateEvent -= _action;
        }

        /// <summary>注册 FixedUpdate 回调。</summary>
        public void AddFixedUpdateListener(Action _action)
        {
            _m_fixedUpdateEvent += _action;
        }
        /// <summary>移除 FixedUpdate 回调。</summary>
        public void RemoveFixedUpdateListener(Action _action)
        {
            _m_fixedUpdateEvent -= _action;
        }

        public void ClearAllUpdateListener()
        {
            _m_updateEvent = null;
        }
        public void ClearAllFixedUpdateListener()
        {
            _m_fixedUpdateEvent = null;
        }
        public void ClearAllLateUpdateListener()
        {
            _m_lateUpdateEvent = null;
        }

        private void Update()
        {
            _m_updateEvent?.Invoke();
            if (_m_nextUpdateActionQueue == null) return;
            executeQueuedActions(_m_nextUpdateActionQueue);
        }
        private void LateUpdate()
        {
            _m_lateUpdateEvent?.Invoke();
            if (_m_nextLateUpdateActionQueue == null) return;
            executeQueuedActions(_m_nextLateUpdateActionQueue);

        }
        private void FixedUpdate()
        {
            _m_fixedUpdateEvent?.Invoke();
            if (_m_nextFixedUpdateActionQueue == null) return;
            executeQueuedActions(_m_nextFixedUpdateActionQueue);
        }

        /// <summary>在下一帧 Update 执行委托。</summary>
        public void DoInNextUpdate(Action _action)
        {
            if (_action != null)
            {
                _m_nextUpdateActionQueue.Enqueue(_action);
            }
        }

        /// <summary>在下一帧 FixedUpdate 执行委托。</summary>
        public void DoInNextFixedUpdate(Action _action)
        {
            if (_action != null)
            {
                _m_nextFixedUpdateActionQueue.Enqueue(_action);
            }
        }

        /// <summary>在下一帧 LateUpdate 执行委托。</summary>
        public void DoInNextLateUpdate(Action _action)
        {
            if (_action != null)
            {
                _m_nextLateUpdateActionQueue.Enqueue(_action);
            }
        }

        public void DoDelay(Action _action, float _delay)
        {
            if (_action == null)
                return;

            Tween tween = DOTween.Sequence().AppendInterval(_delay).OnComplete(() =>
            {
                _action.Invoke();
            });
            _m_tweenContainer?.RegDoTween(tween);

        }

        /// <summary>取出并执行队列中全部委托，然后清空队列。</summary>
        private void executeQueuedActions(Queue<Action> _actionsQueue)
        {
            if (_actionsQueue.Count > 0)
            {
                var actionsToExecute = new List<Action>(_actionsQueue);
                _actionsQueue.Clear();

                foreach (var action in actionsToExecute)
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"SCTaskHelper:Error executing queued action: {ex.Message}");
                    }
                }
            }
        }

    }
}

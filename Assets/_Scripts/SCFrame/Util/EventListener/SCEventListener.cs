using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SCFrame
{
    /// <summary>
    /// 内置事件枚举（与 Unity 事件钩子对应）。
    /// </summary>
    public enum ESCEventType
    {
        ON_MOUSE_ENTER = -10001,
        ON_MOUSE_EXIT = -10002,
        ON_MOUSE_LEFT_CLICK = -10003,
        ON_MOUSE_LEFT_CLICK_DOWN = -10004,
        ON_MOUSE_LEFT_CLICK_UP = -10005,
        ON_MOUSE_RIGHT_CLICK = -10022,
        ON_MOUSE_RIGHT_CLICK_DOWN = -10023,
        ON_MOUSE_RIGHT_CLICK_UP = -10024,
        ON_MOUSE_MIDDLE_CLICK = -10025,
        ON_MOUSE_MIDDLE_CLICK_DOWN = -10026,
        ON_MOUSE_MIDDLE_CLICK_UP = -10027,
        ON_DRAG = -10006,
        ON_BEGIN_DRAG = -10007,
        ON_END_DRAG = -10008,
        ON_COLLISION_ENTER = -10009,
        ON_COLLISION_STAY = -10010,
        ON_COLLISION_EXIT = -10011,
        ON_COLLISION_ENTER_2D = -10012,
        ON_COLLISION_STAY_2D = -10013,
        ON_COLLISION_EXIT_2D = -10014,
        ON_TRIGGER_ENTER = -10015,
        ON_TRIGGER_STAY = -10016,
        ON_TRIGGER_EXIT = -10017,
        ON_TRIGGER_ENTER_2D = -10018,
        ON_TRIGGER_STAY_2D = -10019,
        ON_TRIGGER_EXIT_2D = -10020,
        ON_RELEASE_ADDRESSABLE_ASSET = -10021,

    }

    public interface ISCMouseEvent : 
        IPointerEnterHandler, 
        IPointerExitHandler, 
        IPointerClickHandler, 
        IPointerDownHandler, 
        IPointerUpHandler, 
        IBeginDragHandler, 
        IEndDragHandler, 
        IDragHandler
    { }




    public class SCEventListener : MonoBehaviour, ISCMouseEvent
    {
        // key：事件类型；value：分类型打包器，运行时按类型分派触发。
        private Dictionary<int, ISCEventListenerEventPack> _m_eventPackDic = new Dictionary<int, ISCEventListenerEventPack>();

        /// <summary>
        /// 添加监听（自定义 int 类型）
        /// </summary>
        public void AddListener<T>(int _eventTypeInt, Action<T, object[]> _action, params object[] _args)
        {
            if (_m_eventPackDic.TryGetValue(_eventTypeInt, out ISCEventListenerEventPack info))
            {
                (info as SCEventListenerEventPack<T>).AddListener(_action, _args);
            }
            else
            {
                SCEventListenerEventPack<T> infos = SCPoolMgr.instance.GetObject<SCEventListenerEventPack<T>>();
                infos.AddListener(_action, _args);
                _m_eventPackDic.Add(_eventTypeInt, infos);
            }
        }
        /// <summary>
        /// 添加监听（<see cref="ESCEventType"/>）
        /// </summary>
        public void AddListener<T>(ESCEventType _eventType, Action<T, object[]> _action, params object[] _args)
        {
            AddListener<T>((int)_eventType, _action, _args);
        }

        /// <summary>
        /// 移除监听（自定义 int 类型）
        /// </summary>
        public void RemoveListener<T>(int _eventTypeInt, Action<T, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            if (_m_eventPackDic.TryGetValue(_eventTypeInt, out ISCEventListenerEventPack info))
            {
                (info as SCEventListenerEventPack<T>).RemoveListener(_action, _checkArgs, _args);
            }
        }
        /// <summary>
        /// 移除监听（<see cref="ESCEventType"/>）
        /// </summary>
        public void RemoveListener<T>(ESCEventType _eventType, Action<T, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveListener((int)_eventType, _action, _checkArgs, _args);
        }

        /// <summary>
        /// 移除某一事件类型下的全部监听。
        /// </summary>
        public void RemoveAllListener(int _eventTypeInt)
        {
            if (_m_eventPackDic.TryGetValue(_eventTypeInt, out ISCEventListenerEventPack infos))
            {
                infos.RemoveAll();
                _m_eventPackDic.Remove(_eventTypeInt);
            }
        }
        /// <summary>
        /// 移除某一事件类型下的全部监听。
        /// </summary>
        public void RemoveAllListener(ESCEventType _eventType)
        {
            RemoveAllListener((int)_eventType);
        }
        /// <summary>
        /// 移除全部事件的监听。
        /// </summary>
        public void RemoveAllListener()
        {
            foreach (ISCEventListenerEventPack infos in _m_eventPackDic.Values)
            {
                infos.RemoveAll();
            }

            _m_eventPackDic.Clear();
        }

        /// <summary>
        /// 触发事件（自定义 int 类型）
        /// </summary>
        public void TriggerAction<T>(int _eventTypeInt, T _eventData)
        {
            if (_m_eventPackDic.TryGetValue(_eventTypeInt, out ISCEventListenerEventPack infos))
            {
                (infos as SCEventListenerEventPack<T>).TriggerEvent(_eventData);
            }
        }
        /// <summary>
        /// 触发事件（<see cref="ESCEventType"/>）
        /// </summary>
        public void TriggerAction<T>(ESCEventType _eventType, T _eventData)
        {
            TriggerAction<T>((int)_eventType, _eventData);
        }

        #region UI
        /// <summary>与 <see cref="SCInputListener.canInput"/> 一致，淡入淡出等场景下不派发指针订阅。</summary>
        private static bool CanDispatchUIPointerSubscriptions()
        {
            return SCInputListener.instance.canInput;
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            TriggerAction(ESCEventType.ON_MOUSE_ENTER, _eventData);
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            TriggerAction(ESCEventType.ON_MOUSE_EXIT, _eventData);
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if (!CanDispatchUIPointerSubscriptions())
                return;
            switch (_eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    TriggerAction(ESCEventType.ON_MOUSE_LEFT_CLICK, _eventData);
                    break;
                case PointerEventData.InputButton.Right:
                    TriggerAction(ESCEventType.ON_MOUSE_RIGHT_CLICK, _eventData);
                    break;
                case PointerEventData.InputButton.Middle:
                    TriggerAction(ESCEventType.ON_MOUSE_MIDDLE_CLICK, _eventData);
                    break;
            }
        }

        public void OnPointerDown(PointerEventData _eventData)
        {
            if (!CanDispatchUIPointerSubscriptions())
                return;
            switch (_eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    TriggerAction(ESCEventType.ON_MOUSE_LEFT_CLICK_DOWN, _eventData);
                    break;
                case PointerEventData.InputButton.Right:
                    TriggerAction(ESCEventType.ON_MOUSE_RIGHT_CLICK_DOWN, _eventData);
                    break;
                case PointerEventData.InputButton.Middle:
                    TriggerAction(ESCEventType.ON_MOUSE_MIDDLE_CLICK_DOWN, _eventData);
                    break;
            }
        }

        public void OnPointerUp(PointerEventData _eventData)
        {
            if (!CanDispatchUIPointerSubscriptions())
                return;
            switch (_eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    TriggerAction(ESCEventType.ON_MOUSE_LEFT_CLICK_UP, _eventData);
                    break;
                case PointerEventData.InputButton.Right:
                    TriggerAction(ESCEventType.ON_MOUSE_RIGHT_CLICK_UP, _eventData);
                    break;
                case PointerEventData.InputButton.Middle:
                    TriggerAction(ESCEventType.ON_MOUSE_MIDDLE_CLICK_UP, _eventData);
                    break;
            }
        }

        public void OnBeginDrag(PointerEventData _eventData)
        {
            if (!CanDispatchUIPointerSubscriptions())
                return;
            TriggerAction(ESCEventType.ON_BEGIN_DRAG, _eventData);
        }

        public void OnEndDrag(PointerEventData _eventData)
        {
            if (!CanDispatchUIPointerSubscriptions())
                return;
            TriggerAction(ESCEventType.ON_END_DRAG, _eventData);
        }

        public void OnDrag(PointerEventData _eventData)
        {
            if (!CanDispatchUIPointerSubscriptions())
                return;
            TriggerAction(ESCEventType.ON_DRAG, _eventData);
        }


        #endregion

        #region Collision
        private void OnCollisionEnter(Collision collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_ENTER, collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_STAY, collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_EXIT, collision);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_ENTER_2D, collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_STAY_2D, collision);
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_EXIT_2D, collision);
        }
        #endregion

        #region Trigger
        private void OnTriggerEnter(Collider other)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_ENTER, other);
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_STAY, other);
        }
        private void OnTriggerExit(Collider other)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_EXIT, other);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_ENTER_2D, collision);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_STAY_2D, collision);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_EXIT_2D, collision);
        }
        #endregion

        #region 资源
        private void OnDestroy()
        {
            TriggerAction(ESCEventType.ON_RELEASE_ADDRESSABLE_ASSET, gameObject);
        }
        #endregion
    }
}

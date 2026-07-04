using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    public class SCPoolMgr : Singleton<SCPoolMgr>
    {
        private GameObject _m_poolRoot;
        private bool _m_poolRootAutoCreated;

        /// <summary>
        /// GameObject对象容器
        /// </summary>
        private Dictionary<string, GameObjectPoolData> _m_gameObjectPoolDic;

        /// <summary>
        /// 普通类 对象容器
        /// </summary>
        private Dictionary<string, ObjectPoolData> _m_objectPoolDic;

        public override void OnInitialize()
        {
            _m_gameObjectPoolDic = new Dictionary<string, GameObjectPoolData>();
            _m_objectPoolDic = new Dictionary<string, ObjectPoolData>();
            EnsurePoolRoot();
        }

        public override void OnDiscard()
        {
            if (_m_gameObjectPoolDic != null)
            {
                _m_gameObjectPoolDic.Clear();
                _m_gameObjectPoolDic = null;
            }

            if (_m_objectPoolDic != null)
            {
                _m_objectPoolDic.Clear();
                _m_objectPoolDic = null;
            }

            if (_m_poolRootAutoCreated && _m_poolRoot != null)
            {
                SCCommon.DestoryGameObject(_m_poolRoot);
                _m_poolRoot = null;
                _m_poolRootAutoCreated = false;
            }
        }

        private void EnsurePoolRoot()
        {
            if (_m_poolRoot != null)
                return;

            if (SCGameMono.instance != null && SCGameMono.instance.poolRoot != null)
            {
                _m_poolRoot = SCGameMono.instance.poolRoot;
                return;
            }

            if (SCGameMono.instance == null)
                return;

            _m_poolRoot = new GameObject("PoolRoot");
            _m_poolRoot.transform.SetParent(SCGameMono.instance.transform);
            _m_poolRootAutoCreated = true;
        }

        #region GameObject对象相关操作

        /// <summary>
        /// 获取GameObject,但是如果没有则返回Null
        /// </summary>
        public GameObject GetGameObject(string _assetName, GameObject _prefab = null, Transform _parent = null)
        {
            GameObject obj = null;
            if (!_m_gameObjectPoolDic.ContainsKey(_assetName))
            {
                for (int i = 0; i < SCConst.POOL_INIT_SPAWN_AMOUNT; i++)
                {
                    obj = _parent != null
                        ? SCCommon.InstantiateGameObject(_prefab, _parent)
                        : SCCommon.InstantiateGameObject(_prefab);
                    PushGameObject(obj);
                }

                return GetGameObject(_assetName);
            }

            if (_m_gameObjectPoolDic.TryGetValue(_assetName, out GameObjectPoolData poolData) &&
                poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(_parent);
            }

            return obj;
        }

        /// <summary>
        /// GameObject放进对象池
        /// </summary>
        public void PushGameObject(GameObject _obj)
        {
            EnsurePoolRoot();
            string name = _obj.name.Replace("(Clone)", "");
            if (_m_gameObjectPoolDic.TryGetValue(name, out GameObjectPoolData poolData))
            {
                poolData.PushObj(_obj);
            }
            else
            {
                _m_gameObjectPoolDic.Add(name, new GameObjectPoolData(_obj, _m_poolRoot));
            }
        }

        #endregion

        #region 普通对象相关操作

        /// <summary>
        /// 获取普通对象
        /// </summary>
        public T GetObject<T>() where T : class, new()
        {
            T obj;
            if (CheckObjectCache<T>())
            {
                string name = typeof(T).FullName;
                obj = (T)_m_objectPoolDic[name].GetObj();
                return obj;
            }

            for (int i = 0; i < SCConst.POOL_INIT_SPAWN_AMOUNT; i++)
            {
                obj = new T();
                PushObject(obj);
            }

            return GetObject<T>();
        }

        /// <summary>
        /// 普通对象放进对象池
        /// </summary>
        public void PushObject(object _obj)
        {
            string name = _obj.GetType().FullName;
            if (_m_objectPoolDic.ContainsKey(name))
            {
                _m_objectPoolDic[name].PushObj(_obj);
            }
            else
            {
                _m_objectPoolDic.Add(name, new ObjectPoolData(_obj));
            }
        }

        private bool CheckObjectCache<T>()
        {
            string name = typeof(T).FullName;
            return _m_objectPoolDic.ContainsKey(name) && _m_objectPoolDic[name].poolQueue.Count > 0;
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除全部
        /// </summary>
        public void Clear(bool _clearGameObject = true, bool _clearCObject = true)
        {
            if (_clearGameObject && _m_poolRoot != null)
            {
                for (int i = _m_poolRoot.transform.childCount - 1; i >= 0; i--)
                {
                    SCCommon.DestoryGameObject(_m_poolRoot.transform.GetChild(i).gameObject);
                }

                _m_gameObjectPoolDic.Clear();
            }

            if (_clearCObject)
            {
                _m_objectPoolDic.Clear();
            }
        }

        public void ClearAllGameObject()
        {
            Clear(true, false);
        }

        public void ClearGameObject(string _prefabName)
        {
            if (_m_poolRoot == null)
                return;

            Transform child = _m_poolRoot.transform.Find(_prefabName);
            if (child != null)
            {
                SCCommon.DestoryGameObject(child.gameObject);
                _m_gameObjectPoolDic.Remove(_prefabName);
            }
        }

        public void ClearGameObject(GameObject _prefab)
        {
            ClearGameObject(_prefab.name);
        }

        public void ClearAllObject()
        {
            Clear(false, true);
        }

        public void ClearObject<T>()
        {
            _m_objectPoolDic.Remove(typeof(T).FullName);
        }

        public void ClearObject(Type _type)
        {
            _m_objectPoolDic.Remove(_type.FullName);
        }

        #endregion
    }
}

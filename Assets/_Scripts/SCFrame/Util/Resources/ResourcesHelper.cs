using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SCFrame
{
    /// <summary>
    /// SCFrame 资源加载辅助（Addressables同步 / 异步加载）
    /// </summary>
    public static class ResourcesHelper
    {

        /// <summary>
        /// 同步加载 Unity 资产（AudioClip / Sprite / 预制等）。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadAsset<T>(string _assetName) where T : UnityEngine.Object
        {
            try
            {
                return Addressables.LoadAssetAsync<T>(_assetName).WaitForCompletion();
            }
            catch(Exception ex)
            {
                Debug.LogError("ResourcesHelper 同步加载资产失败：" + ex);
                return null;
            }
        }



        /// <summary>
        /// 实例化场景物体（可选父节点）。
        /// </summary>
        /// <param name="_assetName">Addressable 资产名（Key）</param>
        /// <param name="_parent">父物体</param>
        /// <param name="_automaticRelease">销毁时在 <see cref="AddReleaseAddressableAsset"/> 中自动触发一次 Release</param>
        /// <returns></returns>
        public static GameObject LoadGameObject(string _assetName, Transform _parent = null, bool _automaticRelease = true)
        {
            try
            {
                GameObject go = null;
                go = Addressables.InstantiateAsync(_assetName, _parent).WaitForCompletion();
                if (_automaticRelease)
                {
                    go.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                go.name = _assetName;
                return go;
            }
            catch(Exception ex)
            {
                Debug.LogError("ResourcesHelper 加载游戏物体失败" + ex);
                return null;
            }
        }

        /// <summary>
        /// 在指定位置、旋转实例化场景物体。
        /// </summary>
        /// <param name="_assetName">Addressable Key</param>
        /// <param name="_position">世界坐标</param>
        /// <param name="_quaternion">旋转</param>
        /// <param name="_automaticRelease">销毁时自动 Release</param>
        /// <returns></returns>
        public static GameObject LoadGameObject(string _assetName,Vector3 _position, Quaternion _quaternion , bool _automaticRelease = true)
        {
            try
            {
                GameObject go = null;
                go = Addressables.InstantiateAsync(_assetName, _position, _quaternion).WaitForCompletion();
                if (_automaticRelease)
                {
                    go.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                go.name = _assetName;
                return go;
            }
            catch(Exception ex)
            {
                Debug.LogError("ResourcesHelper加载游戏物体失败" + ex);
                return null;
            }
        }

        /// <summary>
        /// 自动释放绑定：监听 <see cref="ESCEventType.ON_RELEASE_ADDRESSABLE_ASSET"/>，回调里 ReleaseInstance。
        /// </summary>
        private static void AutomaticReleaseAssetAction(GameObject _obj, object[] _arg2)
        {
            Addressables.ReleaseInstance(_obj);
        }


        /// <summary>
        /// 同步加载并获取组件（先实例 GameObject）。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="_parent"></param>
        /// <returns></returns>
        public static T Load<T>(string _assetName, Transform _parent = null) where T : Component
        {
            return LoadGameObject(_assetName, _parent).GetComponent<T>(); ;
        }

        /// <summary>
        /// 异步加载带组件的预制实例。
        /// </summary>
        /// <typeparam name="T">返回组件类型</typeparam>
        public static void LoadGameObjectAsync<T>(string _assetName, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {

            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectAsync<T>(_assetName, _callBack, _parent));
        }

        public static void LoadGameObjectAsync<T>(string _assetName, Vector3 _pos,Quaternion _rotate, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {

            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectAsync<T>(_assetName, _pos, _rotate,_callBack, _parent));

        }
        static IEnumerator DoLoadGameObjectAsync<T>(string _assetName, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _parent);
            yield return request;
            _callBack?.Invoke(request.Result.GetComponent<T>());
        }
        static IEnumerator DoLoadGameObjectAsync<T>(string _assetName, Vector3 _pos,Quaternion _rotate, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _parent);
            yield return request;
            request.Result.transform.position = _pos;
            request.Result.transform.rotation = _rotate;
            _callBack?.Invoke(request.Result.GetComponent<T>());
        }

        /// <summary>
        /// 异步加载单一 Addressable 资产（AudioClip / Sprite / GameObject 等）。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="_callBack"></param>
        public static void LoadAssetAsync<T>(string _assetName, Action<T> _callBack) where T : UnityEngine.Object
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadAssetAsync<T>(_assetName, _callBack));
        }

        static IEnumerator DoLoadAssetAsync<T>(string _assetName, Action<T> _callBack) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> request = Addressables.LoadAssetAsync<T>(_assetName);
            yield return request;
            _callBack?.Invoke(request.Result);
        }

        /// <summary>
        /// 同步按 Label/Key 加载一组资产。
        /// </summary>
        public static IList<T> LoadAssets<T>(string _keyName, Action<T> _callBack = null)
        {
            return Addressables.LoadAssetsAsync<T>(_keyName, _callBack).WaitForCompletion();
        }

        /// <summary>
        /// 异步加载指定 Key 下的一组资产。
        /// </summary>
        public static void LoadAssetsAsync<T>(string _keyName, Action<IList<T>> _callBack = null, Action<T> _callBackOnEveryOne = null)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadAssetsAsync<T>(_keyName, _callBack, _callBackOnEveryOne));
        }

        static IEnumerator DoLoadAssetsAsync<T>(string _keyName, Action<IList<T>> _callBack = null, Action<T> _callBackOnEveryOne = null)
        {
            AsyncOperationHandle<IList<T>> request = Addressables.LoadAssetsAsync<T>(_keyName, _callBackOnEveryOne);
            yield return request;
            _callBack?.Invoke(request.Result);
        }
        /// <summary>
        /// 异步实例化 GameObject，完成回调直接拿到实例（不取组件）。
        /// </summary>
        /// <param name="_assetName">Addressable Key</param>
        /// <param name="_parent">父节点</param>
        /// <param name="_automaticRelease">销毁时自动释放</param>
        /// <param name="_callBack">完成回调（成功时返回 GameObject）</param>
        public static void LoadGameObjectDirectAsync(string _assetName, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectDirectAsync(_assetName, _callBack, _parent, _automaticRelease));
        }

        /// <summary>
        /// 异步实例化（带位置、旋转），回调直接返回 GameObject。
        /// </summary>
        /// <param name="_assetName">Addressable Key</param>
        /// <param name="_pos">位置</param>
        /// <param name="_rot">旋转</param>
        /// <param name="_callBack">完成回调</param>
        /// <param name="_parent">父节点</param>
        /// <param name="_automaticRelease">销毁时自动释放</param>
        public static void LoadGameObjectDirectAsync(string _assetName, Vector3 _pos, Quaternion _rot, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectDirectAsync(_assetName, _pos, _rot, _callBack, _parent, _automaticRelease));
        }

        /// <summary>
        /// 协程：直接实例化并回调 GameObject。
        /// </summary>
        private static IEnumerator DoLoadGameObjectDirectAsync(string _assetName, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _parent);
            yield return request;

            GameObject resultGO = null;
            if (request.Status == AsyncOperationStatus.Succeeded && request.Result != null)
            {
                resultGO = request.Result;
                if (_automaticRelease)
                {
                    resultGO.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                resultGO.name = _assetName;
            }
            else
            {
                Debug.LogError($"直接加载 GameObject 失败，资产名「{_assetName}」，异常：{request.OperationException?.Message}");
            }

            _callBack?.Invoke(resultGO);
        }

        /// <summary>
        /// 协程：带 Transform 的直接实例化。
        /// </summary>
        private static IEnumerator DoLoadGameObjectDirectAsync(string _assetName, Vector3 _pos, Quaternion _rot, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _pos, _rot, _parent);
            yield return request;

            GameObject resultGO = null;
            if (request.Status == AsyncOperationStatus.Succeeded && request.Result != null)
            {
                resultGO = request.Result;
                if (_automaticRelease)
                {
                    resultGO.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                resultGO.name = _assetName;

            }
            else
            {
                Debug.LogError($"直接加载 GameObject 失败，资产名「{_assetName}」，异常：{request.OperationException?.Message}");
            }

            _callBack?.Invoke(resultGO);
        }
        public static void Release<T>(T _obj)
        {
            Addressables.Release<T>(_obj);
        }
        /// <summary>
        /// 释放实例化对象（ReleaseInstance）。
        /// </summary>
        public static bool ReleaseInstance(GameObject _obj)
        {
            return Addressables.ReleaseInstance(_obj);
        }
    }
}

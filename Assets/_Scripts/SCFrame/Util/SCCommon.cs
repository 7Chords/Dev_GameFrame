using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using System;
using SCFrame.RefData;

namespace SCFrame
{
    /// <summary>
    /// 框架通用方法
    /// </summary>
    public static class SCCommon
    {

        /// <summary>
        /// 设置物体显隐
        /// </summary>
        /// <param name="_obj"></param>
        /// <param name="_isEnable"></param>
        public static void SetGameObjectEnable(GameObject _obj, bool _isEnable)
        {
            if (_obj == null)
                return;
            _obj.SetActive(_isEnable);
        }

        /// <summary>
        /// 设置物体显隐
        /// </summary>
        /// <param name="_objs"></param>
        /// <param name="_isEnable"></param>
        public static void SetGameObjectEnable(GameObject[] _objs, bool _isEnable)
        {
            if (_objs == null || _objs.Length == 0)
                return;

            foreach(var go in _objs)
                go.SetActive(_isEnable);
        }

        /// <summary>
        /// 设置物体显隐
        /// </summary>
        /// <param name="_objs"></param>
        /// <param name="_isEnable"></param>
        public static void SetGameObjectEnable(List<GameObject> _objs, bool _isEnable)
        {
            if (_objs == null || _objs.Count == 0)
                return;

            foreach (var go in _objs)
                go.SetActive(_isEnable);
        }

        /// <summary>
        /// 生成物体
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        public static GameObject InstantiateGameObject(GameObject _obj)
        {
            if (_obj == null)
                return null;
            GameObject go = GameObject.Instantiate(_obj);
            return go;
        }

        /// <summary>
        /// 生成物体（传入父物体生成时就设置）
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        public static GameObject InstantiateGameObject(GameObject _obj,Transform _parent)
        {
            if (_obj == null)
                return null;
            GameObject go = GameObject.Instantiate(_obj, _parent);
            return go;
        }

        /// <summary>
        /// 销毁物体
        /// </summary>
        /// <param name="_obj"></param>
        public static void DestoryGameObject(GameObject _obj)
        {
            if (_obj == null)
                return;
            GameObject.Destroy(_obj);
        }


#if UNITY_EDITOR
        /// <summary>
        /// 是否处于预制体状态下
        /// </summary>
        /// <returns></returns>
        public static bool IsInPrefabStage()
        {
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            return stage != null;
        }

#endif

        /// <summary>
        /// 解析float
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        public static float ParseFloat(string _str)
        {
            if(float.TryParse(_str, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }
            Debug.LogError("string 无法解析为 float：" + _str);
            return 0f;
        }

        /// <summary>
        /// 解析int
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        public static int ParseInt(string _str)
        {
            if(int.TryParse(_str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            Debug.LogError("string 无法解析为 int：" + _str);
            return 0;
        }


        public static object ParseEnum(string _str,Type _enumType)
        {
            if (string.IsNullOrEmpty(_str))
            {
                Debug.LogError("_str 为空或无效");
                return 0;
            }

            object obj = Enum.Parse(_enumType, _str);
            return obj;
        }

        /// <summary>
        /// 解析long
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        public static long ParseLong(string _str)
        {
            if (long.TryParse(_str, NumberStyles.Number, CultureInfo.InvariantCulture, out long result))
            {
                return result;
            }
            Debug.LogError("string 无法解析为 long：" + _str);
            return 0L;
        }


        public static List<T> ParseList<T>(string _name, bool _canNull = true)
        {
            List<T> list = new List<T>();

            string tempValue = _name;
            if (tempValue == "*")
                return list;

            string[] strs = tempValue.Split(new char[] { ';' });
            for (var i = 0; i < strs.Length; i++)
            {
                string tempStr = strs[i];
                object value = ParseValue(tempStr, typeof(T));
                if (value == null)
                {
                    continue;
                }
                else
                {
                    list.Add((T)value);
                }
            }


            return list;
        }

        public static object ParseValue(string _value, Type _type)
        {
            try
            {
                if (_value.Equals(string.Empty))
                {
                    if (_type == typeof(string))
                    {
                        return "";
                    }
                    return Activator.CreateInstance(_type, true);
                }
                else
                {
                    _value = _value.Trim();

                    // 枚举
                    if (_type.IsEnum)
                    {
                        return Enum.Parse(_type, _value);
                    }

                    // 字符串
                    else if (_type == typeof(string))
                    {
                        return _value;
                    }

                    // float
                    else if (_type == typeof(float))
                    {
                        if (_value == "0" || _value == "" || _value == string.Empty)
                            return 0f;

                        return float.Parse(_value, CultureInfo.InvariantCulture);
                    }

                    // int
                    else if (_type == typeof(int))
                    {
                        if (_value == "")
                            return 0;

                        return int.Parse(_value);
                    }

                    else if (_type == typeof(bool))
                    {
                        return bool.Parse(_value);
                    }

                    else if (_type == typeof(long))
                    {
                        return long.Parse(_value);
                    }
                    else if (_type.IsSubclassOf(typeof(_AEffectObjBase)))
                    {
                        return ParseEffectObj(_value, _type);
                    }
                    else if(_type == typeof(object))
                    {
                        return _value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseValue type:{_type.ToString()}, value:{_value}, failed: {ex}");
            }
            return null;
        }

        /// <summary>
        /// 解析效果obj
        /// </summary>
        /// <param name="_str"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static _AEffectObjBase ParseEffectObj(string _str, Type _type)
        {
            if (string.IsNullOrEmpty(_str))
                return null;
            if (_type == null || !_type.IsSubclassOf(typeof(_AEffectObjBase)))
                return null;

            try
            {
                _AEffectObjBase effectObj = Activator.CreateInstance(_type) as _AEffectObjBase;
                effectObj?.Deserialize(_str);
                return effectObj;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseEffectObj failed. type:{_type}, value:{_str}, ex:{ex}");
                return null;
            }

        }
    }

}

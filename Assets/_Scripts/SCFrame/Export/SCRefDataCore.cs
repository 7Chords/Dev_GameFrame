using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SCFrame.RefData
{
    /// <summary>
    /// 配表基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SCRefDataCore
    {
        private string _m_assetPath;// 资源相对路径
        private string _m_sheetName;// table名
        public SCRefDataCore()
        {

        }

        public SCRefDataCore(string _assetPath, string _sheetName)
        {
            _m_assetPath = _assetPath;
            _m_sheetName = _sheetName;
        }

        /// <summary>资源根目录下的导出表相对路径。</summary>

        private Dictionary<string, string> _m_keyValueMap = new Dictionary<string, string>();

        private const string EXCEL_EMPTY_FLAG = "*";

        /// <summary>表头以该前缀开头：整列注释，Excel 导出时跳过。</summary>
        public const string MEMO_COLUMN_PREFIX = "#";

        /// <summary>表头下一行单元格以该前缀开头：竖向注释格，导出保留，解析时跳过整行。</summary>
        public const string MEMO_CELL_PREFIX = "~";

        public void readFromTxt()
        {
            if (string.IsNullOrEmpty(_m_assetPath) || string.IsNullOrEmpty(_m_sheetName))
            {
                Debug.LogError(_m_assetPath + "或" + _m_sheetName + "为空");
                return;
            }
            string resourcesPath = "Assets/Resources/RefData/ExportTxt/" + _m_sheetName + ".txt";
            string streamingPath = Application.streamingAssetsPath + "/ExportTxt/" + _m_sheetName + ".txt";
            string usePath = null;
            if (File.Exists(resourcesPath))
                usePath = resourcesPath;
            else if (File.Exists(streamingPath))
                usePath = streamingPath;

            if (string.IsNullOrEmpty(usePath))
            {
                Debug.LogError($"配表文件不存在：{resourcesPath} 或 {streamingPath}");
                return;
            }

            using (StreamReader sr = new StreamReader(usePath))
            {
                string str = sr.ReadToEnd();
                parseFromTxt(str);
            }
        }
        protected void parseFromTxt(string _string)
        {
            if (string.IsNullOrEmpty(_string))
            {
                Debug.LogError("txt 内容为空");
                return;
            }

            _m_keyValueMap.Clear();
            string[] lineArray = _string.Split('\n');
            for (int i = 1; i < lineArray.Length; i++)
            {
                string line = lineArray[i].TrimEnd('\r');
                if (string.IsNullOrEmpty(line) || line == "\t")
                {
                    continue;
                }
                if (isMemoRow(line))
                    continue;

                string[] lineSplit = line.Split('\t');
                if (lineSplit.Length < 2)
                {
                    Debug.LogError($"general 表格格式错误：表 {_m_sheetName} 第{i} 行：\n{line}");
                    continue;
                }
                try
                {
                    _m_keyValueMap.Add(lineSplit[0].Trim(), lineSplit[1].Trim());
                }
                catch(Exception ex)
                {
                    Debug.LogError($"general 表格重复 key：表 {_m_sheetName} 添加字典出错，key：" + lineSplit[0] + ",value:" +lineSplit[1]+
                        "----"+ex) ;
                }
            }

            try
            {
                _parseFromString();
            }
            catch (Exception e)
            {
                Debug.LogError($"SCRefDataCore:{e}");
                return;
            }
        }
        public void singleParseFormTxt(string _keys,string _values)
        {
            if (string.IsNullOrEmpty(_keys) || string.IsNullOrEmpty(_values))
                return;

            string[] keySplitArr = _keys.Split("\t");
            string[] valSplitArr = _values.Split("\t");

            if(keySplitArr.Length != valSplitArr.Length)
            {
                Debug.LogError(_m_sheetName + " 要求的表头列数与实际数据列数不一致！");
                return;
            }
            _m_keyValueMap.Clear();
            for (int i =0;i<keySplitArr.Length;i++)
            {
                string key = keySplitArr[i]?.Trim();
                if (string.IsNullOrEmpty(key))
                    continue;

                _m_keyValueMap[key] = valSplitArr[i]?.Trim();
            }
            //_parseFromString();
            try
            {
                _parseFromString();
            }
            catch (Exception e)
            {
                Debug.LogError($"SCRefDataCore:{e}");
                return;
            }
        }
        protected abstract void _parseFromString();

        protected static bool isMemoValue(string _value)
        {
            return !string.IsNullOrEmpty(_value) && _value.StartsWith(MEMO_CELL_PREFIX);
        }

        /// <summary>表头下一行的竖向注释行：非空单元格均以 ~ 开头。</summary>
        public static bool isMemoRow(string _line)
        {
            if (string.IsNullOrWhiteSpace(_line))
                return false;

            string[] cells = _line.Split('\t');
            bool hasContent = false;
            for (int i = 0; i < cells.Length; i++)
            {
                string cell = cells[i]?.Trim();
                if (string.IsNullOrEmpty(cell))
                    continue;

                hasContent = true;
                if (!isMemoValue(cell))
                    return false;
            }
            return hasContent;
        }

        protected static bool isMemoColumn(string _key)
        {
            return !string.IsNullOrEmpty(_key) && _key.StartsWith(MEMO_COLUMN_PREFIX);
        }

        #region getXXX

        protected string getString(string _key)
        {
            string result;
            if (!_m_keyValueMap.TryGetValue(_key, out result))
            {
                foreach (KeyValuePair<string, string> pair in _m_keyValueMap)
                {
                    if (string.Equals(pair.Key, _key, StringComparison.OrdinalIgnoreCase))
                        return pair.Value;
                }
                Debug.LogError($"_key 未找到：{_key}");
                return null;
            }
            return result;
        }

        protected int getInt(string _name, bool _canNull = false)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue) || tempValue == EXCEL_EMPTY_FLAG)
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return 0;
            }
            if (!int.TryParse(tempValue, out int result))
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 int");
            }
            return result;
        }

        protected object getEnum(string _name,Type _type)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return 0;
            }

            object obj = Enum.Parse(_type, tempValue);
            if (obj == null)
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 enum");
            }
            return obj;
        }

        protected long getLong(string _name, bool _canNull = true)
        {

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return 0;
            }
            if (!long.TryParse(tempValue, out long result))
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 long");
            }
            return result;
        }

        protected bool getBool(string _name, bool _canNull = true)
        {

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return false;
            }
            if (!bool.TryParse(tempValue, out bool result))
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 bool");
            }
            return result;
        }

        protected float getFloat(string _name, bool _canNull = true)
        {

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return 0;
            }
            if (!float.TryParse(tempValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 float");
            }
            return result;
        }

        protected Vector2 getVector2(string _name, bool _canNull = true)
        {
            Vector2 v2 = new Vector2();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return v2;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 2)
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 Vector2");
                return v2;
            }
            v2.Set(SCCommon.ParseFloat(strs[0]), SCCommon.ParseFloat(strs[1]));
            return v2;
        }

        protected Vector3 getVector3(string _name, bool _canNull = true)
        {
            Vector3 v3 = new Vector3();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return v3;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 3)
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 Vector3");
                return v3;
            }

            v3.Set(SCCommon.ParseFloat(strs[0]), SCCommon.ParseFloat(strs[1]), SCCommon.ParseFloat(strs[2]));
            return v3;
        }

        protected Rect getRect(string _name, bool _canNull = true)
        {
            Rect rect = new Rect();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return rect;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 4)
            {
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，值「{tempValue}」无法解析为 Rect");
                return rect;
            }

            rect.Set(SCCommon.ParseFloat(strs[0]), SCCommon.ParseFloat(strs[1]), SCCommon.ParseFloat(strs[2]), SCCommon.ParseFloat(strs[3]));
            return rect;
        }

        protected T getEffect<T>(string _name) where T : _AEffectObjBase
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue) || tempValue == EXCEL_EMPTY_FLAG)
                return null;

            T effect = SCCommon.ParseEffectObj(tempValue, typeof(T)) as T;
            if (effect == null)
                Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，效果解析失败，类型：{typeof(T)}");

            return effect;
        }

        protected List<T> getList<T>(string _name, bool _canNull = true)
        {
            List<T> list = new List<T>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return list;
            }
            // "*" 表示空列表占位（Excel 约定）
            if (tempValue == EXCEL_EMPTY_FLAG)
                return list;

            string[] strs = tempValue.Split(new char[] { ';' });
            for (var i = 0; i < strs.Length; i++)
            {
                string tempStr = strs[i];
                object value = ParseValue(tempStr, typeof(T));
                if (value == null)
                {
                    Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，元素「{tempStr}」解析失败，列表类型：{typeof(T)}");
                }
                else
                {
                    list.Add((T)value);
                }
            }


            return list;
        }

        protected List<Vector2> getVector2List(string _name, bool _canNull = true)
        {
            List<Vector2> vector2List = new List<Vector2>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return vector2List;
            }
            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 2)
                {
                    Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，Vector2 项解析失败，原始数据：{valueStrs}");
                }
                else
                {
                    float x = SCCommon.ParseFloat(valueStrs[0]);
                    float y = SCCommon.ParseFloat(valueStrs[1]);
                    vector2List.Add(new Vector2(x, y));
                }
            }
            return vector2List;
        }

        protected List<Vector2Int> getVector2IntList(string _name, bool _canNull = true)
        {
            List<Vector2Int> Vector2IntList = new List<Vector2Int>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return Vector2IntList;
            }
            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 2)
                {
                    Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，Vector2Int 项解析失败，原始数据：{valueStrs}");
                }
                else
                {
                    int x = SCCommon.ParseInt(valueStrs[0]);
                    int y = SCCommon.ParseInt(valueStrs[1]);
                    Vector2IntList.Add(new Vector2Int(x, y));
                }
            }
            return Vector2IntList;
        }

        protected List<Vector3> getVector3List(string _name, bool _canNull = true)
        {
            List<Vector3> vector3List = new List<Vector3>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName} 中字段{_name}为空");
                return vector3List;
            }
            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 3)
                {
                    Debug.LogError($"表 \"{_m_assetPath},{_m_sheetName}\"\t填写错误：字段 {_name}，Vector3 项解析失败，原始数据：{valueStrs}");
                }
                else
                {
                    float x = SCCommon.ParseFloat(valueStrs[0]);
                    float y = SCCommon.ParseFloat(valueStrs[1]);
                    float z = SCCommon.ParseFloat(valueStrs[2]);
                    vector3List.Add(new Vector3(x, y, z));
                }
            }
            return vector3List;
        }

        /// <summary>将单元格字符串解析为字段值。</summary>
        protected static object ParseValue(string _value, Type _type)
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
                        return SCCommon.ParseEffectObj(_value, _type);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseValue type:{_type.ToString()}, value:{_value}, failed: {ex}");
            }
            return null;
        }

        #endregion

    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SCFrame.RefData
{
    /// <summary>
    /// 表多行读取：表格第一行为表头（字段 key），其后每行映射为一条 <typeparamref name="T"/>。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SCRefDataList<T> where T : SCRefDataCore,new()
    {
        private List<T> _m_refDataList;
        public List<T> refDataList => _m_refDataList;

        private string _m_assetPath;
        private string _m_sheetName;
        public SCRefDataList(string _assetPath, string _sheetName)
        {
            _m_assetPath = _assetPath;
            _m_sheetName = _sheetName;
            _m_refDataList = new List<T>();

        }

        public void readFromTxt()
        {
            if(string.IsNullOrEmpty(_m_assetPath) || string.IsNullOrEmpty(_m_sheetName))
            {
                Debug.LogError(_m_assetPath + " 或 " + _m_sheetName + " 未配置路径/表名，读取失败");
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
            string[] lineArray = _string.Split('\n');
            // 多行 list：首行为表头 key，其后每行与各 key 对齐
            string keyLine = lineArray[0];
            for (int i = 1; i < lineArray.Length; i++)
            {
                string line = lineArray[i].TrimEnd('\r', '\n', ' ', '\t');
                if (string.IsNullOrEmpty(line) || SCRefDataCore.isMemoRow(line))
                    continue;

                string firstCell = line.Split('\t')[0]?.Trim();
                if (string.IsNullOrEmpty(firstCell))
                    continue;

                T dataCore = new T();
                dataCore.singleParseFormTxt(keyLine, line);
                _m_refDataList.Add(dataCore);
            }
        }
    }


}

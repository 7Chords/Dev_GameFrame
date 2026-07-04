using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SCFrame.RefData
{
    /// <summary>
    /// Excel 配表导出编辑器面板
    /// </summary>
    public class SCExcelExporterWindow : EditorWindow
    {
        private const string PREF_EXCEL_PATH = "SCExcelExporter_ExcelPath";
        private const string PREF_TXT_PATH = "SCExcelExporter_TxtPath";

        private string _excelPath;
        private string _txtPath;
        private Vector2 _scrollPos;
        private List<ExcelFileInfo> _excelFiles = new List<ExcelFileInfo>();
        private readonly HashSet<string> _expandedFiles = new HashSet<string>();
        private readonly HashSet<string> _selectedSheets = new HashSet<string>();

        [MenuItem("Excel导出/打开导出面板")]
        public static void ShowWindow()
        {
            var window = GetWindow<SCExcelExporterWindow>("Excel导出");
            window.minSize = new Vector2(480, 360);
            window.Show();
        }

        private void OnEnable()
        {
            _excelPath = EditorPrefs.GetString(PREF_EXCEL_PATH, SCExcelExporter.GAME_EXCEL_PATH);
            _txtPath = EditorPrefs.GetString(PREF_TXT_PATH, SCExcelExporter.GAME_TXT_PATH);
            RefreshExcelList();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("路径设置", EditorStyles.boldLabel);

            DrawPathRow("Excel 文件夹", ref _excelPath, OnExcelPathChanged);
            DrawPathRow("导出 Txt 路径", ref _txtPath, OnTxtPathChanged);

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"可导出表 ({_excelFiles.Count} 个文件 / {GetTotalSheetCount()} 个页签)", EditorStyles.boldLabel);
            if (GUILayout.Button("全选", GUILayout.Width(50)))
            {
                SelectAllSheets();
            }
            if (GUILayout.Button("取消全选", GUILayout.Width(70)))
            {
                _selectedSheets.Clear();
            }
            if (GUILayout.Button("刷新列表", GUILayout.Width(80)))
            {
                RefreshExcelList();
            }
            EditorGUILayout.EndHorizontal();

            DrawExcelTree();

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = HasSheetSelection();
            string selectedLabel = _selectedSheets.Count > 0
                ? $"导入选中页签 ({_selectedSheets.Count})"
                : "导入选中页签";
            if (GUILayout.Button(selectedLabel, GUILayout.Height(28)))
            {
                ExportSelected();
            }
            GUI.enabled = GetTotalSheetCount() > 0;
            if (GUILayout.Button("导入全部页签", GUILayout.Height(28)))
            {
                ExportAll();
            }
            GUI.enabled = true;
            if (GUILayout.Button("复制到 StreamingAssets", GUILayout.Height(28)))
            {
                CopyToStreamingAssets();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "一级：Excel 文件；二级：页签（每个页签对应一个 Txt）。支持多选页签批量导入。\n复制到 StreamingAssets 供运行时读取。",
                MessageType.Info);
        }

        private void DrawPathRow(string label, ref string path, System.Action onChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(110));
            string newPath = EditorGUILayout.TextField(path);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string absolute = ToAbsolutePath(path);
                string selected = EditorUtility.OpenFolderPanel(label, absolute, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    newPath = ToProjectRelativePath(selected);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (newPath != path)
            {
                path = newPath;
                onChanged?.Invoke();
            }
        }

        private void DrawExcelTree()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MinHeight(200));

            if (_excelFiles.Count == 0)
            {
                EditorGUILayout.LabelField("未找到 Excel 文件（.xls / .xlsx）", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int fileIndex = 0; fileIndex < _excelFiles.Count; fileIndex++)
                {
                    DrawExcelFileNode(fileIndex, _excelFiles[fileIndex]);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawExcelFileNode(int fileIndex, ExcelFileInfo fileInfo)
        {
            bool expanded = _expandedFiles.Contains(fileInfo.RelativePath);
            int selectedCount = GetSelectedSheetCountInFile(fileInfo);
            bool allSelected = fileInfo.SheetNames.Count > 0 && selectedCount == fileInfo.SheetNames.Count;
            bool noneSelected = selectedCount == 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.showMixedValue = !allSelected && !noneSelected;
            bool fileToggle = EditorGUILayout.Toggle(allSelected, GUILayout.Width(16));
            EditorGUI.showMixedValue = false;

            if (fileToggle != allSelected)
            {
                SetFileSelection(fileInfo, fileToggle);
            }

            bool newExpanded = EditorGUILayout.Foldout(expanded, fileInfo.RelativePath, true);
            EditorGUILayout.LabelField($"({fileInfo.SheetNames.Count} 页签)", EditorStyles.miniLabel, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            if (newExpanded != expanded)
            {
                if (newExpanded)
                {
                    _expandedFiles.Add(fileInfo.RelativePath);
                }
                else
                {
                    _expandedFiles.Remove(fileInfo.RelativePath);
                }
            }

            if (!newExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            for (int sheetIndex = 0; sheetIndex < fileInfo.SheetNames.Count; sheetIndex++)
            {
                string sheetName = fileInfo.SheetNames[sheetIndex];
                string key = MakeSheetKey(fileInfo.RelativePath, sheetName);
                bool selected = _selectedSheets.Contains(key);
                bool newSelected = EditorGUILayout.ToggleLeft(sheetName, selected);

                if (newSelected)
                {
                    _selectedSheets.Add(key);
                }
                else
                {
                    _selectedSheets.Remove(key);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void OnExcelPathChanged()
        {
            EditorPrefs.SetString(PREF_EXCEL_PATH, _excelPath);
            RefreshExcelList();
        }

        private void OnTxtPathChanged()
        {
            EditorPrefs.SetString(PREF_TXT_PATH, _txtPath);
        }

        private void RefreshExcelList()
        {
            _excelFiles = SCExcelExporter.GetExcelFileInfoList(_excelPath);

            for (int i = 0; i < _excelFiles.Count; i++)
            {
                _expandedFiles.Add(_excelFiles[i].RelativePath);
            }

            PruneInvalidSelections();
        }

        private void PruneInvalidSelections()
        {
            var validKeys = new HashSet<string>();
            for (int i = 0; i < _excelFiles.Count; i++)
            {
                ExcelFileInfo file = _excelFiles[i];
                for (int j = 0; j < file.SheetNames.Count; j++)
                {
                    validKeys.Add(MakeSheetKey(file.RelativePath, file.SheetNames[j]));
                }
            }

            _selectedSheets.RemoveWhere(key => !validKeys.Contains(key));
        }

        private void SelectAllSheets()
        {
            _selectedSheets.Clear();
            for (int i = 0; i < _excelFiles.Count; i++)
            {
                SetFileSelection(_excelFiles[i], true);
            }
        }

        private void SetFileSelection(ExcelFileInfo fileInfo, bool selected)
        {
            for (int i = 0; i < fileInfo.SheetNames.Count; i++)
            {
                string key = MakeSheetKey(fileInfo.RelativePath, fileInfo.SheetNames[i]);
                if (selected)
                {
                    _selectedSheets.Add(key);
                }
                else
                {
                    _selectedSheets.Remove(key);
                }
            }
        }

        private int GetSelectedSheetCountInFile(ExcelFileInfo fileInfo)
        {
            int count = 0;
            for (int i = 0; i < fileInfo.SheetNames.Count; i++)
            {
                if (_selectedSheets.Contains(MakeSheetKey(fileInfo.RelativePath, fileInfo.SheetNames[i])))
                {
                    count++;
                }
            }
            return count;
        }

        private static string MakeSheetKey(string relativePath, string sheetName)
        {
            return relativePath + "|" + sheetName;
        }

        private bool HasSheetSelection()
        {
            return _selectedSheets.Count > 0;
        }

        private int GetTotalSheetCount()
        {
            int count = 0;
            for (int i = 0; i < _excelFiles.Count; i++)
            {
                count += _excelFiles[i].SheetNames.Count;
            }
            return count;
        }

        private void ExportSelected()
        {
            if (!HasSheetSelection())
            {
                return;
            }

            if (!ValidatePaths(out string excelRoot, out string txtRoot))
            {
                return;
            }

            for (int i = 0; i < _excelFiles.Count; i++)
            {
                ExcelFileInfo file = _excelFiles[i];
                string excelFilePath = Path.Combine(excelRoot, file.RelativePath);

                for (int j = 0; j < file.SheetNames.Count; j++)
                {
                    string sheetName = file.SheetNames[j];
                    if (!_selectedSheets.Contains(MakeSheetKey(file.RelativePath, sheetName)))
                    {
                        continue;
                    }

                    SCExcelExporter.ExportExcelSheet(excelFilePath, sheetName, txtRoot);
                }
            }

            AssetDatabase.Refresh();
        }

        private void ExportAll()
        {
            if (!ValidatePaths(out string excelRoot, out string txtRoot))
            {
                return;
            }

            SCExcelExporter.ExportAll(excelRoot, txtRoot);
            RefreshExcelList();
            AssetDatabase.Refresh();
        }

        private void CopyToStreamingAssets()
        {
            if (!ValidateTxtPath(out string txtRoot))
            {
                return;
            }

            SCExcelExporter.CopyTxtToStreamingAssets(txtRoot);
            AssetDatabase.Refresh();
        }

        private bool ValidatePaths(out string excelRoot, out string txtRoot)
        {
            excelRoot = ToAbsolutePath(_excelPath);
            txtRoot = ToAbsolutePath(_txtPath);

            if (!Directory.Exists(excelRoot))
            {
                EditorUtility.DisplayDialog("路径错误", $"Excel 文件夹不存在:\n{excelRoot}", "确定");
                return false;
            }

            if (!Directory.Exists(txtRoot))
            {
                if (EditorUtility.DisplayDialog("创建目录", $"导出 Txt 路径不存在，是否创建?\n{txtRoot}", "创建", "取消"))
                {
                    Directory.CreateDirectory(txtRoot);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateTxtPath(out string txtRoot)
        {
            txtRoot = ToAbsolutePath(_txtPath);

            if (!Directory.Exists(txtRoot))
            {
                EditorUtility.DisplayDialog("路径错误", $"导出 Txt 路径不存在:\n{txtRoot}", "确定");
                return false;
            }

            return true;
        }

        private static string ToAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path.Replace('\\', '/');
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(projectRoot, path).Replace('\\', '/');
        }

        private static string ToProjectRelativePath(string absolutePath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
            string normalized = absolutePath.Replace('\\', '/');

            if (normalized.StartsWith(projectRoot))
            {
                string relative = normalized.Substring(projectRoot.Length);
                if (relative.StartsWith("/"))
                {
                    relative = relative.Substring(1);
                }
                return relative;
            }

            return normalized;
        }
    }
}

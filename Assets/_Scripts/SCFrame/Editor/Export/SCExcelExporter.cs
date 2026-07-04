using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SCFrame.RefData
{
    /// <summary>
    /// Excel 文件及其页签信息
    /// </summary>
    public class ExcelFileInfo
    {
        public string RelativePath;
        public List<string> SheetNames = new List<string>();
    }

    /// <summary>
    /// SCFrame中的配表数据导出器
    /// </summary>
    public static class SCExcelExporter
    {
        public const string GAME_EXCEL_PATH = "Assets/Resources/RefData/Excel";
        public const string GAME_TXT_PATH = "Assets/Resources/RefData/ExportTxt";
        public const int TITLE_START_INDEX = 0;//标题列索引

        /// <summary>
        /// 获取指定目录下所有可导出的 Excel 文件（相对路径）
        /// </summary>
        public static List<string> GetExcelFileList(string excelFolderPath)
        {
            var result = new List<string>();
            List<ExcelFileInfo> infos = GetExcelFileInfoList(excelFolderPath);
            for (int i = 0; i < infos.Count; i++)
            {
                result.Add(infos[i].RelativePath);
            }
            return result;
        }

        /// <summary>
        /// 获取 Excel 文件列表及每个文件内的页签名称
        /// </summary>
        public static List<ExcelFileInfo> GetExcelFileInfoList(string excelFolderPath)
        {
            var result = new List<ExcelFileInfo>();
            string absoluteRoot = ToAbsolutePath(excelFolderPath);

            if (!Directory.Exists(absoluteRoot))
            {
                return result;
            }

            DirectoryInfo direction = new DirectoryInfo(absoluteRoot);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            int validCount = 0;

            for (int i = 0; i < files.Length; i++)
            {
                string ext = Path.GetExtension(files[i].FullName);
                if (ext != ".xls" && ext != ".xlsx")
                {
                    continue;
                }

                if (files[i].Name.StartsWith("~$"))
                {
                    continue;
                }

                validCount++;
            }

            int current = 0;
            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string ext = Path.GetExtension(files[i].FullName);
                    if (ext != ".xls" && ext != ".xlsx")
                    {
                        continue;
                    }

                    if (files[i].Name.StartsWith("~$"))
                    {
                        continue;
                    }

                    string relativePath = files[i].FullName.Substring(absoluteRoot.Length).TrimStart('\\', '/').Replace('\\', '/');
                    EditorUtility.DisplayProgressBar("读取 Excel 页签", relativePath, validCount > 0 ? (float)current / validCount : 0f);
                    current++;

                    var info = new ExcelFileInfo { RelativePath = relativePath };

                    try
                    {
                        IWorkbook workbook = CreatWrokbook(files[i].FullName);
                        for (int s = 0; s < workbook.NumberOfSheets; s++)
                        {
                            info.SheetNames.Add(workbook.GetSheetName(s));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"读取 Excel 页签失败 [{relativePath}]: {ex.Message}");
                    }

                    result.Add(info);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            result.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.Ordinal));
            return result;
        }

        /// <summary>
        /// 导出指定目录下所有的 Excel 表
        /// </summary>
        public static void ExportAll(string excelFolderPath, string txtOutputPath)
        {
            List<string> excelFiles = GetExcelFileList(excelFolderPath);
            if (excelFiles.Count == 0)
            {
                Debug.LogError("没有找到可以导出的Excel！！！");
                return;
            }

            string excelRoot = ToAbsolutePath(excelFolderPath);
            string txtRoot = EnsureDirectory(txtOutputPath);

            for (int i = 0; i < excelFiles.Count; i++)
            {
                ExportExcelFile(Path.Combine(excelRoot, excelFiles[i]), txtRoot);
            }

            Debug.Log("所有的Excel都导出成功！！！");
        }

        /// <summary>
        /// 导出单个 Excel 文件内的全部页签
        /// </summary>
        public static void ExportExcelFile(string excelFilePath, string txtOutputPath)
        {
            string txtRoot = EnsureDirectory(txtOutputPath);
            IWorkbook workbook = CreatWrokbook(excelFilePath);

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ExportSheet(workbook.GetSheetAt(i), Path.Combine(txtRoot, workbook.GetSheetName(i) + ".txt"));
            }

            Debug.Log("导出 " + Path.GetFileName(excelFilePath) + " 成功！！！");
        }

        /// <summary>
        /// 导出 Excel 文件内的单个页签
        /// </summary>
        public static void ExportExcelSheet(string excelFilePath, string sheetName, string txtOutputPath)
        {
            string txtRoot = EnsureDirectory(txtOutputPath);
            IWorkbook workbook = CreatWrokbook(excelFilePath);
            ISheet sheet = workbook.GetSheet(sheetName);

            if (sheet == null)
            {
                Debug.LogError($"页签不存在: {Path.GetFileName(excelFilePath)} / {sheetName}");
                return;
            }

            ExportSheet(sheet, Path.Combine(txtRoot, sheetName + ".txt"));
            Debug.Log($"导出 {Path.GetFileName(excelFilePath)} / {sheetName} 成功！！！");
        }

        private static void ExportSheet(ISheet sheet, string txtFilePath)
        {
            if (sheet == null)
            {
                return;
            }

            IRow headerRow = sheet.GetRow(TITLE_START_INDEX);
            if (headerRow == null)
            {
                return;
            }

            List<int> exportColumnIdxList = new List<int>();
            for (int k = 0; k <= headerRow.LastCellNum; k++)
            {
                ICell headerCell = headerRow.GetCell(k);
                if (headerCell == null || string.IsNullOrEmpty(headerCell.ToString()))
                {
                    continue;
                }

                if (headerCell.ToString().StartsWith(SCRefDataCore.MEMO_COLUMN_PREFIX))
                {
                    continue;
                }

                exportColumnIdxList.Add(k);
            }

            using (FileStream fs = File.Open(txtFilePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    writeRowCells(sw, headerRow, exportColumnIdxList);

                    for (int j = TITLE_START_INDEX + 1; j <= sheet.LastRowNum; j++)
                    {
                        IRow row = sheet.GetRow(j);
                        if (row == null)
                        {
                            continue;
                        }

                        writeRowCells(sw, row, exportColumnIdxList);
                    }
                }
            }
        }

        /// <summary>
        /// 按表头列索引导出整行，空单元格写占位以保持列对齐（支持表头下方竖向 ~ 注释行）。
        /// </summary>
        private static void writeRowCells(StreamWriter _sw, IRow _row, List<int> _exportColumnIdxList)
        {
            for (int idx = 0; idx < _exportColumnIdxList.Count; idx++)
            {
                ICell cell = _row.GetCell(_exportColumnIdxList[idx]);
                _sw.Write(cell?.ToString() ?? "");
                if (idx < _exportColumnIdxList.Count - 1)
                    _sw.Write("\t");
            }
            _sw.Write("\n");
        }

        /// <summary>
        /// 创建工作簿
        /// </summary>
        private static IWorkbook CreatWrokbook(string _excelPath)
        {
            using (FileStream stream = File.Open(_excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Path.GetExtension(_excelPath) == ".xls")
                {
                    return new HSSFWorkbook(stream);
                }
                else
                {
                    return new XSSFWorkbook(stream);
                }
            }
        }

        /// <summary>
        /// 复制 Txt 到 StreamingAssets 下，因为 Unity 导出运行时读不到 Resources 下的 txt
        /// </summary>
        public static void CopyTxtToStreamingAssets(string txtSourcePath, string streamingAssetsSubFolder = "ExportTxt")
        {
            try
            {
                string sourcePath = ToAbsolutePath(txtSourcePath);
                string targetPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsSubFolder);

                if (!Directory.Exists(sourcePath))
                {
                    Debug.LogError($"Txt 源目录不存在: {sourcePath}");
                    return;
                }

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                DirectoryInfo direction = new DirectoryInfo(sourcePath);
                FileInfo[] files = direction.GetFiles("*.txt", SearchOption.AllDirectories);
                int copiedCount = 0;

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        string targetFile = Path.Combine(targetPath, files[i].Name);
                        File.Copy(files[i].FullName, targetFile, true);
                        copiedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"复制文件 {files[i].Name} 时出错: {ex.Message}");
                    }
                }

                Debug.Log($"复制完成! 共复制 {copiedCount} 个txt文件");
            }
            catch (Exception ex)
            {
                Debug.LogError($"复制过程中发生错误: {ex.Message}");
            }
        }

        private static string EnsureDirectory(string path)
        {
            string absolute = ToAbsolutePath(path);
            if (!Directory.Exists(absolute))
            {
                Directory.CreateDirectory(absolute);
            }
            return absolute;
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
    }
}

/*
 * Copyright (c) 2025 Gil Barbosa Reis
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#if !UNITY_ANDROID && !UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SQLite.Editor
{
    public class SQLiteAssetBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (SQLiteAsset sqliteAsset in GetAffectedAssets())
            {
                string filePath = $"Assets/StreamingAssets/{sqliteAsset.StreamingAssetsPath}";
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                File.WriteAllBytes(filePath, sqliteAsset.Bytes);
                sqliteAsset.Bytes = Array.Empty<byte>();
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            foreach (SQLiteAsset sqliteAsset in GetAffectedAssets())
            {
                string filePath = $"Assets/StreamingAssets/{sqliteAsset.StreamingAssetsPath}";
                if (File.Exists(filePath))
                {
                    sqliteAsset.Bytes = File.ReadAllBytes(filePath);
                    FileUtil.DeleteFileOrDirectory(filePath);
                    FileUtil.DeleteFileOrDirectory(filePath + ".meta");
                    DeleteEmptyDirectories(Path.GetDirectoryName(filePath));
                }
            }
        }

        private static void DeleteEmptyDirectories(string directory)
        {
            while (!string.IsNullOrWhiteSpace(directory))
            {
                if (Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    return;
                }
                FileUtil.DeleteFileOrDirectory(directory);
                FileUtil.DeleteFileOrDirectory(directory + ".meta");
                directory = Path.GetDirectoryName(directory);
            }
        }

        private static IEnumerable<SQLiteAsset> GetAffectedAssets()
        {
            return AssetDatabase.FindAssets($"t:{nameof(SQLiteAsset)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SQLiteAsset>)
                .Where(sqlite => sqlite.UseStreamingAssets);
        }
    }
}
#endif

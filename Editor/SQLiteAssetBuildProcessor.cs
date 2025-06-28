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

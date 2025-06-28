using System.IO;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace SQLite.Editor
{
    [ScriptedImporter(0, new[] { "sqlite", "sqlite2", "sqlite3" })]
    public class SQLiteAssetImporter : ScriptedImporter
    {
        [Tooltip("Flags controlling how the SQLite connection should be opened. 'ReadWrite' and 'Create' flags will be ignored, since SQLite assets are read-only.")]
        [SerializeField] private SQLiteOpenFlags _openFlags = SQLiteOpenFlags.ReadOnly;

        [Tooltip("Whether to store DateTime properties as ticks (true) or strings (false).")]
        [SerializeField] private bool _storeDateTimeAsTicks = true;

        [Tooltip("Name of the file created for the database inside Streaming Assets folder during builds.\n\n"
            + "If empty, the database bytes will be stored in the asset itself.\n\n"
            + "Loading databases from Streaming Assets is not supported in Android and WebGL platforms.")]
        [SerializeField] private string _streamingAssetsPath;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = ScriptableObject.CreateInstance<SQLiteAsset>();
            asset.OpenFlags = _openFlags;
            asset.StoreDateTimeAsTicks = _storeDateTimeAsTicks;
            asset.Bytes = File.ReadAllBytes(ctx.assetPath);
            asset.StreamingAssetsPath = _streamingAssetsPath;
            ctx.AddObjectToAsset("sqlite", asset);
            ctx.SetMainObject(asset);
        }
    }
}

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
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SQLite.Editor
{
    [ScriptedImporter(0, null, new[] { "sql" })]
    public class SQLScriptDatabaseImporter : ScriptedImporter
    {
        [Header("SQLite asset options")]
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
            SQLiteAsset asset;
            using (var tempDb = new SQLiteConnection(""))
            {
                string contents = File.ReadAllText(assetPath);
                tempDb.ExecuteScript(contents);
                asset = tempDb.SerializeToAsset(null, _openFlags, _storeDateTimeAsTicks, _streamingAssetsPath);
            }
            ctx.AddObjectToAsset("main", asset);
            ctx.SetMainObject(asset);
        }
    }
}

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
using System;
using UnityEngine;

namespace SQLite
{
    public class SQLiteAsset : ScriptableObject
    {
        [Tooltip("Flags controlling how the SQLite connection should be opened. 'ReadWrite' and 'Create' flags will be ignored, since SQLite assets are read-only.")]
        [SerializeField] private SQLiteOpenFlags _openFlags = SQLiteOpenFlags.ReadOnly;

        [Tooltip("Whether to store DateTime properties as ticks (true) or strings (false).")]
        [SerializeField] private bool _storeDateTimeAsTicks = true;

        [Tooltip("Name of the file created for the database inside Streaming Assets folder during builds.\n\n"
            + "If empty, the database bytes will be stored in the asset itself.\n\n"
            + "Loading databases from Streaming Assets is not supported in Android and WebGL platforms.")]
        [SerializeField] private string _streamingAssetsPath;

        [SerializeField, HideInInspector] private byte[] _bytes;

        /// <summary>
        /// Flags controlling how the SQLite connection should be opened.
        /// </summary>
        /// <remarks>
        /// 'ReadWrite' and 'Create' flags will be ignored, since SQLite assets are read-only.
        /// </remarks>
        public SQLiteOpenFlags OpenFlags
        {
            get => _openFlags;
            set => _openFlags = value & ~(SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }

        /// <summary>
        /// Whether to store DateTime properties as ticks (true) or strings (false).
        /// </summary>
        /// <seealso cref="SQLiteConnectionString.StoreDateTimeAsTicks"/>
        public bool StoreDateTimeAsTicks
        {
            get => _storeDateTimeAsTicks;
            set => _storeDateTimeAsTicks = value;
        }

        /// <summary>
        /// Bytes that compose the SQLite database file.
        /// </summary>
        public byte[] Bytes
        {
            get => _bytes;
            set => _bytes = value;
        }

        /// <summary>
        /// If true, the database bytes will be read from a file located at the Streaming Assets folder instead of storing all bytes in memory.
        /// </summary>
        public string StreamingAssetsPath
        {
            get => _streamingAssetsPath;
            set => _streamingAssetsPath = value;
        }

        /// <summary>
        /// If true, the database bytes will be read from a file located at the Streaming Assets folder instead of storing all bytes in memory.
        /// </summary>
        public bool UseStreamingAssets => !string.IsNullOrWhiteSpace(_streamingAssetsPath);

        /// <summary>
        /// Creates a new connection to the read-only SQLite database represented by this asset.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">If <see cref="Bytes"/> is null.</exception>
        public SQLiteConnection CreateConnection()
        {
#if !UNITY_EDITOR && !UNITY_ANDROID && !UNITY_WEBGL
            if (UseStreamingAssets)
            {
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, _streamingAssetsPath);
                return new SQLiteConnection(path);
            }
#endif
            if (Bytes == null)
            {
                throw new NullReferenceException(nameof(Bytes));
            }

            return new SQLiteConnection("").Deserialize(Bytes, null, SQLite3.DeserializeFlags.ReadOnly);
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (_openFlags.HasFlag(SQLiteOpenFlags.ReadWrite))
            {
                Debug.LogWarning($"{nameof(SQLiteAsset)} does not support writing to the database. Ignoring \"ReadWrite\" flag.", this);
                _openFlags &= ~SQLiteOpenFlags.ReadWrite;
            }
            if (_openFlags.HasFlag(SQLiteOpenFlags.Create))
            {
                Debug.LogWarning($"{nameof(SQLiteAsset)} does not support creating database. Ignoring \"Create\" flag.", this);
                _openFlags &= ~SQLiteOpenFlags.Create;
            }
        }
#endif
    }
}

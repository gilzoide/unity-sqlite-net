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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SQLite.Editor
{
    [CustomEditor(typeof(SQLiteAsset))]
    [CanEditMultipleObjects]
    public class SQLiteAssetEditor : UnityEditor.Editor
    {
        private class TableInfo
        {
            public string Name { get; set; }
            public string Sql { get; set; }

            public void Deconstruct(out string name, out string sql)
            {
                name = Name;
                sql = Sql;
            }
        }

        [SerializeField] private List<string> _expandedTables = new List<string>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (serializedObject.isEditingMultipleObjects)
            {
                return;
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Database size in bytes", EditorUtility.FormatBytes(((SQLiteAsset) target).Bytes.Length));
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(true))
            using (var db = ((SQLiteAsset) target).CreateConnection())
            {
                EditorGUILayout.LabelField("Tables", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach ((string name, string sql) in db.Query<TableInfo>("SELECT name, sql FROM SQLite_schema WHERE type = 'table'"))
                {
                    bool previouslyExpanded = _expandedTables.Contains(name);
                    bool expanded = EditorGUILayout.Foldout(previouslyExpanded, name, true);
                    if (previouslyExpanded && !expanded)
                    {
                        _expandedTables.Remove(name);
                    }
                    else if (!previouslyExpanded && expanded)
                    {
                        _expandedTables.Add(name);
                    }

                    if (expanded)
                    {
                        EditorGUILayout.TextField("SQL", sql);
                        int count = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM {SQLiteConnection.Quote(name)}");
                        EditorGUILayout.IntField("Row Count", count);
                    }
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}

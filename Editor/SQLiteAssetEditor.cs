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
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Database size in bytes", EditorUtility.FormatBytes(((SQLiteAsset) target).Bytes.Length));
            }

            if (serializedObject.isEditingMultipleObjects)
            {
                return;
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

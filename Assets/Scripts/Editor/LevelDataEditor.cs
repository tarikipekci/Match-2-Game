using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        private LevelData levelData;
        private int rowCount = 1;

        private void OnEnable()
        {
            levelData = (LevelData)target;
            levelData.startingRows ??= Array.Empty<TileRow>();
            rowCount = levelData.startingRows.Length;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            DrawDefaultInspectorExcept("startingRows"); 

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Setup", EditorStyles.boldLabel);

            rowCount = EditorGUILayout.IntField("Row Count", rowCount);
            if (rowCount < 0) rowCount = 0;

            if (rowCount != levelData.startingRows.Length)
            {
                Array.Resize(ref levelData.startingRows, rowCount);
                for (int i = 0; i < rowCount; i++)
                {
                    levelData.startingRows[i] ??= new TileRow();
                }
            }

            for (int r = 0; r < rowCount; r++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Row {r + 1}", EditorStyles.boldLabel);

                levelData.startingRows[r].tiles ??= Array.Empty<TileData>();

                int tileCount = EditorGUILayout.IntField("Tile Count", levelData.startingRows[r].tiles.Length);
                if (tileCount < 0) tileCount = 0;

                if (tileCount != levelData.startingRows[r].tiles.Length)
                {
                    Array.Resize(ref levelData.startingRows[r].tiles, tileCount);
                    for (int t = 0; t < tileCount; t++)
                    {
                        levelData.startingRows[r].tiles[t] ??= new TileData();
                    }
                }

                for (int t = 0; t < tileCount; t++)
                {
                    EditorGUILayout.BeginHorizontal();
                    levelData.startingRows[r].tiles[t].tileType =
                        (TileType)EditorGUILayout.EnumPopup("Tile Type", levelData.startingRows[r].tiles[t].tileType);

                    if (levelData.startingRows[r].tiles[t].tileType == TileType.Cube)
                    {
                        levelData.startingRows[r].tiles[t].tileColor =
                            (TileColor)EditorGUILayout.EnumPopup("Color", levelData.startingRows[r].tiles[t].tileColor);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Color N/A");
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
                EditorUtility.SetDirty(levelData);
        }

        private void DrawDefaultInspectorExcept(string propertyName)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.name != propertyName)
                    EditorGUILayout.PropertyField(property, true);
            }
        }
    }
}

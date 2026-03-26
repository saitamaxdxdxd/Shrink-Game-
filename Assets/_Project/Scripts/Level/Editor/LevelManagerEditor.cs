using UnityEditor;
using UnityEngine;

namespace Shrink.Level.Editor
{
    /// <summary>
    /// Inspector custom para LevelManager.
    /// Reemplaza el campo numérico startLevelIndex por un dropdown con los nombres
    /// de los LevelData asignados en el array Levels.
    /// </summary>
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Dibujar el array de niveles normalmente
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levels"), true);

            EditorGUILayout.Space(6);

            // Construir opciones del dropdown desde el array
            var levelsProp = serializedObject.FindProperty("levels");
            var indexProp  = serializedObject.FindProperty("startLevelIndex");

            int count = levelsProp.arraySize;

            if (count == 0)
            {
                EditorGUILayout.HelpBox("Asigna niveles en el array Levels primero.", MessageType.Warning);
            }
            else
            {
                string[] options = new string[count];
                for (int i = 0; i < count; i++)
                {
                    var asset = levelsProp.GetArrayElementAtIndex(i).objectReferenceValue as LevelData;
                    options[i] = asset != null
                        ? $"{i + 1:D2}  —  {asset.name}"
                        : $"{i + 1:D2}  —  (vacío)";
                }

                int current  = Mathf.Clamp(indexProp.intValue, 0, count - 1);
                int selected = EditorGUILayout.Popup("Start Level", current, options);

                if (selected != current)
                    indexProp.intValue = selected;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

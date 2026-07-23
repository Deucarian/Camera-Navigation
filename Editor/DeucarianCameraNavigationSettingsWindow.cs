using UnityEditor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Editor
{
    public sealed class DeucarianCameraNavigationSettingsWindow : EditorWindow
    {
        public const string MenuPath =
            "Tools/Deucarian/Experience and Interaction/World Interaction/Camera Navigation";
        public const string CanonicalControlsAssetPath =
            "Assets/Resources/Deucarian/CameraNavigationControls.asset";

        private DeucarianCameraNavigationControls controls;
        private SerializedObject serializedControls;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath, priority = 230)]
        public static void OpenWindow()
        {
            DeucarianCameraNavigationSettingsWindow window =
                GetWindow<DeucarianCameraNavigationSettingsWindow>(
                    "Camera Navigation");
            window.minSize = new Vector2(460f, 620f);
            window.Show();
        }

        private void OnEnable()
        {
            SelectControls(FindPreferredControls());
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is DeucarianCameraNavigationControls selected)
            {
                SelectControls(selected);
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Camera Navigation",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure the complete Orbit and Fly speed profile used by Deucarian Camera Navigation. New assets start with the proven legacy Report Viewer feel.",
                MessageType.Info);

            DeucarianCameraNavigationControls selected =
                (DeucarianCameraNavigationControls)EditorGUILayout.ObjectField(
                    "Controls Asset",
                    controls,
                    typeof(DeucarianCameraNavigationControls),
                    false);
            if (selected != controls)
            {
                SelectControls(selected);
            }

            DrawAssetActions();
            EditorGUILayout.Space();

            if (controls == null || serializedControls == null)
            {
                EditorGUILayout.HelpBox(
                    "Create or select a controls asset to edit navigation values.",
                    MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            serializedControls.Update();
            DrawControlProperties(serializedControls);
            if (serializedControls.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(controls);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Restore Legacy Defaults"))
            {
                Undo.RecordObject(controls, "Restore Camera Navigation Defaults");
                controls.ResetToDefaults();
                EditorUtility.SetDirty(controls);
                serializedControls.Update();
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawControlProperties(SerializedObject serializedObject)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.name == "m_Script")
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }
        }

        private void DrawAssetActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Project Controls"))
                {
                    SelectControls(CreateProjectControls());
                }

                using (new EditorGUI.DisabledScope(controls == null))
                {
                    if (GUILayout.Button("Select Asset"))
                    {
                        Selection.activeObject = controls;
                        EditorGUIUtility.PingObject(controls);
                    }
                }
            }
        }

        private void SelectControls(DeucarianCameraNavigationControls selected)
        {
            controls = selected;
            serializedControls = controls != null
                ? new SerializedObject(controls)
                : null;
        }

        private static DeucarianCameraNavigationControls FindPreferredControls()
        {
            DeucarianCameraNavigationControls canonical =
                AssetDatabase.LoadAssetAtPath<DeucarianCameraNavigationControls>(
                    CanonicalControlsAssetPath);
            if (canonical != null)
            {
                return canonical;
            }

            string[] guids =
                AssetDatabase.FindAssets("t:DeucarianCameraNavigationControls");
            return guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<DeucarianCameraNavigationControls>(
                    AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;
        }

        private static DeucarianCameraNavigationControls CreateProjectControls()
        {
            EnsureFolder("Assets/Resources/Deucarian");
            DeucarianCameraNavigationControls existing =
                AssetDatabase.LoadAssetAtPath<DeucarianCameraNavigationControls>(
                    CanonicalControlsAssetPath);
            if (existing != null)
            {
                return existing;
            }

            DeucarianCameraNavigationControls created =
                CreateInstance<DeucarianCameraNavigationControls>();
            created.ResetToDefaults();
            AssetDatabase.CreateAsset(created, CanonicalControlsAssetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = created;
            EditorGUIUtility.PingObject(created);
            return created;
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }
    }
}

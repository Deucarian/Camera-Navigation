using Deucarian.Editor;
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
            using (DeucarianEditorWorkbenchPanelScope page =
                   DeucarianEditorWorkbenchGUI.BeginSettingsPage(
                       GUILayout.ExpandHeight(true)))
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DeucarianEditorChrome.DrawPackageHeader(
                    "camera",
                    "Camera Navigation",
                    "Configure the complete Orbit and Fly profile with the proven legacy Report Viewer feel.");

                DrawProjectControls();
                DrawNavigationProfile();

                DeucarianEditorChrome.DrawFooterVersion(
                    "com.deucarian.camera-navigation",
                    "0.2.2");
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawProjectControls()
        {
            DeucarianEditorChrome.DrawSectionHeader("Project Controls");
            DeucarianEditorChrome.BeginSection();

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

            if (controls == null)
            {
                DeucarianEditorWorkbenchGUI.DrawStatusIconRow(
                    "circle-alert",
                    "No controls asset is selected.",
                    DeucarianEditorStatus.Warning);
            }
            else
            {
                DeucarianEditorWorkbenchGUI.DrawStatusIconRow(
                    "circle-check",
                    AssetDatabase.GetAssetPath(controls) == CanonicalControlsAssetPath
                        ? "The canonical project controls asset is active."
                        : "A project controls asset is selected.",
                    DeucarianEditorStatus.Success);
            }

            DrawAssetActions();
            DeucarianEditorChrome.EndSection();
        }

        private void DrawNavigationProfile()
        {
            DeucarianEditorChrome.DrawSectionHeader("Navigation Profile");
            DeucarianEditorChrome.BeginSection();

            if (controls == null || serializedControls == null)
            {
                EditorGUILayout.HelpBox(
                    "Create or select a controls asset to edit navigation values.",
                    MessageType.Info);
                DeucarianEditorChrome.EndSection();
                return;
            }

            serializedControls.Update();
            DrawControlProperties(serializedControls);
            if (serializedControls.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(controls);
            }

            GUILayout.Space(DeucarianEditorWorkbenchGUI.PanelSpacing);
            DeucarianEditorSettingsActions.DrawResetToDefaultsButton(
                RestoreLegacyDefaults,
                "Restore the legacy Report Viewer Orbit and Fly defaults.");

            DeucarianEditorChrome.EndSection();
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
                if (GUILayout.Button(
                    "Create Project Controls",
                    DeucarianEditorWorkbenchGUI.PrimaryButtonStyle))
                {
                    SelectControls(CreateProjectControls());
                }

                using (new EditorGUI.DisabledScope(controls == null))
                {
                    if (GUILayout.Button(
                        "Select Asset",
                        DeucarianEditorWorkbenchGUI.SecondaryButtonStyle))
                    {
                        Selection.activeObject = controls;
                        EditorGUIUtility.PingObject(controls);
                    }
                }
            }
        }

        private void RestoreLegacyDefaults()
        {
            if (controls == null)
            {
                return;
            }

            Undo.RecordObject(controls, "Restore Camera Navigation Defaults");
            controls.ResetToDefaults();
            EditorUtility.SetDirty(controls);
            serializedControls.Update();
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

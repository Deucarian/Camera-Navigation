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
        public const string CanonicalFramingAssetPath =
            "Assets/Resources/Deucarian/CameraFramingSettings.asset";

        private DeucarianCameraNavigationControls controls;
        private SerializedObject serializedControls;
        private DeucarianCameraFramingSettings framingSettings;
        private SerializedObject serializedFramingSettings;
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
            SelectFramingSettings(FindPreferredFramingSettings());
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is DeucarianCameraNavigationControls selected)
            {
                SelectControls(selected);
                Repaint();
            }
            else if (Selection.activeObject is
                     DeucarianCameraFramingSettings selectedFraming)
            {
                SelectFramingSettings(selectedFraming);
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
                    "Configure the complete Orbit and Fly profile with the approved Deucarian feel.");

                DrawProjectAssets();
                DrawNavigationProfile();
                DrawFramingProfile();

                DeucarianEditorChrome.DrawFooterVersion(
                    "com.deucarian.camera-navigation",
                    "0.2.9");
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawProjectAssets()
        {
            DeucarianEditorChrome.DrawSectionHeader("Project Assets");
            DeucarianEditorChrome.BeginSection();

            DeucarianCameraNavigationControls selectedControls =
                (DeucarianCameraNavigationControls)EditorGUILayout.ObjectField(
                    "Controls Asset",
                    controls,
                    typeof(DeucarianCameraNavigationControls),
                    false);
            if (selectedControls != controls)
            {
                SelectControls(selectedControls);
            }

            DeucarianCameraFramingSettings selectedFraming =
                (DeucarianCameraFramingSettings)EditorGUILayout.ObjectField(
                    "Framing Asset",
                    framingSettings,
                    typeof(DeucarianCameraFramingSettings),
                    false);
            if (selectedFraming != framingSettings)
            {
                SelectFramingSettings(selectedFraming);
            }

            if (controls == null || framingSettings == null)
            {
                DeucarianEditorWorkbenchGUI.DrawStatusIconRow(
                    "circle-alert",
                    "Create or select both project assets.",
                    DeucarianEditorStatus.Warning);
            }
            else
            {
                bool canonical =
                    AssetDatabase.GetAssetPath(controls) ==
                    CanonicalControlsAssetPath &&
                    AssetDatabase.GetAssetPath(framingSettings) ==
                    CanonicalFramingAssetPath;
                DeucarianEditorWorkbenchGUI.DrawStatusIconRow(
                    "circle-check",
                    canonical
                        ? "The canonical project camera assets are active."
                        : "Project camera assets are selected.",
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
                RestoreApprovedDefaults,
                "Restore the approved Deucarian Orbit and Fly defaults.");

            DeucarianEditorChrome.EndSection();
        }

        private void DrawFramingProfile()
        {
            DeucarianEditorChrome.DrawSectionHeader("Automatic Framing");
            DeucarianEditorChrome.BeginSection();

            if (framingSettings == null ||
                serializedFramingSettings == null)
            {
                EditorGUILayout.HelpBox(
                    "Create or select a framing asset to edit automatic framing.",
                    MessageType.Info);
                DeucarianEditorChrome.EndSection();
                return;
            }

            serializedFramingSettings.Update();
            DrawControlProperties(serializedFramingSettings);
            if (serializedFramingSettings.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(framingSettings);
            }

            GUILayout.Space(DeucarianEditorWorkbenchGUI.PanelSpacing);
            DeucarianEditorSettingsActions.DrawResetToDefaultsButton(
                RestoreApprovedFramingDefaults,
                "Restore the approved Deucarian automatic framing defaults.");

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
                    "Create Project Assets",
                    DeucarianEditorWorkbenchGUI.PrimaryButtonStyle))
                {
                    SelectControls(CreateProjectControls());
                    SelectFramingSettings(
                        CreateProjectFramingSettings());
                }

                using (new EditorGUI.DisabledScope(
                           controls == null &&
                           framingSettings == null))
                {
                    if (GUILayout.Button(
                        "Ping Active Asset",
                        DeucarianEditorWorkbenchGUI.SecondaryButtonStyle))
                    {
                        Object selected = framingSettings != null
                            ? (Object)framingSettings
                            : controls;
                        Selection.activeObject = selected;
                        EditorGUIUtility.PingObject(selected);
                    }
                }
            }
        }

        private void RestoreApprovedDefaults()
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

        private void RestoreApprovedFramingDefaults()
        {
            if (framingSettings == null)
            {
                return;
            }

            Undo.RecordObject(
                framingSettings,
                "Restore Camera Framing Defaults");
            framingSettings.ResetToDefaults();
            EditorUtility.SetDirty(framingSettings);
            serializedFramingSettings.Update();
        }

        private void SelectControls(DeucarianCameraNavigationControls selected)
        {
            controls = selected;
            serializedControls = controls != null
                ? new SerializedObject(controls)
                : null;
        }

        private void SelectFramingSettings(
            DeucarianCameraFramingSettings selected)
        {
            framingSettings = selected;
            serializedFramingSettings = framingSettings != null
                ? new SerializedObject(framingSettings)
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

        private static DeucarianCameraFramingSettings
            FindPreferredFramingSettings()
        {
            DeucarianCameraFramingSettings canonical =
                AssetDatabase
                    .LoadAssetAtPath<DeucarianCameraFramingSettings>(
                        CanonicalFramingAssetPath);
            if (canonical != null)
            {
                return canonical;
            }

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:DeucarianCameraFramingSettings");
            return guids.Length > 0
                ? AssetDatabase
                    .LoadAssetAtPath<DeucarianCameraFramingSettings>(
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

        private static DeucarianCameraFramingSettings
            CreateProjectFramingSettings()
        {
            EnsureFolder("Assets/Resources/Deucarian");
            DeucarianCameraFramingSettings existing =
                AssetDatabase
                    .LoadAssetAtPath<DeucarianCameraFramingSettings>(
                        CanonicalFramingAssetPath);
            if (existing != null)
            {
                return existing;
            }

            DeucarianCameraFramingSettings created =
                CreateInstance<DeucarianCameraFramingSettings>();
            created.ResetToDefaults();
            AssetDatabase.CreateAsset(
                created,
                CanonicalFramingAssetPath);
            AssetDatabase.SaveAssets();
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

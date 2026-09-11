using Deucarian.Editor;
using UnityEditor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Editor
{
    public sealed class DeucarianCameraNavigationSettingsWindow : EditorWindow
    {
        public const string CanonicalControlsAssetPath = "Assets/Resources/Deucarian/CameraNavigationControls.asset";
        public const string CanonicalFramingAssetPath = "Assets/Resources/Deucarian/CameraFramingSettings.asset";
        private DeucarianEditorPageSession session;

        public static void OpenWindow() => DeucarianEditorToolWindow.Open(DeucarianToolIds.CameraNavigation);
        public static IDeucarianEditorPage CreatePage() => new CameraNavigationPage().Page;

        private void CreateGUI()
        {
            session?.Dispose();
            session = new DeucarianEditorPageSession(this, DeucarianToolIds.CameraNavigation, CreatePage());
        }

        private void OnDisable()
        {
            session?.Dispose(); session = null;
        }

        internal static DeucarianCameraNavigationControls FindPreferredControls()
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

        internal static DeucarianCameraFramingSettings
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

        internal static DeucarianCameraNavigationControls CreateProjectControls()
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

        internal static DeucarianCameraFramingSettings
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

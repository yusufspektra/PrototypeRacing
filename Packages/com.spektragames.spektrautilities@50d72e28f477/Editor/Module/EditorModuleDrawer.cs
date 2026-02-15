using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;


namespace SpektraGames.SpektraUtilities.Editor
{
    public static class EditorModuleDrawer
    {
        private static GUISkin spektraHelperGUISkin = null;

        public static GUISkin SpektraHelperGUISkin
        {
            get
            {
                if (spektraHelperGUISkin == null)
                {
                    spektraHelperGUISkin = Resources.Load<GUISkin>("Module/SpektraHelperGUISkin");
                }

                return spektraHelperGUISkin;
            }
        }

        private static GUIStyle guiStyleForTitle = null;

        private static GUIStyle GuiStyleForTitle
        {
            get
            {
                if (guiStyleForTitle == null)
                    guiStyleForTitle =
                        SpektraHelperGUISkin.customStyles.FirstOrDefault(x => x.name == "ModuleTitleLabel");

                return guiStyleForTitle;
            }
        }

        private static GUIStyle guiStyleForLoadingText = null;

        private static GUIStyle GuiStyleForLoadingText
        {
            get
            {
                if (guiStyleForLoadingText == null)
                {
                    var customStyles = EditorModuleDrawer.SpektraHelperGUISkin.customStyles;
                    guiStyleForLoadingText = customStyles.FirstOrDefault(x => x.name == "GeneralLoadingText");
                }

                return guiStyleForLoadingText;
            }
        }

        private static Dictionary<ModuleTitleBackgroundColor, Texture2D> titleBackgroundTexturePool = new();

        private static Texture2D GetTitleBackgroundTexture(ModuleTitleBackgroundColor titleBackgroundColor)
        {
            if (titleBackgroundTexturePool.TryGetValue(titleBackgroundColor, out Texture2D texture) &&
                texture != null)
            {
                return texture;
            }

            titleBackgroundTexturePool[titleBackgroundColor] =
                Resources.Load<Texture2D>("Module/TitleBackgroundTextures/" + titleBackgroundColor.ToString());
            if (titleBackgroundTexturePool[titleBackgroundColor] == null)
            {
                Debug.LogError("GetTitleBackgroundTexture:: Resources not found for ModuleTitleBackgroundColor." +
                               titleBackgroundColor.ToString());
            }

            return titleBackgroundTexturePool[titleBackgroundColor];
        }

        public enum ModuleTitleBackgroundColor
        {
            Blue = 0,
            Red = 1
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleProperty"></param>
        /// <param name="title"></param>
        /// <param name="titleBackgroundTexture"></param>
        /// <returns>Can draw inner</returns>
        public static bool BeginModule(
            Module module,
            SerializedProperty moduleProperty,
            string title,
            ModuleTitleBackgroundColor titleBackgroundColor)
        {
            EditorGUILayout.BeginVertical();
            SirenixEditorGUI.BeginBox();
            DrawTitle(title, titleBackgroundColor);

            if (module == null)
            {
                Debug.LogError("Module is null");
                return false;
            }
            else if (moduleProperty == null)
            {
                module.SetErrorText("Module property is null");
                return false;
            }
            else if (moduleProperty.serializedObject.isEditingMultipleObjects)
            {
                module.SetErrorText("You can't edit multiple object");
                return false;
            }
            else
            {
                if (module.IsErrorActive)
                {
                    DrawErrorForCurrentModule(module.ErrorText);
                }
            }

            if (module != null)
            {
                EditorGUI.BeginDisabledGroup(module.IsLoadingActive);
            }

            return true;
        }

        public static void EndModule(Module module)
        {
            SirenixEditorGUI.EndBox();
            EditorGUILayout.EndVertical();

            if (module != null)
            {
                EditorGUI.EndDisabledGroup();
                if (module.IsLoadingActive)
                {
                    var innerAreaRect = GUILayoutUtility.GetLastRect();
                    innerAreaRect.y += 28f;
                    innerAreaRect.height -= 28f;
                    SirenixEditorGUI.DrawSolidRect(innerAreaRect, new Color(0f, 0f, 0f, 0.25f));
                    EditorGUI.LabelField(innerAreaRect, module.LoadingText, GuiStyleForLoadingText);
                }
            }
        }

        private static void DrawErrorForCurrentModule(string error)
        {
            SirenixEditorGUI.MessageBox(error, MessageType.Error);
        }

        private static void DrawTitle(string title, ModuleTitleBackgroundColor titleBackgroundColor)
        {
            EditorGUILayout.Space(1f);

            string titleText = title + " <color=#8C8C8C><size=9>by Spektra Games</size></color>";

            Rect titleTextRect = EditorGUILayout.GetControlRect(GUILayout.Height(28f));
            titleTextRect = titleTextRect.AddY(-5f);
            Rect titleBackgroundRect = titleTextRect;
            titleBackgroundRect = titleBackgroundRect.HorizontalPadding(-3.2f);

            EditorGUI.DrawPreviewTexture(titleBackgroundRect, GetTitleBackgroundTexture(titleBackgroundColor));
            EditorGUI.LabelField(titleTextRect, titleText, GuiStyleForTitle);
        }

        public class Module
        {
            private string _errorText = null;
            private string _loadingText = null;

            public string ErrorText
            {
                get { return _errorText; }
            }

            public string LoadingText
            {
                get { return _loadingText; }
            }

            public void SetErrorText(string errorText)
            {
                this._errorText = errorText;
            }

            public void SetLoadingText(string loadingText)
            {
                this._loadingText = loadingText;
            }

            public void ClearError()
            {
                _errorText = null;
            }

            public void ClearLoading()
            {
                _loadingText = null;
            }

            public bool IsErrorActive
            {
                get { return !string.IsNullOrEmpty(_errorText); }
            }

            public bool IsLoadingActive
            {
                get { return !string.IsNullOrEmpty(_loadingText); }
            }
        }
    }
}
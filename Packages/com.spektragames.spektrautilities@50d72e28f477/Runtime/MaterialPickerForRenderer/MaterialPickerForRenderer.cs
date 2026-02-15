using System.Collections.Generic;
using Sirenix.Utilities;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace _Game.Scripts.Utils
{
    [System.Serializable]
    public class MaterialPickerForRenderer
    {
        [SerializeField] private Renderer renderer = null;
        [SerializeField] private int materialIndex = -1;

        public Renderer Renderer
        {
            get { return renderer; }
            set { renderer = value; }
        }

        public int MaterialIndex
        {
            get { return materialIndex; }
            set { materialIndex = value; }
        }

        #region Material Property Block

        private MaterialPropertyBlock _propertyBlock = null;
        public MaterialPropertyBlock PropertyBlock
        {
            get
            {
                if (_propertyBlock == null)
                {
                    if (IsValid())
                    {
                        _propertyBlock = new MaterialPropertyBlock();
                        //renderer.GetPropertyBlock(_propertyBlock, materialIndex); // It's causing a bug??
                        renderer.GetPropertyBlock(_propertyBlock);
                    }
                }

                return _propertyBlock;
            }
        }

        public void SetMaterialColorWithMPB(string propertyName, Color color)
        {
            if (IsValid())
            {
                PropertyBlock.SetColor(propertyName, color);
                renderer.SetPropertyBlock(PropertyBlock, materialIndex);
            }
        }

        public void SetMaterialColorWithMPB(int propertyName, Color color)
        {
            if (IsValid())
            {
                PropertyBlock.SetColor(propertyName, color);
                renderer.SetPropertyBlock(PropertyBlock, materialIndex);
            }
        }

        public Color GetMaterialColorWithMPB(string propertyName)
        {
            if (IsValid())
            {
                return PropertyBlock.GetColor(propertyName);
            }

            return default;
        }

        public void SetMaterialTextureWithMPB(string propertyName, Texture texture)
        {
            if (IsValid())
            {
                PropertyBlock.SetTexture(propertyName, texture);
                renderer.SetPropertyBlock(PropertyBlock, materialIndex);
            }
        }

        public void SetMaterialTextureWithMPB(int propertyName, Texture texture)
        {
            if (IsValid())
            {
                PropertyBlock.SetTexture(propertyName, texture);
                renderer.SetPropertyBlock(PropertyBlock, materialIndex);
            }
        }

        public Texture GetMaterialTextureWithMPB(string propertyName)
        {
            if (IsValid())
            {
                return PropertyBlock.GetTexture(propertyName);
            }

            return default;
        }

        public void SetMaterialFloatWithMPB(string propertyName, float val)
        {
            if (IsValid())
            {
                PropertyBlock.SetFloat(propertyName, val);
                renderer.SetPropertyBlock(PropertyBlock, materialIndex);
            }
        }

        public void SetMaterialFloatWithMPB(int propertyName, float val)
        {
            if (IsValid())
            {
                PropertyBlock.SetFloat(propertyName, val);
                renderer.SetPropertyBlock(PropertyBlock, materialIndex);
            }
        }

        public float GetMaterialFloatWithMPB(string propertyName)
        {
            if (IsValid())
            {
                return PropertyBlock.GetFloat(propertyName);
            }

            return default;
        }

        #endregion

        public bool IsValid()
        {
            return IsValid(out Material _);
        }

        public bool IsValid(out Material sharedMaterial)
        {
            if (renderer != null &&
                renderer.sharedMaterials != null &&
                renderer.sharedMaterials.HaveIndex(materialIndex) &&
                renderer.sharedMaterials[materialIndex] != null)
            {
                sharedMaterial = renderer.sharedMaterials[materialIndex];
                return true;
            }

            sharedMaterial = null;
            return false;
        }

        public Material GetMaterialAsShared()
        {
            if (IsValid(out Material material))
                return material;

            return null;
        }
#if UNITY_EDITOR
        public class TrafficRoadLevelDataDrawer : OdinValueDrawer<MaterialPickerForRenderer>
        {
            private string[] _optionsForPopup = null;

            protected override void Initialize()
            {
                base.Initialize();

                RefreshOptionsForPopup();
            }

            protected override void DrawPropertyLayout(GUIContent label)
            {
                MaterialPickerForRenderer value = this.ValueEntry.SmartValue;

                bool isDirty = false;
                Rect rect = EditorGUILayout.GetControlRect();

                bool pushedColor = false;
                if (!value.IsValid())
                {
                    GUIHelper.PushColor(Color.red);
                    pushedColor = true;
                }

                if (label != null && !string.IsNullOrEmpty(label.text))
                    rect = EditorGUI.PrefixLabel(rect, label);

                this.ValueEntry.TryGetSerializedPropertyObject(out UnityEngine.Object serializedObject);

                var oldRenderer = value.Renderer;
                value.Renderer =
                    EditorGUI.ObjectField(rect.AlignLeft(rect.width * 0.65f), value.Renderer, typeof(Renderer), true) as
                        Renderer;
                if (value.Renderer != oldRenderer)
                {
                    RefreshOptionsForPopup();
                    value.MaterialIndex = 0;
                    isDirty = true;
                }

                rect = rect.AlignRight(rect.width * 0.35f);

                if (value.Renderer == null)
                {
                    if (value.materialIndex != -1)
                    {
                        value.MaterialIndex = -1;
                        isDirty = true;
                    }

                    GUIHelper.PushIsBoldLabel(true);
                    GUIHelper.PushColor(Color.white);
                    EditorGUI.LabelField(rect, "Pick a Renderer", EditorStyles.centeredGreyMiniLabel);
                    GUIHelper.PopColor();
                    GUIHelper.PopIsBoldLabel();
                }
                else
                {
                    if (_optionsForPopup == null)
                        RefreshOptionsForPopup();

                    int oldIndex = value.materialIndex;
                    value.materialIndex = EditorGUI.Popup(rect, value.materialIndex, _optionsForPopup);

                    if (value.materialIndex != oldIndex)
                    {
                        isDirty = true;
                    }
                }

                if (pushedColor)
                    GUIHelper.PopColor();

                this.ValueEntry.SmartValue = value;
                if (serializedObject != null && isDirty)
                {
                    Property.Tree.UnitySerializedObject.SetIsDifferentCacheDirty();
                    EditorUtility.SetDirty(serializedObject);
                }
            }

            private void RefreshOptionsForPopup()
            {
                MaterialPickerForRenderer value = this.ValueEntry.SmartValue;

                if (value == null || value.Renderer == null)
                    return;

                var materials = value.Renderer.sharedMaterials;

                _optionsForPopup = new string[materials.Length];

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null)
                    {
                        _optionsForPopup[i] = i.ToString() + " NULL";
                    }
                    else
                    {
                        _optionsForPopup[i] = i.ToString() + " \"" + materials[i].name + "\"";
                    }
                }
            }
        }
#endif
    }
}
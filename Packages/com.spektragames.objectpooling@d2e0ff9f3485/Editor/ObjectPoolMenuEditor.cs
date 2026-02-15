using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DG.DemiEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    public class ObjectPoolMenuEditor : OdinMenuEditorWindow
    {
        protected virtual string PoolObjectFolderPath { get; } = "Assets/_Game/Packages/ObjectPooling/Demo/Data";
        
        // public virtual string GetPoolObjectFolderPath() => "Assets/_Game/ObjectPooling/Demo/Data";
        
        private static List<Type> _cachedEnumTypes = null;
        private static Dictionary<Type, List<object>> _cachedFieldTypes = null;

        private static List<Type> CachedEnumTypes
        {
            get
            {
                if (_cachedEnumTypes == null)
                {
                    _cachedEnumTypes = new();
                    _cachedFieldTypes = new Dictionary<Type, List<object>>();

                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (var i = 0; i < assemblies.Length; i++)
                    {
                        var types = assemblies[i].GetTypes()
                            .Where(x => !x.IsAbstract && !x.IsInterface &&
                                        x.BaseType != null && x.IsSubclassOf(typeof(PoolEnum)))
                            .Select(x => x);
                        var rootTypes = types.Where(type => !types.Any(
                            otherType => otherType != type && otherType.IsSubclassOf(type))).ToList();

                        foreach (var type in rootTypes)
                        {
                            _cachedEnumTypes.Add(type);
                            
                            var fieldTypes = type.GetFields(BindingFlags.Static | BindingFlags.Public)
                                .Where(field => field.FieldType.IsSubclassOf(typeof(PoolEnum)))
                                .ToList();

                            _cachedFieldTypes.TryGetValue(type, out var fieldValueList);
                            fieldValueList ??= new List<object>();
                            for (var j = 0; j < fieldTypes.Count; j++)
                            {
                                var fieldValue = fieldTypes[j].GetValue(null);
                                var poolEnum = fieldValue as PoolEnum;
                                if (fieldTypes[j].Name != poolEnum.EnumName)
                                {
                                    Debug.LogError($"Static field {fieldTypes[j].Name} is corrupt");
                                }
                                fieldValueList.Add(fieldValue);
                            }

                            _cachedFieldTypes[type] = fieldValueList;
                        }
                    }
                }

                return _cachedEnumTypes;
            }
        }
        private static Dictionary<Type, List<object>> CachedFieldTypes
        {
            get
            {
                if (_cachedFieldTypes == null)
                {
                    var dummy = CachedEnumTypes;
                }
                return _cachedFieldTypes;
            }
        }
        
        [MenuItem("Tools/Object Pooling/Object Pooling Editor")]
        public static void ShowWindow()
        {
            GetWindow<ObjectPoolMenuEditor>().Show();
        }
        
        protected override OdinMenuTree BuildMenuTree()
        {
            CreatePoolIfNeeded();
            
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;
            tree.Config.DrawSearchToolbar = true;

            var poolCategoryList = PoolContainer.Instance.poolCategoryList;
            for (var i = 0; i < poolCategoryList.Count; i++)
            {
                var categoryName = poolCategoryList[i].categoryType.ToString(); 
                tree.Add(categoryName, new PoolItemCategoryMenu(this, categoryName));
                
                tree.AddAllAssetsAtPath(categoryName, $"{PoolObjectFolderPath}/{categoryName}", 
                    typeof(PoolItem), true, true)
                    .SortMenuItemsByName(PoolItemComparer);
                // var poolItemDictionary = poolCategoryList[i].poolItemDictionary;
                // foreach (var poolPair in poolItemDictionary)
                // {
                // }
                
                // TODO: Access all pool items in that category
                // var vehicleAccessoryItemTypes = VehicleAccessoryItemContainer.Instance.vehicleAccessoryTypes;
                // foreach (var vehicleAccessoryItemType in vehicleAccessoryItemTypes)
                // {
                //     tree.Add("Vehicle Accessory/" + vehicleAccessoryItemType, 
                //         new VehicleAccessoryItemMenu(vehicleAccessoryItemType));
                // }
                // tree.AddAllAssetsAtPath("Vehicle Accessory", VehicleAccessoriesFolderPath, 
                //     typeof(VehicleAccessoryItemDataSO), true, false)
                //     .SortMenuItemsByName(AccessoryComparer);
            }
            
            return tree;
        }

        private void CreatePoolIfNeeded()
        {
            var enumTypes = CachedEnumTypes;
            for (var i = 0; i < enumTypes.Count; i++)
            {
                var enumType = enumTypes[i];
                var enumFolderPath = Path.Combine(PoolObjectFolderPath, enumType.Name);

                // Check if the folder exists
                if (!AssetDatabase.IsValidFolder(enumFolderPath))
                {
                    // Create the folder
                    var parentFolder = PoolObjectFolderPath;
                    AssetDatabase.CreateFolder(parentFolder, enumType.Name);
                    AssetDatabase.CreateFolder($"{parentFolder}/{enumType.Name}", "Items");
                    Debug.Log($"Created folder for {enumType.Name} at {enumFolderPath}");
                }

                // Create the ScriptableObject for the category if it doesn't exist
                string soPath = Path.Combine(enumFolderPath, $"{enumType.Name}.asset");
                var poolCategory = AssetDatabase.LoadAssetAtPath<PoolCategory>(soPath);

                if (poolCategory == null)
                {
                    // Create new PoolCategory SO
                    poolCategory = ScriptableObject.CreateInstance<PoolCategory>();
                    poolCategory.categoryType = enumType.Name;// (PoolEnum)Enum.Parse(typeof(PoolEnum), enumType.Name);
                    // poolCategory.poolItemDictionary = new PoolItemDictionary();
                    poolCategory.poolItemList = new();

                    // Save SO in the appropriate folder
                    AssetDatabase.CreateAsset(poolCategory, soPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Created ScriptableObject for {enumType.Name} at {soPath}");
                }

                // Add the ScriptableObject reference to PoolContainer if not already added
                if (!PoolContainer.Instance.poolCategoryList.Contains(poolCategory))
                {
                    PoolContainer.Instance.poolCategoryList.Add(poolCategory);
                    EditorUtility.SetDirty(PoolContainer.Instance);  // Mark PoolContainer as dirty to register the change
                    Debug.Log($"Added {enumType.Name} to PoolContainer poolCategoryList");
                }
            }

            // Refresh the AssetDatabase after creation
            AssetDatabase.Refresh();
        }
        
        protected virtual int PoolItemComparer(OdinMenuItem item1, OdinMenuItem item2)
        {
            if (item1.ChildMenuItems.Count != 0 || item2.ChildMenuItems.Count != 0)
                return 0;

            var poolItem1 = (PoolItem)item1.Value;
            var poolItem2 = (PoolItem)item2.Value;
            var poolItem1Id = (int)poolItem1.type;
            var poolItem2Id = (int)poolItem2.type;
            return poolItem1Id.CompareTo(poolItem2Id);
        }
        
        protected override void OnBeginDrawEditors()
        {
            if (this.MenuTree != null &&
                this.MenuTree.Selection != null)
            {
                OdinMenuTreeSelection selected = this.MenuTree.Selection;

                if (this.MenuTree.Selection.SelectedValue != null &&
                    this.MenuTree.Selection.SelectedValue is PoolItem poolObject)
                {
                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        var poolType = (selected.SelectedValue as PoolItem).type; 
                        var title = $"{poolType.Category.Nicify()} - {poolType.EnumName.Nicify()}";
                        SirenixEditorGUI.Title(title, null, TextAlignment.Left, true);
                        
                        if (SirenixEditorGUI.ToolbarButton(SdfIconType.BinocularsFill))
                        {
                            EditorGUIUtility.PingObject(poolObject);
                        }
                        
                        if (SirenixEditorGUI.ToolbarButton(SdfIconType.TrashFill))
                        {
                            if (EditorUtility.DisplayDialog("Delete Asset",
                                    "Are you sure you want to delete '" + poolObject.name + "' asset?", "Delete!", "Cancel"))
                            {
                                string path = AssetDatabase.GetAssetPath(poolObject);
                                AssetDatabase.DeleteAsset(path);
                                AssetDatabase.SaveAssets();
                                RemoveNullsFromDataContainers();
                            }
                        }
                        
                    }
                    SirenixEditorGUI.EndHorizontalToolbar();
                }
                else if (this.MenuTree.Selection.SelectedValue != null &&
                         this.MenuTree.Selection.SelectedValue is PoolItemCategoryMenu categoryMenu)
                {
                    SirenixEditorGUI.BeginHorizontalToolbar();

                    var title = categoryMenu.PoolCategory.categoryType.Nicify();
                    SirenixEditorGUI.Title(title, null, TextAlignment.Left, true);
                    
                    if (SirenixEditorGUI.ToolbarButton(SdfIconType.BinocularsFill))
                    {
                        EditorGUIUtility.PingObject(categoryMenu.PoolCategory);
                    }
                    
                    SirenixEditorGUI.EndHorizontalToolbar();
                }
            }
        }
        
        private class PoolItemCategoryMenu
        {
            [InlineEditor(Expanded = true)]
            public PoolItem item = null;

            // [SerializeField, InlineEditor(Expanded = true)]
            private PoolCategory _poolCategory;
            public PoolCategory PoolCategory => _poolCategory;
            private ObjectPoolMenuEditor _menuEditor;

            public PoolItemCategoryMenu(ObjectPoolMenuEditor menuEditor, string categoryName)
            {
                RemoveNullsFromDataContainers();
                this._poolCategory =
                    PoolContainer.Instance.poolCategoryList.FirstOrDefault(o => o.categoryType == categoryName);
                _menuEditor = menuEditor;
                if (_poolCategory == null)
                {
                    Debug.LogError($"Pool category '{categoryName}' not found");
                    return;
                }
                
                item = ScriptableObject.CreateInstance<PoolItem>();
                foreach (var cachedFieldType in CachedFieldTypes)
                {
                    if (cachedFieldType.Key.Name == categoryName)
                    {
                        item.type = cachedFieldType.Value.FirstOrDefault() as PoolEnum;
                        break;
                    }
                }
            }
            
            [Button(ButtonSizes.Medium)]
            private void CreateNew()
            {
                if (string.IsNullOrEmpty(item.type.UniqueIdentifier))
                {
                    EditorUtility.DisplayDialog("Error", "Enum type is not set", "OK");
                    return;
                }

                if (_poolCategory.poolItemList.FirstOrDefault(o=>o.type == item.type) != null)
                {
                    EditorUtility.DisplayDialog("Error", $"Enum type {item.type.EnumName} is already defined before", "OK");
                    return;
                }

                if (item.type.Category != _poolCategory.categoryType)
                {
                    EditorUtility.DisplayDialog("Error", "Enum category must be the same with pool category", "OK");
                    return;
                }
                
                if (item.prefab == null)
                {
                    EditorUtility.DisplayDialog("Error", "Prefab cannot be null", "OK");
                    return;
                }

                string savePath = $"{_menuEditor.PoolObjectFolderPath}/{_poolCategory.categoryType}/Items/{item.type.EnumName}.asset";
                // string savePath = VehicleAccessoriesFolderPath + "/" + item.vehicleAccessoryType + "/" + item.itemName + ".asset";
                AssetDatabase.CreateAsset(item, savePath);
                AssetDatabase.SaveAssets();
                var savedAsset = AssetDatabase.LoadAssetAtPath<PoolItem>(savePath);
                _poolCategory.poolItemList.Add(savedAsset);
                PoolContainer.Instance.poolObjectDictionary[savedAsset.type] = savedAsset;
                RemoveNullsFromDataContainers();
                EditorUtility.SetDirty(_poolCategory);
                EditorUtility.SetDirty(PoolContainer.Instance);
            }
        }

        private static void RemoveNullsFromDataContainers()
        {
            var changeDetected = false;
            
            var keysToRemove = new List<PoolEnum>();
            foreach (var keyValuePair in PoolContainer.Instance.poolObjectDictionary)
            {
                if (keyValuePair.Value == null)
                {
                    keysToRemove.Add(keyValuePair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                PoolContainer.Instance.poolObjectDictionary.Remove(key);
                changeDetected = true;
            }
            
            for (var i = PoolContainer.Instance.poolCategoryList.Count - 1; i >= 0; i--)
            {
                if (PoolContainer.Instance.poolCategoryList[i] == null)
                {
                    PoolContainer.Instance.poolCategoryList.RemoveAt(i);
                    changeDetected = true;
                }
                
                for (var j = PoolContainer.Instance.poolCategoryList[i].poolItemList.Count - 1; j >= 0; j--)
                {
                    if (PoolContainer.Instance.poolCategoryList[i].poolItemList[j] == null)
                    {
                        PoolContainer.Instance.poolCategoryList[i].poolItemList.RemoveAt(j);
                        changeDetected = true;
                    }
                }
            }

            if (changeDetected)
            {
                EditorUtility.SetDirty(PoolContainer.Instance);
                Debug.LogError("Change detected in PoolContainer");
            }
        }
    }
}

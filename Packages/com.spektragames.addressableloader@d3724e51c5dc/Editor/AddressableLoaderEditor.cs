using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpektraGames.AddressableLoader.Runtime;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace SpektraGames.AddressableLoader.Editor
{
    public class AddressableLoaderEditor
    {
        [MenuItem("Tools/Addressable Loader/Create Assets Mapping File")]
        public static void GenerateAssetMappingFileForDebug()
        {
            string assetMappingFilePath = "Assets/Resources/AddressableLoader/AddressableMapping.json";

            Helpers.File.CreateDirectoriesIfNotExistOnPath(assetMappingFilePath);

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            List<AssetEntryForDebug> assetEntries = new List<AssetEntryForDebug>();

            var groups = settings.groups;
            if (groups != null)
            {
                for (var i = 0; i < groups.Count; i++)
                {
                    if (groups[i] == null)
                        continue;

                    var entries = groups[i].entries.ToList();
                    if (entries != null)
                    {
                        for (int j = 0; j < entries.Count; j++)
                        {
                            if (entries[j] != null)
                            {
                                var entry = entries[j];

                                assetEntries.Add(new AssetEntryForDebug()
                                {
                                    guid = entry.guid.ToLower().Trim(),
                                    assetName = entry.MainAsset.name,
                                    assetPath = entry.AssetPath
                                });
                            }
                        }
                    }
                }
            }

            File.WriteAllText(assetMappingFilePath, assetEntries.SerializeObject(true));
            AssetDatabase.ImportAsset(assetMappingFilePath);
            AssetDatabase.Refresh();
        }
    }
}
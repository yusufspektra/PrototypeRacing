using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CapyTools.Common
{
    /// <summary>
    /// TagService is a scriptable object that holds all the tags in the project
    /// and allows access to them at runtime.
    /// </summary>
    public class TagService : ScriptableObject
    {
        [SerializeField] private string[] tags;

        /// <summary>
        /// Get all the tags used in the project.
        /// </summary>
        public string[] Tags => tags;

        public static string[] GetTags()
        {
            return Instance.Tags;
        }

        protected static TagService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<TagService>("TagService");
                }
                if (instance == null)
                {
                    instance = CreateInstance<TagService>();

                    if (Application.isPlaying)
                    {
                        // Fallback default tags
                        instance.tags = new string[]
                        {
                            "Untagged", "Respawn", "Finish", "EditorOnly",
                            "MainCamera", "Player", "GameController"
                        };
                    }
                }

                return instance;
            }
        }

        private static TagService instance;

        /// <summary>
        /// Allows the editor script to update tags.
        /// </summary>
        public void SetTags(string[] newTags)
        {
            tags = newTags;
        }
    }
}

using Sirenix.OdinInspector;
using UnityEngine;

namespace ForDebug
{
    public class TimeScaleAdjuster : MonoBehaviour
    {
#if UNITY_EDITOR
        [ShowInInspector, DisableInEditorMode]
        private bool Enabled { get; set; } = false;

        [ShowInInspector, DisableInEditorMode]
        [Range(0f, 3f)]
        private float TimeScale { get; set; } = 1f;

        private void Start()
        {
        }

        private void Update()
        {
            if (Enabled)
            {
                if (!Mathf.Approximately(Time.timeScale, TimeScale))
                    Time.timeScale = TimeScale;
            }
        }
#endif
    }
}
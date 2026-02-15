using Sirenix.OdinInspector;
using UnityEngine;

namespace ForDebug
{
    public class LowFpsSimulator : MonoBehaviour
    {
#if UNITY_EDITOR
        [ShowInInspector, DisableInEditorMode]
        private bool Enabled { get; set; } = false;
        [MinMaxSlider(0, 120)]
        [ShowInInspector, DisableInEditorMode]
        private Vector2Int LowFPSValues { get; set; } = new Vector2Int(60, 60);

        private void Start()
        {
        }

        private void Update()
        {
            if (Enabled)
                Application.targetFrameRate = UnityEngine.Random.Range(LowFPSValues.x, LowFPSValues.y);
        }
#endif
    }
}
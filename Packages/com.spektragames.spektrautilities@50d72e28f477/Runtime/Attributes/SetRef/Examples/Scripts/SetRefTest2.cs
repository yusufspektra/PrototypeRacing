using UnityEngine;
using UnityEngine.UI;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public class SetRefTest2 : MonoBehaviour
    {
        // [SerializeField, SetRef("Canvas", "Image_Test", typeof(Image))]
        // private Image _testImage;
        //
        // [SerializeField, SetRef("Canvas", "Transform_Test")]
        // private Transform _transformTest;
        
        [SerializeField, SetRef(2, "Target")]
        private Transform _targetTransform;
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public class SetRefTest : MonoBehaviour
    {
        [SerializeField, SetRef("Image_Test", typeof(Image))]
        private Image _testImage;
        
        [SerializeField, SetRef(typeof(Button))]
        private Button _testButton;
        
        [SerializeField, SetRef("Transform_Test")]
        private Transform _testTransform;
    }
}
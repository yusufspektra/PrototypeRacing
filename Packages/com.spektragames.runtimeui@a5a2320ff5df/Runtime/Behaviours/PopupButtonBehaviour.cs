using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpektraGames.RuntimeUI.Runtime
{
    public class PopupButtonBehaviour : MonoBehaviour
    {
        [SerializeField, SetRef(typeof(Button))]
        private Button _button = null;
        public Button Button => _button;
        
        [SerializeField, SetRef(typeof(TMP_Text))]
        private TMP_Text _text = null;

        private Action action = null;
        
        private void Awake()
        {
        }

        private void OnDestroy()
        {
            action = null;
        }

        private void OnDisable()
        {
            action = null;
        }

        private void Start()
        {
        
        }

        public void SetButton(string text, Action action)
        {
            _text?.SetText(text);
            this.action = action;
        }
    }
}
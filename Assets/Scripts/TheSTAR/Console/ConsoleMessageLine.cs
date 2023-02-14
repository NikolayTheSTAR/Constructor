using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TheSTAR.Console
{
    public class ConsoleMessageLine : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        public string Message => _text.text;
        public Color Color => _text.color;

        public void SetMessage(ConsoleMessageLine ml)
        {
            SetMessage(ml.Message, ml.Color);
        }

        public void SetMessage(string message)
        {
            SetMessage(message, Color.white);
        }

        public void SetMessage(string message, Color color)
        {
            _text.text = message;
            _text.color = color;
        }
    }
}
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public class AutoResizeMultilineAttribute : PropertyAttribute
    {
        public int MinLines { get; private set; }  // Minimum number of lines
        public int MaxLines { get; private set; }  // Maximum number of lines

        public AutoResizeMultilineAttribute(int minLines = 1, int maxLines = 5)
        {
            MinLines = minLines;
            MaxLines = maxLines;
        }
    }
}
using UnityEngine;

namespace VertigoWheel.UI
{
    /// <summary>Pre-resolved, presentation-ready content for a single wheel chamber.</summary>
    public readonly struct SliceDisplay
    {
        public readonly Sprite Icon;
        public readonly string Text;
        public readonly bool IsBomb;

        public SliceDisplay(Sprite icon, string text, bool isBomb)
        {
            Icon = icon;
            Text = text;
            IsBomb = isBomb;
        }
    }
}

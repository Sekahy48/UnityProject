using Unity.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View
{
    public static class UIElementUtils
    {
        public static void AddAmountLabel(VisualElement element, int amount)
        {
            Label amountLabel = new Label(amount.ToString());
            amountLabel.AddToClassList("amount-label");
            element.Add(amountLabel);
        }

        public static void SetBackgroundTexture(VisualElement element, string texturePath)
        {
            Texture2D tex = TextureCache.Instance.Get(texturePath);
            if (tex != null)
            {
                element.style.backgroundImage = new StyleBackground(tex);
                element.AddToClassList("icon-fit");
            } 
            else
                Debug.LogWarning($"No texture found at '{texturePath}'");
        }
    }
}
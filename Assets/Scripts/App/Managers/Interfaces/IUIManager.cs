using UnityEngine;
using UnityEngine.UI;

namespace Prototype.TopDown2DNetworked
{
    public interface IUIManager
    {
        GameObject Canvas { get; set; }
        CanvasScaler CanvasScaler { get; set; }
        IUIElement CurrentPage { get; set; }
        void SetPage<T>(bool hideAll = false) where T : IUIElement;
        IUIElement GetPage<T>() where T : IUIElement;
        void HideAllPages();
    }
}
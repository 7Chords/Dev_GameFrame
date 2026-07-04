using UnityEngine;

namespace SCFrame.UI
{
    /// <summary>
    /// UIMono配置抽象基类
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class _ASCUIMonoBase : MonoBehaviour
    {
        [Header("CanvasGroup")]
        public CanvasGroup canvasGroup;
        [Header("淡入时间")]
        public float fadeInDuration;
        [Header("淡出时间")]
        public float fadeOutDuration;

    }
}

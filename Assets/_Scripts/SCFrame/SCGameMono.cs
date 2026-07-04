using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 功能相当于全局Mono
    /// </summary>
    public class SCGameMono : SingletonPersistent<SCGameMono>
    {

        [Header("UI")]
        public Canvas mainCanvas;
        public GameObject fullLayerRoot;
        public GameObject additionLayerRoot;
        public GameObject topLayerRoot;

        [Header("Camera")]
        public Camera gameCamera;

        [Header("Pool")]
        public GameObject poolRoot;

        [Header("Audio")]
        public GameObject bgmRoot;
        public GameObject sfxRoot;

    }
}

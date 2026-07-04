using System;

namespace SCFrame
{
    [Serializable]
    public class SCSettingData
    {
        public ELanguageType languageType = ELanguageType.zh_CN;
        public float bgmVolumeFactor = 0.15f;
        public float sfxVolumeFactor = 1f;
        public bool crtEnabled;
    }
}

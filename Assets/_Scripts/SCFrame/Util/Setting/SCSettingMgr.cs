using UnityEngine;

namespace SCFrame
{
    public class SCSettingMgr : Singleton<SCSettingMgr>
    {
        private const string LanguagePrefKey = "SCSetting.LanguageType";
        private const string BgmVolumePrefKey = "SCSetting.BgmVolume";
        private const string SfxVolumePrefKey = "SCSetting.SfxVolume";
        private const string CrtEnabledPrefKey = "SCSetting.CrtEnabled";

        public SCSettingData data = new SCSettingData();

        public ELanguageType languageType
        {
            get => data.languageType;
            set
            {
                if (data.languageType == value)
                    return;

                data.languageType = value;
                PlayerPrefs.SetInt(LanguagePrefKey, (int)data.languageType);
                PlayerPrefs.Save();
            }
        }

        public float bgmVolumeFactor
        {
            get => data.bgmVolumeFactor;
            set
            {
                data.bgmVolumeFactor = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(BgmVolumePrefKey, data.bgmVolumeFactor);
                PlayerPrefs.Save();
            }
        }

        public float sfxVolumeFactor
        {
            get => data.sfxVolumeFactor;
            set
            {
                data.sfxVolumeFactor = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(SfxVolumePrefKey, data.sfxVolumeFactor);
                PlayerPrefs.Save();
            }
        }

        public bool crtEnabled
        {
            get => data.crtEnabled;
            set
            {
                if (data.crtEnabled == value)
                    return;

                data.crtEnabled = value;
                PlayerPrefs.SetInt(CrtEnabledPrefKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public override void OnInitialize()
        {
            data.languageType = (ELanguageType)PlayerPrefs.GetInt(LanguagePrefKey, (int)ELanguageType.zh_CN);
            data.bgmVolumeFactor = PlayerPrefs.GetFloat(BgmVolumePrefKey, data.bgmVolumeFactor);
            data.sfxVolumeFactor = PlayerPrefs.GetFloat(SfxVolumePrefKey, data.sfxVolumeFactor);
            data.crtEnabled = PlayerPrefs.GetInt(CrtEnabledPrefKey, 0) == 1;
        }
    }
}

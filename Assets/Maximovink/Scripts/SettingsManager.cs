using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MaximovInk
{
    public class SettingsManager : MonoBehaviour
    {
        public Text Antialising, Vsync , Language;

        private void Start()
        {
            Load();
        }

        public void Display()
        {
            Vsync.text = LanguageManager.instance.GetValueByKey("_vsync");
            Antialising.text = LanguageManager.instance.GetValueByKey("_aa");
            Language.text = LanguageManager.instance.GetValueByKey("_language");
        }

        public void Load()
        {
            QualitySettings.vSyncCount = PlayerPrefs.GetInt("vsync");
            QualitySettings.antiAliasing = PlayerPrefs.GetInt("aa");
            LanguageManager.instance.isEnglish = PlayerPrefs.GetInt("lang",0) == 0;
        }

        public void ChangeLanguage()
        {
            LanguageManager.instance.isEnglish = !LanguageManager.instance.isEnglish;
            PlayerPrefs.SetInt("lang",LanguageManager.instance.isEnglish ? 0 : 1);
            Display();
        }

        public void IncreaseAA()
        {
            if (QualitySettings.antiAliasing == 8)
                QualitySettings.antiAliasing = 0;
            else if (QualitySettings.antiAliasing != 0)
                QualitySettings.antiAliasing *= 2;
            else
                QualitySettings.antiAliasing = 2;
            PlayerPrefs.SetInt("aa",  QualitySettings.antiAliasing);
            
            Display();
        }

        public void ToggleVSync()
        {
            if (QualitySettings.vSyncCount == 0)
            {
                QualitySettings.vSyncCount = 1;
            }
            else if (QualitySettings.vSyncCount == 1)
            {
                QualitySettings.vSyncCount = 2;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }
            PlayerPrefs.SetInt("vsync",  QualitySettings.vSyncCount);
            
            Display();
        }

        public void Close()
        {
            GameManager.Instance.PauseMenu.SetActive(SceneManager.GetActiveScene().name != "Menu");
            GameManager.Instance.MainMenu.SetActive(SceneManager.GetActiveScene().name == "Menu");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class TranslatableText : MonoBehaviour
    {
        public string key;

        private Text ui_text;

        private void Start()
        {
            ui_text = GetComponent<Text>();
            if (ui_text == null)
            {
                Destroy(gameObject);
                return;
            }

            LanguageManager.instance.onLanguageChanged += OnLanguageChanged;
            ui_text.text = LanguageManager.instance.GetValueByKey(key);
        }

        private void OnLanguageChanged()
        {
            ui_text.text = LanguageManager.instance.GetValueByKey(key);
        }
    }
}
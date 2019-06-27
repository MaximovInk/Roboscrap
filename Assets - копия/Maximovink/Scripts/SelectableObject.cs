using UnityEngine;

namespace MaximovInk
{
    public class SelectableObject : MonoBehaviour
    {
        protected string message = string.Empty;
        public TextMesh text;

        public bool Selected { get; private set; }

        private void Start()
        {
            OnHide();
        }

        public virtual void OnShow()
        {
            text.gameObject.SetActive(true);
            text.text = message;
            Selected = true;
        }

        public void OnHide()
        {
            text.gameObject.SetActive(false);
            Selected = false;
        }

    }
}
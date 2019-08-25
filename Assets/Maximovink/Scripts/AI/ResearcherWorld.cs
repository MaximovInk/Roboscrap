using UnityEngine;

namespace MaximovInk.AI
{
    public class ResearcherWorld : MonoBehaviour
    {
        public float iterationsDelay = 0.1f;

        private float _timer;


        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > iterationsDelay)
            {
                _timer = 0;

            }
        }
    }
}
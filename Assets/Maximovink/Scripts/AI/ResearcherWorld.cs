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

                var pos = ChunkManager.Instance.WorldToChunk(transform.position);

                var d = ChunkManager.Instance.LoadChunkData(pos.x, pos.y);
                ChunkManager.Instance.SaveChunkData(d);

                MapViewer.instance.InspectChunk(ChunkManager.Instance.LoadChunkData(pos.x,pos.y));
            }
        }
    }
}
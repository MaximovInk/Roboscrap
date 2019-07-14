using UnityEngine;

namespace MaximovInk
{
    public class BackgroundScroller : MonoBehaviour
    {
        private Transform cam;
        private Vector2 half_size;
        private Vector2 cam_size_half;

        void Start () {
            cam = Camera.main.transform;
            half_size = GetComponent<SpriteRenderer>().bounds.size / 2;
            cam_size_half = new Vector2(Camera.main.aspect * 2f * Camera.main.orthographicSize, 2f * Camera.main.orthographicSize)/2;
        }
	
        void Update () {
            if (cam.position.x+cam_size_half.x > transform.position.x + half_size.x || cam.position.x - cam_size_half.x < transform.position.x - half_size.x)
            {
                transform.position = new Vector2(cam.transform.position.x,transform.position.y);
            }
            if (cam.position.y + cam_size_half.y > transform.position.y + half_size.y || cam.position.y - cam_size_half.y < transform.position.y - half_size.y)
            {
                transform.position = new Vector2(transform.position.x, cam.transform.position.y);
            }
        }
    }
}
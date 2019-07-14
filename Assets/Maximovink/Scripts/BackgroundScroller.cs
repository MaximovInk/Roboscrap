using UnityEngine;

namespace MaximovInk
{
    public class BackgroundScroller : MonoBehaviour
    {
        public Transform target;

        private Vector2 size = Vector2.one;

        public Transform tl, tc, tr, ml, c, mr, bl, bc, br;

        private void Start () {
            size = c.GetComponent<SpriteRenderer>().bounds.size;
        }

        private void Update () {
            
            
            c.transform.position = new Vector2(Mathf.Round(target.transform.position.x/size.x)*size.x,Mathf.Round(target.transform.position.y/size.y)*size.y);
            tl.transform.position = (Vector2)c.transform.position + new Vector2(-size.x,size.y);
            tc.transform.position = (Vector2)c.transform.position + new Vector2(0,size.y);
            tr.transform.position = (Vector2)c.transform.position + new Vector2(size.x,size.y);
            ml.transform.position = (Vector2)c.transform.position + new Vector2(-size.x,0);
            mr.transform.position = (Vector2)c.transform.position + new Vector2(size.x,0);
            bl.transform.position = (Vector2)c.transform.position + new Vector2(-size.x,-size.y);
            bc.transform.position = (Vector2)c.transform.position + new Vector2(0,-size.y);
            br.transform.position = (Vector2)c.transform.position + new Vector2(size.x,-size.y);
        }
    }
}
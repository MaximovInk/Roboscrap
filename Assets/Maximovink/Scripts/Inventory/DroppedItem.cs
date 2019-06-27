using UnityEngine;

namespace MaximovInk
{
    public class DroppedItem : SelectableObject
    {
        public ItemInstance item;


        public override void OnShow()
        {
            message = "<color=#" + Extenshions.GetColorFrom(item.item.Rarity) + ">" + item.item.Name + "(" + item.count + ")</color>";
            base.OnShow();
        }

        public void UpdateSprite()
        {
            GetComponent<SpriteRenderer>().sprite = item.item != null ? item.item.GetDropSprite() : null;
            var _sprite = GetComponent<SpriteRenderer>();
            var _collider = GetComponent<BoxCollider2D>();
            _collider.offset = new Vector2(0, 0);
            _collider.size = new Vector3(_sprite.bounds.size.x / transform.lossyScale.x,
                _sprite.bounds.size.y / transform.lossyScale.y,
                _sprite.bounds.size.z / transform.lossyScale.z);
        }
    }
}
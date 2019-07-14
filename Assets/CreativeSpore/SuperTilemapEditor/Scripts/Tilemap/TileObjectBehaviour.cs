using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{
    /// <summary>
    /// Attached to a gameobject used as tile prefab, it will change the sprite renderer to display the tile that has instantiated the prefab
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode] //fix ShouldRunBehaviour warning when using OnTilePrefabCreation
#endif
    public class TileObjectBehaviour : MonoBehaviour 
    {
        [Tooltip("If true, the sorting layer and sorting order won't be changed with the values of the tilemap.")]
        public bool ChangeSpriteOnly = false;

        protected virtual void OnTilePrefabCreation(TilemapChunk.OnTilePrefabCreationData data)
        {
            DoOnTilePrefabCreation(data, GetComponent<SpriteRenderer>(), ChangeSpriteOnly);
        }

        public static void DoOnTilePrefabCreation(TilemapChunk.OnTilePrefabCreationData data, SpriteRenderer spriteRenderer, bool changeSpriteOnly)
        {
            var tileSprite = GetOrCreateSprite(data);
            if (tileSprite)
            {                
                spriteRenderer.sprite = tileSprite;
                if (!changeSpriteOnly)
                {
                    spriteRenderer.sortingLayerID = data.ParentTilemap.SortingLayerID;
                    spriteRenderer.sortingOrder = data.ParentTilemap.OrderInLayer;
                }
            }
        }

        private static Dictionary<string, Sprite> s_spriteCache = new Dictionary<string, Sprite>();
        public static Sprite GetOrCreateSprite(TilemapChunk.OnTilePrefabCreationData data)
        {
            Sprite sprite = null;
            var tileId = Tileset.GetTileIdFromTileData(data.ParentTilemap.GetTileData(data.GridX, data.GridY));
            var tile = data.ParentTilemap.Tileset.GetTile(tileId);
            if (tile != null)
            {
                var pixelsPerUnit = data.ParentTilemap.Tileset.TilePxSize.x / data.ParentTilemap.CellSize.x;
                var atlasSize = new Vector2(data.ParentTilemap.Tileset.AtlasTexture.width, data.ParentTilemap.Tileset.AtlasTexture.height);
                var spriteUV = new Rect(Vector2.Scale(tile.uv.position, atlasSize), Vector2.Scale(tile.uv.size, atlasSize));
                var spriteName = data.ParentTilemap.Tileset.name + "_" + tileId + "_" + pixelsPerUnit;
                if (!s_spriteCache.TryGetValue(spriteName, out sprite) || !sprite)
                {
                    sprite = Sprite.Create(data.ParentTilemap.Tileset.AtlasTexture, spriteUV, new Vector2(.5f, .5f), pixelsPerUnit);
                    sprite.name = spriteName;
                    s_spriteCache[spriteName] = sprite;
                }
            }
            return sprite;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace CreativeSpore.TiledImporter
{
    public class TmxTilemap
    {

        public string FilePathDirectory { get; protected set; }
        public Dictionary<int, TilesetTextureData> DicTilesetTex2D { get { return m_dicTilesetTex2d; } }
        public TmxMap Map { get { return m_map; } }

        TmxMap m_map;

        public class TilesetTextureData
        {
            public Texture2D tilesetTexture;
            public Rect[] tileRects; //NOTE: this is only used in collection of sprites
            public int[] tileIds;//NOTE: this is only used in collection of sprites
            public bool isCollectionOfSprites { get{ return tileRects != null && tileRects.Length > 0; }}
        }
        Dictionary<int, TilesetTextureData> m_dicTilesetTex2d = new Dictionary<int, TilesetTextureData>();

      
        private TmxTilemap( TmxMap map )
        {
            m_map = map;
        }

        private void LoadImageData()
        {
            foreach( var tileset in m_map.Tilesets )
            {
                if (tileset.Image == null)
                {
                    //Try to create a tileset from tile images
                    if (tileset.TilesWithProperties.Count > 0)
                    {
                        var tileImages = new List<Texture2D>();
                        foreach(var tmxTile in tileset.TilesWithProperties)
                        {
                            if(tmxTile.Image != null)
                                tileImages.Add( LoadTexture(tmxTile.Image.Width, tmxTile.Image.Height, tmxTile.Image.Source, tmxTile.Id.ToString()) );
                        }
                        var atlasTexture = new Texture2D(8192, 8192, TextureFormat.ARGB32, false, false);
                        var tileRects = atlasTexture.PackTextures(tileImages.ToArray(), 0, 8192);
                        //convert rect to pixel units
                        var textureSize = new Vector2( atlasTexture.width, atlasTexture.height );
                        for (var i = 0; i < tileRects.Length; ++i )
                        {
                            var rect = tileRects[i];
                            rect.position = Vector2.Scale(rect.position, textureSize);
                            rect.size = Vector2.Scale(rect.size, textureSize);
                            tileRects[i] = rect;
                        }
                        m_dicTilesetTex2d.Add(
                            tileset.FirstGId,
                            new TilesetTextureData() { tilesetTexture = atlasTexture, tileRects = tileRects, tileIds = tileset.TilesWithProperties.Select(x => x.Id).ToArray() }
                        );
                    }
                    else
                    {
                        Debug.LogWarning("No texture found for tileset " + tileset.Name);
                    }
                }
                else
                {
                    var texture = LoadTexture(tileset.Image.Width, tileset.Image.Height, tileset.Image.Source, tileset.Name);
                    m_dicTilesetTex2d.Add(
                        tileset.FirstGId,
                        new TilesetTextureData() { tilesetTexture = texture}
                    );
                }
            }
        }        

        Texture2D LoadTexture(int width, int height, string source, string name = "")
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            texture.filterMode = FilterMode.Point;
            texture.name = name;
            var texturePath = Path.Combine(FilePathDirectory, source);
            if (File.Exists(texturePath))
                texture.LoadImage(File.ReadAllBytes(texturePath));
            else
                Debug.LogError("Texture file not found: " + texturePath);
            texture.hideFlags = HideFlags.DontSave;
            return texture;
        }

        public TmxTileset FindTileset( TmxLayerTile tile )
        {
            // Value 0 means no tileset associated
            if (tile.GId == 0) return null;

            int tilesetIdx;
            for (tilesetIdx = 0; tilesetIdx < m_map.Tilesets.Count; tilesetIdx++ )
            {
                var tileset = m_map.Tilesets[tilesetIdx];
                if (tileset.FirstGId > tile.GId)
                {
                    break;
                }
            }
            tilesetIdx--;

            return m_map.Tilesets[tilesetIdx];
        }

        public int GetTileAbsoluteId(TmxLayerTile tile)
        {
            // Value 0 means no tileset associated
            if (tile.GId == 0) return -1;

            var tileGlobalId = (int)(tile.GId & 0x1FFFFFFF); // remove flip flags

            var tileAbsoluteId = 0;
            for (var tilesetIdx = 0; tilesetIdx < m_map.Tilesets.Count; tilesetIdx++)
            {
                var tileset = m_map.Tilesets[tilesetIdx];
                if (tileGlobalId >= (tileset.FirstGId + tileset.TileCount))
                {
                    tileAbsoluteId += tileset.TileCount;
                }
                else
                {
                    tileAbsoluteId += (tileGlobalId - tileset.FirstGId);
                    break;
                }
            }

            return tileAbsoluteId;
        }

        public static TmxTilemap LoadFromFile(string sFilePath)
        {
            var objSerializer = new XMLSerializer();
            var map = objSerializer.LoadFromXMLFile<TmxMap>(sFilePath);
            map.FixExportedTilesets(Path.GetDirectoryName(sFilePath));

            var tilemap_ret = new TmxTilemap(map);

            tilemap_ret.FilePathDirectory = Path.GetDirectoryName(sFilePath);

            tilemap_ret.LoadImageData();

            return tilemap_ret;
        }

        static int s_sortingOrder = 0;
        public void ImportIntoScene()
        {
            foreach (var layer in m_map.Layers)
            {
                ImportIntoScene(layer);
                s_sortingOrder++;
            }
        }

        void ImportIntoScene(TmxLayer layer)
        {
            var layerObj = new GameObject("L:" + layer.Name);            
            for (var tile_x = 0; tile_x < layer.Width; tile_x++)
                for (var tile_y = 0; tile_y < layer.Height; tile_y++)
                {
                    var tileIdx = tile_y * layer.Width + tile_x;
                    var tile = layer.Tiles[tileIdx];

                    //skip non valid tiles
                    if (tile.GId == 0) continue;
                    var tileGlobalId = (int)(tile.GId & 0x1FFFFFFF); // remove flip flags

                    var objTileset = FindTileset(tile);

                    // Draw Tile
                    var texTileset = m_dicTilesetTex2d[objTileset.FirstGId].tilesetTexture;
                    var dstRect = new Rect(tile_x * objTileset.TileWidth, (layer.Height - 1 - tile_y) * objTileset.TileHeight, objTileset.TileWidth, objTileset.TileHeight);

                    var tileBaseIdx = tileGlobalId - objTileset.FirstGId;
                    var scanLine = objTileset.Image.Width / objTileset.TileWidth;
                    var tileset_x = tileBaseIdx % scanLine;
                    var tileset_y = tileBaseIdx / scanLine;
                    var srcRect = new Rect(tileset_x * objTileset.TileWidth, texTileset.height - (tileset_y + 1) * objTileset.TileHeight, objTileset.TileWidth, objTileset.TileHeight);

                    var tileObj = new GameObject("tile" + tile_x + "_" + tile_y);
                    tileObj.transform.SetParent(layerObj.transform);
                    var tileRenderer = tileObj.AddComponent<SpriteRenderer>();
                    tileRenderer.sortingOrder = s_sortingOrder;
                    tileRenderer.sprite = Sprite.Create(texTileset, srcRect, Vector2.zero);
                    tileObj.transform.localPosition = dstRect.position / 100f;
                }

        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CreativeSpore.SuperTilemapEditor
{
    public class AtlasEditorWindow : EditorWindow 
    {
        private Tileset m_tileset;
        private Texture2D m_atlasTexture;
        private Vector2 m_tileSize;
        private Vector2 m_slicePadding;
        private Vector2 m_sliceOffset;
        private int m_extrude;
        private int m_padding;
        private Texture2D m_previewTexture;

        [MenuItem("SuperTilemapEditor/Window/Atlas Editor Window")]
        public static void Display()
        {
            var wnd = (AtlasEditorWindow)EditorWindow.GetWindow(typeof(AtlasEditorWindow), false, "Atlas Editor", true);
            wnd.minSize = new Vector2(337f, 314f);
        }

        private Vector2 m_scrollPos;
        void OnGUI()
        {
            var sliceTileset = false;
            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Atlas Texture:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel += 1;
                    EditorGUI.BeginChangeCheck();
                    m_tileset = (Tileset)EditorGUILayout.ObjectField("Tileset (optional)", m_tileset, typeof(Tileset), false);
                    if (EditorGUI.EndChangeCheck() && m_tileset)
                    {
                        m_padding = (int)Mathf.Max(m_slicePadding.x, m_slicePadding.y);
                        m_extrude = 0;
                    }
                    // Read Data From Tileset
                    if (m_tileset)
                    {
                        m_tileSize = m_tileset.TilePxSize;
                        m_slicePadding = m_tileset.SlicePadding;
                        m_sliceOffset = m_tileset.SliceOffset;
                        m_atlasTexture = m_tileset.AtlasTexture;                        
                    }
                    m_atlasTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas texture", m_atlasTexture, typeof(Texture2D), false);
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUILayout.EndHorizontal();

                GUI.enabled = m_atlasTexture != null;
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Slice Settings:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel += 1;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Tile Size (pixels)", GUILayout.MaxWidth(120f));
                    m_tileSize = EditorGUILayout.Vector2Field("", m_tileSize, GUILayout.MaxWidth(180f), GUILayout.MaxHeight(1f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Offset", GUILayout.MaxWidth(120f));
                    m_sliceOffset = EditorGUILayout.Vector2Field("", m_sliceOffset, GUILayout.MaxWidth(180f), GUILayout.MaxHeight(1f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Padding", GUILayout.MaxWidth(120f));
                    m_slicePadding = EditorGUILayout.Vector2Field("", m_slicePadding, GUILayout.MaxWidth(180f), GUILayout.MaxHeight(1f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("New Settings:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel += 1;
                    m_padding = EditorGUILayout.IntField( new GUIContent("Padding (pixels)", "Separation between tiles in pixels"), m_padding);
                    m_extrude = EditorGUILayout.IntField( new GUIContent("Extrude (pixels)", "How many pixels the color is extruded from tile border"), m_extrude);
                    m_padding = Mathf.Max(0, m_padding, m_extrude * 2);
                    m_extrude = Mathf.Max(0, m_extrude);
                    if (GUILayout.Button("Preview"))
                    {
                        m_previewTexture = BuildAtlas();
                        var atlasPreviewWnd = EditorWindow.GetWindow<AtlasPreviewWindow>(true, "Atlas Preview", true);
                        atlasPreviewWnd.Texture = m_previewTexture;
                        atlasPreviewWnd.TileSize = m_tileSize;
                        atlasPreviewWnd.Padding = m_padding;
                        atlasPreviewWnd.Extrude = m_extrude;
                        atlasPreviewWnd.ShowUtility();
                    }
                    if (GUILayout.Button("Apply Settings"))
                    {
                        var outputAtlas = BuildAtlas();
                        var pngData = outputAtlas.EncodeToPNG();
                        var atlasTexturePath = Application.dataPath + AssetDatabase.GetAssetPath(m_atlasTexture).Substring(6);
                        System.IO.File.WriteAllBytes(atlasTexturePath, pngData);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_atlasTexture));
                        Debug.Log("Saving atlas texture: " + atlasTexturePath);
                        if (m_tileset)
                        {
                            m_sliceOffset = new Vector2(m_extrude, m_extrude);
                            m_slicePadding = new Vector2(m_padding, m_padding);
                            sliceTileset = true;
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUILayout.EndVertical();

                if (m_previewTexture)
                {
                    GUILayout.Box(m_previewTexture, GUILayout.Width(position.width - 24f));
                }

                GUI.enabled = true;
            }
            EditorGUILayout.EndScrollView();

            // Save Data into tileset
            if (m_tileset)
            {
                m_tileset.TilePxSize = m_tileSize;
                m_tileset.SlicePadding = m_slicePadding;
                m_tileset.SliceOffset = m_sliceOffset;
                m_tileset.AtlasTexture = m_atlasTexture;
                if (sliceTileset)
                {
                    m_tileset.Slice();
                    foreach (var tilemap in FindObjectsOfType<STETilemap>())
                    {
                        if (tilemap.Tileset == m_tileset) tilemap.Refresh(true, false);
                    }
                }
            }

            if (GUI.changed)
            {
                if (m_tileset) EditorUtility.SetDirty(m_tileset);
            }
        }

        private Texture2D BuildAtlas()
        {
            return m_tileset?
                BuildAtlas(m_atlasTexture, m_padding, m_extrude, m_tileset)
                :
                BuildAtlas(m_atlasTexture, m_padding, m_extrude, m_tileSize, m_sliceOffset, m_slicePadding);
        }

        public static Texture2D BuildAtlas(Texture2D atlasTexture, int tilePadding, int tileExtrude, Vector2 tileSize, Vector2 sliceOffset, Vector2 slicePadding)
        {
            var widthInTiles = Mathf.FloorToInt((atlasTexture.width - sliceOffset.x + slicePadding.x) / (tileSize.x + slicePadding.x)); //NOTE: "+ slicePadding.x" makes sure to count the last tile even if no padding pixels are added to the right
            var heightInTiles = Mathf.FloorToInt((atlasTexture.height - sliceOffset.y + +slicePadding.y) / (tileSize.y + slicePadding.y));                      
            var rects = GenerateGridSpriteRectangles(atlasTexture, sliceOffset, tileSize, slicePadding);
            return BuildAtlas(atlasTexture, tilePadding, tileExtrude, tileSize, widthInTiles, heightInTiles, rects);
        }

        public static Texture2D BuildAtlas(Texture2D atlasTexture, int tilePadding, int tileExtrude, Tileset tileset)
        {
            var widthInTiles = tileset.Width;
            var heightInTiles = tileset.Height;
            var texselSizeInv = new Vector2(1f / tileset.AtlasTexture.texelSize.x, 1f / tileset.AtlasTexture.texelSize.y);
            var rects = tileset.Tiles.Select(x => new Rect( Vector2.Scale(x.uv.position, texselSizeInv), Vector2.Scale(x.uv.size, texselSizeInv))).ToList();
            return BuildAtlas(atlasTexture, tilePadding, tileExtrude, tileset.TilePxSize, widthInTiles, heightInTiles, rects);
        }

        public static Texture2D BuildAtlas(Texture2D atlasTexture, int tilePadding, int tileExtrude, Vector2 tileSize, int widthInTiles, int heightInTiles, List<Rect> rects)
        {
            var padTileWidth = Mathf.RoundToInt(tileSize.x + tilePadding);
            var padTileHeight = Mathf.RoundToInt(tileSize.y + tilePadding);
            var width = widthInTiles * padTileWidth + tilePadding;
            var height = heightInTiles * padTileHeight + tilePadding;
            var output = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            output.filterMode = FilterMode.Point;
            output.SetPixels32(new Color32[width * height]);
            output.Apply();
            TilemapUtilsEditor.MakeTextureReadable(atlasTexture);
            var offset = tilePadding - tileExtrude;
            for (int ty = 0, idx = 0; ty < heightInTiles; ++ty)
            {
                for (var tx = 0; tx < widthInTiles; ++tx, ++idx)
                {
                    if (idx >= rects.Count)
                        continue;
                    var rect = rects[idx];
                    var rx = Mathf.RoundToInt(rect.x);
                    var ry = Mathf.RoundToInt(rect.y);
                    var rw = Mathf.RoundToInt(rect.width);
                    var rh = Mathf.RoundToInt(rect.height);
                    var srcTileColors = atlasTexture.GetPixels(rx, ry, rw, rh);

                    var dstX = tx * padTileWidth + tilePadding - offset;
                    var dstY = output.height - (ty + 1) * padTileHeight + offset;//- tilePadding;
                    output.SetPixels(dstX, dstY, rw, rh, srcTileColors);
                    //Extend border color to fill the padding area
                    Color[] paddingColors;
                    for(var p = 0; p < tileExtrude; ++p)
                    {
                        paddingColors = atlasTexture.GetPixels(rx, ry, rw, 1);
                        output.SetPixels(dstX, dstY - p - 1, rw, 1, paddingColors); // bottom padding
                        paddingColors = atlasTexture.GetPixels(rx, ry + rh - 1, rw, 1);
                        output.SetPixels(dstX, dstY + rh + p, rw, 1, paddingColors); // top padding
                        paddingColors = atlasTexture.GetPixels(rx, ry, 1, rh);
                        output.SetPixels(dstX - p - 1, dstY, 1, rh, paddingColors); // left padding
                        paddingColors = atlasTexture.GetPixels(rx + rw - 1, ry, 1, rh);
                        output.SetPixels(dstX + rw + p, dstY, 1, rh, paddingColors); // right padding
                    }

                    if (tileExtrude > 0)
                    {
                        paddingColors = Enumerable.Repeat(srcTileColors[0], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX - tileExtrude, dstY - tileExtrude, tileExtrude, tileExtrude, paddingColors); // bottom-left corner
                        paddingColors = Enumerable.Repeat(srcTileColors[rw - 1], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX + rw, dstY - tileExtrude, tileExtrude, tileExtrude, paddingColors); // bottom-right corner
                        paddingColors = Enumerable.Repeat(srcTileColors[srcTileColors.Length - rw], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX - tileExtrude, dstY + rh, tileExtrude, tileExtrude, paddingColors); // top-left corner
                        paddingColors = Enumerable.Repeat(srcTileColors[srcTileColors.Length - 1], tileExtrude * tileExtrude).ToArray();
                        output.SetPixels(dstX + rw, dstY + rh, tileExtrude, tileExtrude, paddingColors); // top-right corner
                    }
                }
            }
            output.Apply();
            return output;
        }

        //NOTE: Unlike UnityEditorInternal.InternalSpriteUtility.GenerateGridSpriteRectangles, this will take full transparent tiles into account
        public static List<Rect> GenerateGridSpriteRectangles(Texture2D texture, Vector2 offset, Vector2 size, Vector2 padding)
        {
            var rects = new List<Rect>();
            if (texture != null)
            {
                var uInc = Mathf.RoundToInt(size.x + padding.x);
                var vInc = Mathf.RoundToInt(size.y + padding.y);
                if (uInc > 0 && vInc > 0)
                {
                    for (var y = Mathf.RoundToInt(offset.y); y + size.y <= texture.height; y += vInc)
                    {
                        for (var x = Mathf.RoundToInt(offset.x); x + size.x <= texture.width; x += uInc)
                        {
                            rects.Add(new Rect(x, texture.height - y - size.y, size.x, size.y));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(" Error while slicing. There is something wrong with slicing parameters. uInc = " + uInc + "; vInc = " + vInc);
                }
            }
            return rects;
        }        
    }

    public class AtlasPreviewWindow : EditorWindow
    {
        public Texture2D Texture;
        public Vector2 TileSize;
        public int Padding;
        public int Extrude;
        private int m_zoom = 1;

        void OnLostFocus()
        {
            Close();
        }

        private Vector2 m_scrollPos;
        void OnGUI()
        {
            if (!Texture) Close();

            var e = Event.current;

            var emptyColor = new Color();
            var gridColor = new Color(0f, 1f, 1f, 0.5f);
            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
            {
                if (e.type == EventType.ScrollWheel)
                {
                    if (e.delta.y < 0f) m_zoom++;
                    else m_zoom--;
                    m_zoom = Mathf.Max(1, m_zoom);
                }
                if (e.type == EventType.MouseDrag)
                {
                    m_scrollPos -= e.delta;
                    Repaint();
                }
                GUILayoutUtility.GetRect(Texture.width * m_zoom, Texture.height * m_zoom);
                GUI.DrawTexture(new Rect(0, 0, Texture.width * m_zoom, Texture.height * m_zoom), Texture);
                var tileRect = new Rect(new Vector2(Extrude, Extrude), TileSize);
                for (; tileRect.yMax <= Texture.height; tileRect.y += TileSize.y + Padding)
                {
                    tileRect.x = Extrude;
                    for (; tileRect.xMax <= Texture.width; tileRect.x += TileSize.x + Padding)
                    {
                        var scaledRect = tileRect; scaledRect.position *= m_zoom; scaledRect.size *= m_zoom;
                        HandlesEx.DrawRectWithOutline(scaledRect, emptyColor, gridColor);
                    }
                }
            }
            GUILayout.EndScrollView();
        }
    }
}

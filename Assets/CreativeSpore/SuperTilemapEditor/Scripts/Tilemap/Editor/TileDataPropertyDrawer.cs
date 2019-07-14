using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Reflection;

namespace CreativeSpore.SuperTilemapEditor
{
    /// <summary>
    /// Displays a preview of a tile using the int property as tile data and finding the tileset in a parent STETilemap component
    /// </summary>
    [CustomPropertyDrawer(typeof(TileDataAttribute))]
    public class TileDataPropertyDrawer : PropertyDrawer
    {
        const float k_TilePreviewSize = 64f;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return k_TilePreviewSize + EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                Debug.LogError("SortedLayer property should be an integer");
            }
            else
            {
                var targetObj = property.serializedObject.targetObject as MonoBehaviour;
                var parentTilemap = targetObj.GetComponentInParent<STETilemap>();
                if (parentTilemap && parentTilemap.Tileset)
                {
                    TileDataField(position, label, property, parentTilemap.Tileset);
                }
                else
                {
                    //No STETilemap was found as parent of the component owner of the property
                    Debug.LogWarning("No STETilemap was found as parent of the component owner of the property", property.serializedObject.targetObject);
                }
            }
        }

        public static void TileDataField(Rect position, GUIContent label, SerializedProperty property, Tileset tileset)
        {
            var e = Event.current;
            var isLeftMouseReleased = e.type == EventType.MouseUp && e.button == 0;
            //NOTE: there is a bug with DrawTextureWithTexCoords where the texture disappears. It is fixed by overriding the Editor Script with a CustomEditor.
            var rVisualTile = new Rect(position.x, position.y, k_TilePreviewSize, k_TilePreviewSize);
            var tileData = (uint)property.intValue;
            var brush = tileset.FindBrush(Tileset.GetBrushIdFromTileData(tileData));
            if (brush)
                tileData = brush.PreviewTileData();

            TilesetEditor.DoGUIDrawTileFromTileData(rVisualTile, tileData, tileset);
            if (isLeftMouseReleased && rVisualTile.Contains(e.mousePosition))
            {
                var wnd = EditorWindow.focusedWindow;
                TileSelectionWindow.Show(tileset);
                TileSelectionWindow.Instance.Ping();
                wnd.Focus();
                GUI.FocusControl("");
            }
            EditorGUI.PropertyField(new Rect(position.x, position.y + k_TilePreviewSize, position.width, position.height - k_TilePreviewSize), property, label);
        }  
    }
}

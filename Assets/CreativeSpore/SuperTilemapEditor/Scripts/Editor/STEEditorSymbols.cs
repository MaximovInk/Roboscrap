using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
 
namespace CreativeSpore.SuperTilemapEditor
{
    /// <summary>
    /// Adds the given define symbols to PlayerSettings define symbols.
    /// Just add your own define symbols to the Symbols property at the below.
    /// </summary>
    [InitializeOnLoad]
    public class STEEditorSymbols : Editor
    {

        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        public static readonly string[] Symbols = new string[] {
            "SUPER_TILEMAP_EDITOR",
        };

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static STEEditorSymbols()
        {
            //Debug.Log("Running STEEditorSymbols ...");
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
    }
}
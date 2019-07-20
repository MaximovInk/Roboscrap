﻿
namespace MaximovInk.AI
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(FieldOfView))]
    public class FieldOfViewEditor : Editor
    {
        //private bool DrawAllView = false;

        /* public override void OnInspectorGUI()
         {
             DrawDefaultInspector();
             DrawAllView = GUILayout.Toggle(DrawAllView,"Draw blind area");
         }*/

        private void OnSceneGUI()
        {
            FieldOfView fow = (FieldOfView) target;
            var fows = fow.GetChildsFow();
            DrawFow(fow);
            for (int i = 0; i < fows.Count; i++)
            {
                DrawFow(fows[i]);
            }
        }

        private void DrawFow(FieldOfView fow)
        {
            Handles.color = new Color(1, 1, 1, 0.1f);
            Handles.DrawSolidArc(fow.transform.position, Vector3.forward, Vector3.up, 360, fow.viewRadius);
            Handles.color = new Color(1, 1, 1, 1);
            Handles.DrawWireArc(fow.transform.position, Vector3.forward, Vector3.up, 360, fow.viewRadius);
            Vector3 A = fow.DirFromAngle(-fow.viewAngle / 2, false);
            Vector3 B = fow.DirFromAngle(fow.viewAngle / 2, false);
            Handles.DrawLine(fow.transform.position, fow.transform.position + A * fow.viewRadius);
            Handles.DrawLine(fow.transform.position, fow.transform.position + B * fow.viewRadius);

            foreach (Transform item in fow.visibleTargets)
            {
                Handles.color = Color.red;
                Handles.DrawLine(fow.transform.position, item.transform.position);
            }
        }
    }
}
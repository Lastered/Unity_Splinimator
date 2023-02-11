using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// custom editor for Splinimator component
[CustomEditor(typeof(Splinimator))]
public class SplinimatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // get the Splinimator component
        Splinimator splinimator = target as Splinimator;
        
        // add point button
        if (GUILayout.Button("Add Point"))
        {
            // add to undo stack
            Undo.RecordObject(splinimator, "Add Point");

            // create a new point
            splinimator.CreatePoint();
        }

        // delete point button
        if (GUILayout.Button("Delete Last Point"))
        {
            // add to undo stack
            Undo.RecordObject(splinimator, "Delete Point");

            // delete the last point
            splinimator.DeletePoint();
        }

        // draw the default inspector
        DrawDefaultInspector();




    }

    public void OnDrawGizmos()
    {
        Splinimator splinimator = target as Splinimator;

        Color blue = Color.blue;
        blue.a = 0.5f;

        // draw each handle in the list
        for (int i = 0; i < splinimator.segments.Count; i++)
        {


            Gizmos.color = blue;
            Gizmos.DrawSphere(splinimator.segments[i].handle, 0.05f);

        }
    }


    // Custom in-scene UI for when Splinimator
    // component is selected.
    public void OnSceneGUI()
    {
        Splinimator splinimator = target as Splinimator;

        // make position handles for each point
        splinimator.transform.position = Handles.PositionHandle(splinimator.transform.position, Quaternion.identity);
        for (int i = 0; i < splinimator.segments.Count; i++)
        {
            splinimator.segments[i].endPoint = Handles.PositionHandle(splinimator.segments[i].endPoint, Quaternion.identity);
        }

        // make position handles for each handle
        for (int i = 0; i < splinimator.segments.Count; i++)
        {
            // dont draw handle if it is linear
            if (splinimator.segments[i].isLinear) continue;
            splinimator.segments[i].handle = Handles.PositionHandle(splinimator.segments[i].handle, Quaternion.identity);
        }



        if (splinimator.segments.Count < 1) return;
        
        // draw bezier curve between each point

        Handles.DrawBezier(splinimator.transform.position, splinimator.segments[0].endPoint, splinimator.segments[0].handle, splinimator.segments[0].endPoint, Color.green, null, 5f);

        for (int i = 1; i < splinimator.segments.Count; i++)
        {
            Handles.DrawBezier(splinimator.segments[i].endPoint, splinimator.segments[i - 1].endPoint, splinimator.segments[i].handle, splinimator.segments[i - 1].endPoint, Color.green, null, 5f);

        }


    }
}

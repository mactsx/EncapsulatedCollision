using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicCollider))]
public class PopUpEditor : Editor
{
    private static ColliderDetails window;

    public override void OnInspectorGUI()
    {
        DynamicCollider myScript = (DynamicCollider)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Check if the boolean flag is true
        if (myScript.expandedView)
        {
            // Open the custom editor window if it's not already open
            if (window == null)
            {
                window = ColliderDetails.ShowWindow();
            }
        }
        else
        {
            // Close the window if the boolean flag is false
            if (window != null)
            {
                window.Close();
                window = null;
            }
        }
        if (window == null)
        {
            myScript.expandedView = false;
        }
    }
}
    

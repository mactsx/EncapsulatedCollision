using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicCollider))]
public class PopUpEditor : Editor
{
    private static ColliderDetails window;

    public override void OnInspectorGUI()
    {
        DynamicCollider dynamC = (DynamicCollider)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Check if the boolean flag is true
        if (dynamC.expandDetails)
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
            dynamC.expandDetails = false;
        }
    }
}
    

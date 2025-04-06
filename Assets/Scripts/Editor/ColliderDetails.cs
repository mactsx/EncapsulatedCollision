using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using static UnityEngine.Tilemaps.Tile;
using Codice.Client.Common.GameUI;
using PlasticPipe.PlasticProtocol.Messages;
using Unity.VisualScripting.FullSerializer.Internal;
using PlasticGui.Gluon.WorkspaceWindow;

public class ColliderDetails : EditorWindow
{
    private GameObject objectToEdit;
    private DynamicCollider dynamCollider;

    private Collider[] colliders;
    private ColliderTypes colliderType;
    private string[] colliderNames;
    private int colliderIndexOnObject;

    private bool enableDynamCollision;
    private bool hasMeshCollider = false;
    private float maxSliderSize = 10f;
    private float scaler;

    private Vector3 boxOriginalSize;
    private bool hasOriginalSize = false;
    private float boxScale;
    private float capOriginalRadius = 0.5f;
    private float capOriginalHeight = 2f;

    private enum ColliderTypes
    {
        BoxCollider, CapsuleCollider, SphereCollider, MeshCollider
    }

    
    public static ColliderDetails ShowWindow()
    {
        ColliderDetails window = EditorWindow.GetWindow<ColliderDetails>("Custom Editor");
        return window;
    }

    private void OnEnable()
    {
        // When the window is created, get the selected object (if any)
        dynamCollider = Selection.activeGameObject?.GetComponent<DynamicCollider>();


    }

    // What happens in the window
    private void OnGUI()
    {
        // Space to hold the gameobject that is being edited
        objectToEdit = (GameObject)EditorGUILayout.ObjectField("Object to Edit", objectToEdit, typeof(GameObject), true);
        objectToEdit = Selection.activeGameObject;

        if (!hasOriginalSize && objectToEdit.TryGetComponent(out BoxCollider box))
        {
            boxOriginalSize = box.size;
            scaler = 1f;
            hasOriginalSize = true;
        }

        enableDynamCollision = dynamCollider.isDynamicEnabled;

        colliders = objectToEdit?.GetComponents<Collider>();
        
        // Prepare an array of names to show in the dropdown
        
        colliderNames = new string[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            // Use the GameObject name the collider is attached to
            colliderNames[i] = colliders[i].GetType().Name;
        }

        colliderIndexOnObject = EditorGUILayout.Popup("Collider To Edit", colliderIndexOnObject, colliderNames);


        if (colliders.Length > 0 )
        {
            DisplayColliderOptions(colliders[colliderIndexOnObject]);

            EditorGUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Size:");
            GUILayout.FlexibleSpace();
            maxSliderSize = EditorGUILayout.FloatField(maxSliderSize, GUILayout.Width(50));

            GUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            // Button to remove selected collider
            if (GUILayout.Button("Remove Selected Collider"))
            {
                // Get collider to remove
                Collider col = colliders[colliderIndexOnObject];
                if (col != null)
                {
                    DestroyImmediate(col);
                    // As long as there is still an object in the array
                    if (colliders[0] != null)
                    {
                        // Reset the index to the first collider
                        colliderIndexOnObject = 0;
                    }
                }
                else
                {
                    Debug.Log("Cannot remove collider");
                }

            }
        }


        EditorGUILayout.Space(50);

        // Add new collider
        colliderType = (ColliderTypes)EditorGUILayout.EnumPopup("Collider To Add", colliderType);

        if (GUILayout.Button("Add Collider"))
        {
            AddNewCollider(colliderType);
            scaler = 1;
        }
        


        enableDynamCollision = EditorGUILayout.Toggle("Enable Dynamic Collision", enableDynamCollision);

        if (enableDynamCollision)
        {
            hasMeshCollider = false;

            // Check if there is a mesh collider already added
            foreach (Collider collider in colliders)
            {
                if (collider is MeshCollider)
                {
                    hasMeshCollider = true;
                }
            }
            if (!hasMeshCollider)
            {
                objectToEdit.AddComponent<MeshCollider>();
            }
            if (dynamCollider != null)
            {
                dynamCollider.isDynamicEnabled = true;
            }
        }
        else
        {
            dynamCollider.isDynamicEnabled = false;
        }

        // Close button
        if (GUILayout.Button("Close"))
        {
            Close();
        }
        
    }

    // Override destroy to reset the bool any time the window is closed
    private void OnDestroy()
    {
        if (dynamCollider != null)
        {
            dynamCollider.expandedView = false;
        }
    }

    private void AddNewCollider(ColliderTypes colType)
    {
        switch (colType)
        {
            case ColliderTypes.BoxCollider:
                objectToEdit.AddComponent<BoxCollider>();
                break;
            case ColliderTypes.SphereCollider:
                objectToEdit.AddComponent<SphereCollider>();
                break;
            case ColliderTypes.CapsuleCollider:
                objectToEdit.AddComponent<CapsuleCollider>();
                break;
            case ColliderTypes.MeshCollider:
                objectToEdit.AddComponent<MeshCollider>();
                break;
        }
    }


    private void DisplayColliderOptions(Collider collider)
    {
        
        if (collider is BoxCollider)
        {
            BoxCollider boxCollider = (BoxCollider) collider;
            DisplayBoxOptions(boxCollider);
        }
        if (collider is CapsuleCollider)
        {
            CapsuleCollider capCollider = (CapsuleCollider) collider;
            DisplayCapsuleOptions(capCollider);
        }
        if (collider is SphereCollider)
        {
            SphereCollider sphereCollider = (SphereCollider) collider;
            DisplaySphereOptions(sphereCollider);
        }
        
    }

    private void DisplayBoxOptions(BoxCollider col)
    {
        // Local variables
        Vector3 currentSize;
        float oldScale;

        oldScale = scaler;


        scaler = EditorGUILayout.Slider("Scale", scaler, 0.1f, 5f);

        // Get the current size
        currentSize = col.size;

        // Add sliders for the collider size
        col.size = new Vector3(
            EditorGUILayout.Slider("X Size", col.size.x, 0.1f, maxSliderSize),
            EditorGUILayout.Slider("Y Size", col.size.y, 0.1f, maxSliderSize),
            EditorGUILayout.Slider("Z Size", col.size.z, 0.1f, maxSliderSize)
        );

        //if the scale is not 0 and the old scale is not the same as the new scale
        if (oldScale != scaler && scaler != 0)
        {
            if (oldScale > scaler)
            {
                col.size *= (scaler/oldScale);
            }
            else
            {
                col.size = (boxOriginalSize * scaler);
            }
        }
        else  // Scales are not different, check if col size has changed
        { 
            // If the col size is not the same as the original
            if (scaler != 0 && currentSize != col.size)
            {
                // Reset the original size and scale
                boxOriginalSize = col.size;
            }
        }
        // If the scale is set back to 1, reset the size to the original
         if (scaler == 1)
        {
            col.size = boxOriginalSize;
            
        }

    }

    private void DisplayCapsuleOptions(CapsuleCollider col)
    {
        float currentRad;
        float currentHeight;
        float oldScale;

        oldScale = scaler;


        scaler = EditorGUILayout.Slider("Scale", scaler, 0.1f, 5f);


        currentRad = col.radius;
        currentHeight = col.height;

        EditorGUILayout.LabelField("Adjust Capsule Collider Size", EditorStyles.boldLabel);

        col.radius = EditorGUILayout.Slider("Radius", col.radius, 0.01f, maxSliderSize);
        col.height = EditorGUILayout.Slider("Height", col.height, 0.01f, maxSliderSize);


        //if the scale is not 0 and the old scale is not the same as the new scale
        if (oldScale != scaler && scaler != 0)
        {
            if (oldScale > scaler)
            {
                col.radius *= (scaler / oldScale);
                col.height *= (scaler / oldScale);

            }
            else
            {
                col.radius = (capOriginalRadius * scaler);
                col.height = (capOriginalHeight * scaler);
            }
        }
        else  // Scales are not different, check if col size has changed
        {
            // If the col size is not the same as the original
            if (scaler != 0 && (currentRad != col.radius || currentHeight != col.height))
            {
                // Reset the original size and scale
                capOriginalRadius = col.radius;
                capOriginalHeight = col.height;
            }
        }
        // If the scale is set back to 1, reset the size to the original
        if (scaler == 1)
        {
            col.radius = capOriginalRadius;
            col.height = capOriginalHeight;
        }


    }
    private void DisplaySphereOptions(SphereCollider col)
    {
        EditorGUILayout.LabelField("Adjust Sphere Collider Radius", EditorStyles.boldLabel);

        col.radius = EditorGUILayout.Slider("Radius", col.radius, 0.01f, maxSliderSize);
    }

}



/// To Do:
/// 
/// 1) Fix Scaling Issue
/// 2) Dynamic Enabled Bool Fix
/// 3) Choose which collider works with hit detection 
/// 4) Add Positioning Variables/Add to Children Option
/// 5) Refactor and Clean
/// 6) Improve GUI Design
/// 7) Responsive Damage Options

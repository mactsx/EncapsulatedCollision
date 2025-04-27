using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using static UnityEngine.Tilemaps.Tile;
using Codice.Client.Common.GameUI;
using PlasticPipe.PlasticProtocol.Messages;
using Unity.VisualScripting.FullSerializer.Internal;
using PlasticGui.Gluon.WorkspaceWindow;
using System.Xml.Serialization;
using System.Collections.Generic;

public class ColliderDetails : EditorWindow
{
    private GameObject objectToEdit;
    private DynamicCollider dynamCollider;

    private Collider[] colliders;
    private bool isTrigger;
    private ColliderTypes colliderType;
    private string[] colliderNames;
    private int colliderIndexOnObject;
    private int triggerIndex;
    private GameObject[] children;

    private bool enableDynamCollision;
    private bool hasMeshCollider = false;
    private float maxSliderSize = 10f;
    private float maxSliderPos = 5f;
    private bool scaleX = false;
    private bool scaleY = false;
    private bool scaleZ = false;
    private float scaler;
    private bool affectInverse = false;
    private bool affectChildren = false;

    private Vector3 boxOriginalSize;
    private bool hasOriginalSize = false;
    private float capOriginalRadius = 0.5f;
    private float capOriginalHeight = 2f;

    private Vector2 scrollPosition;

    private enum ColliderTypes
    {
        BoxCollider, CapsuleCollider, SphereCollider, MeshCollider
    }

    public static ColliderDetails ShowWindow()
    {
        ColliderDetails window = EditorWindow.GetWindow<ColliderDetails>("Collider Details");
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
        // Make scrollable
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

        EditorGUILayout.Space(3);
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

        // Get all children if there are any
        if (objectToEdit.transform.childCount > 0)
        {
            children = GetAllChildren(objectToEdit.transform);
        }

        // Prepare an array of names to show in the dropdown
        colliderNames = new string[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            // Get the type of collider
            colliderNames[i] = colliders[i].GetType().Name;
        }

        EditorGUILayout.Space(4);
        colliderIndexOnObject = EditorGUILayout.Popup("Collider To Edit", colliderIndexOnObject, colliderNames);


        if (colliders.Length > 0)
        {
            GUIStyle labelText = new GUIStyle(EditorStyles.label);
            labelText.fontSize = 16;  // Set the font size to 20

            EditorGUILayout.Space(5);
            // Formatting Label
            GUILayout.Label("Size", labelText);
            

            DisplayColliderOptions(colliders[colliderIndexOnObject]);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Size:");
            GUILayout.FlexibleSpace();
            maxSliderSize = EditorGUILayout.FloatField(maxSliderSize, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            

            EditorGUILayout.Space(20);

            

            // Apply the custom style to the label
            GUILayout.Label("Position", labelText);

            EditorGUI.indentLevel = 1;
            ConfigurePos(colliders[colliderIndexOnObject]);            

            // Button to remove selected collider
            if (GUILayout.Button("Remove Selected Collider"))
            {
                RemoveCollider();
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Remove From Parent and Children"))
            {
                RemoveCollider();
                RemoveFromChildren();
            }
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Remove From Children"))
            {
                RemoveFromChildren();
            }
        }

        EditorGUILayout.Space(20);

        // Add new collider
        colliderType = (ColliderTypes)EditorGUILayout.EnumPopup("Collider To Add", colliderType);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Add Collider"))
        {
            // Add new collider and reset scale
            AddNewCollider(colliderType);
            scaler = 1;
        }

        EditorGUILayout.Space(5);

        // Add new collider
        if (GUILayout.Button("Add To Children"))
        {
            AddColliderToChildren(colliderType);
        }

        EditorGUILayout.Space(20);

        enableDynamCollision = EditorGUILayout.Toggle("Enable Dynamic Collision", enableDynamCollision);
        EditorGUILayout.Space(5);

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

            if (!CheckForTrigger(colliders))
            {
                Debug.Log("A collider needs to be set as trigger for dynamic collision to work.");
            }
        }
        else
        {
            dynamCollider.isDynamicEnabled = false;
        }

        triggerIndex = EditorGUILayout.Popup("Trigger Collider", triggerIndex, colliderNames);
        EditorGUILayout.Space(2);

        // Check if it is already set to trigger
        if (colliders.Length > 0 && colliders[triggerIndex] != null)
        {
            isTrigger = colliders[triggerIndex].isTrigger;
        }
        // Flag to set collider as trigger
        isTrigger = EditorGUILayout.Toggle("Is Trigger", isTrigger);
        // If yes - set as trigger or set to not trigger
        if (isTrigger && colliders.Length > 0)
        {
            SetTrigger(triggerIndex, colliders);
            dynamCollider.triggerCollider = colliders[triggerIndex];
        }
        else if (!isTrigger)
        {
            UndoTrigger(triggerIndex, colliders);
        }
        EditorGUILayout.Space(10);

        // Close button
        if (GUILayout.Button("Close"))
        {
            Close();
        }

        EditorGUILayout.Space(30);
        EditorGUILayout.EndScrollView();


    }

    // Override destroy to reset the bool any time the window is closed
    private void OnDestroy()
    {
        if (dynamCollider != null)
        {
            dynamCollider.expandDetails = false;
        }
    }

    private void RemoveCollider()
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

    private void RemoveFromChildren()
    {
        foreach (GameObject obj in children)
        {
            // Get collider to remove
            Collider col = colliders[colliderIndexOnObject];
            Collider colInChild = obj.GetComponent<Collider>();
            if (colInChild != null)
            {
                DestroyImmediate(colInChild);
            }
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

    private void AddColliderToChildren(ColliderTypes colType)
    {
        // For each child, add the collider
        foreach (GameObject obj in children)
        {
            switch (colType)
            {
                case ColliderTypes.BoxCollider:
                    obj.AddComponent<BoxCollider>();
                    break;
                case ColliderTypes.SphereCollider:
                    obj.AddComponent<SphereCollider>();
                    break;
                case ColliderTypes.CapsuleCollider:
                    obj.AddComponent<CapsuleCollider>();
                    break;
                case ColliderTypes.MeshCollider:
                    obj.AddComponent<MeshCollider>();
                    break;
            }
        }
    }

    // Recursive function to get all children and descendants
    private GameObject[] GetAllChildren(Transform parent)
    {
        // Get the number of immediate children
        int childCount = parent.childCount;
        List<GameObject> allChildren = new List<GameObject>();

        // Loop through all children and add them to the list
        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = parent.GetChild(i);
            allChildren.Add(childTransform.gameObject);

            // Recursively get the children of the child - Add the array to the list
            allChildren.AddRange(GetAllChildren(childTransform));
        }

        return allChildren.ToArray();
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

    private void SetTrigger(int i, Collider[] cols)
    {
        if (cols.Length > 0)
            dynamCollider.SetTrigger(i, cols);
    }

    private void UndoTrigger(int i, Collider[] cols)
    {
        if (cols.Length > 0)
            dynamCollider.UndoisTrigger(i, cols);
    }

    private bool CheckForTrigger(Collider[] cols)
    {
        // For each collider on the object
        foreach (Collider col in cols)
        {
            // Skip over mesh colliders
            if (col is MeshCollider) { }
            else
            {
                // If there is a trigger collider, return
                if (col.isTrigger)
                {
                    return true;
                }
            }
        }
        // Otherwise there is no trigger
        return false;
    }

    private void DisplayBoxOptions(BoxCollider col)
    {
        // Add options to change the scale
        ConfigureScaleBox(col);
    }

    private void ConfigureScaleBox(BoxCollider col)
    {
        // Local variables
        Vector3 currentSize;
        float oldScale;

        oldScale = scaler;


        scaler = EditorGUILayout.Slider("Scale", scaler, 0.1f, 5f);

        EditorGUI.indentLevel = 1;

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
                col.size *= (scaler / oldScale);
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

        if (affectChildren)
        {
            foreach (GameObject obj in children)
            {
                BoxCollider cBox = obj.GetComponent<BoxCollider>();
                if (cBox != null)
                {
                    cBox.size = col.size;
                }
            }
        }
    }

    private Vector3 GetColCenter(Collider col)
    {
        if (col is BoxCollider)
        {
            BoxCollider boxCollider = (BoxCollider)col;
            return boxCollider.center;
        }
        if (col is CapsuleCollider)
        {
            CapsuleCollider capCollider = (CapsuleCollider)col;
            return capCollider.center;
        }
        if (col is SphereCollider)
        {
            SphereCollider sphCollider = (SphereCollider)col;
            return sphCollider.center;
        }
        return Vector3.zero;
    }
    private void SetColCenter(Collider col, Vector3 center)
    {
        if (col is BoxCollider)
        {
            BoxCollider boxCollider = (BoxCollider)col;
            boxCollider.center = center;
            if (affectChildren)
            {
                foreach (GameObject obj in children)
                {
                    BoxCollider cBox = obj.GetComponent<BoxCollider>();
                    if (cBox != null)
                    {
                        cBox.center = center;
                    }
                }
            }
        }
        if (col is CapsuleCollider)
        {
            CapsuleCollider capCollider = (CapsuleCollider)col;
            capCollider.center = center;
            if (affectChildren)
            {
                foreach (GameObject obj in children)
                {
                    CapsuleCollider cCap = obj.GetComponent<CapsuleCollider>();
                    if (cCap != null)
                    {
                        cCap.center = center;
                    }
                }
            }
        }
        if (col is SphereCollider)
        {
            SphereCollider sphCollider = (SphereCollider)col;
            sphCollider.center = center;
            if (affectChildren)
            {
                foreach (GameObject obj in children)
                {
                    SphereCollider cSphere = obj.GetComponent<SphereCollider>();
                    if (cSphere != null)
                    {
                        cSphere.center = center;
                    }
                }
            }
        }
    }

    private void ConfigurePos(Collider collider)
    {
        Vector3 currentPos;
        currentPos = GetColCenter(collider);
        Vector3 placeholder;
        placeholder = currentPos;
            
        // Section for position
        GUILayout.Label("Center Position:");

        // Add sliders for the collider size
        currentPos = new Vector3(
            EditorGUILayout.Slider("Center X", placeholder.x, -maxSliderPos, maxSliderPos),
            EditorGUILayout.Slider("Center Y", placeholder.y, -maxSliderPos, maxSliderPos),
            EditorGUILayout.Slider("Center Z", placeholder.z, -maxSliderPos, maxSliderPos)
        );

        EditorGUI.indentLevel = 0;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Position Bounds:");
        GUILayout.FlexibleSpace();
        maxSliderPos = EditorGUILayout.FloatField(maxSliderPos, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(2);

        // Depending on the booleans selected, link each position
        EditorGUILayout.Space(5);
        GUILayout.Label("Link Position Vales:");
        GUILayout.BeginHorizontal();
        scaleX = EditorGUILayout.ToggleLeft("Link X", scaleX, GUILayout.Width(80));
        GUILayout.Space(100);
        scaleY = EditorGUILayout.ToggleLeft("Link Y", scaleY, GUILayout.Width(80));
        GUILayout.Space(100);
        scaleZ = EditorGUILayout.ToggleLeft("Link Z", scaleZ, GUILayout.Width(80));
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(1);
        GUILayout.BeginHorizontal();
        affectInverse = EditorGUILayout.Toggle("Affect Inversely", affectInverse);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        GUILayout.BeginHorizontal();
        affectChildren = EditorGUILayout.Toggle("Affect Children", affectChildren);
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        float diff1;
        float diff2;
        float diff3;

        // TO-DO: Refactor Later
        // Based on the bools - link the positions
        if (scaleX && scaleY && scaleZ)
        {
            // Figure out how much each has changed
            diff1 = currentPos.x - placeholder.x;
            diff2 = currentPos.y - placeholder.y;
            diff3 = currentPos.z - placeholder.z;

            // If one of them has changed
            if (diff1 != 0)
            {
                // Update all linked positions
                if (affectInverse)
                {
                    currentPos.y -= diff1;
                    currentPos.z -= diff1;
                }
                else
                {
                    currentPos.y += diff1;
                    currentPos.z += diff1;
                }
            }
            if (diff2 != 0)
            {
                if (affectInverse)
                {
                    currentPos.x -= diff2;
                    currentPos.z -= diff2;
                }
                else
                {
                    currentPos.x += diff2;
                    currentPos.z += diff2;
                }
            }
            if (diff3 != 0)
            {
                if (affectInverse)
                {
                    currentPos.y -= diff3;
                    currentPos.x -= diff3;
                }
                else
                {
                    currentPos.y += diff3;
                    currentPos.x += diff3;
                }
            }
        }

        else if (scaleX && scaleY)
        {
            diff1 = currentPos.x - placeholder.x;
            diff2 = currentPos.y - placeholder.y;

            if (diff1 != 0)
            {
                if(affectInverse)
                {
                    currentPos.y -= diff1;
                }
                else 
                    currentPos.y += diff1;
            }
            if (diff2 != 0)
            {
                if (affectInverse)
                {
                    currentPos.x -= diff2;
                }
                else
                    currentPos.x += diff2;
            }
        }

        else if (scaleX && scaleZ)
        {
            diff1 = currentPos.x - placeholder.x;
            diff2 = currentPos.z - placeholder.z;

            if (diff1 != 0)
            {
                if (affectInverse)
                {
                    currentPos.z -= diff1;
                }
                else
                    currentPos.z += diff1;
            }
            if (diff2 != 0)
            {
                if (affectInverse)
                {
                    currentPos.x -= diff2;
                }
                else
                    currentPos.x += diff2;
            }
        }
        else if (scaleZ && scaleY)
        {
            diff1 = currentPos.z - placeholder.z;
            diff2 = currentPos.y - placeholder.y;

            if (diff1 != 0)
            {
                if (affectInverse)
                {
                    currentPos.y -= diff1;
                }
                else
                    currentPos.y += diff1;
            }
            if (diff2 != 0)
            {
                if (affectInverse)
                {
                    currentPos.z -= diff2;
                }
                else
                    currentPos.z += diff2;
            }
        }

        SetColCenter(collider, currentPos);

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

        if (affectChildren)
        {
            foreach (GameObject obj in children)
            {
                CapsuleCollider cCap = obj.GetComponent<CapsuleCollider>();
                if (cCap != null)
                {
                    cCap.radius = col.radius;
                    cCap.height = col.height;
                }
            }
        }


    }
    private void DisplaySphereOptions(SphereCollider col)
    {
        EditorGUILayout.LabelField("Adjust Sphere Collider Radius", EditorStyles.boldLabel);

        col.radius = EditorGUILayout.Slider("Radius", col.radius, 0.01f, maxSliderSize);

        if (affectChildren)
        {
            foreach (GameObject obj in children)
            {
                SphereCollider cSph = obj.GetComponent<SphereCollider>();
                if (cSph != null)
                {
                    cSph.radius = col.radius;
                }
            }
        }
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

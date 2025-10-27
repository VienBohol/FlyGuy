using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CrosshairCollisionDestroyer : MonoBehaviour
{
    // Store all destroyable objects currently overlapping the crosshair collider
    private List<DestroyableObject> overlappedObjects = new List<DestroyableObject>();

    void OnTriggerEnter(Collider other)
    {
        DestroyableObject destroyable = other.GetComponent<DestroyableObject>();
        if (destroyable != null && !overlappedObjects.Contains(destroyable))
        {
            overlappedObjects.Add(destroyable);
            destroyable.SetHovered(true); // Optional: highlight
        }
    }

    void OnTriggerExit(Collider other)
    {
        DestroyableObject destroyable = other.GetComponent<DestroyableObject>();
        if (destroyable != null && overlappedObjects.Contains(destroyable))
        {
            overlappedObjects.Remove(destroyable);
            destroyable.SetHovered(false); // Optional: un-highlight
        }
    }

    void Update()
    {
        if (overlappedObjects.Count == 0)
            return;

        // Check for shape button input
        if (Gamepad.current == null)
            return;

        // Iterate backwards since we may remove objects while iterating
        for (int i = overlappedObjects.Count - 1; i >= 0; i--)
        {
            var obj = overlappedObjects[i];
            if (obj.IsCorrectShapeButtonPressed())
            {
                obj.SetHovered(false);
                Destroy(obj.gameObject);
                overlappedObjects.RemoveAt(i);
            }
        }
    }
}

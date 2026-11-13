using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BuildingPiece : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody objectRigidBody;
    
    public Transform buildingZone;

    private Collider objectCollider;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        objectRigidBody = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        
        grabInteractable.selectExited.AddListener(OnReleased);
        grabInteractable.selectEntered.AddListener(OnPickUp);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        SnapToGrid();
        objectRigidBody.isKinematic = true;
        Debug.Log("Object released !");
    }

    private void OnPickUp(SelectEnterEventArgs args)
    {
        objectRigidBody.isKinematic = false;
    }

    private void SnapToGrid()
    {
        Debug.Log($"Building zone position: {buildingZone.position}");
        Debug.Log($"Before: {transform.position}");
        float xPosition = Mathf.Round((transform.position.x - buildingZone.position.x) / 1f) * 1f;
        float zPosition = Mathf.Round((transform.position.z - buildingZone.position.z) / 1f) * 1f;

        float halfHeight = objectCollider.bounds.size.y / 2;
        
        RaycastHit raycastHit;
        float newY = 0;
        Vector3 rayOrigin = new Vector3(xPosition + buildingZone.position.x,transform.position.y + halfHeight + 0.01f, zPosition + buildingZone.position.z);
        
        objectCollider.enabled = false;

        Vector3 rotation = new Vector3(Mathf.Round(transform.eulerAngles.x / 90) * 90, Mathf.Round(transform.eulerAngles.y / 90) * 90, Mathf.Round(transform.eulerAngles.z / 90) * 90);
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out raycastHit, 10f))
        {
            newY = raycastHit.point.y + halfHeight;
            Debug.Log($"Hit object: {raycastHit.collider.name}");
            Debug.Log($"Hit position: {raycastHit.point}");
        }

        objectCollider.enabled = true;
        
        transform.position = new Vector3(xPosition + buildingZone.position.x, newY, zPosition + buildingZone.position.z);
        transform.eulerAngles = rotation;
        Debug.Log($"After: {transform.position}");
    }
}

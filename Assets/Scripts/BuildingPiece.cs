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
        float xPosition = Mathf.Round((transform.position.x - buildingZone.position.x) / 0.2f) * 0.2f;
        float zPosition = Mathf.Round((transform.position.z - buildingZone.position.z) / 0.2f) * 0.2f;

        //RaycastHit raycastHit = Physics.Raycast(grabInteractable.transform.position);
        
        float yPosition = Mathf.Round((transform.position.y - buildingZone.position.y) / 0.2f) * 0.2f;

        transform.position = new Vector3(xPosition + buildingZone.position.x, yPosition + buildingZone.position.y, zPosition + buildingZone.position.z);
        Debug.Log($"After: {transform.position}");
    }
}

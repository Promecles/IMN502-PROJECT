using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BuildingPiece : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody objectRigidBody;
    
    public Transform buildingZone;
    public Transform buildingStructure;

    private bool isPartOfStructure;
    
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
        objectRigidBody.isKinematic = true;
        
        SnapToGrid();

        if (isPartOfStructure)
            return;
        
        objectRigidBody.isKinematic = false;
        objectRigidBody.linearVelocity = Vector3.zero;
        objectRigidBody.angularVelocity = Vector3.zero;
        
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

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 2f);

        BuildingPiece hitPiece = null;
        float closesDistance = float.MaxValue;

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            BuildingPiece piece = nearbyCollider.GetComponent<BuildingPiece>();
            
            if (piece == null || piece == this)
                continue;

            float distance = Vector3.Distance(transform.position, piece.transform.position);

            if (distance < closesDistance)
            {
                closesDistance = distance;
                hitPiece = piece;
            }
        }
        
        if (hitPiece == null)
            return;
        
        Vector3 rotation = new Vector3(Mathf.Round(transform.eulerAngles.x / 90) * 90, Mathf.Round(transform.eulerAngles.y / 90) * 90, Mathf.Round(transform.eulerAngles.z / 90) * 90);
        transform.eulerAngles = rotation;
        Physics.SyncTransforms();

        float verticalExtent = objectCollider.bounds.extents.y;

        float newY = buildingZone.position.y + verticalExtent;
        
        objectCollider.enabled = false;
        
        if (hitPiece != null)
        {
            transform.rotation = hitPiece.transform.rotation;
            Physics.SyncTransforms();
            
            Vector3 localDirection = hitPiece.transform.InverseTransformDirection(
                transform.position - hitPiece.transform.position
            );

            if (Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.y) &&
                Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.z))
            {
                localDirection = new Vector3(Mathf.Sign(localDirection.x), 0, 0);
            }
            else if (Mathf.Abs(localDirection.y) > Mathf.Abs(localDirection.z))
            {
                localDirection = new Vector3(0, Mathf.Sign(localDirection.y), 0);
            }
            else
            {
                localDirection = new Vector3(0, 0, Mathf.Sign(localDirection.z));
            }

            Vector3 direction = hitPiece.transform.TransformDirection(localDirection);

            verticalExtent = objectCollider.bounds.extents.y;

            Vector3 newPosition = hitPiece.transform.position + direction;

            xPosition = newPosition.x - buildingZone.position.x;
            zPosition = newPosition.z - buildingZone.position.z;
            newY = newPosition.y;
        }

        objectCollider.enabled = true;
        
        transform.position = new Vector3(xPosition + buildingZone.position.x, newY, zPosition + buildingZone.position.z);

        if (buildingStructure == null)
        {
            buildingStructure = hitPiece.buildingStructure;
            if (buildingStructure == null)
            {
                GameObject newStructure = new GameObject("BuildingStructure");
                buildingStructure = newStructure.transform;

                Rigidbody structureRigidbody = newStructure.AddComponent<Rigidbody>();
                structureRigidbody.useGravity = true;

                newStructure.AddComponent<XRGrabInteractable>();
            }
        }
        
        buildingStructure.position = hitPiece.transform.position;
        
        hitPiece.transform.SetParent(buildingStructure);
        hitPiece.buildingStructure = buildingStructure;
        transform.SetParent(buildingStructure);

        hitPiece.grabInteractable.enabled = false;
        grabInteractable.enabled = false;

        Destroy(hitPiece.grabInteractable);
        Destroy(grabInteractable);
        
        buildingStructure.GetComponent<XRGrabInteractable>().enabled = true;

        hitPiece.isPartOfStructure = true;
        isPartOfStructure = true;

        StartCoroutine(RemoveRigidBodies(hitPiece));
        
        Debug.Log($"After: {transform.position}");
    }

    private IEnumerator RemoveRigidBodies(BuildingPiece piece)
    {
        yield return null;

        Destroy(piece.objectRigidBody);
        Destroy(objectRigidBody);
    }

    public void DetachFromStructure()
    {
        XRGrabInteractable structureInteractable = buildingStructure.GetComponent<XRGrabInteractable>();
        structureInteractable.colliders.Remove(objectCollider);
        
        transform.SetParent(null);

        objectRigidBody = gameObject.AddComponent<Rigidbody>();
        objectRigidBody.useGravity = true;
        objectRigidBody.isKinematic = false;

        grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnReleased);
        grabInteractable.selectEntered.AddListener(OnPickUp);

        isPartOfStructure = false;
        buildingStructure = null;
    }
}

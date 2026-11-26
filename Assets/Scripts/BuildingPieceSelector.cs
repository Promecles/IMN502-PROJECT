using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPieceSelector : MonoBehaviour
{
    public Transform controllerTransform;
    public BuildingPiece buildingPiece;
    public float maximumDetectionDistance = 10f;
    public InputActionReference detachPieceAction;

    public void Update()
    {
        DetectBuildingPiece();
    }

    public void OnEnable()
    {
        detachPieceAction.action.Enable();
        detachPieceAction.action.performed += OnDetachPiece;
    }

    private void OnDisable()
    {
        detachPieceAction.action.performed -= OnDetachPiece;
        detachPieceAction.action.Disable();
    }

    private void DetectBuildingPiece()
    {
        RaycastHit hit;

        if (Physics.Raycast(controllerTransform.position, controllerTransform.forward, out hit,
                maximumDetectionDistance))
        {
            buildingPiece = hit.collider.GetComponent<BuildingPiece>();
        }
        else
        {
            buildingPiece = null;
        }
    }
    
    private void OnDetachPiece(InputAction.CallbackContext context)
    {
        if (buildingPiece.buildingStructure == null)
            return;
        
        if(!buildingPiece.transform.IsChildOf(buildingPiece.buildingStructure))
            return;
        
        buildingPiece.DetachFromStructure();
    }
}
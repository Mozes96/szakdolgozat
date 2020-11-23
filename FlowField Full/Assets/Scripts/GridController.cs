using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Vector2Int gridSize;
    public float cellRadius = 0.5f;
    public FlowField curFlowField;
    public FlowField centerFlowField;
	public GridDebug gridDebug;
    public GameObject center = null;

    public void InitializeFlowField()
	{
        curFlowField = new FlowField(cellRadius, gridSize);
        curFlowField.CreateGrid();
		gridDebug.SetFlowField(curFlowField);
	}

    private void InitializeCenterFlowField()
    {
        centerFlowField = new FlowField(cellRadius, gridSize);
        centerFlowField.CreateGrid();
    }

	private void Update()
	{
		if (Input.GetMouseButtonDown(0)) // left click
		{
            BuildCurFlowField();
        }
        if(center != null)
        {
            // rebuild the flow field to the center
            InitializeCenterFlowField();
            centerFlowField.CreateCostField();

            Cell destinationCell = centerFlowField.GetCellFromWorldPos(center.transform.position);
            centerFlowField.CreateIntegrationField(destinationCell);

            centerFlowField.CreateFlowField();
        }

	}

    // flow field building for the center to follow it
    public void BuildCurFlowField()
    {
        InitializeFlowField();

        curFlowField.CreateCostField();

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Cell destinationCell = curFlowField.GetCellFromWorldPos(worldMousePos);
        curFlowField.CreateIntegrationField(destinationCell);

        curFlowField.CreateFlowField();

        gridDebug.DrawFlowField();
    }
}

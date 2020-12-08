using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Vector2Int gridSize; //basic: 40, 22
    public float cellRadius; // basic 0.5f

    public FlowField curFlowField;
    public FlowField centerFlowField;
    public FlowField curFlowField2;
    public FlowField centerFlowField2;

    private Vector2Int center2DestinationIndex;
    private Vector2Int centerDiffVector;

	public GridDebug gridDebug;
    public GameObject center = null;
    public GameObject center2 = null;

    public void InitializeFlowField()
	{
        curFlowField = new FlowField(cellRadius, gridSize);
        curFlowField.CreateGrid();
        gridDebug.SetFlowField(curFlowField);
    }

    public void InitializeFlowField2()
    {
        curFlowField2 = new FlowField(cellRadius, gridSize);
        curFlowField2.CreateGrid();
    }

    private void InitializeCenterFlowField()
    {
        centerFlowField = new FlowField(cellRadius, gridSize);
        centerFlowField.CreateGrid();
    }

    private void InitializeCenterFlowField2()
    {
        centerFlowField2 = new FlowField(cellRadius, gridSize);
        centerFlowField2.CreateGrid();
    }

	private void Update()
	{
		if (Input.GetMouseButtonDown(0)) // left click
		{
            BuildCurFlowField();
        }
        if (Input.GetMouseButtonDown(2)) // middle click
        {

            InitializeFlowField2();

            centerDiffVector = new Vector2Int(0, 0);

            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            center2DestinationIndex = curFlowField2.GetCellFromWorldPos(worldMousePos).gridIndex;
        }
        if(center != null)
        {
            // rebuild the flow field to the center
            InitializeCenterFlowField();
            centerFlowField.CreateCostField();

            Cell destinationCell = centerFlowField.GetCellFromWorldPos(center.transform.position);
            centerFlowField.CreateIntegrationField(destinationCell);

            centerFlowField.CreateFlowField();

            // for other center
            InitializeFlowField2();

            curFlowField2.CreateCostField();

            Cell destinationCell2 = MaskDestination(curFlowField2.GetCellFromWorldPos(center.transform.position));

            curFlowField2.CreateIntegrationField(destinationCell2);

            curFlowField2.CreateFlowField();
        }
        if (center2 != null)
        {
            // rebuild the flow field to the center
            InitializeCenterFlowField2();
            centerFlowField2.CreateCostField();

            Cell destinationCell = centerFlowField2.GetCellFromWorldPos(center2.transform.position);
            centerFlowField2.CreateIntegrationField(destinationCell);

            centerFlowField2.CreateFlowField();
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

        // for other center
        InitializeFlowField2();

        curFlowField2.CreateCostField();

        Cell destinationCell2 = MaskDestination(destinationCell);

        curFlowField2.CreateIntegrationField(destinationCell2);

        curFlowField2.CreateFlowField();
    }

    private Cell MaskDestination(Cell destinationCell)
    {
        // vector from base center to second center

        if (centerDiffVector.x == 0 && centerDiffVector.y == 0)
        {
            int indexX = center2DestinationIndex.x - destinationCell.gridIndex.x;
            int indexY = center2DestinationIndex.y - destinationCell.gridIndex.y;

            centerDiffVector = new Vector2Int(indexX, indexY);
        }
        Vector2Int maskedcenterDiffVector = new Vector2Int(0, 0);

        // mask the x coordination
        if (destinationCell.gridIndex.x + centerDiffVector.x > gridSize.x - 1)
            maskedcenterDiffVector.x = gridSize.x - 1;
        else if (destinationCell.gridIndex.x + centerDiffVector.x < 0)
            maskedcenterDiffVector.x = 0;
        else
            maskedcenterDiffVector.x = destinationCell.gridIndex.x + centerDiffVector.x;

        // mask the y coordination
        if (destinationCell.gridIndex.y + centerDiffVector.y > gridSize.y - 1)
            maskedcenterDiffVector.y = gridSize.y - 1;
        else if (destinationCell.gridIndex.y + centerDiffVector.y < 0)
            maskedcenterDiffVector.y = 0;
        else
            maskedcenterDiffVector.y = destinationCell.gridIndex.y + centerDiffVector.y;

        return curFlowField2.grid[maskedcenterDiffVector.x, maskedcenterDiffVector.y];
    }
}



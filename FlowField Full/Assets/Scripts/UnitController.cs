using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public GridController gridController;
    public GameObject unitPrefab;
    public GameObject centerPrefab;
    public int numUnitsPerSpawn;
    private float moveSpeed = 6;
    private Vector3 destinationPos;

    private bool usePathTesting = true;

    private bool destinationReached = false;
    private bool centerInitialized = false;
    private bool testing = false;

    private List<FlowField> flowFieldCollection;
    private List<double> timeCollection;
    private List<Cell> usedCells;
    private int testNumber = 3;
    private List<Vector3> testStartingPos;
    private float deltaTime;

    int unitsStopped = 0;

    private List<GameObject> unitsInGame;

	private void Awake()
	{
		unitsInGame = new List<GameObject>();
        flowFieldCollection = new List<FlowField>();
        timeCollection = new List<double>();
        usedCells = new List<Cell>();
        testStartingPos = new List<Vector3>();
	}

	void Update()
	{
        if (Input.GetMouseButton(1))
        {
            usePathTesting = !usePathTesting;
        }
		if (Input.GetKeyDown(KeyCode.Alpha1)) // spawn units
		{
			SpawnUnits();
        }

		if (Input.GetKeyDown(KeyCode.Alpha2)) // destroy units
		{
			DestroyUnits();
		}
        if (Input.GetMouseButton(0)) // new destination
        {
            destinationReached = false;
            if (centerInitialized)
            {
                    testing = true;
                foreach (GameObject unit in unitsInGame)
                {
                    testStartingPos.Add(unit.transform.position);
                }
                deltaTime = 0;
            }
        }
        if (testing && usePathTesting) // testing is in progress
        {
            moveSpeed = 60;
        }
        else // simulate the best path
        {
            moveSpeed = 6;
        }

        if (testing && usePathTesting)
        {
            Testing();
        }
        unitMoving();
    }

	private void SpawnUnits()
	{
		Vector2Int gridSize = gridController.gridSize;
		float nodeRadius = gridController.cellRadius;
		Vector2 maxSpawnPos = new Vector2(gridSize.x * nodeRadius * 2 + nodeRadius, gridSize.y * nodeRadius * 2 + nodeRadius);
		int colMask = LayerMask.GetMask("Wall", "Units");
		Vector3 newPos;
		for (int i = 0; i < numUnitsPerSpawn; i++)
		{
			GameObject newUnit = Instantiate(unitPrefab);
			newUnit.transform.parent = transform;
			unitsInGame.Add(newUnit);
			do
			{
				newPos = new Vector3(Random.Range(0, maxSpawnPos.x), 0, Random.Range(0, maxSpawnPos.y));
                newUnit.transform.position = newPos;
			}
			while (Physics.OverlapSphere(newPos, 0.25f, colMask).Length > 0);
		}

        //same for the center
        if (!centerInitialized)
        {
            GameObject newCenter = Instantiate(centerPrefab);
            newCenter.transform.parent = transform;
            unitsInGame.Add(newCenter);
            do
            {
                newPos = gridController.curFlowField.destinationCell.worldPos;
                newCenter.transform.position = newPos;
            }
            while (Physics.OverlapSphere(newPos, 0.25f, colMask).Length > 0);
            centerInitialized = true;
            gridController.center = newCenter;
        }
        foreach (GameObject unit in unitsInGame)
        {
            Physics.IgnoreCollision(unit.GetComponent<Collider>(), gridController.center.GetComponent<Collider>());
        }

    }

	private void DestroyUnits()
	{
		foreach (GameObject go in unitsInGame)
		{
			Destroy(go);
		}
		unitsInGame.Clear();
	}

    private void unitMoving()
    {
        
        if (gridController.curFlowField == null || gridController.centerFlowField == null) { return; }
        else
        {
            foreach (GameObject unit in unitsInGame)
            {
                if (unit.layer == 11)
                {
                    destinationPos = gridController.curFlowField.destinationCell.worldPos;
                }
                else if (unit.layer == 10)
                {
                    destinationPos = gridController.centerFlowField.destinationCell.worldPos;
                }
                Cell cellBelow;
                Vector3 moveDirection = new Vector3(0, 0, 0);
                if (unit.layer == 11) // center unit
                {
                    cellBelow = gridController.curFlowField.GetCellFromWorldPos(unit.transform.position);
                    moveDirection = new Vector3(cellBelow.bestDirection.Vector.x, 0, cellBelow.bestDirection.Vector.y);

                    // save the cell to the list if testing
                    if (testing)
                    {
                        if (!usedCells.Contains(cellBelow) && cellBelow != gridController.curFlowField.destinationCell)
                        {
                            usedCells.Add(cellBelow);
                        }
                    }
                }
                else if (
                    unit.layer == 10 &&
                    gridController.centerFlowField != null
                    ) // simple unit
                {
                    cellBelow = gridController.centerFlowField.GetCellFromWorldPos(unit.transform.position);
                    moveDirection = new Vector3(cellBelow.bestDirection.Vector.x, 0, cellBelow.bestDirection.Vector.y);
                }

                Rigidbody unitRB = unit.GetComponent<Rigidbody>();
                
                if (unitRB.position.x <= destinationPos.x + (float)unitsInGame.Count * 0.02 &&
                   unitRB.position.x >= destinationPos.x - (float)unitsInGame.Count * 0.02 &&
                   unitRB.position.z <= destinationPos.z + (float)unitsInGame.Count * 0.02 &&
                   unitRB.position.z >= destinationPos.z - (float)unitsInGame.Count * 0.02 &&
                   unit.layer == 10
                   ) // the Unit is near to the center point
                {
                    unitRB.velocity = new Vector3(0, 0, 0);
                    unitsStopped = unitsStopped + 1;
                }
                else // the Unit can move
                {
                    unitRB.velocity = moveDirection * moveSpeed;
                }
            }
            //Debug.Log("units: " + unitsInGame.Count + "stopped: " + unitsStopped);
            if (unitsStopped >= unitsInGame.Count &&
                unitsInGame.Count != 0
                ) // all units reached the destination
            {
                Debug.Log("destination reached");
                destinationReached = true;
                testing = false;
            }
        }
        unitsStopped = 0;
    }

    private void Testing()
    {
        if(testNumber <= 0) // test ended
        {
            testing = false;
            int fastestPos = 0; ;
            double minTime = 99999;
            for(int i = 0; i < timeCollection.Count; i++)
            {
                if(timeCollection[i] < minTime)
                {
                    minTime = timeCollection[i];
                    fastestPos = i;
                }
            }
            if (fastestPos < flowFieldCollection.Count)
            {
                gridController.curFlowField = flowFieldCollection[fastestPos];
            }
            flowFieldCollection.Clear();
            testStartingPos.Clear();
            moveSpeed = 6;
            destinationReached = true;
            testNumber = 3;
            return;
        }

        if (!destinationReached) // units have to move
        {
            deltaTime += Time.deltaTime;
        }
        else // units stopped
        {
            TestRestart();
        }
        if(deltaTime >= 1) // get into infinite loop
        {
            TestRestart();
        }

    }

    private void TestRestart()
    {
        //reset the units position
        for (int i = 0; i < unitsInGame.Count; i++)
        {
            unitsInGame[i].transform.position = testStartingPos[i];
        }


        testNumber -= 1;

        timeCollection.Add(deltaTime);
        deltaTime = 0;

        Cell destination = gridController.curFlowField.destinationCell;
        flowFieldCollection.Add(gridController.curFlowField);

        foreach (Cell cell in usedCells)
        {
            if (cell.bestCost == 0) Debug.Log("dest");
            gridController.curFlowField.grid[cell.gridIndex.x, cell.gridIndex.y].IncreaseCost(4);
            gridController.curFlowField.grid[cell.gridIndex.x, cell.gridIndex.y].ResetBestCost();
        }

        
        //foreach (Cell c in gridController.curFlowField.grid)
        //{
        //    if (c.cost != 1 && c.cost != 4 && c.cost != 255)
        //        Debug.Log(c.cost);
        //}
        gridController.curFlowField.CreateIntegrationField(destination);
        gridController.curFlowField.CreateFlowField();
        usedCells.Clear();
    }
}

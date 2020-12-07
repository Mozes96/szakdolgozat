using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public GridController gridController;

    // prefabs
    public GameObject unitPrefab;
    public GameObject centerPrefab;
    public GameObject unitPrefab2;
    public GameObject centerPrefab2;

    // for moving
    public int numUnitsPerSpawn;
    private float moveSpeed = 6;

    private Vector3 destinationPos;
    private Vector3 destinationPos2;

    private List<GameObject> unitsInGame;

    // for unit stop
    private bool unitsCanStop = false;
    private float stopTime = 0;

    // for center following
    private bool destinationReached = false;
    private bool centerInitialized = false;
    int unitsStopped = 0;
    private double stopDistance = 0.025;

    // for path testing
    private bool usePathTesting = true;
    private bool testing = false;
    private List<FlowField> flowFieldCollection;
    private List<double> timeCollection;
    private List<Cell> usedCells;
    private int testNumber = 5;
    private List<Vector3> testStartingPos;
    private float deltaTime;


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
        if (Input.GetMouseButton(1)) // quit from path testing
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
            stopTime = 0;
            unitsCanStop = false;
            destinationReached = false;
            if (centerInitialized)
            {
                testing = true;
                foreach (GameObject unit in unitsInGame) // set the starting destination for path testing
                {
                    testStartingPos.Add(unit.transform.position);

                    // ignore collision with walls so units won't stuck in walls
                    if (usePathTesting)
                    {
                        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
                        foreach (GameObject wall in walls)
                        {
                            Physics.IgnoreCollision(unit.GetComponent<Collider>(), wall.GetComponent<Collider>());
                        }
                    }
                }
                deltaTime = 0;
            }
        }
        if (testing && usePathTesting) // testing is in progress
        {
            moveSpeed = 15;
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

		int colMask = LayerMask.GetMask("Wall", "Units", "Unit2"); // don't spawn points on these colliders
		Vector3 newPos;

        // spawn units for true center
		for (int i = 0; i < numUnitsPerSpawn; i++) // spawn units
		{
			GameObject newUnit = Instantiate(unitPrefab);
			newUnit.transform.parent = transform;
			unitsInGame.Add(newUnit);

            do
            {
                newPos = new Vector3(Random.Range(0, maxSpawnPos.x), 0, Random.Range(0, maxSpawnPos.y));
                newUnit.transform.position = newPos;
            }
            while (Physics.OverlapSphere(newPos, 0.25f, colMask).Length > 0); // look for good position until the unit doesn't collide with a unit or a wall
		}

        //spawn units for other center
        for (int i = 0; i < numUnitsPerSpawn; i++) // spawn units
        {
            GameObject newUnit = Instantiate(unitPrefab2);
            newUnit.transform.parent = transform;
            unitsInGame.Add(newUnit);

            do
            {
                newPos = new Vector3(Random.Range(0, maxSpawnPos.x), 0, Random.Range(0, maxSpawnPos.y));
                newUnit.transform.position = newPos;
            }
            while (Physics.OverlapSphere(newPos, 0.25f, colMask).Length > 0); // look for good position until the unit doesn't collide with a unit or a wall
        }

        // same for the center
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

            // for center2
            GameObject newCenter2 = Instantiate(centerPrefab2);
            newCenter2.transform.parent = transform;
            unitsInGame.Add(newCenter2);
            do
            {
                newPos = gridController.curFlowField2.destinationCell.worldPos;
                newCenter2.transform.position = newPos;
            }
            while (Physics.OverlapSphere(newPos, 0.25f, colMask).Length > 0);
            
            gridController.center = newCenter;
            gridController.center2 = newCenter2;
        }

        // ignore the collision between units and center
        foreach (GameObject unit in unitsInGame)
        {
            Physics.IgnoreCollision(unit.GetComponent<Collider>(), gridController.center.GetComponent<Collider>());
            Physics.IgnoreCollision(unit.GetComponent<Collider>(), gridController.center2.GetComponent<Collider>());
        }

    }

    // delete every unit(including center) from the game
	private void DestroyUnits()
	{
		foreach (GameObject go in unitsInGame)
		{
			Destroy(go);
		}
		unitsInGame.Clear();
        centerInitialized = false;
	}

    // set the units velocity
    private void unitMoving()
    {
        // if there isn't flow field or center, than the units won't move
        if (
            gridController.curFlowField == null ||
            gridController.centerFlowField == null ||
            gridController.curFlowField2 == null ||
            gridController.centerFlowField2 == null
            )
        {
            return;
        }
        else
        {
            foreach (GameObject unit in unitsInGame)
            {
                // set destination
                if (unit.layer == 11) // center unit
                {
                    destinationPos = gridController.curFlowField.destinationCell.worldPos;
                }
                else if (unit.layer == 10) // simple unit
                {
                    destinationPos = gridController.centerFlowField.destinationCell.worldPos;
                }
                else if(unit.layer == 13 && gridController.curFlowField2.destinationCell != null) // second center
                {
                    destinationPos = gridController.curFlowField2.destinationCell.worldPos;
                }
                else if(unit.layer == 12) // second type units
                {
                    destinationPos = gridController.centerFlowField2.destinationCell.worldPos;
                }

                Cell cellBelow;
                Vector3 moveDirection = new Vector3(0, 0, 0);

                // set move direction
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
                else if (unit.layer == 13) //second center unit
                {
                    cellBelow = gridController.curFlowField2.GetCellFromWorldPos(unit.transform.position);
                    moveDirection = new Vector3(cellBelow.bestDirection.Vector.x, 0, cellBelow.bestDirection.Vector.y);
                }
                else if(
                    unit.layer == 12 &&
                    gridController.centerFlowField2 != null
                    )
                {
                    cellBelow = gridController.centerFlowField2.GetCellFromWorldPos(unit.transform.position);
                    moveDirection = new Vector3(cellBelow.bestDirection.Vector.x, 0, cellBelow.bestDirection.Vector.y);
                }

                Rigidbody unitRB = unit.GetComponent<Rigidbody>();
                
                if (unitRB.position.x <= destinationPos.x + Mathf.Sqrt(unitsInGame.Count/ (numUnitsPerSpawn/2)) * (float)unitsInGame.Count * (float)stopDistance &&
                   unitRB.position.x >= destinationPos.x - Mathf.Sqrt(unitsInGame.Count / (numUnitsPerSpawn/2)) * (float)unitsInGame.Count * (float)stopDistance &&
                   unitRB.position.z <= destinationPos.z + Mathf.Sqrt(unitsInGame.Count / (numUnitsPerSpawn/2)) * (float)unitsInGame.Count * (float)stopDistance &&
                   unitRB.position.z >= destinationPos.z - Mathf.Sqrt(unitsInGame.Count / (numUnitsPerSpawn/2)) * (float)unitsInGame.Count * (float)stopDistance
                   ) // the simple unit is near to the center point
                {
                    unitsStopped = unitsStopped + 1;
                }

                unitRB.velocity = moveDirection * moveSpeed;

                //stop the units
                if (unitsCanStop && (unit.layer == 10 || unit.layer == 12) && stopTime > (unitsInGame.Count/1.5)/(numUnitsPerSpawn))
                {
                    unitRB.velocity = new Vector3(0, 0, 0);
                }
            }

            // stop the units after some time
            if (unitsStopped >= unitsInGame.Count &&
                unitsInGame.Count != 0 && !testing
                ) // all units reached the destination
            {
                unitsCanStop = true;
                stopTime += Time.deltaTime;
            }
        }

        // reset the stopped units
        unitsStopped = 0;
    }

    // test the possible paths
    private void Testing()
    {
        // the test ended
        if(testNumber <= 0)
        {
            testing = false;
            unitsCanStop = false;

            // check the fastest path
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

            // set the fastest flowfield for use
            if (fastestPos < flowFieldCollection.Count)
            {
                gridController.curFlowField = flowFieldCollection[fastestPos];
            }
            // reset variables
            flowFieldCollection.Clear();
            testStartingPos.Clear();
            moveSpeed = 6;
            destinationReached = true;
            testNumber = 5;
            usedCells.Clear();

            // set back the collision
            foreach (GameObject unit in unitsInGame)
            {
                GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
                foreach (GameObject wall in walls)
                {
                    Physics.IgnoreCollision(unit.GetComponent<Collider>(), wall.GetComponent<Collider>(), false);
                }
            }

            return;
        }

        if (unitsStopped < unitsInGame.Count) // units have to move
        {
            deltaTime += Time.deltaTime;
        }
        else // units stopped
        {
            TestRestart();
        }
        if(deltaTime >= (unitsInGame.Count/(numUnitsPerSpawn/2)) / 1.5) // get into infinite loop
        {
            TestRestart();
        }

    }

    // set everything after a test ended and a new will come
    private void TestRestart()
    {
        //reset the units position
        for (int i = 0; i < unitsInGame.Count; i++)
        {
            unitsInGame[i].transform.position = testStartingPos[i];
        }

        unitsCanStop = false;

        // set global variables
        testNumber -= 1;
        timeCollection.Add(deltaTime);
        deltaTime = 0;

        Cell destination = gridController.curFlowField.destinationCell;
        flowFieldCollection.Add(gridController.curFlowField);
        foreach(Cell c in gridController.curFlowField.grid)
        {
            if (c.cost != 1 && c.cost != 4 && c.cost != 255)
                Debug.Log(c.cost);
        }

        // initialize new flowfield
        gridController.InitializeFlowField();
        gridController.curFlowField.CreateCostField();

        // increase every cell's cost which was used by the center unit
        foreach(Cell usedCell in usedCells)
        {
            if(usedCell.gridIndex != destination.gridIndex)
                gridController.curFlowField.grid[usedCell.gridIndex.x, usedCell.gridIndex.y].IncreaseCost(15);
        }
        gridController.curFlowField.CreateIntegrationField(gridController.curFlowField.grid[destination.gridIndex.x, destination.gridIndex.y]);
        gridController.curFlowField.CreateFlowField();
        gridController.gridDebug.DrawFlowField();
    }
}

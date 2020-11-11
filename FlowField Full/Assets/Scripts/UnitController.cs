using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public GridController gridController;
    public GameObject unitPrefab;
    public GameObject centerPrefab;
    public int numUnitsPerSpawn;
    public float moveSpeed;
    private Vector3 destinationPos;
    public bool destinationReached = false;
    public bool centerInitialized = false;

    private List<GameObject> unitsInGame;

	private void Awake()
	{
		unitsInGame = new List<GameObject>();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SpawnUnits();
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			DestroyUnits();
		}
        if (Input.GetMouseButton(0))
        {
            destinationReached = false;
        }
	}

	private void FixedUpdate()
	{

        int unitsStopped = 0;
        if (gridController.curFlowField == null) { return; }
        else
        {
            destinationPos = gridController.curFlowField.destinationCell.worldPos;
        }
		foreach (GameObject unit in unitsInGame)
		{
            GameObject farAwayUnit = new GameObject();
			Cell cellBelow;
			Vector3 moveDirection = new Vector3(0, 0, 0);
            if (unit.layer == 11) // center unit
            {
                cellBelow = gridController.curFlowField.GetCellFromWorldPos(unit.transform.position);
                moveDirection = new Vector3(cellBelow.bestDirection.Vector.x, 0, cellBelow.bestDirection.Vector.y);
            }
            else if (unit.layer == 10) // simple unit
            {
                cellBelow = gridController.centerFlowField.GetCellFromWorldPos(unit.transform.position);
                moveDirection = new Vector3(cellBelow.bestDirection.Vector.x, 0, cellBelow.bestDirection.Vector.y);
            }

			Rigidbody unitRB = unit.GetComponent<Rigidbody>();

            if(unitRB.position.x <= destinationPos.x + (float)unitsInGame.Count * 0.02 &&
               unitRB.position.x >= destinationPos.x - (float)unitsInGame.Count * 0.02 &&
               unitRB.position.z <= destinationPos.z + (float)unitsInGame.Count * 0.02 &&
               unitRB.position.z >= destinationPos.z - (float)unitsInGame.Count * 0.02
               ) // the Unit is on the destination
            {
                unitRB.velocity = new Vector3 (0, 0, 0);
                unitsStopped += 1;
            }
            else // the Unit can move
            {
                unitRB.velocity = moveDirection * moveSpeed;
            }
		}
        if(unitsStopped == unitsInGame.Count)
        {
            //Debug.Log("Destination reached");
        }

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
                newPos = new Vector3(Random.Range(0, maxSpawnPos.x), 0, Random.Range(0, maxSpawnPos.y));
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
}

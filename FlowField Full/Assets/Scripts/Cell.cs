using UnityEngine;

public class Cell
{
	public Vector3 worldPos;
	public Vector2Int gridIndex;
    public int cost; //255 will be the max value for this
	public int bestCost;
	public GridDirection bestDirection;

	public Cell(Vector3 _worldPos, Vector2Int _gridIndex)
	{
		worldPos = _worldPos;
		gridIndex = _gridIndex;
		cost = 1;
		bestCost = 60000;
		bestDirection = GridDirection.None;
    }

	public void IncreaseCost(int amnt)
	{
		if (cost ==255)
        {
            return;
        }
		if (amnt + cost >= 255)
        {
            cost = 255;
        }
		else
        {
            cost += amnt;
        }
	}
}
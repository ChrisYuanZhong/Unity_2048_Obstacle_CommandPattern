using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;
    public float animationDuration = 0.1f;

    public Tile tilePrefab;
    public TileState[] tileStates;

    private TileGrid grid;
    private List<Tile> tiles;
    private bool moving;

    private void Awake()
    {
        tilePrefab.animationDuration = this.animationDuration;

        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (Tile tile in tiles)
        {
            Destroy(tile.gameObject);
        }

        tiles.Clear();
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], 2);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    private void Update()
    {
        if (moving)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Up");

            MoveTiles(Vector2Int.up, 0, 1, 1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("Down");

            MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Left");

            MoveTiles(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Right");

            MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
        }
    }

    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool moved = false;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.occupied)
                {
                    moved |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (moved)
        {
            StartCoroutine(WaitForMoving());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                // Merging
                if (CanMerge(tile, adjacent.tile))
                {
                    Merge(tile, adjacent.tile);
                    return true;
                }

                break;
            }
            else
            {
                newCell = adjacent;
                adjacent = grid.GetAdjacentCell(adjacent, direction);
            }
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CanMerge(Tile a, Tile b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.MergeWith(b.cell);

        StartCoroutine(WaitForMerging(b));
    }

    private IEnumerator WaitForMerging(Tile tile)
    {
        yield return new WaitForSeconds(animationDuration - 0.01f);

        int index = Mathf.Clamp(IndexOf(tile.state) + 1, 0, tileStates.Length - 1);
        int number = tile.number * 2;

        tile.SetState(tileStates[index], number);

        gameManager.IncreaseScore(number);
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (tileStates[i] == state)
            {
                return i;
            }
        }

        return -1;
    }

    private IEnumerator WaitForMoving()
    {
        moving = true;

        yield return new WaitForSeconds(animationDuration);

        moving = false;

        foreach (Tile tile in tiles)
        {
            tile.locked = false;
        }

        if (tiles.Count < grid.size)
        {
            CreateTile();
        }

        if (tiles.Count >= grid.size && CheckGameOver())
        {
            gameManager.GameOver();
        }
    }

    private bool CheckGameOver()
    {

        foreach (Tile tile in tiles)
        {
            if (CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.up)?.tile))
            {
                return false;
            }

            if (CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.down)?.tile))
            {
                return false;
            }

            if (CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.left)?.tile))
            {
                return false;
            }

            if (CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.right)?.tile))
            {
                return false;
            }
        }

        return true;
    }
}

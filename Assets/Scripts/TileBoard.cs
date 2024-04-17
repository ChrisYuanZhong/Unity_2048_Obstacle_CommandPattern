using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;
    public static int undoCapacity = 3;
    public static float animationDuration = 0.1f;
    public static float obstacleChangeChance = 0.1f;

    public Tile tilePrefab;
    public TileState[] tileStates;
    public TileState obstacleState;

    private TileGrid grid;
    private List<Tile> tiles;
    private Tile obstacle;
    private bool moving;

    public TileGrid Grid => grid;

    CommandManager commandManager;

    private void Awake()
    {
        tilePrefab.animationDuration = animationDuration;

        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    private void Start()
    {
        commandManager = new CommandManager(undoCapacity);
    }

    public void NewGame()
    {
        commandManager.Clear();

        ClearBoard();

        SpawnRandomTile();
        SpawnRandomTile();
    }

    public void Undo()
    {
        commandManager.Undo();
    }

    public void Redo()
    {
        commandManager.Redo();
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

    public void SpawnRandomTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], 2);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);

        // Obstacle
        if (obstacle == null)
        {
            obstacle = Instantiate(tilePrefab, grid.transform);
            obstacle.SetState(obstacleState, -1);
            obstacle.Spawn(grid.GetRandomEmptyCell());
            tiles.Add(obstacle);
        }
        else if (Random.value < obstacleChangeChance)
        {
            obstacle.MoveTo(grid.GetRandomEmptyCell());
        }
    }

    private void Update()
    {
        if (moving)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ICommand command = new MoveCommand(Vector2Int.up, this);
            commandManager.AddCommand(command);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ICommand command = new MoveCommand(Vector2Int.down, this);
            commandManager.AddCommand(command);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ICommand command = new MoveCommand(Vector2Int.left, this);
            commandManager.AddCommand(command);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ICommand command = new MoveCommand(Vector2Int.right, this);
            commandManager.AddCommand(command);
        }
    }

    public void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
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
        if (tile.number == -1)
        {
            return false;
        }

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

    public void SpawnTile(TileCell cell, int number)
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        
        if (number == -1)
        {
            tile.SetState(obstacleState, number);
            obstacle = tile;
        }
        else
        {
            // The index of the tile state is the power of 2 of the number - 1
            int index = Mathf.RoundToInt(Mathf.Log(number, 2)) - 1;

            // Clamp the index between 0 and the length of the tile states array
            index = Mathf.Clamp(index, 0, tileStates.Length - 1);

            tile.SetState(tileStates[index], number);
        }
        
        tile.Spawn(cell);
        tiles.Add(tile);
    }

    private bool CanMerge(Tile a, Tile b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a.number == b.number && !b.locked && a.number != -1;
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
            SpawnRandomTile();
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

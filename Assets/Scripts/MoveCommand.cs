using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    private Vector2Int _direction;
    private TileBoard _board;

    // For undo
    private int currentScore;
    private int[,] currentBoard;

    public MoveCommand(Vector2Int direction, TileBoard board)
    {
        _direction = direction;
        _board = board;
    }

    public void Execute()
    {
        SaveState();

        if (_direction == Vector2Int.up)
        {
            _board.MoveTiles(Vector2Int.up, 0, 1, 1, 1);
        }
        else if (_direction == Vector2Int.down)
        {
            _board.MoveTiles(Vector2Int.down, 0, 1, _board.Grid.height - 2, -1);
        }
        else if (_direction == Vector2Int.left)
        {
            _board.MoveTiles(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (_direction == Vector2Int.right)
        {
            _board.MoveTiles(Vector2Int.right, _board.Grid.width - 2, -1, 0, 1);
        }
    }

    public void Undo()
    {
        RestoreState();
    }

    private void SaveState()
    {
        currentScore = _board.gameManager.score;
        currentBoard = new int[_board.Grid.width, _board.Grid.height];

        for (int x = 0; x < _board.Grid.width; x++)
        {
            for (int y = 0; y < _board.Grid.height; y++)
            {
                TileCell cell = _board.Grid.GetCell(x, y);

                if (cell.occupied)
                {
                    currentBoard[x, y] = cell.tile.number;
                }
                else
                {
                    currentBoard[x, y] = 0;
                }
            }
        }
    }

    private void RestoreState()
    {
        _board.gameManager.score = currentScore;

        _board.ClearBoard();

        for (int x = 0; x < _board.Grid.width; x++)
        {
            for (int y = 0; y < _board.Grid.height; y++)
            {
                if (currentBoard[x, y] != 0)
                {
                    TileCell cell = _board.Grid.GetCell(x, y);
                    _board.SpawnTile(cell, currentBoard[x, y]);
                    //tile.locked = true;
                }
            }
        }
    }
}

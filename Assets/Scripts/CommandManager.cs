using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandManager
{
    private int capacity = 3;

    private Button undoButton;
    private Button redoButton;

    LinkedList<ICommand> commands;
    LinkedList<ICommand> redoCommands;

    public CommandManager()
    {
        commands = new LinkedList<ICommand>();
        redoCommands = new LinkedList<ICommand>();

        undoButton = GameObject.FindWithTag("UndoButton").GetComponent<Button>();
        redoButton = GameObject.FindWithTag("RedoButton").GetComponent<Button>();
    }

    public CommandManager(int capacity)
    {
        commands = new LinkedList<ICommand>();
        redoCommands = new LinkedList<ICommand>();
        this.capacity = capacity;

        undoButton = GameObject.FindWithTag("UndoButton").GetComponent<Button>();
        redoButton = GameObject.FindWithTag("RedoButton").GetComponent<Button>();
    }

    public void Clear()
    {
        commands.Clear();
        redoCommands.Clear();

        if (undoButton != null)
            undoButton.interactable = false;
        if (redoButton != null)
            redoButton.interactable = false;
    }

    public void AddCommand(ICommand command)
    {
        command.Execute();

        if (commands.Count == capacity)
        {
            commands.RemoveFirst();
        }

        commands.AddLast(command);
        redoCommands.Clear();

        if (undoButton != null)
            undoButton.interactable = true;
        if (redoButton != null)
            redoButton.interactable = false;
    }

    public void Undo()
    {
        if (commands.Count == 0)
        {
            return;
        }

        ICommand command = commands.Last.Value;
        command.Undo();
        redoCommands.AddLast(command);
        commands.RemoveLast();

        if (commands.Count == 0 && undoButton != null)
        {
            undoButton.interactable = false;
        }

        if (redoButton != null)
            redoButton.interactable = true;
    }

    public void Redo()
    {
        if (redoCommands.Count == 0)
        {
            return;
        }

        ICommand command = redoCommands.Last.Value;
        command.Execute();
        commands.AddLast(command);
        redoCommands.RemoveLast();

        if (redoCommands.Count == 0 && redoButton != null)
        {
            redoButton.interactable = false;
        }

        if (undoButton != null)
            undoButton.interactable = true;
    }
}

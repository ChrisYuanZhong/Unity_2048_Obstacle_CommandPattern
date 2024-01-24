using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandManager
{
    private int capacity = 3;

    private Button undoButton;
    private Button redoButton;

    Stack<ICommand> commands;
    Stack<ICommand> redoCommands;

    public CommandManager()
    {
        commands = new Stack<ICommand>(capacity);
        redoCommands = new Stack<ICommand>(capacity);

        undoButton = GameObject.FindWithTag("UndoButton").GetComponent<Button>();
        redoButton = GameObject.FindWithTag("RedoButton").GetComponent<Button>();
    }

    public CommandManager(int capacity)
    {
        commands = new Stack<ICommand>(capacity);
        redoCommands = new Stack<ICommand>(capacity);
        this.capacity = capacity;

        undoButton = GameObject.FindWithTag("UndoButton").GetComponent<Button>();
        redoButton = GameObject.FindWithTag("RedoButton").GetComponent<Button>();
    }

    public void Clear()
    {
        commands.Clear();
        redoCommands.Clear();

        undoButton.interactable = false;
        redoButton.interactable = false;
    }

    public void AddCommand(ICommand command)
    {
        command.Execute();

        if (commands.Count == capacity)
        {
            RemoveOldestCommand();
        }

        commands.Push(command);
        redoCommands.Clear();

        undoButton.interactable = true;
        redoButton.interactable = false;
    }

    private void RemoveOldestCommand()
    {
        Stack<ICommand> temp = new Stack<ICommand>(capacity - 1);

        for (int i = 0; i < capacity - 1; i++)
        {
            temp.Push(commands.Pop());
        }
        commands.Pop();

        for (int i = 0; i < capacity - 1; i++)
        {
            commands.Push(temp.Pop());
        }

        temp.Clear();

        return;
    }

    public void Undo()
    {
        if (commands.Count == 0)
        {
            return;
        }

        ICommand command = commands.Pop();
        command.Undo();
        redoCommands.Push(command);

        if (commands.Count == 0)
        {
            undoButton.interactable = false;
        }

        redoButton.interactable = true;
    }

    public void Redo()
    {
        if (redoCommands.Count == 0)
        {
            return;
        }

        ICommand command = redoCommands.Pop();
        command.Execute();
        commands.Push(command);

        if (redoCommands.Count == 0)
        {
            redoButton.interactable = false;
        }

        undoButton.interactable = true;
    }
}

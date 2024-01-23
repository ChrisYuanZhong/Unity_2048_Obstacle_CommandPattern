using System.Collections.Generic;
using UnityEngine;

public class CommandManager
{
    Stack<ICommand> commands;
    Stack<ICommand> redoCommands;

    public CommandManager()
    {
        commands = new Stack<ICommand>(3);
        redoCommands = new Stack<ICommand>(3);
    }

    public CommandManager(int capacity)
    {
        commands = new Stack<ICommand>(capacity);
        redoCommands = new Stack<ICommand>(capacity);
    }

    public void AddCommand(ICommand command)
    {
        command.Execute();
        commands.Push(command);
        redoCommands.Clear();
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
    }
}

using UnityEngine;

public interface ICommand
{
    public abstract void Execute();
    public virtual void Undo() { }
}

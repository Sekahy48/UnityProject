using System;

public interface IInventoryInputSource
{
    event Action<int> OnInventoryRequested;
}
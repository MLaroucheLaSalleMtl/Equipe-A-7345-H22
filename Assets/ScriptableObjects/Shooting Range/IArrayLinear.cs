using UnityEngine;

public interface IArrayLinear
{
    // SECTION - Property ===================================================================
    public int Count { get; }
    public int Length { get; }

    public bool IsNull { get; }
    public bool IsEmpty { get; }
    public bool IsFull { get; }

    public int CurrentIndex { get; }
}

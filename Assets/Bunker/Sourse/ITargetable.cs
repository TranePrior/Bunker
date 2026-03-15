using UnityEngine;

public interface ITargetable
{
    Transform TargetTransform { get; }
    bool IsAlive { get; }
}

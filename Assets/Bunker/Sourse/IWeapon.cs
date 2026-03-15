public interface IWeapon
{
    float Range { get; }
    float TargetSearchInterval { get; }

    bool CanFire(float currentTime);
    bool IsTargetVisible(UnityEngine.Transform target);
    bool TryFire(IDamageable target, float currentTime);
}

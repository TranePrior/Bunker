using UnityEngine;

public class AutoShooter : MonoBehaviour, ICharacterShooter
{
    private ITargetable currentTarget;
    private CarHealth ownerCar;
    private IWeapon weapon;
    private float searchTimer;

    public void EquipWeapon(IWeapon equippedWeapon)
    {
        weapon = equippedWeapon;
        currentTarget = null;
        searchTimer = 0f;
    }

    private void Awake()
    {
        ownerCar = GetComponentInParent<CarHealth>();
        WeaponBehaviour existingWeapon = GetComponentInChildren<WeaponBehaviour>(true);
        if (existingWeapon != null)
        {
            EquipWeapon(existingWeapon);
        }
    }

    private void Update()
    {
        if (weapon == null)
        {
            return;
        }

        if (ownerCar != null && ownerCar.IsDead)
        {
            return;
        }

        if (!IsTargetValid(currentTarget))
        {
            currentTarget = null;
        }

        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            AcquireTarget();
            searchTimer = weapon.TargetSearchInterval;
        }

        if (currentTarget == null)
        {
            return;
        }

        if (!weapon.CanFire(Time.time))
        {
            return;
        }

        IDamageable damageable = currentTarget as IDamageable;
        if (damageable == null)
        {
            return;
        }

        weapon.TryFire(damageable, Time.time);
    }

    private void AcquireTarget()
    {
        float range = weapon.Range;
        float maxRangeSqr = range * range;
        float bestDistanceSqr = maxRangeSqr;
        ITargetable bestTarget = null;

        for (int i = 0; i < EnemyHealth.ActiveEnemies.Count; i++)
        {
            ITargetable candidate = EnemyHealth.ActiveEnemies[i];
            if (!IsTargetValid(candidate))
            {
                continue;
            }

            float distanceSqr = (candidate.TargetTransform.position - transform.position).sqrMagnitude;
            if (distanceSqr > bestDistanceSqr)
            {
                continue;
            }

            bestDistanceSqr = distanceSqr;
            bestTarget = candidate;
        }

        currentTarget = bestTarget;
    }

    private bool IsTargetValid(ITargetable target)
    {
        if (target == null || !target.IsAlive)
        {
            return false;
        }

        float range = weapon.Range;
        float distanceSqr = (target.TargetTransform.position - transform.position).sqrMagnitude;
        if (distanceSqr > range * range)
        {
            return false;
        }

        return weapon.IsTargetVisible(target.TargetTransform);
    }

    private void OnDrawGizmosSelected()
    {
        if (weapon == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, weapon.Range);
    }
}

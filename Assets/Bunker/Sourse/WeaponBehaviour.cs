using UnityEngine;

public class WeaponBehaviour : MonoBehaviour, IWeapon
{
    [Header("Combat")]
    [SerializeField] private int damage = 8;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float range = 8f;
    [SerializeField] private float targetSearchInterval = 0.15f;

    [Header("Projectile")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float projectileLifetime = 2.5f;

    [Header("VFX")]
    [SerializeField] private GameObject shotVfxPrefab;
    [SerializeField] private Vector3 shotVfxOffset = Vector3.zero;

    [Header("Aim")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private bool requireLineOfSight;
    [SerializeField] private LayerMask obstacleMask;

    private float nextShotTime;

    public float Range => Mathf.Max(0.1f, range);
    public float TargetSearchInterval => Mathf.Max(0.05f, targetSearchInterval);

    private void Awake()
    {
        if (muzzlePoint == null)
        {
            Transform shot = transform.Find("Shot");
            if (shot != null)
            {
                muzzlePoint = shot;
            }
        }
    }

    public bool CanFire(float currentTime)
    {
        return currentTime >= nextShotTime;
    }

    public bool IsTargetVisible(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        if (!requireLineOfSight)
        {
            return true;
        }

        Vector2 origin = GetOriginPosition();
        Vector2 direction = (target.position - (Vector3)origin);
        float distance = direction.magnitude;
        if (distance <= 0.001f)
        {
            return true;
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, distance, obstacleMask);
        return hit.collider == null;
    }

    public bool TryFire(IDamageable target, float currentTime)
    {
        if (!CanFire(currentTime) || target == null)
        {
            return false;
        }

        Transform targetTransform = target as Transform;
        if (targetTransform == null)
        {
            Component targetComponent = target as Component;
            if (targetComponent != null)
            {
                targetTransform = targetComponent.transform;
            }
        }

        if (targetTransform == null)
        {
            return false;
        }

        Vector3 origin = GetOriginPosition();
        Vector3 direction = (targetTransform.position - origin).normalized;

        SpawnShotVfx(origin, direction);

        if (bulletPrefab != null)
        {
            SpawnProjectile(target, origin, direction);
        }
        else
        {
            target.TakeDamage(Mathf.Max(1, damage));
        }

        float shotsPerSecond = Mathf.Max(0.1f, fireRate);
        nextShotTime = currentTime + (1f / shotsPerSecond);
        return true;
    }

    private Vector3 GetOriginPosition()
    {
        return muzzlePoint != null ? muzzlePoint.position : transform.position;
    }

    private void SpawnShotVfx(Vector3 origin, Vector3 direction)
    {
        if (shotVfxPrefab == null)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        GameObject vfxObject = Instantiate(shotVfxPrefab, origin + shotVfxOffset, rotation);

        ParticleSystem particle = vfxObject.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            var main = particle.main;
            if (main.loop)
            {
                main.loop = false;
            }

            float destroyDelay = main.duration + main.startLifetime.constantMax + 0.2f;
            Destroy(vfxObject, Mathf.Max(0.5f, destroyDelay));
        }
        else
        {
            Destroy(vfxObject, 2f);
        }
    }

    private void SpawnProjectile(IDamageable target, Vector3 origin, Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        GameObject bulletObject = Instantiate(bulletPrefab, origin, rotation);

        BulletProjectile projectile = bulletObject.GetComponent<BulletProjectile>();
        if (projectile == null)
        {
            projectile = bulletObject.AddComponent<BulletProjectile>();
        }

        projectile.Initialize(
            target,
            Mathf.Max(1, damage),
            Mathf.Max(0.1f, projectileSpeed),
            Mathf.Max(0.1f, projectileLifetime),
            direction);
    }
}

using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float hitDistance = 0.2f;

    private IDamageable target;
    private float speed;
    private float lifeTime;
    private int damage;
    private Vector3 fallbackDirection;

    public void Initialize(IDamageable targetDamageable, int projectileDamage, float projectileSpeed, float projectileLifeTime, Vector3 direction)
    {
        target = targetDamageable;
        damage = Mathf.Max(1, projectileDamage);
        speed = Mathf.Max(0.1f, projectileSpeed);
        lifeTime = Mathf.Max(0.1f, projectileLifeTime);
        fallbackDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.right;
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 aimPoint;
        Component targetComponent = target as Component;
        if (targetComponent != null)
        {
            aimPoint = targetComponent.transform.position;
        }
        else
        {
            aimPoint = transform.position + fallbackDirection;
        }

        Vector3 toTarget = aimPoint - transform.position;
        if (toTarget.magnitude <= hitDistance)
        {
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        Vector3 direction = toTarget.normalized;
        fallbackDirection = direction;
        transform.position += direction * speed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable, ITargetable
{
    private static readonly List<EnemyHealth> ActiveEnemiesInternal = new List<EnemyHealth>();

    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    public static IReadOnlyList<EnemyHealth> ActiveEnemies => ActiveEnemiesInternal;

    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public bool IsAlive => !IsDead;
    public Transform TargetTransform => transform;

    public event Action<EnemyHealth> Died;

    private void Awake()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
    }

    private void OnEnable()
    {
        if (!ActiveEnemiesInternal.Contains(this))
        {
            ActiveEnemiesInternal.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveEnemiesInternal.Remove(this);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead)
        {
            return;
        }

        int damage = Mathf.Max(0, amount);
        if (damage == 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        if (CurrentHealth > 0)
        {
            return;
        }

        Died?.Invoke(this);
        Destroy(gameObject);
    }
}

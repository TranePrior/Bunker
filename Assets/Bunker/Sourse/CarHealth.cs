using System;
using UnityEngine;

public class CarHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> Damaged;
    public event Action Died;

    private void Awake()
    {
        CurrentHealth = maxHealth;
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
        Damaged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth == 0)
        {
            Died?.Invoke();
            Debug.Log("Car destroyed. Game Over.");
        }
    }
}

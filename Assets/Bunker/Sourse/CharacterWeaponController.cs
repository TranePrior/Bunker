using UnityEngine;

[DisallowMultipleComponent]
public class CharacterWeaponController : MonoBehaviour
{
    [Header("Weapon Slots")]
    [SerializeField] private WeaponBehaviour weaponSlot1Prefab;
    [SerializeField] private WeaponBehaviour weaponSlot2Prefab;
    [SerializeField] private WeaponBehaviour weaponSlot3Prefab;
    [SerializeField] [Range(0, 2)] private int activeSlotOnStart;

    [Header("Spawn")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Vector3 weaponLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 weaponLocalEuler = Vector3.zero;

    [Header("Debug Input")]
    [SerializeField] private bool allowKeyboardSwitch;
    [SerializeField] private KeyCode switchWeaponKey = KeyCode.Tab;

    private readonly WeaponBehaviour[] spawnedWeapons = new WeaponBehaviour[3];
    private ICharacterShooter shooter;
    private int activeSlot = -1;

    public IWeapon ActiveWeapon
    {
        get
        {
            if (activeSlot < 0 || activeSlot >= spawnedWeapons.Length)
            {
                return null;
            }

            return spawnedWeapons[activeSlot];
        }
    }

    private void Awake()
    {
        if (weaponHolder == null)
        {
            weaponHolder = transform;
        }

        SpawnConfiguredWeapons();
        if (!TrySwitchWeapon(activeSlotOnStart))
        {
            SwitchToNextWeapon();
        }
    }

    private void Start()
    {
        if (shooter == null)
        {
            AutoShooter autoShooter = GetComponentInParent<AutoShooter>();
            if (autoShooter != null)
            {
                BindShooter(autoShooter);
            }
        }
    }

    private void Update()
    {
        if (!allowKeyboardSwitch || !Input.GetKeyDown(switchWeaponKey))
        {
            return;
        }

        SwitchToNextWeapon();
    }

    public void BindShooter(ICharacterShooter boundShooter)
    {
        shooter = boundShooter;
        PushActiveWeaponToShooter();
    }

    public bool SwitchToNextWeapon()
    {
        if (spawnedWeapons.Length == 0)
        {
            return false;
        }

        int startIndex = activeSlot;
        for (int step = 1; step <= spawnedWeapons.Length; step++)
        {
            int candidate = (startIndex + step + spawnedWeapons.Length) % spawnedWeapons.Length;
            if (spawnedWeapons[candidate] == null)
            {
                continue;
            }

            return TrySwitchWeapon(candidate);
        }

        return false;
    }

    public bool TrySwitchWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spawnedWeapons.Length)
        {
            return false;
        }

        WeaponBehaviour nextWeapon = spawnedWeapons[slotIndex];
        if (nextWeapon == null)
        {
            return false;
        }

        if (activeSlot == slotIndex)
        {
            PushActiveWeaponToShooter();
            return true;
        }

        SetWeaponActive(activeSlot, false);
        activeSlot = slotIndex;
        SetWeaponActive(activeSlot, true);
        PushActiveWeaponToShooter();
        return true;
    }

    private void SpawnConfiguredWeapons()
    {
        spawnedWeapons[0] = SpawnWeapon(weaponSlot1Prefab, 0);
        spawnedWeapons[1] = SpawnWeapon(weaponSlot2Prefab, 1);
        spawnedWeapons[2] = SpawnWeapon(weaponSlot3Prefab, 2);
    }

    private WeaponBehaviour SpawnWeapon(WeaponBehaviour prefab, int slotIndex)
    {
        if (prefab == null)
        {
            return null;
        }

        WeaponBehaviour instance = Instantiate(prefab, weaponHolder);
        instance.name = "WeaponSlot" + (slotIndex + 1);
        instance.transform.localPosition = weaponLocalPosition;
        instance.transform.localRotation = Quaternion.Euler(weaponLocalEuler);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private void SetWeaponActive(int slotIndex, bool isActive)
    {
        if (slotIndex < 0 || slotIndex >= spawnedWeapons.Length)
        {
            return;
        }

        WeaponBehaviour weapon = spawnedWeapons[slotIndex];
        if (weapon == null)
        {
            return;
        }

        weapon.gameObject.SetActive(isActive);
    }

    private void PushActiveWeaponToShooter()
    {
        if (shooter == null)
        {
            return;
        }

        shooter.EquipWeapon(ActiveWeapon);
    }

    private void OnValidate()
    {
        activeSlotOnStart = Mathf.Clamp(activeSlotOnStart, 0, 2);
    }
}

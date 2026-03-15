using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CharacterSlot : MonoBehaviour
{
    [Header("Assigned In Edit Mode")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private WeaponBehaviour weaponPrefab;
    [SerializeField] private Vector3 characterLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 characterLocalEuler = Vector3.zero;
    [SerializeField] private Vector3 weaponLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 weaponLocalEuler = Vector3.zero;

    [Header("Click")]
    [SerializeField] private float clickRadius = 0.75f;

    private bool isOccupied;
    private AutoShooter shooter;
    private SpriteRenderer slotSprite;
    private Collider2D slotCollider;
    private Button slotButton;

    public bool IsOccupied => isOccupied;

    public void Configure(
        GameObject configuredCharacterPrefab,
        WeaponBehaviour configuredWeaponPrefab,
        Vector3 configuredCharacterLocalPosition,
        Vector3 configuredCharacterLocalEuler,
        Vector3 configuredWeaponLocalPosition,
        Vector3 configuredWeaponLocalEuler)
    {
        characterPrefab = configuredCharacterPrefab;
        weaponPrefab = configuredWeaponPrefab;
        characterLocalPosition = configuredCharacterLocalPosition;
        characterLocalEuler = configuredCharacterLocalEuler;
        weaponLocalPosition = configuredWeaponLocalPosition;
        weaponLocalEuler = configuredWeaponLocalEuler;
    }

    public void EnsureEditorSetup()
    {
        slotSprite = GetComponent<SpriteRenderer>();
        slotButton = GetComponent<Button>();

        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle == null)
        {
            circle = gameObject.AddComponent<CircleCollider2D>();
        }

        circle.radius = Mathf.Max(0.05f, clickRadius);
        circle.isTrigger = false;
        slotCollider = circle;
    }

    private void Reset()
    {
        EnsureEditorSetup();
    }

    private void OnValidate()
    {
        EnsureEditorSetup();
    }

    private void Awake()
    {
        shooter = GetComponent<AutoShooter>();
        EnsureEditorSetup();
    }

    private void OnMouseDown()
    {
        TryPlaceCharacter();
    }

    public void OnButtonClicked()
    {
        TryPlaceCharacter();
    }

    public bool TryPlaceCharacter()
    {
        if (isOccupied)
        {
            return false;
        }

        if (characterPrefab == null)
        {
            return false;
        }

        bool hasWeaponController = characterPrefab.GetComponent<CharacterWeaponController>() != null;
        if (!hasWeaponController && weaponPrefab == null)
        {
            return false;
        }

        GameObject characterObject = Instantiate(characterPrefab, transform);
        characterObject.name = "Character";
        characterObject.transform.localPosition = characterLocalPosition;
        characterObject.transform.localRotation = Quaternion.Euler(characterLocalEuler);

        if (shooter == null)
        {
            shooter = gameObject.AddComponent<AutoShooter>();
        }

        CharacterWeaponController weaponController = characterObject.GetComponent<CharacterWeaponController>();
        if (weaponController != null)
        {
            weaponController.BindShooter(shooter);
        }
        else
        {
            WeaponBehaviour mountedWeapon = Instantiate(weaponPrefab, transform);
            mountedWeapon.name = "EquippedWeapon";
            mountedWeapon.transform.localPosition = weaponLocalPosition;
            mountedWeapon.transform.localRotation = Quaternion.Euler(weaponLocalEuler);

            ICharacterShooter characterShooter = shooter;
            characterShooter.EquipWeapon(mountedWeapon);
        }

        isOccupied = true;

        if (slotSprite != null)
        {
            slotSprite.enabled = false;
        }

        if (slotButton != null)
        {
            slotButton.interactable = false;
        }

        if (slotCollider != null)
        {
            slotCollider.enabled = false;
        }

        return true;
    }
}

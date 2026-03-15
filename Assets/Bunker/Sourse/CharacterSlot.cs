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

    private static CharacterSlot selectedSourceSlot;

    private bool isOccupied;
    private AutoShooter shooter;
    private SpriteRenderer slotSprite;
    private Collider2D slotCollider;
    private Button slotButton;
    private GameObject spawnedCharacter;
    private WeaponBehaviour spawnedWeapon;

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
        CacheExistingOccupants();
        ApplyOccupiedVisualState();
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
            selectedSourceSlot = this;
            return true;
        }

        if (selectedSourceSlot == null || selectedSourceSlot == this || !selectedSourceSlot.isOccupied)
        {
            return false;
        }

        MoveFromSelectedSlot();
        selectedSourceSlot = null;
        return true;
    }

    private void MoveFromSelectedSlot()
    {
        GameObject characterToMove = selectedSourceSlot.spawnedCharacter;
        if (characterToMove == null)
        {
            characterToMove = selectedSourceSlot.transform.Find("Character")?.gameObject;
            if (characterToMove == null)
            {
                return;
            }

            selectedSourceSlot.spawnedCharacter = characterToMove;
        }

        WeaponBehaviour weaponToMove = selectedSourceSlot.spawnedWeapon;
        if (weaponToMove == null)
        {
            Transform weaponTransform = selectedSourceSlot.transform.Find("EquippedWeapon");
            if (weaponTransform != null)
            {
                weaponToMove = weaponTransform.GetComponent<WeaponBehaviour>();
                selectedSourceSlot.spawnedWeapon = weaponToMove;
            }
        }

        if (shooter == null)
        {
            shooter = gameObject.AddComponent<AutoShooter>();
        }

        AutoShooter sourceShooter = selectedSourceSlot.shooter;
        if (sourceShooter == null)
        {
            sourceShooter = selectedSourceSlot.GetComponent<AutoShooter>();
            selectedSourceSlot.shooter = sourceShooter;
        }

        if (sourceShooter != null)
        {
            sourceShooter.EquipWeapon(null);
        }

        characterToMove.transform.SetParent(transform, false);
        characterToMove.transform.localPosition = characterLocalPosition;
        characterToMove.transform.localRotation = Quaternion.Euler(characterLocalEuler);

        CharacterWeaponController weaponController = characterToMove.GetComponent<CharacterWeaponController>();
        if (weaponController != null)
        {
            weaponController.BindShooter(shooter);
        }
        else if (weaponToMove != null)
        {
            weaponToMove.transform.SetParent(transform, false);
            weaponToMove.transform.localPosition = weaponLocalPosition;
            weaponToMove.transform.localRotation = Quaternion.Euler(weaponLocalEuler);

            ICharacterShooter characterShooter = shooter;
            characterShooter.EquipWeapon(weaponToMove);
        }

        spawnedCharacter = characterToMove;
        spawnedWeapon = weaponToMove;
        isOccupied = true;
        ApplyOccupiedVisualState();

        selectedSourceSlot.spawnedCharacter = null;
        selectedSourceSlot.spawnedWeapon = null;
        selectedSourceSlot.isOccupied = false;
        selectedSourceSlot.ApplyOccupiedVisualState();
    }

    private void CacheExistingOccupants()
    {
        Transform characterTransform = transform.Find("Character");
        if (characterTransform != null)
        {
            spawnedCharacter = characterTransform.gameObject;
        }

        Transform weaponTransform = transform.Find("EquippedWeapon");
        if (weaponTransform != null)
        {
            spawnedWeapon = weaponTransform.GetComponent<WeaponBehaviour>();
        }

        isOccupied = spawnedCharacter != null;
    }

    private void ApplyOccupiedVisualState()
    {
        if (slotSprite != null)
        {
            slotSprite.enabled = !isOccupied;
        }

        if (slotButton != null)
        {
            slotButton.interactable = true;
        }

        if (slotCollider != null)
        {
            slotCollider.enabled = true;
        }
    }
}

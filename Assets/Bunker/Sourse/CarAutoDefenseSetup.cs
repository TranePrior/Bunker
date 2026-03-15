using System.Collections.Generic;
using UnityEngine;

public class CarAutoDefenseSetup : MonoBehaviour
{
    [Header("Slot Names")]
    [SerializeField] private string slot1Name = "point_1";
    [SerializeField] private string slot2Name = "point_2";
    [SerializeField] private string slot3Name = "point_3";
    [SerializeField] private string slot4Name = "point_4";

    [Header("Characters")]
    [SerializeField] private GameObject medicCharacterPrefab;
    [SerializeField] private GameObject mechanicCharacterPrefab;
    [SerializeField] private GameObject soldierCharacterPrefab;

    [Header("Weapons")]
    [SerializeField] private WeaponBehaviour medicWeaponPrefab;
    [SerializeField] private WeaponBehaviour mechanicWeaponPrefab;
    [SerializeField] private WeaponBehaviour soldierWeaponPrefab;

    [Header("Spawn Offset")]
    [SerializeField] private Vector3 characterLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 characterLocalEuler = Vector3.zero;
    [SerializeField] private Vector3 weaponLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 weaponLocalEuler = Vector3.zero;

    [Header("Legacy Objects To Hide")]
    [SerializeField] private string[] legacyObjectsToHide = { "Gun", "Gun_1", "Gun_2" };

    private readonly Dictionary<string, Transform> slotTransforms = new Dictionary<string, Transform>();
    private readonly Dictionary<string, CharacterId?> slotToCharacter = new Dictionary<string, CharacterId?>();
    private readonly Dictionary<CharacterId, string> characterToSlot = new Dictionary<CharacterId, string>();
    private readonly Dictionary<string, GameObject> slotCharacterObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, WeaponBehaviour> slotWeaponObjects = new Dictionary<string, WeaponBehaviour>();
    private string selectedSourceSlotName;

    private void Start()
    {
        HideLegacyObjects();
        CacheSlots();
        InitializeDefaultState();
    }

    public bool TryGetCharacterInSlot(string slotName, out CharacterId characterId)
    {
        characterId = default;
        if (!slotToCharacter.TryGetValue(slotName, out CharacterId? character))
        {
            return false;
        }

        if (!character.HasValue)
        {
            return false;
        }

        characterId = character.Value;
        return true;
    }

    public bool TryGetSlotForCharacter(CharacterId characterId, out string slotName)
    {
        return characterToSlot.TryGetValue(characterId, out slotName);
    }

    public void HandleSlotClicked(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName) || !slotTransforms.ContainsKey(slotName))
        {
            return;
        }

        if (slotToCharacter.TryGetValue(slotName, out CharacterId? clickedCharacter) && clickedCharacter.HasValue)
        {
            selectedSourceSlotName = slotName;
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedSourceSlotName))
        {
            return;
        }

        if (selectedSourceSlotName == slotName)
        {
            return;
        }

        if (!slotToCharacter.TryGetValue(selectedSourceSlotName, out CharacterId? sourceCharacter) || !sourceCharacter.HasValue)
        {
            selectedSourceSlotName = null;
            return;
        }

        MoveCharacterObject(selectedSourceSlotName, slotName, sourceCharacter.Value);
        selectedSourceSlotName = null;
    }

    private void InitializeDefaultState()
    {
        AssignCharacterToSlot(CharacterId.Medic, slot1Name);
        AssignCharacterToSlot(CharacterId.Mechanic, slot2Name);
        AssignCharacterToSlot(CharacterId.Soldier, slot3Name);
        SetSlotEmpty(slot4Name);
    }

    private void AssignCharacterToSlot(CharacterId characterId, string slotName)
    {
        if (!slotTransforms.TryGetValue(slotName, out Transform slotTransform))
        {
            return;
        }

        if (characterToSlot.TryGetValue(characterId, out string previousSlotName))
        {
            SetSlotEmpty(previousSlotName);
        }

        if (slotToCharacter.TryGetValue(slotName, out CharacterId? existingCharacter) && existingCharacter.HasValue)
        {
            characterToSlot.Remove(existingCharacter.Value);
        }

        ClearSpawnedObjects(slotName);

        GameObject characterPrefab = GetCharacterPrefab(characterId);
        bool hasWeaponController = characterPrefab != null && characterPrefab.GetComponent<CharacterWeaponController>() != null;
        WeaponBehaviour weaponPrefab = hasWeaponController ? null : GetWeaponPrefab(characterId);
        if (characterPrefab == null || (!hasWeaponController && weaponPrefab == null))
        {
            SetSlotVisual(slotTransform, false);
            slotToCharacter[slotName] = null;
            return;
        }

        GameObject characterObject = Instantiate(characterPrefab, slotTransform);
        characterObject.name = "Character";
        characterObject.transform.localPosition = characterLocalPosition;
        characterObject.transform.localRotation = Quaternion.Euler(characterLocalEuler);

        AutoShooter shooter = slotTransform.GetComponent<AutoShooter>();
        if (shooter == null)
        {
            shooter = slotTransform.gameObject.AddComponent<AutoShooter>();
        }

        CharacterWeaponController weaponController = characterObject.GetComponent<CharacterWeaponController>();
        if (weaponController != null)
        {
            weaponController.BindShooter(shooter);
        }
        else
        {
            WeaponBehaviour weaponObject = Instantiate(weaponPrefab, slotTransform);
            weaponObject.name = "EquippedWeapon";
            weaponObject.transform.localPosition = weaponLocalPosition;
            weaponObject.transform.localRotation = Quaternion.Euler(weaponLocalEuler);

            ICharacterShooter characterShooter = shooter;
            characterShooter.EquipWeapon(weaponObject);
            slotWeaponObjects[slotName] = weaponObject;
        }

        slotCharacterObjects[slotName] = characterObject;
        slotToCharacter[slotName] = characterId;
        characterToSlot[characterId] = slotName;

        SetSlotVisual(slotTransform, true);
    }

    private void SetSlotEmpty(string slotName)
    {
        if (!slotTransforms.TryGetValue(slotName, out Transform slotTransform))
        {
            return;
        }

        if (slotToCharacter.TryGetValue(slotName, out CharacterId? character) && character.HasValue)
        {
            characterToSlot.Remove(character.Value);
        }

        ClearSpawnedObjects(slotName);
        slotToCharacter[slotName] = null;
        SetSlotVisual(slotTransform, false);
    }

    private void ClearSpawnedObjects(string slotName)
    {
        if (slotCharacterObjects.TryGetValue(slotName, out GameObject characterObject) && characterObject != null)
        {
            Destroy(characterObject);
        }

        if (slotWeaponObjects.TryGetValue(slotName, out WeaponBehaviour weaponObject) && weaponObject != null)
        {
            Destroy(weaponObject.gameObject);
        }

        slotCharacterObjects.Remove(slotName);
        slotWeaponObjects.Remove(slotName);
    }

    private void SetSlotVisual(Transform slotTransform, bool occupied)
    {
        SpriteRenderer sprite = slotTransform.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.enabled = !occupied;
        }
    }

    private void CacheSlots()
    {
        slotTransforms.Clear();
        slotToCharacter.Clear();
        characterToSlot.Clear();
        slotCharacterObjects.Clear();
        slotWeaponObjects.Clear();

        CacheSlot(slot1Name);
        CacheSlot(slot2Name);
        CacheSlot(slot3Name);
        CacheSlot(slot4Name);
    }

    private void CacheSlot(string slotName)
    {
        Transform slot = FindInChildrenByName(slotName);
        if (slot == null)
        {
            return;
        }

        CharacterSlot oldSlotLogic = slot.GetComponent<CharacterSlot>();
        if (oldSlotLogic != null)
        {
            Destroy(oldSlotLogic);
        }

        CarDefenseSlotClickProxy clickProxy = slot.GetComponent<CarDefenseSlotClickProxy>();
        if (clickProxy == null)
        {
            clickProxy = slot.gameObject.AddComponent<CarDefenseSlotClickProxy>();
        }

        clickProxy.Initialize(this, slotName);

        slotTransforms[slotName] = slot;
        slotToCharacter[slotName] = null;
    }

    private void MoveCharacterObject(string fromSlotName, string toSlotName, CharacterId characterId)
    {
        if (!slotTransforms.TryGetValue(fromSlotName, out Transform fromSlot))
        {
            return;
        }

        if (!slotTransforms.TryGetValue(toSlotName, out Transform toSlot))
        {
            return;
        }

        if (slotToCharacter.TryGetValue(toSlotName, out CharacterId? destinationCharacter) && destinationCharacter.HasValue)
        {
            return;
        }

        if (!slotCharacterObjects.TryGetValue(fromSlotName, out GameObject characterObject) || characterObject == null)
        {
            Transform fallbackCharacter = fromSlot.Find("Character");
            if (fallbackCharacter == null)
            {
                return;
            }

            characterObject = fallbackCharacter.gameObject;
        }

        slotWeaponObjects.TryGetValue(fromSlotName, out WeaponBehaviour weaponObject);

        AutoShooter sourceShooter = fromSlot.GetComponent<AutoShooter>();
        if (sourceShooter != null)
        {
            sourceShooter.EquipWeapon(null);
        }

        AutoShooter destinationShooter = toSlot.GetComponent<AutoShooter>();
        if (destinationShooter == null)
        {
            destinationShooter = toSlot.gameObject.AddComponent<AutoShooter>();
        }

        characterObject.transform.SetParent(toSlot, false);
        characterObject.transform.localPosition = characterLocalPosition;
        characterObject.transform.localRotation = Quaternion.Euler(characterLocalEuler);

        CharacterWeaponController weaponController = characterObject.GetComponent<CharacterWeaponController>();
        if (weaponController != null)
        {
            weaponController.BindShooter(destinationShooter);
        }
        else
        {
            if (weaponObject == null)
            {
                Transform fallbackWeapon = fromSlot.Find("EquippedWeapon");
                if (fallbackWeapon != null)
                {
                    weaponObject = fallbackWeapon.GetComponent<WeaponBehaviour>();
                }
            }

            if (weaponObject != null)
            {
                weaponObject.transform.SetParent(toSlot, false);
                weaponObject.transform.localPosition = weaponLocalPosition;
                weaponObject.transform.localRotation = Quaternion.Euler(weaponLocalEuler);

                ICharacterShooter characterShooter = destinationShooter;
                characterShooter.EquipWeapon(weaponObject);
            }
        }

        slotCharacterObjects.Remove(fromSlotName);
        slotCharacterObjects[toSlotName] = characterObject;

        slotWeaponObjects.Remove(fromSlotName);
        if (weaponObject != null)
        {
            slotWeaponObjects[toSlotName] = weaponObject;
        }
        else
        {
            slotWeaponObjects.Remove(toSlotName);
        }

        slotToCharacter[fromSlotName] = null;
        slotToCharacter[toSlotName] = characterId;
        characterToSlot[characterId] = toSlotName;

        SetSlotVisual(fromSlot, false);
        SetSlotVisual(toSlot, true);
    }

    private GameObject GetCharacterPrefab(CharacterId characterId)
    {
        switch (characterId)
        {
            case CharacterId.Medic:
                return medicCharacterPrefab;
            case CharacterId.Mechanic:
                return mechanicCharacterPrefab;
            case CharacterId.Soldier:
                return soldierCharacterPrefab;
            default:
                return null;
        }
    }

    private WeaponBehaviour GetWeaponPrefab(CharacterId characterId)
    {
        switch (characterId)
        {
            case CharacterId.Medic:
                return medicWeaponPrefab;
            case CharacterId.Mechanic:
                return mechanicWeaponPrefab;
            case CharacterId.Soldier:
                return soldierWeaponPrefab;
            default:
                return null;
        }
    }

    private void HideLegacyObjects()
    {
        if (legacyObjectsToHide == null)
        {
            return;
        }

        for (int i = 0; i < legacyObjectsToHide.Length; i++)
        {
            Transform legacy = FindInChildrenByName(legacyObjectsToHide[i]);
            if (legacy != null)
            {
                legacy.gameObject.SetActive(false);
            }
        }
    }

    private Transform FindInChildrenByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allChildren.Length; i++)
        {
            if (allChildren[i] != transform && allChildren[i].name == objectName)
            {
                return allChildren[i];
            }
        }

        return null;
    }
}

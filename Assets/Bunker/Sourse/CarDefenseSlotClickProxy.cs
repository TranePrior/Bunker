using UnityEngine;

[DisallowMultipleComponent]
public class CarDefenseSlotClickProxy : MonoBehaviour
{
    [SerializeField] private CarAutoDefenseSetup setup;
    [SerializeField] private string slotName;

    public void Initialize(CarAutoDefenseSetup setupRef, string configuredSlotName)
    {
        setup = setupRef;
        slotName = configuredSlotName;
    }

    private void OnMouseDown()
    {
        if (setup == null)
        {
            return;
        }

        setup.HandleSlotClicked(slotName);
    }
}

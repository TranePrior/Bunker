using UnityEngine;

public class CarAutoMove : MonoBehaviour
{
    [Header("Optional manual references")]
    [SerializeField] private Transform carSpawnPoint;
    [SerializeField] private Transform midlePoint;

    [Header("Movement")]
    [SerializeField] private float speed = 120f;
    [SerializeField] private float stopDistance = 1f;

    private bool hasReachedTarget;

    private void Awake()
    {
        if (carSpawnPoint == null)
        {
            GameObject spawn = GameObject.Find("Car_spawn_point");
            if (spawn != null)
            {
                carSpawnPoint = spawn.transform;
            }
        }

        if (midlePoint == null)
        {
            GameObject middle = GameObject.Find("Midle_point");
            if (middle != null)
            {
                midlePoint = middle.transform;
            }
        }
    }

    private void Start()
    {
        if (carSpawnPoint != null)
        {
            transform.position = carSpawnPoint.position;
        }
    }

    private void Update()
    {
        if (hasReachedTarget || midlePoint == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            midlePoint.position,
            speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, midlePoint.position) <= stopDistance)
        {
            transform.position = midlePoint.position;
            hasReachedTarget = true;
        }
    }
}

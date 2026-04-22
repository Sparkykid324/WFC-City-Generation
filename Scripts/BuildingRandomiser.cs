using UnityEngine;

public class BuildingRandomiser : MonoBehaviour
{
    public GameObject[] buildingVariants;

    void Awake()
    {
        if (buildingVariants == null || buildingVariants.Length == 0) return;

        GameObject chosen = buildingVariants[Random.Range(0, buildingVariants.Length)];
        Instantiate(chosen, transform.position, transform.rotation, transform);
    }
}

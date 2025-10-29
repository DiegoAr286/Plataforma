using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Spawner : MonoBehaviour
{
    private Collider[] spawnAreaColliders;
    private Collider spawnArea;

    public GameObject[] fruitPrefabs;

    public GameObject bombPrefab;

    public float maxLifetime = 5f;

    private List<ElementData> spawnSequence;

    public static event System.Action<GameObject> OnFruitSpawned;

    private void Awake()
    {
        spawnAreaColliders = GetComponents<Collider>();
    }

    void Start()
    {
        if (CSVLoader.Instance == null || CSVLoader.Instance.elementSequence.Count == 0)
        {
            Debug.LogError("CSVLoader no está listo o la secuencia está vacía. Asegúrate de cargar la secuencia antes de iniciar la escena.");
            return;
        }

        spawnSequence = CSVLoader.Instance.elementSequence;
    }

    private void OnEnable()
    {
        StartCoroutine(Spawn());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Spawn()
    {
        yield return new WaitForSeconds(2f);

        foreach (ElementData data in spawnSequence)
        {
            GameObject prefab;

            if (data.Type == 5)
                prefab = bombPrefab;
            else
                prefab = fruitPrefabs[data.Type];

            spawnArea = spawnAreaColliders[data.Side];

            Quaternion rotation = Quaternion.Euler(0f, 0f, data.Angle);

            GameObject fruit = Instantiate(prefab, data.Position, rotation);
            Destroy(fruit, maxLifetime);

            OnFruitSpawned?.Invoke(fruit);

            fruit.GetComponent<Rigidbody>().AddForce(fruit.transform.up * data.Force, ForceMode.Impulse);

            yield return new WaitForSeconds(data.WaitTime);
        }
    }
}

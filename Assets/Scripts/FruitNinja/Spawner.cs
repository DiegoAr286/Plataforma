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
        if (CSVLoader.Instance == null)
            return;

        if (CSVLoader.Instance.session == "Pre")
            CSVLoader.Instance.LoadSequence("FruitNinja_Pre");
        else if (CSVLoader.Instance.session == "Post")
            CSVLoader.Instance.LoadSequence("FruitNinja_Post");

        if (CSVLoader.Instance.elementSequence.Count == 0)
            return;

        spawnSequence = CSVLoader.Instance.elementSequence;
        Debug.Log(spawnSequence);
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

        int i = 0;
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
            i++;
            Debug.Log(i);
            yield return new WaitForSeconds(data.WaitTime);
        }
    }
}

using DaqUtils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text scoreText;
    public Image fadeImage;

    public HapticPlugin omni;
    public double forceMagnitude = 0.2;

    public FileManagerFN fileManager;

    private Blade blade;
    private Spawner spawner;

    private int score;

    // Daq Connection
    private DaqConnection daqConnector;
    private bool writeState = false;

    private void Awake()
    {
        blade = FindObjectOfType<Blade>();
        spawner = FindObjectOfType<Spawner>();
        
        daqConnector = new DaqConnection();
        daqConnector.StartConnection();
    }

    private void Start()
    {
        NewGame();
    }
    private void Update()
    {
        HapticPlugin.setForce(omni.configName, new double[] { 0, -forceMagnitude, 0 }, new double[] { 0, 0, 0 });
    }
    private void NewGame()
    {
        Time.timeScale = 1f;

        blade.enabled = true;
        spawner.enabled = true;

        score = 0;
        scoreText.text = score.ToString();

        ClearScene();
    }

    private void ClearScene()
    {
        Fruit[] fruits = FindObjectsOfType<Fruit>();

        foreach (Fruit fruit in fruits)
        {
            Destroy(fruit.gameObject);
        }

        Bomb[] bombs = FindObjectsOfType<Bomb>();

        foreach (Bomb bomb in bombs)
        {
            Destroy(bomb.gameObject);
        }
    }

    public void IncreaseScore(int amount)
    {
        RunTrigger(2);

        score += amount;
        scoreText.text = score.ToString();

        fileManager.StoreCut();
        fileManager.StoreScore(score);
    }

    public void Explode()
    {
        //blade.enabled = false;
        //spawner.enabled = false;

        StartCoroutine(ExplodeSequence());
    }

    private IEnumerator ExplodeSequence()
    {
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(Color.clear, Color.white, t);

            //Time.timeScale = 1f - t;
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(.2f);

        //NewGame();

        elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(Color.white, Color.clear, t);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
    }

    IEnumerator TriggerPulseWidth(int trigger)
    {
        yield return new WaitForSecondsRealtime(0.01f);

        if (writeState)
            writeState = RunTrigger(trigger, endPulse: true);
    }

    public bool RunTrigger(int trigger, bool endPulse = false)
    {
        bool status = false;
        uint[] message = { 1 };
        switch (trigger)
        {
            case 0:
                message = new uint[] { 1, 1, 1, 1, 1, 1, 1, 1 };
                status = false;
                break;
            case 1:
                message = new uint[] { 0, 1, 1, 1, 1, 1, 1, 1 };
                break;
            case 2:
                message = new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 };
                break;
            case 3:
                message = new uint[] { 1, 1, 0, 1, 1, 1, 1, 1 };
                break;
            case 4:
                message = new uint[] { 1, 1, 1, 0, 1, 1, 1, 1 };
                break;
            case 5:
                message = new uint[] { 1, 1, 1, 1, 0, 1, 1, 1 };
                break;
            case 6:
                message = new uint[] { 1, 1, 1, 1, 1, 0, 1, 1 };
                break;
            case 7:
                message = new uint[] { 1, 1, 1, 1, 1, 1, 0, 1 };
                break;
            case 8:
                message = new uint[] { 1, 1, 1, 1, 1, 1, 1, 0 };
                break;
            case 9:
                message = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                break;
        }

        writeState = daqConnector.WriteDigitalValue(message, endPulse, port: 0);

        if (endPulse)
            writeState = false;

        if (trigger != 0)
        {
            fileManager.StoreTrigger(trigger);
            StartCoroutine(TriggerPulseWidth(trigger));
        }

        return status;
    }

    private void OnEnable()
    {
        Spawner spawnerInstance = FindObjectOfType<Spawner>();
        if (spawnerInstance != null)
        {
            Spawner.OnFruitSpawned += OnFruitSpawn;
        }
    }

    private void OnDisable()
    {
        Spawner spawnerInstance = FindObjectOfType<Spawner>();
        if (spawnerInstance != null)
        {
            Spawner.OnFruitSpawned -= OnFruitSpawn;
        }
    }

    private void OnFruitSpawn(GameObject fruit)
    {
        RunTrigger(1);

        fileManager.StoreTrial();
        fileManager.StoreFruit(fruit.name);
        //fileManager.StoreFruit(fruit.GetComponent<Fruit>().points);
    }
}

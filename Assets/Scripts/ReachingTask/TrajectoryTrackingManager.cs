using DaqUtils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrajectoryTrackingManager : MonoBehaviour
{
    private enum TrialState
    {
        Setup,
        WaitingForStart,
        CueTarget,
        ReachingOutbound,
        DwellingAtTarget,
        ReachingInbound,
        InterTrialInterval,
        Finished
    }

    private GameObject homeTarget;
    private List<GameObject> peripheralTargets;

    [Header("Parámetros del Paradigma")]
    //[Tooltip("Tiempo en segundos que el usuario debe mantener el cursor en el objetivo inicial para empezar.")]
    //[SerializeField] private float startDwellTime = 2.0f;
    [Tooltip("Tiempo en segundos que el usuario debe mantener el cursor en el objetivo periférico.")]
    [SerializeField] private float targetDwellTime = 0.5f;
    [Tooltip("Tiempo de pausa en segundos entre el final de un ensayo y el inicio del siguiente.")]
    [SerializeField] private float interTrialInterval = 1.0f;

    [Tooltip("Número total de 'vueltas' completas (cada vuelta incluye todos los targets periféricos una vez).")]
    [SerializeField] private int totalCircuits = 1;

    [Header("Materiales Visuales")]
    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material cueMaterial;
    [SerializeField] private Material correctMaterial;

    [Header("Configuración del Plano")]
    [SerializeField] private GameObject basePlane;
    [Tooltip("Plano del experimento")]
    [SerializeField] private bool verticalPlane = true;

    private TrialState currentState;
    private List<int> trialSequence;
    private int currentTrialIndex = -1;
    //private float dwellTimer = 0f;

    private int completedCircuits = 0;

    private Renderer homeTargetRenderer;

    [Header("Manejo de archivos")]
    public FileManagerRT fileManager;

    void Start()
    {
        StartCoroutine(InitializeParadigm());
    }

    private void SetPlane()
    {
        verticalPlane = PlayerPrefs.GetInt("VerticalPlane", 1) == 1;

        if (!verticalPlane)
        {
            Camera.main.transform.position = new Vector3(0f, 4f, 0f);
            Camera.main.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            basePlane.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private IEnumerator InitializeParadigm()
    {
        yield return null;

        SetPlane();

        homeTarget = GameObject.FindGameObjectWithTag("HomeTarget");
        peripheralTargets = new List<GameObject>(GameObject.FindGameObjectsWithTag("PeripheralTarget"));

        if (homeTarget == null || peripheralTargets.Count == 0)
        {
            Debug.LogError("No se encontraron los targets en la escena. Asegúrate de que están generados y tienen los Tags 'HomeTarget' y 'PeripheralTarget'.");
            this.enabled = false;
            yield break;
        }

        Debug.Log($"Encontrados {peripheralTargets.Count} targets periféricos y 1 target central.");
        homeTargetRenderer = homeTarget.GetComponent<Renderer>();

        SetState(TrialState.Setup);
    }

    void Update()
    {
        switch (currentState)
        {
            case TrialState.WaitingForStart:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("barra presionada");
                    StartNextTrial();
                }
                break;
        }
    }

    private void SetState(TrialState newState)
    {
        currentState = newState;
        Debug.Log($"Nuevo Estado: {newState}");

        switch (newState)
        {
            case TrialState.Setup:
                trialSequence = Enumerable.Range(0, peripheralTargets.Count).OrderBy(x => Random.value).ToList();
                currentTrialIndex = -1;
                SetAllTargetsMaterial(idleMaterial);
                homeTargetRenderer.material = cueMaterial;
                Debug.Log("Setup completado. Presiona ESPACIO con el cursor en el centro para empezar.");
                SetState(TrialState.WaitingForStart);
                break;
        }
    }

    private void GenerateNewTrialSequence()
    {
        trialSequence = Enumerable.Range(0, peripheralTargets.Count).OrderBy(x => Random.value).ToList();
    }

    private void StartNextTrial()
    {
        //dwellTimer = 0f;
        currentTrialIndex++;

        if (currentTrialIndex >= trialSequence.Count)
        {
            completedCircuits++;

            Debug.Log($"Vuelta {completedCircuits} de {totalCircuits} completada.");

            if (completedCircuits >= totalCircuits)
            {
                SetState(TrialState.Finished);
                Debug.Log("¡Sesión finalizada!");
                ExitParadigm();
                return;
            }

            GenerateNewTrialSequence();
            currentTrialIndex = 0;
        }

        int targetIndex = trialSequence[currentTrialIndex];
        peripheralTargets[targetIndex].GetComponent<Renderer>().material = cueMaterial;
        homeTargetRenderer.material = idleMaterial;

        fileManager.StoreTrial();

        SetState(TrialState.ReachingOutbound);
    }

    private void ExitParadigm()
    {
        //RunTrigger(8, endPulse: true);

        fileManager.WriteData();

        //daqConnector.EndConnection();
        SceneManager.LoadScene(0);
    }

    public void OnCursorEnterTarget(GameObject targetObject)
    {
        if (currentState == TrialState.ReachingOutbound)
        {
            int targetIndex = peripheralTargets.IndexOf(targetObject);
            if (targetIndex == trialSequence[currentTrialIndex])
            {
                targetObject.GetComponent<Renderer>().material = correctMaterial;
                Debug.Log("Objetivo alcanzado");
                fileManager.StoreSphere(targetIndex);
                SetState(TrialState.DwellingAtTarget);
                StartCoroutine(DwellAtTargetCoroutine());
            }
        }
        else if (currentState == TrialState.ReachingInbound)
        {
            if (targetObject == homeTarget)
            {
                homeTargetRenderer.material = correctMaterial;
                SetState(TrialState.InterTrialInterval);
                StartCoroutine(InterTrialCoroutine());
            }
        }
    }

    private IEnumerator DwellAtTargetCoroutine()
    {
        yield return new WaitForSeconds(targetDwellTime);
        peripheralTargets[trialSequence[currentTrialIndex]].GetComponent<Renderer>().material = idleMaterial;
        homeTargetRenderer.material = cueMaterial;
        SetState(TrialState.ReachingInbound);
    }

    private IEnumerator InterTrialCoroutine()
    {
        yield return new WaitForSeconds(interTrialInterval);
        homeTargetRenderer.material = idleMaterial;
        StartNextTrial();
    }

    private void SetAllTargetsMaterial(Material mat)
    {
        homeTargetRenderer.material = mat;
        foreach (var target in peripheralTargets)
        {
            target.GetComponent<Renderer>().material = mat;
        }
    }
}
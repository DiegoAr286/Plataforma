using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para la aleatorización (OrderBy)

public class TrajectoryTrackingManager : MonoBehaviour
{
    // --- Definición de los estados del ensayo ---
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

    [Header("Referencias de la Escena")]
    [Tooltip("El objeto que representa el cursor del usuario.")]
    [SerializeField] private Transform cursor;

    private GameObject homeTarget;
    private List<GameObject> peripheralTargets;

    [Header("Parámetros del Paradigma")]
    [Tooltip("Tiempo en segundos que el usuario debe mantener el cursor en el objetivo inicial para empezar.")]
    [SerializeField] private float startDwellTime = 2.0f;
    [Tooltip("Tiempo en segundos que el usuario debe mantener el cursor en el objetivo periférico.")]
    [SerializeField] private float targetDwellTime = 0.5f;
    [Tooltip("Tiempo de pausa en segundos entre el final de un ensayo y el inicio del siguiente.")]
    [SerializeField] private float interTrialInterval = 1.0f;

    [Header("Materiales Visuales")]
    [SerializeField] private Material idleMaterial;   // Color neutro
    [SerializeField] private Material cueMaterial;    // Color que indica "ve aquí"
    [SerializeField] private Material correctMaterial; // Color que indica "estás en el lugar correcto"

    // --- Variables de estado internas ---
    private TrialState currentState;
    private List<int> trialSequence;
    private int currentTrialIndex = -1;
    private float dwellTimer = 0f;

    // --- Referencias a los componentes (cache) ---
    private Renderer homeTargetRenderer;

    void Start()
    {
        StartCoroutine(InitializeParadigm());

    }

    /// <summary>
    /// Inicializa el paradigma después de un breve retardo para dar tiempo
    /// a otros scripts (como el generador de círculos) a ejecutarse.
    /// </summary>
    private IEnumerator InitializeParadigm()
    {
        // Espera un frame para asegurar que todo en Start() se ha ejecutado
        yield return null;

        // --- ✨ LÓGICA DE BÚSQUEDA DINÁMICA DE TARGETS ✨ ---
        homeTarget = GameObject.FindGameObjectWithTag("HomeTarget");
        peripheralTargets = new List<GameObject>(GameObject.FindGameObjectsWithTag("PeripheralTarget"));

        if (homeTarget == null || peripheralTargets.Count == 0)
        {
            Debug.LogError("No se encontraron los targets en la escena. Asegúrate de que están generados y tienen los Tags 'HomeTarget' y 'PeripheralTarget'.");
            this.enabled = false; // Desactiva el script si no hay targets
            yield break;
        }

        Debug.Log($"Encontrados {peripheralTargets.Count} targets periféricos y 1 target central.");
        // --------------------------------------------------------

        // Cacheamos los componentes para mayor eficiencia
        homeTargetRenderer = homeTarget.GetComponent<Renderer>();

        // El resto de la lógica de Setup ahora puede ejecutarse de forma segura
        SetState(TrialState.Setup);
    }

    void Update()
    {
        // --- Máquina de Estados ---
        switch (currentState)
        {
            case TrialState.WaitingForStart:
                // Espera a que el investigador inicie la prueba
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("barra presionada");
                    //// Lógica para detectar si el cursor está en el Home Target
                    //// (Esto se manejará con Triggers, pero como fallback, usamos distancia)
                    //float distanceToHome = Vector3.Distance(cursor.position, homeTarget.transform.position);
                    //if (distanceToHome < homeTarget.transform.localScale.x / 2) // Asume que el target es una esfera/círculo
                    //{
                    //    dwellTimer += Time.deltaTime;
                    //    if (dwellTimer >= startDwellTime)
                    //    {
                    StartNextTrial();
                    //    }
                    //}
                    //else
                    //{
                    //    dwellTimer = 0f; // Resetea el contador si el cursor se sale
                    //}
                }
                break;
        }
    }

    private void SetState(TrialState newState)
    {
        currentState = newState;
        Debug.Log($"Nuevo Estado: {newState}");

        // --- Lógica de entrada a cada estado ---
        switch (newState)
        {
            case TrialState.Setup:
                // Prepara la secuencia aleatoria de ensayos
                trialSequence = Enumerable.Range(0, peripheralTargets.Count).OrderBy(x => Random.value).ToList();
                currentTrialIndex = -1;
                // Configura visuales iniciales
                SetAllTargetsMaterial(idleMaterial);
                homeTargetRenderer.material = cueMaterial;
                Debug.Log("Setup completado. Presiona ESPACIO con el cursor en el centro para empezar.");
                SetState(TrialState.WaitingForStart);
                break;
        }
    }

    private void StartNextTrial()
    {
        dwellTimer = 0f;
        currentTrialIndex++;

        if (currentTrialIndex >= trialSequence.Count)
        {
            SetState(TrialState.Finished);
            Debug.Log("¡Bloque de ensayos finalizado!");
            // Aquí llamarías al DataLogger para guardar el archivo.
            return;
        }

        // Iluminar el siguiente objetivo
        int targetIndex = trialSequence[currentTrialIndex];
        peripheralTargets[targetIndex].GetComponent<Renderer>().material = cueMaterial;
        homeTargetRenderer.material = idleMaterial;

        // Aquí registrarías el evento y enviarías el trigger
        // DataLogger.Instance.LogEvent("Target_Appeared", PrecisionClock.Instance.Time, $"TargetID:{targetIndex}");
        // TriggerController.Instance.SendTrigger();

        SetState(TrialState.ReachingOutbound);
    }

    // --- Métodos para ser llamados por los Triggers de los Objetivos ---
    public void OnCursorEnterTarget(GameObject targetObject)
    {
        if (currentState == TrialState.ReachingOutbound)
        {
            int targetIndex = peripheralTargets.IndexOf(targetObject);
            if (targetIndex == trialSequence[currentTrialIndex])
            {
                // El usuario ha alcanzado el objetivo correcto
                targetObject.GetComponent<Renderer>().material = correctMaterial;
                Debug.Log("objetivo alcanzado");
                // DataLogger.Instance.LogEvent("Target_Reached", ...);
                SetState(TrialState.DwellingAtTarget);
                StartCoroutine(DwellAtTargetCoroutine());
            }
        }
        else if (currentState == TrialState.ReachingInbound)
        {
            if (targetObject == homeTarget)
            {
                // El usuario ha vuelto al centro
                homeTargetRenderer.material = correctMaterial;
                // DataLogger.Instance.LogEvent("Home_Reached", ...);
                SetState(TrialState.InterTrialInterval);
                StartCoroutine(InterTrialCoroutine());
            }
        }
    }

    // --- Coroutines para las pausas ---
    private IEnumerator DwellAtTargetCoroutine()
    {
        yield return new WaitForSeconds(targetDwellTime);
        // El dwell ha terminado, ahora indicamos que vuelva al centro
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
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TMP_InputField subjectIdField;
    [SerializeField] private TMP_Dropdown sessionDropdown;
    [SerializeField] private TMP_Dropdown paradigmDropdown;
    [SerializeField] private Button startButton;

    void Start()
    {
        // --- Configuración Inicial de la UI ---

        // Limpia y añade las opciones al Dropdown de paradigmas
        paradigmDropdown.ClearOptions();
        paradigmDropdown.AddOptions(new List<string>
        {
            "...",
            "Seguimiento de Trayectoria", // El texto que ve el usuario
            "NHPT Cue Based",
            "Fruit Ninja"
        });

        // Limpia y añade las opciones al Dropdown de sesión
        sessionDropdown.ClearOptions();
        sessionDropdown.AddOptions(new List<string> { "Pre", "Post" });

        // Asocia la función StartExperiment al evento OnClick del botón
        startButton.onClick.AddListener(StartExperiment);
    }

    private void StartExperiment()
    {
        string subjectID = subjectIdField.text;
        string session = sessionDropdown.options[sessionDropdown.value].text;
        string paradigmName = paradigmDropdown.options[paradigmDropdown.value].text;

        // Validación simple
        if (string.IsNullOrWhiteSpace(subjectID))
        {
            Debug.LogError("El ID de sujeto no puede estar vacío.");
            return;
        }


        if (GenericDataLogger.Instance != null)
        {
            GenericDataLogger.Instance.SubjectID = subjectID;
            GenericDataLogger.Instance.Session = session;
            GenericDataLogger.Instance.ParadigmName = paradigmName.Replace(" ", "");
        }
        else
        {
            Debug.LogError("¡No se encontró una instancia del DataLogger en la escena!");
            return;
        }

        string sceneToLoad = "";
        switch (paradigmName)
        {
            case "Seguimiento de Trayectoria":
                sceneToLoad = "ReachingTask";
                break;
            case "NHPT Cue Based":
                sceneToLoad = "NHPTCueBased";
                break;
            case "Fruit Ninja":
                sceneToLoad = "FruitNinja";
                break;
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Cargando escena: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
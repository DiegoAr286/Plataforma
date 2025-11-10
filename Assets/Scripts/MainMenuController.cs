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

    //GameObject toggleGroupObject;
    //private const string GroupName = "HandToggle";

    //private void Awake()
    //{
    //    toggleGroupObject = GameObject.Find(GroupName);
    //    toggleGroupObject.SetActive(false);
    //}

    void Start()
    {
        // --- Configuración Inicial de la UI ---

        // Limpia y añade las opciones al Dropdown de paradigmas
        paradigmDropdown.ClearOptions();
        paradigmDropdown.AddOptions(new List<string>
        {
            "...",
            "Seguimiento de Trayectoria", // El texto que ve el usuario
            "NHPT Self Paced",
            "Fruit Ninja"
        });

        // Limpia y añade las opciones al Dropdown de sesión
        sessionDropdown.ClearOptions();
        sessionDropdown.AddOptions(new List<string> { "Pre", "Post" });

        PlayerPrefs.SetInt("CameraRec", 0);
        PlayerPrefs.SetInt("AnalogAcquisition", 0);
        PlayerPrefs.SetInt("SerialConnection", 0);
        PlayerPrefs.SetInt("RightHand_NHPT", 1);

        // Asocia la función StartExperiment al evento OnClick del botón
        startButton.onClick.AddListener(StartExperiment);
    }

    private void StartExperiment()
    {
        string subjectID = subjectIdField.text;
        string session = sessionDropdown.options[sessionDropdown.value].text;
        string paradigmName = paradigmDropdown.options[paradigmDropdown.value].text;

        session += PlayerPrefs.GetInt("RightHand_NHPT") == 1 ? "_RightHand" : "_LeftHand";

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
            GenericDataLogger.Instance.SetFilePath();
        }
        else
        {
            Debug.LogError("¡No se encontró una instancia del DataLogger en la escena!");
            return;
        }

        if (CSVLoader.Instance != null)
        {
            CSVLoader.Instance.session = sessionDropdown.options[sessionDropdown.value].text;
        }
        else
        {
            Debug.LogError("¡No se encontró una instancia del CSVLoader en la escena!");
            return;
        }

        string sceneToLoad = "";
        switch (paradigmName)
        {
            case "Seguimiento de Trayectoria":
                sceneToLoad = "ReachingTask";
                break;
            case "NHPT Self Paced":
                sceneToLoad = "NHPT_SelfPaced_Scene";
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

    //public void EnableHandChoice()
    //{
    //    if (paradigmDropdown.options[paradigmDropdown.value].text == "NHPT Self Paced")
    //        toggleGroupObject.SetActive(true);
    //    else
    //        toggleGroupObject.SetActive(false);
    //}


    public void SetLeftHand(bool isLeftHand)
    {
        PlayerPrefs.SetInt("RightHand_NHPT", isLeftHand ? 1 : 0);
    }
    public void SetRightHand(bool isRightHand)
    {
        PlayerPrefs.SetInt("RightHand_NHPT", isRightHand ? 0 : 1);
    }
}
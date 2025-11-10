using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

/// <summary>
/// Un DataLogger genérico y dinámico que se configura desde el Inspector.
/// Utiliza el tiempo de Unity y tiene columnas dedicadas para eventos y datos continuos.
/// </summary>
public class GenericDataLogger : MonoBehaviour
{
    // --- Implementación del Singleton ---
    public static GenericDataLogger Instance { get; private set; }

    // --- Variables de Configuración (ahora son públicas para ser seteadas por la UI) ---
    public string SubjectID { get; set; } = "S00";
    public string Session { get; set; } = "Pre";
    public string ParadigmName { get; set; } = "DefaultParadigm";

    [Header("Definición de Columnas")]
    [Tooltip("Define aquí los nombres para cada columna de EVENTO. Ej: 'TrialStart', 'Score', 'TriggerSent'.")]
    [SerializeField] private List<string> eventHeaders;

    [Tooltip("Define aquí las cabeceras para los datos de muestreo continuo. Ej: 'Cursor_X', 'Force_Y'.")]
    [SerializeField] private List<string> continuousDataHeaders;

    // --- Estructuras Internas ---
    private StringBuilder csvBuilder;
    private List<string> finalHeader;
    private Dictionary<string, int> columnMap; // Mapa para un acceso ultra rápido a las columnas
    private bool isInitialized = false;

    private void Awake()
    {
        // --- Gestión del Singleton ---
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Asegura que no se destruya al cambiar de escena

        Initialize();
    }

    /// <summary>
    /// Construye dinámicamente la cabecera del CSV y prepara el logger para una nueva sesión.
    /// </summary>
    public void Initialize()
    {
        csvBuilder = new StringBuilder();
        finalHeader = new List<string>();
        columnMap = new Dictionary<string, int>();

        // 1. Añade la columna estándar de Timestamp
        finalHeader.Add("Timestamp");

        // 2. Añade las columnas personalizadas para eventos
        finalHeader.AddRange(eventHeaders);

        // 3. Añade las columnas personalizadas para datos continuos
        finalHeader.AddRange(continuousDataHeaders);

        // 4. Crea el mapa de columnas para un acceso eficiente
        for (int i = 0; i < finalHeader.Count; i++)
        {
            columnMap[finalHeader[i]] = i;
        }

        // 5. Escribe la cabecera en el búfer
        csvBuilder.AppendLine(string.Join(",", finalHeader));
        isInitialized = true;
        Debug.Log("GenericDataLogger inicializado. Cabecera: " + string.Join(",", finalHeader));
    }

    /// <summary>
    /// Registra un evento discreto en su propia columna dedicada.
    /// </summary>
    /// <param name="eventName">El nombre de la columna del evento (debe estar en eventHeaders).</param>
    /// <param name="eventData">El dato a registrar en esa celda.</param>
    public void LogEvent(string eventName, object eventData)
    {
        if (!isInitialized) return;

        if (!columnMap.ContainsKey(eventName))
        {
            Debug.LogWarning($"El nombre de evento '{eventName}' no fue definido en eventHeaders del DataLogger. Se omitirá.");
            return;
        }

        string[] row = new string[finalHeader.Count];
        // Usamos Time.timeSinceLevelLoadAsDouble para tener un tiempo preciso que empieza en 0 con la escena.
        row[columnMap["Timestamp"]] = Time.timeSinceLevelLoadAsDouble.ToString("F6");

        // Coloca el dato en la columna del evento correspondiente
        int eventColumnIndex = columnMap[eventName];
        row[eventColumnIndex] = eventData.ToString();

        csvBuilder.AppendLine(string.Join(",", row));
    }

    /// <summary>
    /// Registra una muestra de datos continuos usando un diccionario.
    /// </summary>
    /// <param name="data">Un diccionario donde la clave es el nombre de la columna y el valor es el dato.</param>
    public void LogContinuousData(Dictionary<string, object> data)
    {
        if (!isInitialized) return;

        string[] row = new string[finalHeader.Count];
        row[columnMap["Timestamp"]] = Time.timeSinceLevelLoadAsDouble.ToString("F6");

        foreach (var entry in data)
        {
            if (columnMap.ContainsKey(entry.Key))
            {
                int columnIndex = columnMap[entry.Key];
                // Formateo para diferentes tipos de datos
                if (entry.Value is float f) row[columnIndex] = f.ToString("F6");
                else if (entry.Value is double d) row[columnIndex] = d.ToString("F6");
                else if (entry.Value is Vector3 v) row[columnIndex] = v.ToString("F6").Replace(",", "."); // Reemplaza comas por puntos
                else row[columnIndex] = entry.Value.ToString();
            }
        }
        csvBuilder.AppendLine(string.Join(",", row));
    }

    public void SetFilePath()
    {
        if (!isInitialized) return;

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{ParadigmName}_{SubjectID}_{Session}_{timestamp}.csv";

        // Application.persistentDataPath es una ubicación segura en cualquier sistema operativo
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        PlayerPrefs.SetString("Name", filePath);
    }


    /// <summary>
    /// Guarda todos los datos acumulados en un archivo CSV.
    /// </summary>
    public void SaveDataToFile()
    {
        if (!isInitialized) return;

        // Genera un nombre de archivo único con ID del sujeto y fecha/hora
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{ParadigmName}_{SubjectID}_{Session}_{timestamp}.csv";

        // Application.persistentDataPath es una ubicación segura en cualquier sistema operativo
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllText(filePath, csvBuilder.ToString());
            Debug.Log($"<color=lime>Datos guardados exitosamente en: {filePath}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar el archivo de datos: {e.Message}");
        }

        // Reinicia el logger para una posible nueva sesión sin tener que reiniciar la aplicación
        Initialize();
    }

    // Asegurarse de guardar los datos si la aplicación se cierra inesperadamente
    private void OnApplicationQuit()
    {
        SaveDataToFile();
    }
}



//Llamadas desde Otros Scripts:

//Para eventos: GenericDataLogger.Instance.LogEvent("Score", 100);

//Para datos continuos: GenericDataLogger.Instance.LogContinuousData(miDiccionarioDeDatos);
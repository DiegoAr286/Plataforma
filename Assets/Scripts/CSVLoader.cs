using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class CSVLoader : MonoBehaviour
{
    public static CSVLoader Instance { get; private set; }

    public List<ElementData> elementSequence { get; private set; } = new List<ElementData>();

    public string session { get; set; } = string.Empty;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool LoadSequence(string fileName)
    {
        elementSequence.Clear();

        TextAsset asset = Resources.Load<TextAsset>(fileName);
        if (asset == null)
        {
            Debug.LogError($"Error: El archivo CSV '{fileName}.csv' no se encuentra en Resources.");
            return false;
        }

        string[] lines = asset.text.Split('\n').Skip(1).ToArray();

        CultureInfo culture = CultureInfo.InvariantCulture;

        foreach (string line in lines)
        {
            string cleanLine = line.Trim();
            if (string.IsNullOrEmpty(cleanLine)) continue;

            string[] values = cleanLine.Split(',');

            // 9 columnas (Index, Type, Side, PosX, PosY, PosZ, WaitTime, Angle, Force)
            if (values.Length < 9) continue;

            ElementData data = new ElementData();

            // Type (Index 1) y Side (Index 2)
            data.Type = int.Parse(values[1]);
            data.Side = int.Parse(values[2]);

            // Position X, Y, Z (Index 3, 4, 5)
            data.Position = new Vector3(
                float.Parse(values[3], culture),
                float.Parse(values[4], culture),
                float.Parse(values[5], culture)
            );

            // WaitTime (Index 6), Angle (Index 7), Force (Index 8)
            data.WaitTime = float.Parse(values[6], culture);
            data.Angle = float.Parse(values[7], culture);
            data.Force = float.Parse(values[8], culture);

            elementSequence.Add(data);
        }

        Debug.Log($"Secuencia '{fileName}' cargada con {elementSequence.Count} elementos.");
        return elementSequence.Count > 0;
    }
}
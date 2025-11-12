using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Genera y visualiza puntos dispuestos uniformemente sobre una circunferencia.
/// </summary>
public class CirclePointGenerator : MonoBehaviour
{
    [Header("Configuración del Círculo")]
    [Tooltip("El radio de la circunferencia sobre la que se generarán los puntos.")]
    [SerializeField] private float radius = .06f;

    [Tooltip("El número de puntos a generar. Para saltos de 45°, serían 8 puntos.")]
    [SerializeField] private int numberOfPoints = 8;

    [Tooltip("El ángulo inicial en grados desde donde se empieza a generar.")]
    [SerializeField] private float startAngle = 0f;

    [Tooltip("Plano donde se generarán")]
    [SerializeField] private bool verticalPlane = true;

    [Header("Visualización")]
    [Tooltip("Arrastra aquí un Prefab (ej. una esfera pequeña) para visualizar los puntos.")]
    [SerializeField] private GameObject pointPrefab;

    // Lista para guardar las posiciones generadas
    private List<Vector3> generatedPoints = new List<Vector3>();

    void Start()
    {
        GeneratePoints();

        if (pointPrefab != null)
        {
            VisualizePoints();
        }
    }

    /// <summary>
    /// Calcula las posiciones de los puntos en la circunferencia.
    /// </summary>
    private void GeneratePoints()
    {
        generatedPoints.Clear();

        verticalPlane = PlayerPrefs.GetInt("VerticalPlane", 1) == 1;

        float angleStep = 360f / numberOfPoints;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float currentAngle = startAngle + (i * angleStep);

            float angleInRad = currentAngle * Mathf.Deg2Rad;
            
            float x, y, z;

            if (verticalPlane)
            {
                x = radius * Mathf.Cos(angleInRad);
                y = radius * Mathf.Sin(angleInRad);
                z = transform.position.z;
            }
            else
            {
                x = radius * Mathf.Cos(angleInRad);
                y = transform.position.y;
                z = radius * Mathf.Sin(angleInRad);
            }

            Vector3 pointPosition = new Vector3(x, y, z) + transform.position;
            generatedPoints.Add(pointPosition);
        }
    }

    /// <summary>
    /// Instancia un prefab en cada una de las posiciones generadas.
    /// </summary>
    private void VisualizePoints()
    {
        foreach (Vector3 pos in generatedPoints)
        {
            GameObject point = Instantiate(pointPrefab, pos, Quaternion.identity);
            point.transform.SetParent(this.transform);
        }
    }
}
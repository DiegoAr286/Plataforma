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

        // Calculamos cuánto ángulo hay entre cada punto
        float angleStep = 360f / numberOfPoints;

        for (int i = 0; i < numberOfPoints; i++)
        {
            // Calculamos el ángulo para el punto actual
            float currentAngle = startAngle + (i * angleStep);

            // Convertimos el ángulo a radianes para las funciones trigonométricas
            float angleInRad = currentAngle * Mathf.Deg2Rad;

            // Calculamos las coordenadas X y Z (para un círculo en el plano horizontal)
            float x = radius * Mathf.Cos(angleInRad);
            float y = radius * Mathf.Sin(angleInRad);
            float z = transform.position.z;

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
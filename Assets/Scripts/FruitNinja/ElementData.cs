using UnityEngine;

// Estructura que mapea directamente las columnas del archivo CSV.
public struct ElementData
{
    // Datos del CSV
    public int Type;        // 0-4 (Fruta) o 5 (Bomba)
    public int Side;        // 0 (Izquierda) o 1 (Derecha)
    public Vector3 Position; // PosX, PosY, PosZ
    public float WaitTime;  // Tiempo de espera (T_Espera)
    public float Angle;     // Ángulo de lanzamiento
    public float Force;     // Fuerza de lanzamiento (float)
}
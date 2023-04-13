using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigMenuColorTasks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Desactivado por defecto
        PlayerPrefs.SetInt("Training", 0);
    }

    public void SetTraining(bool train)
    {
        PlayerPrefs.SetInt("Training", train ? 1 : 0);
    }
}

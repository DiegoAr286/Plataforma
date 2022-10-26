using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public int paradigmID = 0;
    //public int paradigmOptionsID = 0; 

    public GameObject ConfigMenuP1_Obj;
    public GameObject ConfigMenuP2_Obj;

    public GameObject FilenameMenu;

    private void Awake()
    {
        Screen.SetResolution(1366, 768, true);
        if (Display.displays.Length == 1)
            return;

        if (!Display.displays[1].active)
            Display.displays[1].Activate();

        //GetComponentInParent<Canvas>().targetDisplay = 1;
    }
    public void Start()
    {
        //Screen.fullScreen = true;
        //if (!Display.displays[1].active)

        //if (SceneManager.GetActiveScene().name == "NHPT_Scene 2")
        //    SceneManager.UnloadSceneAsync(1);   //Dejo unicamente cargado la escena 0, que es el menu. Si la escena de un paradigma está cargada, al poner comenzar, no redirige al paradigma a veces.
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneManager.GetSceneByBuildIndex(i).isLoaded)
            {
                SceneManager.UnloadSceneAsync(i);
            }
        }

        //Apago todos los objetos para que no se muestre el menú de Config al iniciar
        ConfigMenuP1_Obj.SetActive(false);
        ConfigMenuP2_Obj.SetActive(false);

        FilenameMenu.SetActive(false);
    }


    public void PlayParadigm()
    {
        if (paradigmID == 0)
            Debug.Log("paradigmID= " + paradigmID + ". Cargue un Paradigma");
        if (paradigmID == 1)
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        if (paradigmID == 2)
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 2);
        if (paradigmID == 3)
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 3);
        if (paradigmID == 4)
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 4);
        if (paradigmID == 5)
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 5);
        if (paradigmID == 6)
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 6);
    }

    public void QuitSoftware()
    {
        Debug.Log("Quit Software ...");
        Application.Quit();
    }

    public void SetParadigm(int ParadigmIndex)
    {
        switch (ParadigmIndex)
        {
            case 0:
                paradigmID = 0;
                ConfigMenuP1_Obj.SetActive(false);
                ConfigMenuP2_Obj.SetActive(false);
                FilenameMenu.SetActive(false);
                break;

            case 1: // 1: 9-Hole Peg Test (cue based)
                paradigmID = 1;
                ConfigMenuP1_Obj.SetActive(true);
                ConfigMenuP2_Obj.SetActive(false);
                break;

            case 2: // 2: 9-Hole Peg Test (self paced)
                paradigmID = 2;
                ConfigMenuP1_Obj.SetActive(false);
                ConfigMenuP2_Obj.SetActive(true);
                break;

            case 3: // 3: N-Back Task
                paradigmID = 3;
                ConfigMenuP1_Obj.SetActive(false);
                ConfigMenuP2_Obj.SetActive(false);
                break;

            case 4: // 4: Stop Signal Task
                paradigmID = 4;
                ConfigMenuP1_Obj.SetActive(false);
                ConfigMenuP2_Obj.SetActive(false);
                break;

            case 5: // 5: Color Change Detection Task
                paradigmID = 5;
                ConfigMenuP1_Obj.SetActive(false);
                ConfigMenuP2_Obj.SetActive(false);
                break;

            case 6: // 6: Color Comparison Task
                paradigmID = 6;
                ConfigMenuP1_Obj.SetActive(false);
                ConfigMenuP2_Obj.SetActive(false);
                break;
        }

        if (ParadigmIndex != 0)
            FilenameMenu.SetActive(true);
    }

}

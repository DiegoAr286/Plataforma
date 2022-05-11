using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public int paradigmID = 0;  // 1: 9-Hole Peg Test
    //public int paradigmOptionsID = 0; 

    public GameObject ConfigMenuP1_Obj;
    public GameObject ConfigMenuP2_Obj;

    public void Start()
    {
        //if (SceneManager.GetActiveScene().name == "NHPT_Scene 2")
        //    SceneManager.UnloadSceneAsync(1);   //Dejo unicamente cargado la escena 0, que es el menu. Si la escena de un paradigma est� cargada, al poner comenzar, no redirige al paradigma a veces.
        for (int i = 1; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).isLoaded)
            {
                SceneManager.UnloadSceneAsync(i);
            }
        }

        Screen.fullScreen = true;

        //Apago todos los objetos para que no se muestre el menu de Config al iniciar
        ConfigMenuP1_Obj.SetActive(false);
        ConfigMenuP2_Obj.SetActive(false);
        //ConfigMenuP3_Obj.SetActive(false)
    }


    public void PlayParadigm()
    {
        if (paradigmID == 0)
            Debug.Log("paradigmID= " + paradigmID + ". Cargue un Paradigma");
        if (paradigmID == 1)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        if (paradigmID == 2)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
        if (paradigmID == 3)
            Debug.Log("paradigmID= " + paradigmID);
    }

    public void QuitSoftware()
    {
        Debug.Log("Quit Software ...");
        Application.Quit();
    }

    public void SetParadigm(int ParadigmIndex)
    {
        if (ParadigmIndex == 0)
        {
            paradigmID = 0;
            ConfigMenuP1_Obj.SetActive(false);
            ConfigMenuP2_Obj.SetActive(false);
            //ConfigMenuP3_Obj.SetActive(false);
        }

        if (ParadigmIndex == 1) // 1: 9-Hole Peg Test (cue based)
        {
            paradigmID = 1; 
            ConfigMenuP1_Obj.SetActive(true);
            ConfigMenuP2_Obj.SetActive(false);
            //ConfigMenuP3_Obj.SetActive(false);
        }

        if (ParadigmIndex == 2) // 2: 9-Hole Peg Test (self paced)
        {
            paradigmID = 2;
            ConfigMenuP1_Obj.SetActive(false);
            ConfigMenuP2_Obj.SetActive(true);
            //ConfigMenuP3_Obj.SetActive(false)
        }

        if (ParadigmIndex == 3)
        {
            paradigmID = 3;
            ConfigMenuP1_Obj.SetActive(false);
            ConfigMenuP2_Obj.SetActive(false);
            //ConfigMenuP3_Obj.SetActive(true)
        }

    }

}
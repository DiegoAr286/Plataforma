using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using SimpleFileBrowser;

public class MainMenu : MonoBehaviour
{
    public int paradigmID = 0;

    public GameObject[] ConfigMenu = { };

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
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneManager.GetSceneByBuildIndex(i).isLoaded)
            {
                SceneManager.UnloadSceneAsync(i);
            }
        }

        //Apago todos los objetos para que no se muestre el menú de Config al iniciar
        for (int i = 0; i < ConfigMenu.Length; i++)
            ConfigMenu[i].SetActive(false);
        //ConfigMenuP2_Obj.SetActive(false);
        //ConfigMenuP3_Obj.SetActive(false);
    }


    public void PlayParadigm()
    {
        if (paradigmID != 0)
            StartCoroutine(ShowSaveDialogCoroutine());
    }

    private void StartParadigm()
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
                for (int i = 0; i < ConfigMenu.Length; i++)
                    ConfigMenu[i].SetActive(false);
                break;

            case 1: // 1: 9-Hole Peg Test (cue based)
                paradigmID = 1;
                for (int i = 0; i < ConfigMenu.Length; i++)
                    ConfigMenu[i].SetActive(false);
                ConfigMenu[0].SetActive(true);
                break;

            case 2: // 2: 9-Hole Peg Test (self paced)
                paradigmID = 2;
                for (int i = 0; i < ConfigMenu.Length; i++)
                    ConfigMenu[i].SetActive(false);
                ConfigMenu[0].SetActive(true);
                break;

            case 3: // 3: N-Back Task
                paradigmID = 3;
                for (int i = 0; i < ConfigMenu.Length; i++)
                    ConfigMenu[i].SetActive(false);
                break;

            case 4: // 4: Stop Signal Task
                paradigmID = 4;
                for (int i = 0; i < ConfigMenu.Length; i++)
                    ConfigMenu[i].SetActive(false);
                break;

            case 5: // 5: Color Change Detection Task
                paradigmID = 5;
                for (int i = 0; i < ConfigMenu.Length; i++)
                    ConfigMenu[i].SetActive(false);
                ConfigMenu[1].SetActive(true);
                break;

            case 6: // 6: Color Comparison Task
                paradigmID = 6;
                for (int i = 0; i < ConfigMenu.Length; i++)
                    ConfigMenu[i].SetActive(false);
                ConfigMenu[1].SetActive(true);
                break;
        }
    }

    IEnumerator ShowSaveDialogCoroutine()
    {
        bool training = PlayerPrefs.GetInt("Training", 0) == 1;
        if (((paradigmID == 5 || paradigmID == 6) && !training) || paradigmID != 5 && paradigmID != 6)
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".txt"));

            yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.FilesAndFolders, false, null, "Archivo.txt", "Guardar archivo", "Guardar");

            if (FileBrowser.Success)
                PlayerPrefs.SetString("Name", FileBrowser.Result[0]);
        }
        StartParadigm();
    }
}
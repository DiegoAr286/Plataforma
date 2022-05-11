using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ConfigMenu : MonoBehaviour
{
    //public GameObject[] uiObjects;

    public GameObject mainMenuObj;  // 1: 9-Hole Peg Test
    //public int paradigmOptionsID = 0; 

    //mainMenuObj.paradigmID

    //ToggleGroup toggleGroupHand;

    //void Start()
    //{
    //    toggleGroupHand = GetComponent<toggleGroupHand>();
    //}

    void Start()
    {
        PlayerPrefs.SetInt("CameraRec", 1);

        PlayerPrefs.SetInt("AnalogAcquisition", 1);


        PlayerPrefs.SetInt("RightHand_NHPTcb", 0);
        PlayerPrefs.SetInt("LeftHand_NHPTcb", 1);


        PlayerPrefs.SetInt("RightHand_NHPTsp", 0);
        PlayerPrefs.SetInt("LeftHand_NHPTsp", 1);
    }


    public void SetCameraRec(bool isCamRec)
    {
        Debug.Log(isCamRec);
        PlayerPrefs.SetInt("CameraRec", isCamRec ? 1 : 0);
    }

    public void SetAnalogAcquisition(bool isAnalogAcquisition)
    {
        Debug.Log(isAnalogAcquisition);
        PlayerPrefs.SetInt("AnalogAcquisition", isAnalogAcquisition ? 1 : 0);
    }

    public void SetHandRight_NHPTcb(bool isRightHandcb)
    {
        //toggle toggleHand = toggleGroupHand.ActiveToggles().FirstOrDefault();


        //Debug.Log("isRightHand: " + isRightHand);
        PlayerPrefs.SetInt("RightHand_NHPTcb", isRightHandcb ? 1 : 0);
        PlayerPrefs.SetInt("LeftHand_NHPTcb", isRightHandcb ? 0 : 1);
        Debug.Log("RightHand_NHPTcb:" + PlayerPrefs.GetInt("RightHand_NHPTcb") + "-LeftHand_NHPTcb: " + PlayerPrefs.GetInt("LeftHand_NHPTcb"));
    }

    public void SetHandLeft_NHPTcb(bool isLeftHandcb)
    {
        //toggle toggleHand = toggleGroupHand.ActiveToggles().FirstOrDefault();


        //Debug.Log("isLeftHand: " + isLeftHand);
        PlayerPrefs.SetInt("LeftHand_NHPTcb", isLeftHandcb ? 1 : 0);
        PlayerPrefs.SetInt("RightHand_NHPTcb", isLeftHandcb ? 0 : 1);
        Debug.Log("RightHand_NHPTcb:" + PlayerPrefs.GetInt("RightHand_NHPTcb") + "-LeftHand_NHPTcb: " + PlayerPrefs.GetInt("LeftHand_NHPTcb"));
    }

    public void SetHandRight_NHPTsp(bool isRightHandsp)
    {
        //toggle toggleHand = toggleGroupHand.ActiveToggles().FirstOrDefault();


        //Debug.Log("isRightHand: " + isRightHand);
        PlayerPrefs.SetInt("RightHand_NHPTsp", isRightHandsp ? 1 : 0);
        PlayerPrefs.SetInt("LeftHand_NHPTsp", isRightHandsp ? 0 : 1);
        Debug.Log("RightHand_NHPTsp:" + PlayerPrefs.GetInt("RightHand_NHPTsp") + "-LeftHand_NHPTsp: " + PlayerPrefs.GetInt("LeftHand_NHPTsp"));
    }

    public void SetHandLeft_NHPTsp(bool isLeftHandsp)
    {
        //toggle toggleHand = toggleGroupHand.ActiveToggles().FirstOrDefault();


        //Debug.Log("isLeftHand: " + isLeftHand);
        PlayerPrefs.SetInt("LeftHand_NHPTsp", isLeftHandsp ? 1 : 0);
        PlayerPrefs.SetInt("RightHand_NHPTsp", isLeftHandsp ? 0 : 1);
        Debug.Log("RightHand_NHPTsp:" + PlayerPrefs.GetInt("RightHand_NHPTsp") + "-LeftHand_NHPTsp: " + PlayerPrefs.GetInt("LeftHand_NHPTsp"));
    }
}

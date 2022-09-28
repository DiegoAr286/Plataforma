using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ConfigMenu : MonoBehaviour
{
    //public GameObject mainMenuObj;

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
        PlayerPrefs.SetInt("CameraRec", isCamRec ? 1 : 0);
    }

    public void SetAnalogAcquisition(bool isAnalogAcquisition)
    {
        PlayerPrefs.SetInt("AnalogAcquisition", isAnalogAcquisition ? 1 : 0);
    }

    public void SetHandRight_NHPTcb(bool isRightHandcb)
    {
        PlayerPrefs.SetInt("RightHand_NHPTcb", isRightHandcb ? 1 : 0);
        PlayerPrefs.SetInt("LeftHand_NHPTcb", isRightHandcb ? 0 : 1);
    }

    public void SetHandLeft_NHPTcb(bool isLeftHandcb)
    {
        PlayerPrefs.SetInt("LeftHand_NHPTcb", isLeftHandcb ? 1 : 0);
        PlayerPrefs.SetInt("RightHand_NHPTcb", isLeftHandcb ? 0 : 1);
    }

    public void SetHandRight_NHPTsp(bool isRightHandsp)
    {
        PlayerPrefs.SetInt("RightHand_NHPTsp", isRightHandsp ? 1 : 0);
        PlayerPrefs.SetInt("LeftHand_NHPTsp", isRightHandsp ? 0 : 1);
    }

    public void SetHandLeft_NHPTsp(bool isLeftHandsp)
    {
        PlayerPrefs.SetInt("LeftHand_NHPTsp", isLeftHandsp ? 1 : 0);
        PlayerPrefs.SetInt("RightHand_NHPTsp", isLeftHandsp ? 0 : 1);
    }
}

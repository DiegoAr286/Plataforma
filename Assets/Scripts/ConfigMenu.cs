using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ConfigMenu : MonoBehaviour
{
    public GameObject inputFieldCOM;
    public GameObject textCOM;


    void Start()
    {
        PlayerPrefs.SetInt("CameraRec", 1);

        PlayerPrefs.SetInt("AnalogAcquisition", 1);

        PlayerPrefs.SetInt("SerialConnection", 1);

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

    public void SetSerialConnection(bool isSerialConnection)
    {
        PlayerPrefs.SetInt("SerialConnection", isSerialConnection ? 1 : 0);

        if (isSerialConnection)
        {
            inputFieldCOM.SetActive(true);
            textCOM.SetActive(true);

            PlayerPrefs.SetString("COMNumber", "COM8");
            Debug.Log(PlayerPrefs.GetString("COMNumber"));
        }
        else
        {
            inputFieldCOM.SetActive(false);
            textCOM.SetActive(false);
        }
    }

    public void SetCOMNumber()
    {
        PlayerPrefs.SetString("COMNumber", inputFieldCOM.GetComponent<TMP_InputField>().text);
    }

    public void SetHandRight_NHPT(bool isRightHand)
    {
        PlayerPrefs.SetInt("RightHand_NHPT", isRightHand ? 1 : 0);
    }

    public void SetHandLeft_NHPT(bool isLeftHand)
    {
        PlayerPrefs.SetInt("RightHand_NHPT", isLeftHand ? 0 : 1);
    }
}

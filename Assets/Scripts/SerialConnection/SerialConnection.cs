using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class SerialConnection : MonoBehaviour
{
    SerialPort stream;
    string value;
    bool connected = false;

    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
            //StartCoroutine(GetSerialData());
        }
    }

    public void SerialStartConnection()
    {
        stream = new SerialPort(PlayerPrefs.GetString("COMNumber"), 9600);
        stream.Open();
        connected = true;

        StartCoroutine(GetSerialData());
    }

    public void SerialCloseConnection()
    {
        stream.Close();
        connected = false;
    }

    IEnumerator GetSerialData()
    {
        while (connected)
        {
            value = stream.ReadLine(); //Read the information
            //Debug.Log(value);
            yield return new WaitForSeconds(.02f);
        }
    }

    public string GetAngle()
    {
        return value;
    }

}

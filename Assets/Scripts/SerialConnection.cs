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
            value = stream.ReadLine(); //Read the information
        }
    }

    public void SerialStartConnection()
    {
        Debug.Log(PlayerPrefs.GetString("COMNumber"));
        stream = new SerialPort(PlayerPrefs.GetString("COMNumber"), 9600);
        stream.Open();
        connected = true;
    }

    public void SerialCloseConnection()
    {
        stream.Close();
        connected = false;
    }

    public string GetAngle()
    {
        return value;
    }

}

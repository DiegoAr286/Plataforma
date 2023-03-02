using Janelia;
using OpenLayers.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaqConnection : MonoBehaviour
{
    private bool isAnalogAcquisition; // Opción adquisición

    // NI link
    NiDaqMx.DigitalOutputParams[] digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int lines = 7; // Líneas digitales a escribir

    NiDaqMx.DigitalOutputParams digitalOutputParamPort2; // Parámetros NI
    private bool writeStatePort2 = false;


    // DT Link
    DeviceMgr devicemgr;
    Device device;
    DigitalOutputSubsystem digitalOutputSubsystem;
    int i = 0;

    int allUp = 255;
    int message; // Todos los pines en alto



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DtInitialize()
    {
        devicemgr = DeviceMgr.Get();
        string[] names = devicemgr.GetDeviceNames();
        Debug.Log(names[0]);

        device = devicemgr.GetDevice(names[0]);
        digitalOutputSubsystem = device.DigitalOutputSubsystem(0);
        digitalOutputSubsystem.DataFlow = DataFlow.SingleValue;
        digitalOutputSubsystem.Config();

        message = allUp;
        digitalOutputSubsystem.SetSingleValue(message);
    }

    private void DtDispose()
    {
        digitalOutputSubsystem.Dispose();
        device.Dispose();
    }
}

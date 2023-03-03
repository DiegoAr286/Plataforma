using Janelia;
using OpenLayers.Base;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;

public class DaqConnection : MonoBehaviour
{
    private bool isAnalogAcquisition; // Opción adquisición
    private bool niDaq = true;

    // NI link
    private NiDaqMx.DigitalOutputParams[] digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int lines = 7; // Líneas digitales a escribir

    private NiDaqMx.DigitalOutputParams digitalOutputParamsPort2; // Parámetros NI
    private bool writeStatePort2 = false;



    // DT Link
    private DeviceMgr devicemgr;
    private Device device;
    private DigitalOutputSubsystem digitalOutputSubsystem;

    private int allUp = 255;
    private int dtMessage; // Todos los pines en alto
    private int dtStoredMessage;


    // Start is called before the first frame update
    void Start()
    {
        isAnalogAcquisition = PlayerPrefs.GetInt("AnalogAcquisition", 1) == 1; // if true

        if (isAnalogAcquisition)
        {
            if (NiDaqMx.CheckInit())
            {
                niDaq = true;
                NiPort0OutputInitialize();
                Debug.Log("NIDAQ");
            }
            else if (DtCheckInit())
            {
                niDaq = false;
                DtOutputInitialize();
                Debug.Log("DT");
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    DtWriteDigitalValue(new uint[] { 0, 1, 1, 1, 1, 1, 1, 1 }, 0);
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    DtWriteDigitalValue(new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 }, 0);
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    DtWriteDigitalValue(new uint[] { 1, 1, 0, 1, 1, 1, 1, 1 }, 0);
        //}

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    dtMessage = allUp;
        //}
    }

    public void EndConnection()
    {
        if (isAnalogAcquisition)
        {
            if (niDaq)
            {
                if (writeState)
                    NiPort0Clear();
                if (writeStatePort2)
                    NiPort2Clear();
            }
            else
                DtDispose();
        }
    }

    private void NiPort0OutputInitialize()
    {
        // Inicializa variables NI
        digitalOutputParams = new NiDaqMx.DigitalOutputParams[8];
        for (int i = 0; i < 8; i++)
        {
            digitalOutputParams[i] = new NiDaqMx.DigitalOutputParams();
            _ = NiDaqMx.CreateDigitalOutput(digitalOutputParams[i], false, i);
        }

        uint[] message = { 1, 1, 1, 1, 1, 1, 1, 1 };
        for (int i = 0; i < lines; i++)
            writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams[i], new uint[] { message[i] }, ref numWritten);
    }

    private void NiPort0Clear()
    {
        for (int i = 0; i < 8; i++)
            NiDaqMx.ClearOutputTask(digitalOutputParams[i]);
    }

    public void NiPort2OutputInitialize()
    {
        digitalOutputParamsPort2 = new NiDaqMx.DigitalOutputParams();
        writeStatePort2 = NiDaqMx.CreateDigitalOutput(digitalOutputParamsPort2, port: 2);

        //writeStatePort2 = NiDaqMx.WriteDigitalValue(digitalOutputParamsPort2, new uint[] { 0 }, ref numWritten);
    }

    private void NiPort2Clear()
    {
        NiDaqMx.ClearOutputTask(digitalOutputParamsPort2);
    }

    private bool DtCheckInit()
    {
        devicemgr = DeviceMgr.Get();
        string[] names = devicemgr.GetDeviceNames();
        
        if (names.Length > 0)
            return true;
        else
            return false;
    }

    private void DtOutputInitialize()
    {
        string[] names = devicemgr.GetDeviceNames();
        Debug.Log(names[0]);

        device = devicemgr.GetDevice(names[0]);
        digitalOutputSubsystem = device.DigitalOutputSubsystem(0);
        digitalOutputSubsystem.DataFlow = DataFlow.SingleValue;
        digitalOutputSubsystem.Config();

        dtMessage = allUp;
        digitalOutputSubsystem.SetSingleValue(dtMessage);
    }

    private void DtDispose()
    {
        digitalOutputSubsystem.Dispose();
        device.Dispose();
    }

    private int BinaryToInt(uint[] message)
    {
        int result = 0;

        result += (int)message[0]; // El primer elemento corresponde a 2^0

        if (message.Length > 1)
        {
            for (int i = 1; i < message.Length; i++)
            {
                if (message[i] == 1)
                    result += (int)Mathf.Pow(2, i);
            }
        }
        
        Debug.Log("Resultado final");
        Debug.Log(allUp - result);

        return allUp - result;
    }

    public bool WriteDigitalValue(uint[] message, int port = 0)
    {
        if (niDaq)
        {
            return(NiWriteDigitalValue(message, port));
        }
        else
        {
            return(DtWriteDigitalValue(message, port));
        }
    }

    private bool NiWriteDigitalValue(uint[] message, int port)
    {
        bool writeResult = false;

        if (port == 0)
        {
            for (int i = 0; i < lines; i++)
                writeResult = NiDaqMx.WriteDigitalValue(digitalOutputParams[i], message, ref numWritten);
        }

        else
            writeResult = NiDaqMx.WriteDigitalValue(digitalOutputParamsPort2, message, ref numWritten);

        return writeResult;
    }

    private bool DtWriteDigitalValue(uint[] message, int port)
    {
        int intMessage = BinaryToInt(message);

        if (port == 0)
        {
            if (intMessage == 0)
                dtMessage += dtStoredMessage;
            else
            {
                dtMessage -= intMessage;
                dtStoredMessage = intMessage;
            }

            digitalOutputSubsystem.SetSingleValue(dtMessage);
        }

        else
        {
            if (intMessage == allUp)
            {
                dtMessage -= 128;
                digitalOutputSubsystem.SetSingleValue(dtMessage); // Se baja el pin 7
            }
            else if (intMessage == allUp - 1)
            {
                dtMessage += 128;
                digitalOutputSubsystem.SetSingleValue(dtMessage); // Se sube el pin 7
            }
        }
        Debug.Log("Resultado escrito");
        Debug.Log(dtMessage);

        return true;
    }
}

using Janelia;
using OpenLayers.Base;
using UnityEngine;

public class DaqConnection : MonoBehaviour
{
    private bool isAnalogAcquisition; // Opción adquisición
    private bool niDaq = true;

    // NI link
    private NiDaqMx.DigitalOutputParams[] digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int lines = 8; // Líneas digitales a escribir

    private NiDaqMx.DigitalOutputParams digitalOutputParamsPort2; // Parámetros NI
    private bool writeStatePort2 = false;

    private uint[] allPinsUp = { 1, 1, 1, 1, 1, 1, 1, 1 };
    private uint[] niMessage;


    // DT Link
    private DeviceMgr devicemgr;
    private Device device;
    private DigitalOutputSubsystem digitalOutputSubsystem;

    private int allUp = 255;
    private int dtMessage; // Todos los pines en alto


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

        niMessage = allPinsUp;

        for (int i = 0; i < lines; i++)
            writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams[i], niMessage, ref numWritten);
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

    public bool WriteDigitalValue(uint[] message, bool endPulse, int port = 0)
    {
        if (niDaq)
        {
            return (NiWriteDigitalValue(message, endPulse, port));
        }
        else
        {
            return (DtWriteDigitalValue(message, endPulse, port));
        }
    }

    private bool NiWriteDigitalValue(uint[] message, bool endPulse, int port)
    {
        bool writeResult = false;

        InvertMessage(ref message);

        uint sum;

        if (endPulse == true)
        {
            for (int i = 0; i < message.Length; i++)
            {
                sum = niMessage[i] + message[i];
                if (sum <= 1)
                    niMessage[i] = sum;
            }
        }
        else
        {
            for (int i = 0; i < message.Length; i++)
            {
                sum = niMessage[i] - message[i];
                if (sum >= 0)
                    niMessage[i] = sum;
            }
        }
        for (int i = 0; i < lines; i++)
            writeResult = NiDaqMx.WriteDigitalValue(digitalOutputParams[i], new uint[] { niMessage[i] }, ref numWritten);

        return writeResult;
    }

    private void InvertMessage(ref uint[] message)
    {
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == 0)
                message[i] = 1;
            else
                message[i] = 0;
        }
    }

    private bool DtWriteDigitalValue(uint[] message, bool endPulse, int port)
    {
        InvertMessage(ref message);

        int intMessage = BinaryToInt(message);

        if (endPulse == true)
            dtMessage += intMessage;
        else
            dtMessage -= intMessage;

        digitalOutputSubsystem.SetSingleValue(dtMessage);

        return true;
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

        return result;
    }
}

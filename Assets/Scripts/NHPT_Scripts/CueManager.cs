using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Text.RegularExpressions;


public class CueManager : MonoBehaviour
{
    [Header("Configuraci�n escena")]
    public GameObject paradigmScene;

    public HapticGrabberHand hapticGrabberHand;
    private bool rightHand = true;
    private bool mirrored = false;

    [Header("Grabaci�n de c�mara")]
    public VideoRecord videoRecorder;

    [Header("Conexi�n DAQ")]
    public DaqConnection daqConnector;

    [Header("Conexi�n Serie")]
    public GameObject serialController;

    [Header("Almacenamiento de datos")]
    public FileManager fileManager;
    private bool writtenData = false;

    [Header("Configuraci�n Pegs")]
    public GameObject[] Pegs = { null, null, null, null }; // Array que contiene pegs
    private Vector3[] PegPosition; // Contiene las posiciones iniciales de los pegs
    private Quaternion[] PegRotation; // Contiene las rotaciones iniciales de los pegs

    [HideInInspector] public float currentTime = 0; // Tiempo que pas� desde que comienza el turno de mover un peg, en segundos
    //[HideInInspector] public float totalCurrentTime = 0; // Tiempo que pas� desde el comienzo de la prueba

    public int startSphereWaitTime = 8; // Tiempo que tarda en comenzar la tarea luego de tocar la esfera de inicio
    public int initTime = 1; // Tiempo que tarda en modificarse los colores del peg desde que termina el turno anterior
    public int goTime = 3; // Tiempo que tarda en pasar de rojo a verde un peg
    public int maxTime = 10; // Tiempo m�ximo que durar� un turno

    public int totalPegsEntered = 45;
    public bool modeReset = false;

    [Range(1, 9)] public int pegNumber = 1; // Representa el peg seleccionado para mover

    public Material originalMaterial; // Material original (blanco)
    public Material goMaterial; // Material verde
    public Material highlightMaterial; // Material que resaltar� al peg (rojo)

    [Header("Configuraci�n Holes")]
    public GameObject[] HoleBases = { null, null, null, null }; // Array de agujeros
    [Range(1, 9)] public int holeNumber = 1; // Representa el agujero seleccionado en el que se introducir� el peg

    public PegHoleCollisionEvent[] PHCollisionEvents = { null, null }; // Array que contiene los eventos de colisi�n de los agujeros

    [Header("Posici�n de inicio")]
    public StartSphereCollisionEvent startSphereCollisionEvent; // Evento de colisi�n con esfera de inicio

    private bool startSphereTouch = false;
    GameObject startSphere;

    private bool endTrial = false;

    private bool pegEntered = false; // Booleano que se vuelve true al insertar un peg, terminando el turno

    private List<int> pegOrder;
    private int pegIndex = 0;
    private List<int> holeOrder;

    private int numberPegsEntered = 0; // Variable que cuenta la cantidad de pegs ingresados
    private bool[] pegsEntered = {false, false, false,
                                false, false, false,
                                false, false, false }; // Array de bools que permite encontrar cu�les pegs ya tuvieron su turno
    private bool[] holesEntered = {false, false, false,
                                false, false, false,
                                false, false, false }; // Array de bools que permite encontrar cu�les agujeros ya tuvieron su turno
    private bool pegDeactivate = true;
    private bool pegActivated = false;

    private bool isCamRec; // Opci�n c�mara

    private bool cameraRecording = false;

    private bool isSerialConnection = false;

    private bool isAnalogAcquisition = true; // Opci�n adquisici�n

    private bool writeState = false;


    // Start is called before the first frame update
    void Start()
    {
        // Establece una conexi�n con el evento de toque de la esfera de inicio
        startSphereCollisionEvent.onTriggerEnter.AddListener(StartSphereEvent);

        startSphere = GameObject.Find("StartSphere"); // Busca el objeto esfera de inicio
        startSphere.SetActive(false);

        startSphereWaitTime = Random.Range(5, 9);

        // Opciones
        isCamRec = PlayerPrefs.GetInt("CameraRec", 1) == 1; // if true

        isAnalogAcquisition = PlayerPrefs.GetInt("AnalogAcquisition", 1) == 1; // if true

        isSerialConnection = PlayerPrefs.GetInt("SerialConnection", 1) == 1;

        rightHand = PlayerPrefs.GetInt("RightHand_NHPT", 1) == 1;

        // Eventos
        hapticGrabberHand.onPegGrab.AddListener(PegGrabEvent);
        hapticGrabberHand.onPegRelease.AddListener(PegReleaseEvent);

        // Guarda posiciones y rotaciones iniciales de los pegs
        PegPosition = new Vector3[Pegs.Length];
        PegRotation = new Quaternion[Pegs.Length];
        for (int ii = 0; ii < Pegs.Length; ii++)
        {
            PegPosition[ii] = Pegs[ii].transform.position;
            PegRotation[ii] = Pegs[ii].transform.rotation;
        }

        for (int i = 0; i < HoleBases.Length; i++)
        {
            HoleBases[i].SetActive(false); // Desactiva los pegs fantasma de los agujeros

            PHCollisionEvents[i].onTriggerEnter.AddListener(PegEnter); // Establece una conexi�n con los eventos de entrada de los pegs
        }

        pegOrder = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        holeOrder = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        RandomizeOrder();

        pegNumber = pegOrder[pegIndex];
        holeNumber = holeOrder[pegIndex];


        if (isCamRec || isAnalogAcquisition)
            StartCoroutine(AcquisitionStart());

        if (isSerialConnection)
        {
            serialController.SetActive(true);
        }

        MirrorScene();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RunTrigger(8, endPulse: true);

            daqConnector.EndConnection();

            SceneManager.LoadScene(0); // Salir del test al men� inicial
        }

        if (Input.GetKeyDown(KeyCode.S))
            MirrorScene();

        currentTime += Time.deltaTime; // Aumenta el tiempo en intervalos deltaTime, que es el tiempo entre frames



        if (numberPegsEntered < totalPegsEntered) // Verifica que el n�mero de turnos no sea mayor al total
        {
            if (pegDeactivate && currentTime >= startSphereWaitTime)
                startSphere.SetActive(true);

            if (Mathf.Abs(initTime - currentTime) < 0.05 && startSphereTouch) // Current time raramente toma un valor entero, entonces se define una diferencia m�xima
            {
                if (modeReset && numberPegsEntered != 0)
                    ResetPegs();
                if (!pegActivated)
                    ActivatePeg();
            }
            if (Mathf.Abs(initTime + goTime - currentTime) < 0.05 && startSphereTouch)
            {
                GreenlightPeg();
            }
            if ((Mathf.Abs(maxTime - currentTime) < 0.05) || pegEntered) // Current time raramente toma un valor entero, entonces se define una diferencia m�xima
            {
                DeactivatePeg();
            }
        }
        else
        {
            if (cameraRecording)
            {
                videoRecorder.StopVideoCapture();
                cameraRecording = false;
            }
            if (!writtenData && currentTime > 0.5)
            {
                fileManager.WriteData();

                if (isAnalogAcquisition)
                {
                    // Se marca el fin de la tarea
                    RunTrigger(8, endPulse: true);
                }

                writtenData = true;
            }
            
        }
    }

    IEnumerator AcquisitionStart()
    {
        if (isCamRec)
        {
            videoRecorder.StartVideoCapture();
            cameraRecording = true;
            yield return new WaitForSeconds((float)3);
        }

        if (isAnalogAcquisition)
            RunTrigger(8); // Se marca el inicio de la tarea
    }

    public void ResetPegs() // M�todo que regresa los pegs a su posici�n inicial
    {
        if (PegPosition.Length != Pegs.Length) return;

        for (int ii = 0; ii < Pegs.Length; ii++)
        {
            Pegs[ii].transform.SetPositionAndRotation(PegPosition[ii], PegRotation[ii]);
            Rigidbody RB = (Rigidbody)Pegs[ii].GetComponent(typeof(Rigidbody));
            if (RB) RB.velocity = Vector3.zero;
            if (RB) RB.angularVelocity = Vector3.zero;
        }
    }

    void ActivatePeg()
    {
        Pegs[pegNumber - 1].transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = highlightMaterial; // Resalta un peg
        HoleBases[holeNumber - 1].SetActive(true); // Activa el peg fantasma de un agujero

        pegActivated = true;
        if (isAnalogAcquisition)
        {
            // Trigger 1
            writeState = RunTrigger(1);
        }
    }

    void GreenlightPeg()
    {
        Pegs[pegNumber - 1].transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = goMaterial; // Resalta un peg con color verde

        startSphere.SetActive(false);
        startSphereTouch = false;
    }

    public void DeactivatePeg()
    {
        Pegs[pegNumber - 1].transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = originalMaterial; // Regresa el peg a su color normal
        HoleBases[holeNumber - 1].SetActive(false); // Desactiva el peg fantasma

        if (isAnalogAcquisition)
        {
            writeState = RunTrigger(2);
        }

        if (pegEntered)
            fileManager.StoreTrialOutcome(1);
        else
            fileManager.StoreTrialOutcome(-1);

        pegDeactivate = true;
        pegActivated = false;

        currentTime = 0; // Resetea current time, para el pr�ximo turno
        numberPegsEntered++; // Suma uno a la cantidad de turnos
        pegEntered = false; // Setea en falso para poder iniciar el turno siguiente

        pegsEntered[pegNumber - 1] = true; // Peg pegNumber-1 ingresado
        holesEntered[holeNumber - 1] = true; // Agujero holeNumber-1 usado

        pegIndex++;


        if (pegIndex == pegOrder.Count)
        {
            pegIndex = 0;
            RandomizeOrder();
        }

        pegNumber = pegOrder[pegIndex];
        holeNumber = holeOrder[pegIndex];

        startSphereWaitTime = Random.Range(5, 9);
    }

    void PegGrabEvent(string peg)
    {
        string resultString = Regex.Match(peg, @"\d+").Value;
        int pegGrabbed = int.Parse(resultString);

        fileManager.StorePeg(pegGrabbed);
        fileManager.StoreGrabMoment();
    }

    void PegReleaseEvent()
    {
        if (!pegDeactivate && endTrial)
        {
            DeactivatePeg();
        }
        fileManager.StoreGrabMoment(0);
    }

    void PegEnter(Collider col, string holeName) // M�todo llamado al producirse un evento de entrada de peg
    {
        fileManager.StoreHole(holeNumber - 1);
        pegEntered = true; // Setea pegEntered en true, lo que terminar� el turno
    }

    private void MirrorScene()
    {
        if (rightHand)
        {
            paradigmScene.transform.position = new Vector3(-0.532f, 0.0f, -1.532f);
            startSphere.transform.localPosition = new Vector3(12.58f, 9.07f, 2.89f);

            rightHand = false;
            mirrored = true;
        }
        else if (mirrored)
        {
            paradigmScene.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            startSphere.transform.localPosition = new Vector3(13.58f, 9.07f, -23.37f);

            rightHand = true;
        }

        if (!mirrored)
        {
            mirrored = true;
            rightHand = true;
            return;
        }

        paradigmScene.transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);

        hapticGrabberHand.ChangeHand();

        for (int ii = 0; ii < Pegs.Length; ii++)
        {
            PegPosition[ii] = Pegs[ii].transform.position;
            PegRotation[ii] = Pegs[ii].transform.rotation;
        }

    }

    IEnumerator TriggerPulseWidth(int trigger)
    {
        yield return new WaitForSecondsRealtime((float)0.01);

        if (writeState)
            writeState = RunTrigger(trigger, endPulse: true);
    }

    public bool RunTrigger(int trigger, bool endPulse = false)
    {
        bool status = false;
        uint[] message = { 1 };
        switch (trigger)
        {
            case 0:
                message = new uint[] { 1, 1, 1, 1, 1, 1, 1, 1 };
                status = false;
                break;
            case 1:
                message = new uint[] { 0, 1, 1, 1, 1, 1, 1, 1 };
                break;
            case 2:
                message = new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 };
                break;
            case 3:
                message = new uint[] { 1, 1, 0, 1, 1, 1, 1, 1 };
                break;
            case 4:
                message = new uint[] { 1, 1, 1, 0, 1, 1, 1, 1 };
                break;
            case 5:
                message = new uint[] { 1, 1, 1, 1, 0, 1, 1, 1 };
                break;
            case 6:
                message = new uint[] { 1, 1, 1, 1, 1, 0, 1, 1 };
                break;
            case 7:
                message = new uint[] { 1, 1, 1, 1, 1, 1, 0, 1 };
                break;
            case 8:
                message = new uint[] { 1, 1, 1, 1, 1, 1, 1, 0 };
                break;
            case 9:
                message = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                break;
        }

        writeState = daqConnector.WriteDigitalValue(message, endPulse, port: 0);
        
        if (trigger != 0 && trigger != 8)
        {
            fileManager.StoreTrigger(trigger);
            StartCoroutine(TriggerPulseWidth(trigger));
        }

        if (trigger == 8)
            fileManager.StoreTrigger(trigger + 10);

        return status;
    }
    //public bool RunNITriggerTestPort2(int trigger)
    //{
    //    bool status = false;
    //    uint[] message = { 0 };
    //    switch (trigger)
    //    {
    //        case 0:
    //            message[0] = 0;
    //            status = false;
    //            break;
    //        case 1:
    //            message[0] = 1;
    //            break;
    //    }

    //    //daqConnector.WriteDigitalValue(message, port: 2);

    //    fileManager.StoreTrigger(trigger + 10);
    //    return status;
    //}

    void StartSphereEvent(Collider col)
    {
        if (!startSphereTouch)
        {
            startSphereTouch = true;
            pegDeactivate = false;
            currentTime = 0;
        }
    }

    void RandomizeOrder()
    {
        for (int i = 0; i < pegOrder.Count; i++)
        {
            int temp = pegOrder[i];
            int randomIndex = Random.Range(i, pegOrder.Count);
            pegOrder[i] = pegOrder[randomIndex];
            pegOrder[randomIndex] = temp;
        }
        for (int i = 0; i < holeOrder.Count; i++)
        {
            int temp = holeOrder[i];
            int randomIndex = Random.Range(i, holeOrder.Count);
            holeOrder[i] = holeOrder[randomIndex];
            holeOrder[randomIndex] = temp;
        }
    }
}

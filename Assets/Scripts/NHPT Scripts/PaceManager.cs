using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Janelia;


public class PaceManager : MonoBehaviour
{
    [Header("Configuración escena")]
    public GameObject paradigmScene;

    public HapticGrabberHandSP hapticGrabberHand;
    private bool rightHand = true;
    private bool mirrored = false;

    [Header("Grabación de cámara")]
    public VideoRecord videoRecorder;

    [Header("Almacenamiento de datos")]
    public FileManager fileManager;
    private bool writtenData = false;

    [Header("Configuración Pegs")]
    public GameObject[] Pegs = { null, null, null, null }; // Array que contiene pegs
    private Vector3[] PegPosition; // Contiene las posiciones iniciales de los pegs
    private Quaternion[] PegRotation; // Contiene las rotaciones iniciales de los pegs

    [HideInInspector] public float currentTime = 0; // Tiempo que pasó desde que comienza el turno de mover un peg, en segundos
    //[HideInInspector] public float totalCurrentTime = 0; // Tiempo que pasó desde el comienzo de la prueba

    public int totalPegsEntered = 9;


    [Header("Configuración Holes")]
    public GameObject[] HoleBases = { null, null, null, null }; // Array de agujeros
    private int holeNumber = 1; // Representa el agujero seleccionado en el que se introducirá el peg

    public PegHoleCollisionEvent[] PHCollisionEvents = { null, null }; // Array que contiene los eventos de colisión de los agujeros


    private bool pegEntered = false; // Booleano que se vuelve true al insertar un peg, terminando el turno

    private int numberPegsEntered = 0; // Variable que cuenta la cantidad de pegs ingresados
    
    private bool[] holesEntered = {false, false, false,
                                false, false, false,
                                false, false, false }; // Array de bools que permite encontrar cuáles agujeros ya tuvieron su turno

    private bool holeActivated = false;


    private bool isCamRec; // Opción cámara

    private bool cameraRecording = false;

    // NI link
    NiDaqMx.DigitalOutputParams[] digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int lines = 7; // Líneas digitales a escribir

    NiDaqMx.DigitalOutputParams digitalOutputParamPort2; // Parámetros NI
    private bool writeStatePort2 = false;


    private bool isAnalogAcquisition; // Opción adquisición

    // Start is called before the first frame update
    void Start()
    {
        // Desactiva colisión entre los pegs y el plano invisible
        DisablePegPlaneCollisions();

        // Opciones
        isCamRec = PlayerPrefs.GetInt("CameraRec", 1) == 1; // if true

        isAnalogAcquisition = PlayerPrefs.GetInt("AnalogAcquisition", 1) == 1; // if true

        rightHand = PlayerPrefs.GetInt("RightHand_NHPTsp", 0) == 1;

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

            PHCollisionEvents[i].onTriggerEnter.AddListener(PegEnter); // Establece una conexión con los eventos de entrada de los pegs
        }


        if (isCamRec)
        {
            videoRecorder.StartVideoCapture();
            cameraRecording = true;
        }

        if (isAnalogAcquisition)
        {
            // Inicializa variables NI
            digitalOutputParams = new NiDaqMx.DigitalOutputParams[8];
            for (int i = 0; i < 8; i++)
            {
                digitalOutputParams[i] = new NiDaqMx.DigitalOutputParams();
                _ = NiDaqMx.CreateDigitalOutput(digitalOutputParams[i], false, i);
            }
            writeState = RunNITrigger(0, lines);


            digitalOutputParamPort2 = new NiDaqMx.DigitalOutputParams();
            _ = NiDaqMx.CreateDigitalOutput(digitalOutputParamPort2, port: 1);

            // Se marca el inicio de la tarea
            writeStatePort2 = RunNITriggerTestPort2(1);
        }

        MirrorScene();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2); // Salir del test al menú inicial

        if (Input.GetKeyDown("s"))
            MirrorScene();

        currentTime += Time.deltaTime; // Aumenta el tiempo en intervalos deltaTime, que es el tiempo entre frames

        if (numberPegsEntered < totalPegsEntered) // Verifica que el número de turnos no sea mayor al total
        {
            if (!holeActivated) // Current time raramente toma un valor entero, entonces se define una diferencia máxima
            {
                HoleBases[holeNumber - 1].SetActive(true); // Activa el peg fantasma de un agujero
                holeActivated = true;
            }
            if (pegEntered) // Current time raramente toma un valor entero, entonces se define una diferencia máxima
            {
                HoleBases[holeNumber - 1].SetActive(false); // Desactiva el peg fantasma

                fileManager.StoreTrialOutcome(1);

                numberPegsEntered++; // Suma uno a la cantidad de turnos
                pegEntered = false; // Setea en falso para poder iniciar el turno siguiente
                holeActivated = false;

                holesEntered[holeNumber - 1] = true; // Agujero holeNumber-1 usado

                if (numberPegsEntered < totalPegsEntered) // Verifica que el número de turnos no se mayor al número de pegs
                {
                    holeNumber++;
                }
            }

        }
        else
        {
            if (cameraRecording)
            {
                videoRecorder.StopVideoCapture();
                cameraRecording = false;
            }
            if (!writtenData)
            {
                fileManager.WriteData();
                writtenData = true;
            }
        }
    }


    public void ResetPegs() // Método que regresa los pegs a su posición inicial
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

    void PegEnter(Collider col) // Método llamado al producirse un evento de entrada de peg
    {
        pegEntered = true; // Setea pegEntered en true, lo que terminará el turno
        PHCollisionEvents[holeNumber - 1].onTriggerEnter.RemoveListener(PegEnter); // Se desvincula del evento del agujero usado
    }

    void DisablePegPlaneCollisions()
    {
        GameObject plane = GameObject.Find("CylinderCube");
        Collider planeCollider = plane.GetComponent<Collider>();
        for (int ii = 0; ii < Pegs.Length; ii++)
        {
            Physics.IgnoreCollision(Pegs[ii].transform.GetChild(0).gameObject.GetComponent<Collider>(), planeCollider);
        }
    }

    private void MirrorScene()
    {
        if (rightHand)
        {
            paradigmScene.transform.position = new Vector3(-0.532f, 0.0f, -1.532f);

            rightHand = false;
            mirrored = true;
        }
        else if (mirrored)
        {
            paradigmScene.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

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

    IEnumerator TriggerPulseWidth()
    {
        yield return new WaitForSecondsRealtime((float)0.01);

        if (writeState)
            writeState = RunNITrigger(0, lines);
    }

    public bool RunNITrigger(int trigger, int lines)
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
                message = new uint[] { 0, 0, 0, 1, 1, 1, 1, 1 };
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

        for (int i = 0; i < lines; i++)
            status = NiDaqMx.WriteDigitalValue(digitalOutputParams[i], new uint[] { message[i] }, ref numWritten);

        fileManager.StoreTrigger(trigger);

        if (trigger != 0)
            StartCoroutine(TriggerPulseWidth());

        return status;
    }

    public bool RunNITriggerTestPort2(int trigger)
    {
        bool status = false;
        uint message = 0;
        switch (trigger)
        {
            case 0:
                message = 0;
                status = false;
                break;
            case 1:
                message = 1;
                break;
        }

        status = NiDaqMx.WriteDigitalValue(digitalOutputParamPort2, new uint[] { message }, ref numWritten);
        return status;
    }
}

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
    NiDaqMx.DigitalOutputParams digitalOutputParams;
    private int numWritten = 0;
    private bool writeState = false;
    //private bool toggleDig = true;

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
            PHCollisionEvents[i].onTriggerExit.AddListener(PegExit); // Establece una conexión con los eventos de salida de los pegs
        }


        if (isCamRec)
        {
            videoRecorder.StartVideoCapture();
            cameraRecording = true;
        }

        if (isAnalogAcquisition)
        {
            digitalOutputParams = new NiDaqMx.DigitalOutputParams();
            bool n = NiDaqMx.CreateDigitalOutput(digitalOutputParams); // Inicialización de variables de conexión con placa de adquisición
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

        if (writeState)
            writeState = RunNITrigger(0);


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
        Debug.Log("Peg in"); // Mensaje por consola
        pegEntered = true; // Setea pegEntered en true, lo que terminará el turno
        PHCollisionEvents[holeNumber - 1].onTriggerEnter.RemoveListener(PegEnter); // Se desvincula del evento del agujero usado
    }

    void PegExit(Collider col) // Método llamado al producirse un evento de salida de peg (no usado)
    {
        Debug.Log("Peg out"); // Mensaje por consola
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


    public bool RunNITrigger(int trigger)
    {
        bool status = false;

        if (isAnalogAcquisition)
        {
            switch (trigger)
            {
                case 0:
                    status = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 }, ref numWritten);
                    status = false;
                    break;
                case 1:
                    status = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 0, 1, 1, 1, 1, 1, 1, 1 }, ref numWritten);
                    break;
                case 2:
                    status = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 }, ref numWritten);
                    break;

            }
            fileManager.StoreTrigger(trigger);
        }
        return status;
    }
}

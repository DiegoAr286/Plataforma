using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Janelia;


public class CueManager : MonoBehaviour
{
    [Header("Configuración escena")]
    public GameObject paradigmScene;

    public HapticGrabberHandCB hapticGrabberHand;
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

    public int startSphereWaitTime = 8; // Tiempo que tarda en comenzar la tarea luego de tocar la esfera de inicio
    public int initTime = 1; // Tiempo que tarda en modificarse los colores del peg desde que termina el turno anterior
    public int goTime = 3; // Tiempo que tarda en pasar de rojo a verde un peg
    public int maxTime = 10; // Tiempo máximo que durará un turno

    public int totalPegsEntered = 45;
    public bool modeReset = false;

    [Range(1, 9)] public int pegNumber = 1; // Representa el peg seleccionado para mover

    public Material originalMaterial; // Material original (blanco)
    public Material goMaterial; // Material verde
    public Material highlightMaterial; // Material que resaltará al peg (rojo)

    [Header("Configuración Holes")]
    public GameObject[] HoleBases = { null, null, null, null }; // Array de agujeros
    [Range(1, 9)] public int holeNumber = 1; // Representa el agujero seleccionado en el que se introducirá el peg

    public PegHoleCollisionEvent[] PHCollisionEvents = { null, null }; // Array que contiene los eventos de colisión de los agujeros

    [Header("Posición de inicio")]
    public StartSphereCollisionEvent startSphereCollisionEvent; // Evento de colisión con esfera de inicio

    private bool startSphereTouch = false;
    GameObject startSphere;


    private bool pegEntered = false; // Booleano que se vuelve true al insertar un peg, terminando el turno

    private List<int> pegOrder;
    private int pegIndex = 0;
    private List<int> holeOrder;

    private int numberPegsEntered = 0; // Variable que cuenta la cantidad de pegs ingresados
    private bool[] pegsEntered = {false, false, false,
                                false, false, false,
                                false, false, false }; // Array de bools que permite encontrar cuáles pegs ya tuvieron su turno
    private bool[] holesEntered = {false, false, false,
                                false, false, false,
                                false, false, false }; // Array de bools que permite encontrar cuáles agujeros ya tuvieron su turno
    [HideInInspector] public bool pegDeactivate = true;
    private bool pegActivated = false;

    private bool isCamRec; // Opción cámara

    private bool cameraRecording = false;

    // NI link
    NiDaqMx.DigitalOutputParams digitalOutputParams;
    private int numWritten = 0;
    private bool writeState = false;
    private int frameCounterNI = 0;
    private bool toggleDig = true;

    private bool isAnalogAcquisition = true; // Opción adquisición

    // Start is called before the first frame update
    void Start()
    {
        // Desactiva colisión entre los pegs y el plano invisible
        DisablePegPlaneCollisions();

        // Establece una conexión con el evento de toque de la esfera de inicio
        startSphereCollisionEvent.onTriggerEnter.AddListener(StartSphereEvent);

        startSphere = GameObject.Find("StartSphere"); // Busca el objeto esfera de inicio
        startSphere.SetActive(false);

        // Opciones
        isCamRec = PlayerPrefs.GetInt("CameraRec", 1) == 1; // if true

        isAnalogAcquisition = PlayerPrefs.GetInt("AnalogAcquisition", 1) == 1; // if true
        rightHand = PlayerPrefs.GetInt("RightHand_NHPTcb", 1) == 1;

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

        //pegNumber = Random.Range(1, 10); // Busca un peg random
        //holeNumber = Random.Range(1, 10); // Busca un agujero random

        pegOrder = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        holeOrder = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
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


        pegNumber = pegOrder[pegIndex];
        holeNumber = holeOrder[pegIndex];


        if (isCamRec)
        {
            videoRecorder.StartVideoCapture();
            cameraRecording = true;
        }

        if (isAnalogAcquisition)
        {
            digitalOutputParams = new NiDaqMx.DigitalOutputParams();
            bool n = NiDaqMx.CreateDigitalOutput(digitalOutputParams); // Inicialización de variables de conexión con placa de adquisición
            if (n)
            {
                Debug.Log("NI Conectado");
                // Inicializa salidas digitales en alto
                n = RunNITrigger(0);
            }
            else
                Debug.Log("NI Falló");
        }

        MirrorScene();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1); // Salir del test al menú inicial

        // Da el ancho de pulso
        if (writeState)
        {
            frameCounterNI++;

            if (writeState && frameCounterNI == 7)
            {
                writeState = RunNITrigger(0);
                frameCounterNI = 0;
            }
        }
        
        // Al presionar espacio regresa los pegs a su posición inicial
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //ResetPegs();
            if (toggleDig)
            {
                //writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 }, ref numWritten);
                writeState = RunNITrigger(2);

                toggleDig = false;
                Debug.Log(writeState);
            }
            else
            {
                //writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 1, 1, 1, 1, 1, 1, 1 }, ref numWritten);
                writeState = RunNITrigger(1);

                toggleDig = true;
                Debug.Log(writeState);
            }
            //writeState = RunNITrigger(1);

            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
            MirrorScene();

        currentTime += Time.deltaTime; // Aumenta el tiempo en intervalos deltaTime, que es el tiempo entre frames



        if (numberPegsEntered < totalPegsEntered) // Verifica que el número de turnos no sea mayor al total
        {
            if (pegDeactivate && currentTime >= startSphereWaitTime)
                startSphere.SetActive(true);

            if (Mathf.Abs(initTime - currentTime) < 0.05 && startSphereTouch) // Current time raramente toma un valor entero, entonces se define una diferencia máxima
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
            if ((Mathf.Abs(maxTime - currentTime) < 0.05) || pegEntered) // Current time raramente toma un valor entero, entonces se define una diferencia máxima
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

    void ActivatePeg()
    {
        Pegs[pegNumber - 1].transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = highlightMaterial; // Resalta un peg
        HoleBases[holeNumber - 1].SetActive(true); // Activa el peg fantasma de un agujero

        pegActivated = true;
        // Trigger 1
        writeState = RunNITrigger(1);
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


        writeState = RunNITrigger(2);

        if (pegEntered)
            fileManager.StoreTrialOutcome(1);
        else
            fileManager.StoreTrialOutcome(-1);

        pegDeactivate = true;
        pegActivated = false;

        currentTime = 0; // Resetea current time, para el próximo turno
        numberPegsEntered++; // Suma uno a la cantidad de turnos
        pegEntered = false; // Setea en falso para poder iniciar el turno siguiente

        pegsEntered[pegNumber - 1] = true; // Peg pegNumber-1 ingresado
        holesEntered[holeNumber - 1] = true; // Agujero holeNumber-1 usado

        pegIndex++;


        if (pegIndex == pegOrder.Count)
        {
            pegIndex = 0;
        }

        pegNumber = pegOrder[pegIndex];
        holeNumber = holeOrder[pegIndex];

        //if (numberPegsEntered % Pegs.Length == 0)
        //{
        //    for (int i = 0; i < Pegs.Length; i++)
        //    {
        //        pegsEntered[i] = false;
        //        holesEntered[i] = false;
        //    }
        //}

        //if (numberPegsEntered % Pegs.Length != 0) // Verifica que no se hayan colocado todos los pegs
        //{
        //    do
        //    {
        //        pegNumber = Random.Range(1, 10); // Busca un peg random
        //    }
        //    while (pegsEntered[pegNumber - 1]); // Sigue buscando mientras se haya elegido uno ya usado

        //    do
        //    {
        //        holeNumber = Random.Range(1, 10); // Busca un agujero random
        //    }
        //    while (holesEntered[holeNumber - 1]); // Sigue buscando mientras se haya elegido uno ya usado
        //}
    }

    void PegEnter(Collider col) // Método llamado al producirse un evento de entrada de peg
    {
        //Debug.Log("Peg in"); // Mensaje por consola
        pegEntered = true; // Setea pegEntered en true, lo que terminará el turno
        PHCollisionEvents[holeNumber - 1].onTriggerEnter.RemoveListener(PegEnter); // Se desvincula del evento del agujero usado
    }

    void PegExit(Collider col) // Método llamado al producirse un evento de salida de peg (no usado)
    {
        //Debug.Log("Peg out"); // Mensaje por consola
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

    public void PegGrabEvent()
    {
        fileManager.StoreGrabMoment();
    }


    public bool RunNITrigger(int trigger)
    {
        bool status = false;

        if (isAnalogAcquisition)
        {
            switch (trigger)
            {
                case 0:
                    _ = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 1, 1, 1, 1, 1, 1, 1 }, ref numWritten);
                    status = false;
                    break;
                case 1:
                    status = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 0, 1, 1, 1, 1, 1, 1, 1 }, ref numWritten);
                    break;
                case 2:
                    status = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 }, ref numWritten);
                    break;
            }
        }
        fileManager.StoreTrigger(trigger);

        return status;
    }

    void StartSphereEvent(Collider col)
    {
        if (!startSphereTouch)
        {
            startSphereTouch = true;
            pegDeactivate = false;
            currentTime = 0;
        }
    }
}

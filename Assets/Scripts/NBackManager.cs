using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Janelia;
using UnityEngine.SceneManagement;


public class NBackManager : MonoBehaviour
{
    public GameObject[] Letters = { null, null, null, null }; // Array de letras
    public GameObject correctMarker; // Marcador verde, de trial exitoso
    public GameObject incorrectMarker; // Marcador rojo, de trial fallido
    public GameObject warmUpInit; // Cartel de inicio del calentamiento
    public GameObject taskInit; // Cartel de inicio del registro
    public GameObject finishSign; // Cartel de finalización

    public FileManagerNBack FileManager;

    public Canvas canvas;

    [Range(2, 3)] public int nBack = 2; // Número N de N-Back

    private bool warmUp = false; // Booleano que marca la etapa de calentamiento
    private bool warmUpFinished = false; // Booleano que marca que se terminó la etapa de calentamiento
    private bool taskFinished = false; // Booleano que marca que se terminó la etapa de registro
    private bool task = false; // Booleano que marca la etapa de registro

    private int[] warmUpOrder; // Array que contiene el orden de las letras durante el calentamiento
    private int[] warmUpCoincidences; // Array que contiene dónde se encuentran las coincidencias dentro del calentamiento
    private int[] taskOrder; // Array que contiene el orden de las letras durante el registro
    private int[] taskCoincidences; // Array que contiene dónde se encuentran las coincidencias dentro del registro
    private int coincidenceCounter = 0; // Variable que cuenta la cantidad de coincidencias


    private int fixedFrameCounter = 0; // Contador de la cantidad de frames de FixedUpdate que pasaron
    private int frameCounter; // Contador de la cantidad de frames de Update que pasaron
    private int letterCounter = 0; // Contador de letras que se han mostrado
    private bool activeLetter = false; // Booleano que señala si se encuentra una tecla activa
    private int letter = 0; // Variable que contiene qué letra se mostrará

    private bool repetition = true; // Booleano que marca si se está intentando agregar una coincidencia donde ya la hay

    private bool alreadyPressed = false; // Booleano que marca cuando ya se ha presionado la tecla en un trial
    private bool validTime = false; // Booleano que marca que existe una coincidencia en el trial
    private bool matchTrial = false; // Booleano que marca un trial exitoso
    private bool missTrial = false; // Booleano que marca un trial fallido
    private bool falseAlarm = false; // Booleano que marca la presión de la tecla en un trial sin coincidencia
    private double initTime = 0;
    private double reactionTime;
    private bool matchingTrial = false;

    // Salida digital
    NiDaqMx.DigitalOutputParams[] digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int lines = 3; // Líneas digitales a escribir
    private int frameCounterNI = 0;

    // Entrada digital
    NiDaqMx.DigitalInputParams digitalInputParams; // Parámetros NI
    private uint[] readDigitalData;
    private int numReadPerChannel;
    private int numBytesPerSamp;

    private void Awake()
    {
        Screen.SetResolution(1366, 768, true);
        Application.targetFrameRate = 144;

        if (Display.displays.Length == 1)
            return;

        if (Display.displays[1].active)
            canvas.targetDisplay = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < Letters.Length; i++)
            Letters[i].SetActive(false); // Desactiva las letras

        correctMarker.SetActive(false);
        incorrectMarker.SetActive(false);
        taskInit.SetActive(false);
        finishSign.SetActive(false);

        LetterOrder(nBack);

        Time.fixedDeltaTime = 0.01F; // FixedUpdate a 100Hz

        // Inicializa variables NI
        digitalOutputParams = new NiDaqMx.DigitalOutputParams[8];
        for (int i = 0; i < 8; i++)
        {
            digitalOutputParams[i] = new NiDaqMx.DigitalOutputParams();
            _ = NiDaqMx.CreateDigitalOutput(digitalOutputParams[i], false, i);
        }
        writeState = RunNITrigger(0, lines);

        digitalInputParams = new NiDaqMx.DigitalInputParams();
        _ = NiDaqMx.CreateDigitalInput(digitalInputParams);
        readDigitalData = new uint[32];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !warmUp && !task && !warmUpFinished)
        {
            warmUpInit.SetActive(false);
            warmUp = true;
        }
        if (Input.GetKeyDown(KeyCode.S) && !warmUp && !task && !warmUpFinished)
        {
            warmUpInit.SetActive(false);
            warmUpFinished = true;
            taskInit.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !warmUp && !task && warmUpFinished && !taskFinished)
        {
            taskInit.SetActive(false);
            task = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            for (int i = 0; i < 8; i++)
                NiDaqMx.ClearOutputTask(digitalOutputParams[i]);

            NiDaqMx.ClearInputTask(digitalInputParams);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 3); // Salir del test al menú inicial
        }



        //if (Input.GetKeyDown("return") && (warmUp || task) && !alreadyPressed)
        if (readDigitalData[0] == 1 && (warmUp || task) && !alreadyPressed)
        {
            if (validTime)
            {
                correctMarker.SetActive(true);
                frameCounter = 0;
                reactionTime = Time.realtimeSinceStartup - initTime;
                validTime = false;
                matchTrial = true;
            }
            else
            {
                incorrectMarker.SetActive(true);
                frameCounter = 0;
                reactionTime = -1;
                falseAlarm = true;
            }
            alreadyPressed = true;

            if (task)
                writeState = RunNITrigger(1, lines);
        }

        if (frameCounter < 50)
            frameCounter++;
        if (frameCounter == 49)
        {
            correctMarker.SetActive(false);
            incorrectMarker.SetActive(false);
        }

        TriggerPulseWidth();
    }

    void FixedUpdate()
    {
        //if (readDigitalData[0] == 1)
        //{
        //    writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 }, ref numWritten);
        //}
        // Sesión de prueba
        if (warmUp)
        {
            if (letterCounter <= warmUpOrder.Length)
            {
                _ = NiDaqMx.ReadFromDigitalInput(digitalInputParams, ref readDigitalData, ref numReadPerChannel, ref numBytesPerSamp);

                if (fixedFrameCounter == 50)
                {
                    if (activeLetter)
                    {
                        Letters[letter].SetActive(false);
                        activeLetter = false;
                    }

                }

                if (fixedFrameCounter == 250)
                {
                    fixedFrameCounter = 0;
                    //letter = Random.Range(0, 15);
                    if (letterCounter < warmUpOrder.Length)
                    {
                        letter = warmUpOrder[letterCounter];
                        Letters[letter].SetActive(true);
                        activeLetter = true;

                        if (letterCounter == warmUpCoincidences[coincidenceCounter] + nBack)
                        {
                            if (coincidenceCounter < warmUpCoincidences.Length - 1)
                                coincidenceCounter++;
                            validTime = true;
                        }
                        else
                            validTime = false;
                    }
                    letterCounter++;
                    alreadyPressed = false;
                }
                else if (fixedFrameCounter < 250)
                    fixedFrameCounter++;
            }
            else
            {
                Letters[letter].SetActive(false);
                fixedFrameCounter = 0;
                coincidenceCounter = 0;
                letterCounter = 0;
                activeLetter = false;
                validTime = false;
                alreadyPressed = false;
                warmUp = false;
                warmUpFinished = true;
                taskInit.SetActive(true);
            }
        }

        // Sesión de registro
        if (task)
        {
            if (letterCounter <= taskOrder.Length)
            {
                _ = NiDaqMx.ReadFromDigitalInput(digitalInputParams, ref readDigitalData, ref numReadPerChannel, ref numBytesPerSamp);

                if (fixedFrameCounter == 50)
                {
                    if (activeLetter)
                    {
                        Letters[letter].SetActive(false);
                        activeLetter = false;
                    }

                }

                if (fixedFrameCounter == 250)
                {
                    fixedFrameCounter = 0;
                    if (letterCounter < taskOrder.Length)
                    {
                        letter = taskOrder[letterCounter];
                        Letters[letter].SetActive(true);
                        activeLetter = true;
                        initTime = Time.realtimeSinceStartupAsDouble;
                        writeState = RunNITrigger(2, lines);

                        if (validTime && !matchTrial)
                            missTrial = true;

                        if (letterCounter > 2) // Guarda datos a partir de la 3ra letra
                        {
                            FileManager.StoreDataInBuffer(letterCounter, matchingTrial ? 1 : 0, matchTrial ? 1 : 0, matchTrial ? 1 : 0, missTrial ? 1 : 0,
                                falseAlarm ? 1 : 0, reactionTime, taskOrder[letterCounter-1], taskOrder[letterCounter-2], taskOrder[letterCounter-3]);
                        }

                        if (letterCounter == taskCoincidences[coincidenceCounter] + nBack)
                        {
                            if (coincidenceCounter < taskCoincidences.Length - 1)
                                coincidenceCounter++;
                            validTime = true;
                            matchingTrial = true;
                        }
                        else
                        {
                            
                            validTime = false;
                            matchingTrial = false;
                        }
                        reactionTime = 0;

                    }
                    if (letterCounter == taskOrder.Length)
                    {
                        FileManager.StoreDataInBuffer(letterCounter, matchingTrial ? 1 : 0, matchTrial ? 1 : 0, matchTrial ? 1 : 0, missTrial ? 1 : 0,
                            falseAlarm ? 1 : 0, reactionTime, taskOrder[letterCounter - 1], taskOrder[letterCounter - 2], taskOrder[letterCounter - 3]);
                    }
                    letterCounter++;
                    alreadyPressed = false;
                    matchTrial = false;
                    missTrial = false;
                    falseAlarm = false;
                }
                else if (fixedFrameCounter < 250)
                    fixedFrameCounter++;
            }
            else
            {
                Letters[letter].SetActive(false);
                activeLetter = false;
                validTime = false;
                alreadyPressed = false;
                finishSign.SetActive(true);
                FileManager.WriteData();
                taskFinished = true;
                task = false;
            }
        }
    }

    private void LetterOrder(int nBack)
    {
        warmUpOrder = new int[25]; // 9 coincidencias, 33% de 25
        warmUpCoincidences = new int[9];
        taskOrder = new int[50]; // 17 coincidencias, 33% de 50
        taskCoincidences = new int[17];

        // Se genera el orden aleatorio de las letras para las dos etapas
        for (int i = 0; i < warmUpOrder.Length; i++)
        {
            warmUpOrder[i] = Random.Range(0, 15);
            //Debug.Log(warmUpOrder[i]);
        }
        for (int i = 0; i < warmUpCoincidences.Length; i++)
            warmUpCoincidences[i] = 25;

        for (int i = 0; i < taskOrder.Length; i++)
            taskOrder[i] = Random.Range(0, 15);
        for (int i = 0; i < taskCoincidences.Length; i++)
            taskCoincidences[i] = 50;

        // Se agregan coincidencias para cumplir con lo requerido
        int randomPosition;
        for (int i = 0; i < 9; i++)
        {
            randomPosition = Random.Range(0, 25 - nBack);

            while (repetition == true)
            {
                repetition = false;
                for (int j = 0; j < 9; j++)
                {
                    if (randomPosition == warmUpCoincidences[j] || randomPosition + nBack == warmUpCoincidences[j] || randomPosition - nBack == warmUpCoincidences[j])
                    {
                        randomPosition = Random.Range(0, 25 - nBack);
                        repetition = true;
                    }
                }
            }
            repetition = true;

            warmUpOrder[randomPosition + nBack] = warmUpOrder[randomPosition];
            warmUpCoincidences[i] = randomPosition;

        }
        System.Array.Sort(warmUpCoincidences);

        for (int i = 0; i < 17; i++)
        {
            randomPosition = Random.Range(0, 50 - nBack);

            while (repetition == true)
            {
                repetition = false;
                for (int j = 0; j < 17; j++)
                {
                    if (randomPosition == taskCoincidences[j] || randomPosition + nBack == taskCoincidences[j] || randomPosition - nBack == taskCoincidences[j])
                    {
                        randomPosition = Random.Range(0, 50 - nBack);
                        repetition = true;
                    }
                }
            }
            repetition = true;
            taskOrder[randomPosition + nBack] = taskOrder[randomPosition];
            taskCoincidences[i] = randomPosition;
        }
        System.Array.Sort(taskCoincidences);

        // Se verifica la cantidad de coincidencias y se eliminan las sobrantes
        bool wrongCoincidence;
        for (int i = 0; i < warmUpOrder.Length - nBack; i++)
        {
            if (warmUpOrder[i] == warmUpOrder[i + nBack])
            {
                wrongCoincidence = true;
                for (int j = 0; j < warmUpCoincidences.Length; j++)
                {
                    if (i == warmUpCoincidences[j])
                        wrongCoincidence = false;
                }
                if (wrongCoincidence)
                {
                    while (warmUpOrder[i] == warmUpOrder[i + nBack])
                        warmUpOrder[i + nBack] = Random.Range(0, 15);
                }
            }
        }
        
        for (int i = 0; i < taskOrder.Length - nBack; i++)
        {
            if (taskOrder[i] == taskOrder[i + nBack])
            {
                wrongCoincidence = true;
                for (int j = 0; j < taskCoincidences.Length; j++)
                {
                    if (i == taskCoincidences[j])
                        wrongCoincidence = false;
                }
                if (wrongCoincidence)
                {
                    while (taskOrder[i] == taskOrder[i + nBack])
                        taskOrder[i + nBack] = Random.Range(0, 15);
                }
            }
        }

        //for (int i = 0; i < warmUpOrder.Length; i++)
        //{
        //    Debug.Log(warmUpOrder[i]);
        //}
        //Debug.Log("fffff");
        //for (int i = 0; i < warmUpCoincidences.Length; i++)
        //{
        //    Debug.Log(warmUpCoincidences[i]);
        //}
        //Debug.Log("gggggg");

        //for (int i = 0; i < taskOrder.Length; i++)
        //{
        //    Debug.Log(taskOrder[i]);
        //}
        //Debug.Log("hhhhhhh");
        //for (int i = 0; i < taskCoincidences.Length; i++)
        //{
        //    Debug.Log(taskCoincidences[i]);
        //}

    }

    private void TriggerPulseWidth()
    {
        if (writeState)
        {
            frameCounterNI++;

            if (writeState && frameCounterNI == 7)
            {
                writeState = RunNITrigger(0, lines);
                frameCounterNI = 0;
            }
        }
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
                message = new uint[] { 1, 0, 0, 1, 1, 1, 1, 1 }; // Trial izquierdo e incorrecto
                break;
            case 3:
                message = new uint[] { 1, 1, 0, 1, 1, 1, 1, 1 }; // Trial izquierdo y correcto
                break;
            case 4:
                message = new uint[] { 0, 0, 1, 0, 1, 1, 1, 1 }; // Trial derecho e incorrecto
                break;
            case 5:
                message = new uint[] { 0, 1, 0, 1, 0, 1, 1, 1 }; // Trial derecho y correcto
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
        return status;
    }
}

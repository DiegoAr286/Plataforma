using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Janelia;
using UnityEngine.SceneManagement;

public class StopSignalManager : MonoBehaviour
{
    public GameObject[] Arrows = { null, null }; // Flechas. 0 -> flecha derecha. 1 -> flecha izquierda.
    public GameObject[] ArrowsStop = { null, null };  // 0 -> flecha derecha con signo de parar. 1 -> flecha izquierda con signo de parar.
    public GameObject whiteCircle; // Círculo sin flecha
    public GameObject warmUpInit; // Cartel de inicio del calentamiento
    public GameObject taskInit; // Cartel de inicio del registro
    public GameObject finishSign; // Cartel de finalización
    public GameObject correctMarker; // Marcador verde, de trial exitoso
    public GameObject incorrectMarker; // Marcador rojo, de trial fallido

    public FileManagerStopSignal FileManager;

    private bool warmUp = false; // Booleano que marca la etapa de calentamiento
    private bool warmUpFinished = false; // Booleano que marca que se terminó la etapa de calentamiento
    private bool taskFinished = false; // Booleano que marca que se terminó la etapa de registro
    private bool task = false; // Booleano que marca la etapa de registro

    private int[] warmUpOrder; // Array que contiene el orden de las flechas durante el calentamiento
    public int nWarmUpRepetitions = 1;
    private int[] taskOrder; // Array que contiene el orden de las flechas durante el registro
    public int nTaskRepetitions = 40;
    private int[] taskStopSignals; // Array que contiene dónde se encuentran las señales de parada dentro del registro
    private int[] stopSignalsTime; // Array que contiene el tiempo de retardo de la aparición de las señales de parada
    private int stopSignalCounter = 0; // Variable que cuenta la cantidad de coincidencias


    private int fixedFrameCounter = 0; // Contador de la cantidad de frames de FixedUpdate que pasaron
    private int frameCounter; // Contador de la cantidad de frames de Update que pasaron
    private int arrowCounter = 0; // Contador de flechas que se han mostrado
    private bool activeArrow = false; // Booleano que señala si se encuentra una flecha activa
    private bool activeStopSignal = false; // Booleano que señala si se encuentra una señal de parada activa
    private int arrow = 0; // Variable que contiene qué flecha se mostrará

    private bool noRepetition = true; // Booleano que marca si se está intentando agregar una coincidencia donde ya la hay

    private bool alreadyPressed = false; // Booleano que marca cuando ya se ha presionado la tecla en un trial
    private bool validTime = false; // Booleano que marca que existe una coincidencia en el trial
    private bool matchTrial = false; // Booleano que marca un trial exitoso
    private bool missTrial = false; // Booleano que marca un trial fallido
    private bool wrongKey = false; // Booleano que marca la presión de la tecla en un trial sin coincidencia
    private double initTime = 0;
    private double reactionTime;
    private bool stopSignalTrial = false;

    NiDaqMx.DigitalOutputParams digitalOutputParams; // Parámetros NI
    NiDaqMx.DigitalInputParams digitalInputParams; // Parámetros NI
    //private bool readState = false;
    private bool writeState = false;
    private int frameCounterNI = 0;
    private uint[] readDigitalData;
    private int numWritten = 0;

    private int numReadPerChannel;
    private int numBytesPerSamp;

    //private double testTime;



    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < Arrows.Length; i++)
        {
            Arrows[i].SetActive(false);
            ArrowsStop[i].SetActive(false);
        }
        taskInit.SetActive(false);
        finishSign.SetActive(false);
        correctMarker.SetActive(false);
        incorrectMarker.SetActive(false);

        whiteCircle.SetActive(false);

        ArrowOrder();

        Time.fixedDeltaTime = 0.01F;

        // Inicializa variables NI
        digitalOutputParams = new NiDaqMx.DigitalOutputParams();
        _ = NiDaqMx.CreateDigitalOutput(digitalOutputParams);
        writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 1, 1, 1, 1, 1, 1, 1 }, ref numWritten);

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
            whiteCircle.SetActive(true);
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
            whiteCircle.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 4); // Salir del test al menú inicial


        // Entrada digital es 257 en alto, 1 para click izquierdo y 256 para click derecho
        if (readDigitalData[0] == 1 && (warmUp || task) && !alreadyPressed) // Click izquierdo
        {
            if (validTime && arrow == 1)
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
                reactionTime = Time.realtimeSinceStartup - initTime;
                validTime = false;
                wrongKey = true;
            }
            alreadyPressed = true;

            if (task)
                writeState = RunNITrigger(1);
        }

        if (readDigitalData[0] == 256 && (warmUp || task) && !alreadyPressed) // Click derecho
        {
            if (validTime && arrow == 0)
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
                reactionTime = Time.realtimeSinceStartup - initTime;
                validTime = false;
                wrongKey = true;
            }
            alreadyPressed = true;

            if (task)
                writeState = RunNITrigger(1);
        }
        if (frameCounter < 50)
            frameCounter++;

        if (frameCounter == 49)
        {
            correctMarker.SetActive(false);
            incorrectMarker.SetActive(false);
        }

        if (writeState)
        {
            frameCounterNI++;

            if (writeState && frameCounterNI == 7)
            {
                writeState = RunNITrigger(0);
                frameCounterNI = 0;
            }
        }

    }

    void FixedUpdate()
    {
        //if (readDigitalData[0] == 1)
        //{
        //    writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 }, ref numWritten);
        //}
        //Debug.Log(readDigitalData[0]);

        // Sesión de prueba
        if (warmUp)
        {
            if (arrowCounter <= warmUpOrder.Length)
            {
                _ = NiDaqMx.ReadFromDigitalInput(digitalInputParams, ref readDigitalData, ref numReadPerChannel, ref numBytesPerSamp);

                if (fixedFrameCounter == 50)
                {
                    if (activeArrow)
                    {
                        Arrows[arrow].SetActive(false);
                        whiteCircle.SetActive(true);
                        activeArrow = false;
                        //Debug.Log(Time.realtimeSinceStartup - testTime);

                        if (!alreadyPressed && validTime)
                        {
                            incorrectMarker.SetActive(true);
                            frameCounter = 0;
                        }
                        validTime = false;

                    }
                }

                if (fixedFrameCounter == 150)
                {
                    fixedFrameCounter = 0;
                    if (arrowCounter < warmUpOrder.Length)
                    {
                        arrow = warmUpOrder[arrowCounter];
                        whiteCircle.SetActive(false);
                        Arrows[arrow].SetActive(true);
                        activeArrow = true;
                        validTime = true;

                    }
                    arrowCounter++;
                    alreadyPressed = false;
                }
                else if (fixedFrameCounter < 150)
                    fixedFrameCounter++;
            }
            else
            {
                fixedFrameCounter = 0;
                stopSignalCounter = 0;
                arrowCounter = 0;
                activeArrow = false;
                validTime = false;
                alreadyPressed = false;
                warmUp = false;
                warmUpFinished = true;
                whiteCircle.SetActive(false);
                taskInit.SetActive(true);
            }
        }

        // Sesión de registro
        if (task)
        {
            if (arrowCounter <= taskOrder.Length)
            {
                _ = NiDaqMx.ReadFromDigitalInput(digitalInputParams, ref readDigitalData, ref numReadPerChannel, ref numBytesPerSamp);

                if (fixedFrameCounter == 50)
                {
                    if (activeArrow)
                    {
                        if (activeStopSignal)
                            ArrowsStop[arrow].SetActive(false);
                        else
                            Arrows[arrow].SetActive(false);
                        whiteCircle.SetActive(true);
                        activeArrow = false;

                        if (!alreadyPressed && validTime)
                        {
                            incorrectMarker.SetActive(true);
                            frameCounter = 0;
                        }
                        if (!alreadyPressed && activeStopSignal)
                        {
                            correctMarker.SetActive(true);
                            matchTrial = true;
                            frameCounter = 0;
                        }
                        if (validTime && !matchTrial)
                            missTrial = true;

                        validTime = false;
                    }

                }

                if (fixedFrameCounter == stopSignalsTime[stopSignalCounter] && activeStopSignal)
                {
                    ArrowsStop[arrow].SetActive(true);
                    Arrows[arrow].SetActive(false);
                    //Debug.Log(Time.realtimeSinceStartup - testTime);
                }

                if (fixedFrameCounter == 150)
                {
                    fixedFrameCounter = 0;
                    if (arrowCounter < taskOrder.Length)
                    {
                        arrow = taskOrder[arrowCounter];
                        //testTime = Time.realtimeSinceStartup;

                        whiteCircle.SetActive(false);
                        Arrows[arrow].SetActive(true);
                        activeArrow = true;
                        initTime = Time.realtimeSinceStartupAsDouble;
                        writeState = RunNITrigger(2);
                        if (arrowCounter > 0)
                        {
                            if (stopSignalTrial)
                            {
                                FileManager.StoreDataInBuffer(arrowCounter, stopSignalTrial ? 1 : 0, taskOrder[arrowCounter - 1], stopSignalsTime[stopSignalCounter] * 10,
                                    matchTrial ? 1 : 0, missTrial ? 1 : 0, wrongKey ? 1 : 0, reactionTime);
                            }
                            else
                            {
                                FileManager.StoreDataInBuffer(arrowCounter, stopSignalTrial ? 1 : 0, taskOrder[arrowCounter - 1], -1,
                                    matchTrial ? 1 : 0, missTrial ? 1 : 0, wrongKey ? 1 : 0, reactionTime);
                            }
                        }

                        if (arrowCounter == taskStopSignals[stopSignalCounter])
                        {
                            activeStopSignal = true;
                            if (stopSignalCounter < taskStopSignals.Length - 1)
                                stopSignalCounter++;
                            validTime = false;
                            stopSignalTrial = true;
                        }
                        else
                        {
                            activeStopSignal = false;

                            validTime = true;
                            stopSignalTrial = false;
                        }
                        reactionTime = 0;
                    }
                    if (arrowCounter == taskOrder.Length)
                    {
                        if (stopSignalTrial)
                        {
                            FileManager.StoreDataInBuffer(arrowCounter, stopSignalTrial ? 1 : 0, taskOrder[arrowCounter - 1], stopSignalsTime[stopSignalCounter] * 10,
                                matchTrial ? 1 : 0, missTrial ? 1 : 0, wrongKey ? 1 : 0, reactionTime);
                        }
                        else
                        {
                            FileManager.StoreDataInBuffer(arrowCounter, stopSignalTrial ? 1 : 0, taskOrder[arrowCounter - 1], -1,
                                matchTrial ? 1 : 0, missTrial ? 1 : 0, wrongKey ? 1 : 0, reactionTime);
                        }
                    }
                    arrowCounter++;
                    alreadyPressed = false;
                    matchTrial = false;
                    missTrial = false;
                    wrongKey = false;
                }
                else if (fixedFrameCounter < 150)
                    fixedFrameCounter++;
            }
            else
            {
                Arrows[arrow].SetActive(false);
                activeArrow = false;
                validTime = false;
                alreadyPressed = false;
                correctMarker.SetActive(false);
                incorrectMarker.SetActive(false);
                whiteCircle.SetActive(false);
                finishSign.SetActive(true);
                FileManager.WriteData();
                taskFinished = true;
                task = false;
            }
        }
    }

    private void ArrowOrder()
    {
        warmUpOrder = new int[nWarmUpRepetitions];
        taskOrder = new int[nTaskRepetitions];
        taskStopSignals = new int[10]; // 10 señales de parada
        stopSignalsTime = new int[10];

        // Se genera el orden aleatorio de las flechas para las dos etapas
        for (int i = 0; i < warmUpOrder.Length; i++)
        {
            warmUpOrder[i] = Random.Range(0, 2);
            //Debug.Log(warmUpOrder[i]);
        }

        for (int i = 0; i < taskOrder.Length; i++)
            taskOrder[i] = Random.Range(0, 2);
        for (int i = 0; i < taskStopSignals.Length; i++)
            taskStopSignals[i] = nTaskRepetitions;

        // Se agregan señales de parada para cumplir con lo requerido
        int randomPosition;

        for (int i = 0; i < 10; i++)
        {
            randomPosition = Random.Range(0, nTaskRepetitions);

            stopSignalsTime[i] = Random.Range(10, 46); // Entre 100 y 450 ms

            while (noRepetition == true)
            {
                noRepetition = false;
                for (int j = 0; j < 10; j++)
                {
                    if (randomPosition == taskStopSignals[j])
                    {
                        randomPosition = Random.Range(0, nTaskRepetitions);
                        noRepetition = true;
                    }
                }
            }
            noRepetition = true;
            taskStopSignals[i] = randomPosition;
        }
        System.Array.Sort(taskStopSignals);
    }
    public bool RunNITrigger(int trigger)
    {
        bool status = false;

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
            case 3:
                status = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 0, 0, 1, 1, 1, 1, 1, 1 }, ref numWritten);
                break;
        }
        //fileManager.StoreTrigger(trigger);
        return status;
    }
}

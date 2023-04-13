using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using Janelia;

public class ColorComparisonManager : MonoBehaviour
{
    public GameObject[] LeftSideSquares = { null, null, null };
    public GameObject[] RightSideSquares = { null, null, null };
    public GameObject rightArrow;
    public GameObject leftArrow;
    public GameObject taskInit; // Cartel de inicio del registro
    public GameObject finishSign; // Cartel de finalización
    public GameObject center;

    public FileManagerColorComparison FileManager;

    public Canvas canvas;

    private Quaternion zeroRotation = Quaternion.identity;

    private Vector3[] leftPositions;
    private Vector3[] rightPositions;

    private List<int> leftSquareOrder;
    private List<int> leftPositionsUsed;
    private List<int> rightSquareOrder;
    private List<int> rightPositionsUsed;

    private int score = 0; // 1 -> correcto; 0 -> incorrecto

    private bool randomTrial = false; // Pasado a int, 1 -> coincide; 0 -> no coincide
    private int randomTrials = 0;
    private int notRandomTrials = 0;

    private int squaresQuantity = 2;
    private int randNumber;

    private bool leftSide = true; // true -> lado izquierdo. false -> lado derecho

    private bool task = false;
    private int taskNumber = 0;
    private bool duringTask = false;

    private bool comparisonMade = false;
    private bool comparisonWindow = false;

    public int totalRepetitions = 10;
    private int rightRepetitions = 0;
    private int leftRepetitions = 0;

    //private int fixedFrameCounter = 0;

    // Entrenamiento
    private bool training = false;

    // Salida digital
    NiDaqMx.DigitalOutputParams[] digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int lines = 7; // Líneas digitales a escribir
    //private int frameCounterNI = 0;

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
        rightArrow.SetActive(false);
        leftArrow.SetActive(false);
        taskInit.SetActive(true);
        finishSign.SetActive(false);
        center.SetActive(false);

        leftPositions = new Vector3[9];
        rightPositions = new Vector3[9];

        for (int i = 0; i < LeftSideSquares.Length; i++)
        {
            LeftSideSquares[i].SetActive(false);
            RightSideSquares[i].SetActive(false);

            leftPositions[i] = LeftSideSquares[i].transform.position;
            rightPositions[i] = RightSideSquares[i].transform.position;
        }

        leftSquareOrder = new List<int>();
        leftPositionsUsed = new List<int>();
        rightSquareOrder = new List<int>();
        rightPositionsUsed = new List<int>();

        training = PlayerPrefs.GetInt("Training", 0) == 1; // if true

        if (training)
            totalRepetitions = 2;

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

        Time.fixedDeltaTime = 0.01F;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !task)
        {
            taskInit.SetActive(false);
            task = true;
            center.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (writeState)
            {
                for (int i = 0; i < 8; i++)
                    NiDaqMx.ClearOutputTask(digitalOutputParams[i]);

                NiDaqMx.ClearInputTask(digitalInputParams);
            }

            SceneManager.LoadSceneAsync(0); // Salir del test al menú inicial
        }


        // Entrada digital es 257 en alto, 1 para click izquierdo y 256 para click derecho
        if (readDigitalData[0] == 1 && !comparisonMade && comparisonWindow)
        {
            if (!randomTrial)
                score = 1;
            comparisonMade = true;
            DeactivateSquares();
        }

        if (readDigitalData[0] == 256 && !comparisonMade && comparisonWindow)
        {
            if (randomTrial)
                score = 1;
            comparisonMade = true;
            DeactivateSquares();
        }
    }

    void FixedUpdate()
    {
        if (task)
            _ = NiDaqMx.ReadFromDigitalInput(digitalInputParams, ref readDigitalData, ref numReadPerChannel, ref numBytesPerSamp);

        if (task && !duringTask && squaresQuantity <= 6)
            StartCoroutine(TaskGenerator());

        if (comparisonMade && comparisonWindow)
            TrialReset();

        if (squaresQuantity > 6 && task)
            TaskEnd();
    }

    IEnumerator TaskGenerator()
    {
        duringTask = true;

        yield return new WaitForSeconds((float)0.5);
        TrialInit();

        yield return new WaitForSeconds((float)1.1);
        TrialFirstSquares();

        yield return new WaitForSeconds((float)0.25);
        TrialHideSquares();
        yield return new WaitForSeconds((float)1.3);
        TrialSecondSquares();
    }

    private void TrialInit()
    {
        if ((rightRepetitions + leftRepetitions) == totalRepetitions)
        {
            squaresQuantity += 2;
            rightRepetitions = 0;
            leftRepetitions = 0;
            randomTrials = 0;
        }

        if (rightRepetitions == 5)
            leftSide = true;
        else if (leftRepetitions == 5)
            leftSide = false;
        else
            leftSide = System.Convert.ToBoolean(Random.Range(0, 2));

        if (leftSide)
        {
            leftRepetitions++;
            leftArrow.SetActive(true);
        }
        else
        {
            rightRepetitions++;
            rightArrow.SetActive(true);
        }

        GenerateSquareOrder();
        GenerateSquarePositions();
    }

    private void TrialFirstSquares()
    {
        if (leftSide)
            leftArrow.SetActive(false);
        else
            rightArrow.SetActive(false);

        ActivateSquares();

        writeState = RunNITrigger(1, lines);
    }

    private void TrialHideSquares()
    {
        DeactivateSquares();

        if (randomTrials == (int)(totalRepetitions / 2))
            randomTrial = false;
        else if (notRandomTrials == (int)(totalRepetitions / 2))
            randomTrial = true;
        else
            randomTrial = System.Convert.ToBoolean(Random.Range(0, 2));

        if (randomTrial)
        {
            randomTrials++;

            GenerateSquareOrder(Random.Range(1, squaresQuantity + 1), false);
        }
        else
            notRandomTrials++;
    }

    private void TrialSecondSquares()
    {
        ActivateSquares();

        comparisonMade = false;
        comparisonWindow = true;

        taskNumber++;
    }

    private void TrialReset()
    {
        TrialResetTrigger();

        // Guardar datos
        if (!training)
            FileManager.StoreDataInBuffer(taskNumber, squaresQuantity, leftSide ? 1 : 0, randomTrial ? 1 : 0, score);

        score = 0;
        //fixedFrameCounter = 0;
        comparisonWindow = false;
        leftSquareOrder.Clear();
        leftPositionsUsed.Clear();
        rightSquareOrder.Clear();
        rightPositionsUsed.Clear();

        duringTask = false;
    }

    private void TaskEnd()
    {
        task = false;
        finishSign.SetActive(true);
        center.SetActive(false);
        rightArrow.SetActive(false);
        leftArrow.SetActive(false);

        if (!training)
            FileManager.WriteData();

        StartCoroutine(TaskExit());
    }

    IEnumerator TaskExit()
    {
        //yield on a new YieldInstruction that waits for 3 seconds.
        yield return new WaitForSeconds(3);

        for (int i = 0; i < 8; i++)
            NiDaqMx.ClearOutputTask(digitalOutputParams[i]);
        NiDaqMx.ClearInputTask(digitalInputParams);

        SceneManager.LoadScene(0); // Salir del test al menú inicial
    }

    private void GenerateSquareOrder(int squaresAmount = 0, bool listsCleared = true)
    {
        if (squaresAmount == 0)
            squaresAmount = squaresQuantity;

        // Generar la combinación de cuadrados random
        for (int i = 0; i < squaresAmount; i++)
        {
            // Lado izquierdo
            randNumber = Random.Range(0, 9);
            while (leftSquareOrder.Contains(randNumber))
            {
                randNumber = Random.Range(0, 9);
            }
            if (listsCleared)
                leftSquareOrder.Add(randNumber);
            else
            {
                int insertIndex = Random.Range(0, leftSquareOrder.Count);
                leftSquareOrder.RemoveAt(insertIndex);
                leftSquareOrder.Insert(insertIndex, randNumber);
            }

            // Lado derecho
            randNumber = Random.Range(0, 9);
            while (rightSquareOrder.Contains(randNumber))
            {
                randNumber = Random.Range(0, 9);
            }
            if (listsCleared)
                rightSquareOrder.Add(randNumber);
            else
            {
                int insertIndex = Random.Range(0, rightSquareOrder.Count);
                rightSquareOrder.RemoveAt(insertIndex);
                rightSquareOrder.Insert(insertIndex, randNumber);
            }
        }
    }

    private void GenerateSquarePositions()
    {
        // Generar la posición de cuadrados random
        for (int i = 0; i < squaresQuantity; i++)
        {
            // Lado izquierdo         
            randNumber = Random.Range(0, 9);
            while (leftPositionsUsed.Contains(randNumber))
            {
                randNumber = Random.Range(0, 9);
            }
            leftPositionsUsed.Add(randNumber);

            // Lado derecho
            randNumber = Random.Range(0, 9);

            while (rightPositionsUsed.Contains(randNumber))
            {
                randNumber = Random.Range(0, 9);
            }
            rightPositionsUsed.Add(randNumber);

        }
    }

    private void ActivateSquares()
    {
        // Activar cuadrados
        for (int i = 0; i < squaresQuantity; i++)
        {
            // Lado izquierdo
            LeftSideSquares[leftSquareOrder[i]].SetActive(true);
            LeftSideSquares[leftSquareOrder[i]].transform.SetPositionAndRotation(leftPositions[leftPositionsUsed[i]], zeroRotation);


            // Lado derecho
            RightSideSquares[rightSquareOrder[i]].SetActive(true);
            RightSideSquares[rightSquareOrder[i]].transform.SetPositionAndRotation(rightPositions[rightPositionsUsed[i]], zeroRotation);
        }
    }

    private void DeactivateSquares()
    {
        for (int i = 0; i < squaresQuantity; i++)
        {
            LeftSideSquares[leftSquareOrder[i]].SetActive(false);
            RightSideSquares[rightSquareOrder[i]].SetActive(false);
        }
    }

    private void TrialResetTrigger()
    {
        if (leftSide)
        {
            if (score == 1)
                writeState = RunNITrigger(3, lines);
            else
                writeState = RunNITrigger(2, lines);
        }
        else
        {
            if (score == 1)
                writeState = RunNITrigger(5, lines);
            else
                writeState = RunNITrigger(4, lines);
        }
    }

    IEnumerator TriggerPulseWidth()
    {
        yield return new WaitForSecondsRealtime((float)0.01);
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
                message = new uint[] { 1, 0, 1, 1, 1, 1, 1, 1 }; // Trial izquierdo e incorrecto
                break;
            case 3:
                message = new uint[] { 1, 1, 0, 1, 1, 1, 1, 1 }; // Trial izquierdo y correcto
                break;
            case 4:
                message = new uint[] { 1, 1, 1, 0, 1, 1, 1, 1 }; // Trial derecho e incorrecto
                break;
            case 5:
                message = new uint[] { 1, 1, 1, 1, 0, 1, 1, 1 }; // Trial derecho y correcto
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

        if (trigger != 0)
            StartCoroutine(TriggerPulseWidth());

        return status;
    }
}

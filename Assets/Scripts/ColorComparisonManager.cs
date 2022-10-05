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

    private int squaresQuantity = 2;
    private int randNumber;

    private bool leftSide = true; // true -> lado izquierdo. false -> lado derecho

    private bool task = false;
    private int taskNumber = 0;

    private bool comparisonMade = false;
    private bool comparisonWindow = false;

    public int totalRepetitions = 10;
    private int rightRepetitions = 0;
    private int leftRepetitions = 0;

    private int fixedFrameCounter = 0;

    NiDaqMx.DigitalOutputParams digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int frameCounterNI = 0;


    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1366, 768, true);
        Application.targetFrameRate = 144;

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

        // Inicializa variables NI
        digitalOutputParams = new NiDaqMx.DigitalOutputParams();
        _ = NiDaqMx.CreateDigitalOutput(digitalOutputParams);
        writeState = NiDaqMx.WriteDigitalValue(digitalOutputParams, new uint[] { 1, 1, 1, 1, 1, 1, 1, 1 }, ref numWritten);

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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 6); // Salir del test al menú inicial

        if (writeState)
        {
            frameCounterNI++;

            if (writeState && frameCounterNI == 7)
            {
                writeState = RunNITrigger(0);
                frameCounterNI = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.S) && !comparisonMade && comparisonWindow)
        {
            if (!randomTrial)
                score = 1;
            comparisonMade = true;
            DeactivateSquares();
        }

        if (Input.GetKeyDown(KeyCode.N) && !comparisonMade && comparisonWindow)
        {
            if (randomTrial)
                score = 1;
            comparisonMade = true;
            DeactivateSquares();
        }
    }


    void FixedUpdate()
    {
        if (task) // CAMBIAR CONDICIÓN
            fixedFrameCounter++;

        if (squaresQuantity <= 6)
        {
            if (fixedFrameCounter == 50)
                TrialInit();

            if (fixedFrameCounter == 160)
                TrialFirstSquares();

            if (fixedFrameCounter == 185)
                TrialHideSquares();

            if (fixedFrameCounter == 315)
                TrialSecondSquares();

            if (comparisonMade && comparisonWindow) // CAMBIAR CONDICIÓN
                TrialReset();
        }
        if (squaresQuantity > 6 && task)
            TaskEnd();
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

        writeState = RunNITrigger(1);
    }

    private void TrialHideSquares()
    {
        DeactivateSquares();

        if (randomTrials == (int)(totalRepetitions / 2))
            randomTrial = false;
        else
            randomTrial = System.Convert.ToBoolean(Random.Range(0, 2));

        if (randomTrial)
        {
            randomTrials++;

            GenerateSquareOrder(Random.Range(1, squaresQuantity + 1), false);
        }
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
        writeState = RunNITrigger(2);

        // Guardar datos
        FileManager.StoreDataInBuffer(taskNumber, squaresQuantity, leftSide ? 1 : 0, randomTrial ? 1 : 0, score);

        score = 0;
        fixedFrameCounter = 0;
        comparisonWindow = false;
        leftSquareOrder.Clear();
        leftPositionsUsed.Clear();
        rightSquareOrder.Clear();
        rightPositionsUsed.Clear();
    }

    private void TaskEnd()
    {
        task = false;
        finishSign.SetActive(true);
        center.SetActive(false);
        rightArrow.SetActive(false);
        leftArrow.SetActive(false);
        FileManager.WriteData();
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
        return status;
    }
}

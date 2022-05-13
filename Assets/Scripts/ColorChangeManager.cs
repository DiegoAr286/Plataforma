using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using Janelia;

public class ColorChangeManager : MonoBehaviour
{
    public GameObject[] LeftSideSquares = { null, null, null };
    public GameObject[] RightSideSquares = { null, null, null };
    public GameObject rightArrow;
    public GameObject leftArrow;
    public GameObject taskInit; // Cartel de inicio del registro
    public GameObject finishSign; // Cartel de finalización
    public GameObject center;

    public FileManagerColorChangeDetection FileManager;

    private Quaternion zeroRotation = Quaternion.identity;

    private Vector3[] leftPositions;
    private Vector3[] rightPositions;

    private List<int> leftSquareOrder;
    private List<int> leftPositionsUsed;
    private List<int> rightSquareOrder;
    private List<int> rightPositionsUsed;

    private int score = 0;
    private int mistakes = 0;

    private int squaresQuantity = 2;
    private int randNumber;

    private bool leftSide = true; // true -> lado izquierdo. false -> lado derecho

    private bool task = false;
    private int taskNumber = 0;

    public int totalRepetitions = 10;
    private int rightRepetitions = 0;
    private int leftRepetitions = 0;

    private int fixedFrameCounter = 0;

    NiDaqMx.DigitalOutputParams digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int frameCounterNI = 0;

    private bool continueClick = true; // Click para continuar con la tarea


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

            EnableButtons(LeftSideSquares[i].transform.GetChild(0).GetComponentsInChildren<Button>(), false);
            EnableButtons(RightSideSquares[i].transform.GetChild(0).GetComponentsInChildren<Button>(), false);
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 5); // Salir del test al menú inicial

        if (writeState)
        {
            frameCounterNI++;

            if (writeState && frameCounterNI == 7)
            {
                writeState = RunNITrigger(0);
                frameCounterNI = 0;
            }
        }

        if (Input.GetMouseButtonDown(0))
                continueClick = true;
    }


    void FixedUpdate()
    {
        if (task && continueClick)
            fixedFrameCounter++;

        if (squaresQuantity <= 6)
        {
            if ((score + mistakes) == squaresQuantity)
            {
                //Debug.Log(score);

                writeState = RunNITrigger(2);

                // Guardar datos
                FileManager.StoreDataInBuffer(taskNumber, squaresQuantity, leftSide ? 1 : 0, score, mistakes);

                for (int i = 0; i < squaresQuantity; i++)
                {
                    LeftSideSquares[leftSquareOrder[i]].SetActive(false);
                    RightSideSquares[rightSquareOrder[i]].SetActive(false);

                    EnableButtons(LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Button>(), false);
                    EnableButtons(RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Button>(), false);
                }

                score = 0;
                mistakes = 0;
                fixedFrameCounter = 0;
                continueClick = false;
                leftSquareOrder.Clear();
                leftPositionsUsed.Clear();
                rightSquareOrder.Clear();
                rightPositionsUsed.Clear();
            }

            if (fixedFrameCounter == 50)
            {
                if ((rightRepetitions + leftRepetitions) == totalRepetitions)
                {
                    squaresQuantity++;
                    rightRepetitions = 0;
                    leftRepetitions = 0;
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
            }

            if (fixedFrameCounter == 160)
            {
                if (leftSide)
                    leftArrow.SetActive(false);
                else
                    rightArrow.SetActive(false);

                // Generar la combinación de cuadrados random
                for (int i = 0; i < squaresQuantity; i++)
                {
                    // Lado izquierdo
                    randNumber = Random.Range(0, 9);
                    while (leftSquareOrder.Contains(randNumber))
                    {
                        randNumber = Random.Range(0, 9);
                    }
                    leftSquareOrder.Add(randNumber);

                    randNumber = Random.Range(0, 9);
                    while (leftPositionsUsed.Contains(randNumber))
                    {
                        randNumber = Random.Range(0, 9);
                    }
                    leftPositionsUsed.Add(randNumber);

                    LeftSideSquares[leftSquareOrder[i]].SetActive(true);
                    LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(false);
                    LeftSideSquares[leftSquareOrder[i]].transform.SetPositionAndRotation(leftPositions[leftPositionsUsed[i]], zeroRotation);


                    // Lado derecho
                    randNumber = Random.Range(0, 9);
                    while (rightSquareOrder.Contains(randNumber))
                    {
                        randNumber = Random.Range(0, 9);
                    }
                    rightSquareOrder.Add(randNumber);

                    randNumber = Random.Range(0, 9);
                    while (rightPositionsUsed.Contains(randNumber))
                    {
                        randNumber = Random.Range(0, 9);
                    }
                    rightPositionsUsed.Add(randNumber);

                    RightSideSquares[rightSquareOrder[i]].SetActive(true);
                    RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(false);
                    RightSideSquares[rightSquareOrder[i]].transform.SetPositionAndRotation(rightPositions[rightPositionsUsed[i]], zeroRotation);


                    if (leftSide)
                        EnableButtons(LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Button>(), true);
                    else
                        EnableButtons(RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Button>(), true);
                }

                writeState = RunNITrigger(1);
            }

            if (fixedFrameCounter == 185)
            {
                for (int i = 0; i < squaresQuantity; i++)
                {
                    LeftSideSquares[leftSquareOrder[i]].SetActive(false);
                    RightSideSquares[rightSquareOrder[i]].SetActive(false);
                }
            }

            if (fixedFrameCounter == 315)
            {
                for (int i = 0; i < squaresQuantity; i++)
                {
                    // Lado izquierdo
                    LeftSideSquares[leftSquareOrder[i]].SetActive(true);
                    LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(true);


                    // Lado derecho
                    RightSideSquares[rightSquareOrder[i]].SetActive(true);
                    RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(true);
                }

                taskNumber++;
            }
        }

        if (squaresQuantity == 7 && task)
        {
            task = false;
            finishSign.SetActive(true);
            center.SetActive(false);
            rightArrow.SetActive(false);
            leftArrow.SetActive(false);
            FileManager.WriteData();
        }
    }



    public void CheckColor()
    {
        string squareName = EventSystem.current.currentSelectedGameObject.transform.parent.transform.parent.name;
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        Button[] buttons = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponentsInChildren<Button>();

        EnableButtons(buttons, false);
        switch (buttonName)
        {
            case "BlackButton":
                if (squareName == "BlackSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "RedButton":
                if (squareName == "RedSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "GreenButton":
                if (squareName == "GreenSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "BlueButton":
                if (squareName == "BlueSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "YellowButton":
                if (squareName == "YellowSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "CyanButton":
                if (squareName == "CyanSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "MagentaButton":
                if (squareName == "MagentaSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "OrangeButton":
                if (squareName == "OrangeSquare")
                    score++;
                else
                    mistakes++;
                break;
            case "WhiteButton":
                if (squareName == "WhiteSquare")
                    score++;
                else
                    mistakes++;
                break;
            default:
                Debug.Log("clicked");
                break;
        }
        //Debug.Log("clicked");

    }

    void EnableButtons(Button[] buttons, bool enable)
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].enabled = enable;
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

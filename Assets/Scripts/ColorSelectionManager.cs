using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using Janelia;

public class ColorSelectionManager : MonoBehaviour
{
    public GameObject[] LeftSideSquares = { null, null, null };
    public GameObject[] RightSideSquares = { null, null, null };
    public GameObject rightArrow;
    public GameObject leftArrow;
    public GameObject taskInit; // Cartel de inicio del registro
    public GameObject finishSign; // Cartel de finalización
    public GameObject center;

    public FileManagerColorSelection FileManager;

    public Canvas canvas;

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
    private bool duringTask = false;

    public int totalRepetitions = 10;
    private int rightRepetitions = 0;
    private int leftRepetitions = 0;

    //private int fixedFrameCounter = 0;

    // Entrenamiento
    private bool training = false;

    NiDaqMx.DigitalOutputParams[] digitalOutputParams; // Parámetros NI
    private bool writeState = false;
    private int numWritten = 0;
    private int lines = 7; // Líneas digitales a escribir
    //private int frameCounterNI = 0;

    //private bool continueClick = true; // Click para continuar con la tarea

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

            EnableButtons(LeftSideSquares[i].transform.GetChild(0).GetComponentsInChildren<Button>(), false);
            EnableButtons(RightSideSquares[i].transform.GetChild(0).GetComponentsInChildren<Button>(), false);
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
            }

            SceneManager.LoadScene(0); // Salir del test al menú inicial
        }

        //if (Input.GetMouseButtonDown(0))
        //    continueClick = true;
    }

    void FixedUpdate()
    {
        if (task && !duringTask && squaresQuantity <= 6)
            StartCoroutine(TaskGenerator());

        if ((score + mistakes) == squaresQuantity)
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

    private void TrialFirstSquares()
    {
        if (leftSide)
            leftArrow.SetActive(false);
        else
            rightArrow.SetActive(false);

        GenerateSquareOrder();
        GenerateSquarePositions();
        ActivateSquares();

        writeState = RunNITrigger(1, lines);
    }

    private void TrialHideSquares()
    {
        DeactivateSquares();
    }

    private void TrialSecondSquares()
    {
        ActivateButtons();

        taskNumber++;
    }

    private void TrialReset()
    {
        TrialResetTrigger();

        // Guardar datos
        if (!training)
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
        //fixedFrameCounter = 0;
        //continueClick = false;
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

        SceneManager.LoadScene(0); // Salir del test al menú inicial
    }

    private void GenerateSquareOrder()
    {
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

            // Lado derecho
            randNumber = Random.Range(0, 9);
            while (rightSquareOrder.Contains(randNumber))
            {
                randNumber = Random.Range(0, 9);
            }
            rightSquareOrder.Add(randNumber);
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
            LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(false);
            LeftSideSquares[leftSquareOrder[i]].transform.SetPositionAndRotation(leftPositions[leftPositionsUsed[i]], zeroRotation);


            // Lado derecho
            RightSideSquares[rightSquareOrder[i]].SetActive(true);
            RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(false);
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

    private void ActivateButtons()
    {
        for (int i = 0; i < squaresQuantity; i++)
        {
            // Lado izquierdo
            LeftSideSquares[leftSquareOrder[i]].SetActive(true);
            LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(true);


            // Lado derecho
            RightSideSquares[rightSquareOrder[i]].SetActive(true);
            RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).gameObject.SetActive(true);


            if (leftSide)
            {
                EnableButtons(LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Button>(), true);
                EnableButtonsImages(LeftSideSquares[leftSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Image>(), false);
            }
            else
            {
                EnableButtons(RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Button>(), true);
                EnableButtonsImages(RightSideSquares[rightSquareOrder[i]].transform.GetChild(0).GetComponentsInChildren<Image>(), false);
            }
        }
    }

    public void CheckColor()
    {
        string squareName = EventSystem.current.currentSelectedGameObject.transform.parent.transform.parent.name;
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        Button[] buttons = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponentsInChildren<Button>();
        Image[] images = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponentsInChildren<Image>();

        EnableButtons(buttons, false);
        EnableButtonsImages(images, true);

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
    }

    void EnableButtons(Button[] buttons, bool enable)
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].enabled = enable;
    }

    void EnableButtonsImages(Image[] images, bool enable)
    {

        if (enable)
        {
            images[0].color = Color.black;
            images[1].color = new Color32(255, 0, 0, 255);
            images[2].color = new Color32(0, 255, 0, 255);
            images[3].color = new Color32(0, 0, 255, 255);
            images[4].color = new Color32(255, 255, 0, 255);
            images[5].color = new Color32(0, 255, 255, 255);
            images[6].color = new Color32(255, 0, 255, 255);
            images[7].color = Color.white;
            images[8].color = new Color32(255, 128, 0, 255);

        }
        else
        {
            for (int i = 0; i < images.Length; i++)
                images[i].color = Color.white;
        }
    }

    private void TrialResetTrigger()
    {
        if (leftSide)
        {
            if (score == squaresQuantity)
                writeState = RunNITrigger(3, lines);
            else
                writeState = RunNITrigger(2, lines);
        }
        else
        {
            if (score == squaresQuantity)
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

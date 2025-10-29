using UnityEngine;

public class Blade : MonoBehaviour
{
    private Camera mainCamera;
    private Collider bladeCollider;
    private TrailRenderer bladeTrail;
    private bool slicing;

    private Vector3 oldPosition;

    public Vector3 direction { get; private set; }
    public float sliceForce = 5f;
    public float minSliceVelocity = 0.01f;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        mainCamera = Camera.main;
        bladeCollider = GetComponent<Collider>();
        bladeTrail = GetComponentInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        StopSlicing();
    }

    private void OnDisable()
    {
        StopSlicing();
    }

    private void Start()
    {
        StartSlicing();
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    StartSlicing();
        //}
        //else if (Input.GetMouseButtonUp(0))
        //{
        //    StopSlicing();
        //}
        //else if (slicing)
        //{
        ContinueSlicing();
        //}
    }

    private void StartSlicing()
    {
        //Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        //newPosition.z = 0f;

        //transform.position = newPosition;

        oldPosition = transform.position;

        slicing = true;
        bladeCollider.enabled = true;
        bladeTrail.enabled = true;
        bladeTrail.Clear();
    }

    private void StopSlicing()
    {
        slicing = false;
        bladeCollider.enabled = false;
        bladeTrail.enabled = false;
    }

    private void ContinueSlicing()
    {
        //Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        //newPosition.z = 0f;

        //direction = newPosition - transform.position;

        direction = transform.position - oldPosition;

        float velocity = direction.magnitude / Time.deltaTime;
        bladeCollider.enabled = velocity > minSliceVelocity;

        oldPosition = transform.position;

        //transform.position = newPosition;
    }
}

using UnityEngine;

public class FloorPlanCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Orbit")]
    [SerializeField] private float orbitSpeed = 5f;
    [SerializeField] private float minVerticalAngle = 10f;
    [SerializeField] private float maxVerticalAngle = 85f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 100f;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 0.5f;

    [Header("Smoothing")]
    [SerializeField] private float movementSmoothness = 12f;

    private float yaw;
    private float pitch;

    private float targetDistance;
    private float currentDistance;

    private Vector3 targetPosition;

    private bool controlsEnabled = false;



    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning(
                "[FloorPlanCameraController] " +
                "No target assigned yet."
            );

            return;
        }

        SynchronizeWithCamera();
    }




    private void Update()
    {
        if (!controlsEnabled || target == null)
            return;

        HandleOrbit();

        HandleZoom();

        HandlePan();

        UpdateCamera();
    }


    
    public void SynchronizeWithCamera()
    {
        if (target == null)
            return;


        

        Vector3 rawOffset =
            transform.position -
            target.position;

        currentDistance =
            rawOffset.magnitude;

        targetDistance =
            currentDistance;


       

        Vector3 angles =
            transform.eulerAngles;


        yaw =
            angles.y;

        pitch =
            angles.x;

       
        if (pitch > 180f)
        {
            pitch -= 360f;
        }


       

        Quaternion reconstructedRotation =
            Quaternion.Euler(pitch, yaw, 0f);

        Vector3 reconstructedForward =
            reconstructedRotation * Vector3.forward;

        targetPosition =
            transform.position +
            reconstructedForward * currentDistance;
    }


  

    private void HandleOrbit()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX =
                Input.GetAxis("Mouse X");

            float mouseY =
                Input.GetAxis("Mouse Y");


            yaw +=
                mouseX *
                orbitSpeed;


            pitch -=
                mouseY *
                orbitSpeed;


            pitch =
                Mathf.Clamp(
                    pitch,
                    minVerticalAngle,
                    maxVerticalAngle
                );
        }
    }


  

    private void HandleZoom()
    {
        float scroll =
            Input.GetAxis(
                "Mouse ScrollWheel"
            );


        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetDistance -=
                scroll *
                zoomSpeed;


            targetDistance =
                Mathf.Clamp(
                    targetDistance,
                    minDistance,
                    maxDistance
                );
        }
    }




    private void HandlePan()
    {
        if (Input.GetMouseButton(2))
        {
            float mouseX =
                Input.GetAxis("Mouse X");


            float mouseY =
                Input.GetAxis("Mouse Y");


            Vector3 right =
                transform.right;


            Vector3 up =
                transform.up;


            targetPosition -=
                right *
                mouseX *
                panSpeed;


            targetPosition -=
                up *
                mouseY *
                panSpeed;
        }
    }




    private void UpdateCamera()
    {
        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );


        Vector3 direction =
            rotation *
            Vector3.forward;


        Vector3 desiredPosition =
            targetPosition -
            direction *
            targetDistance;


        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                Time.deltaTime *
                movementSmoothness
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotation,
                Time.deltaTime *
                movementSmoothness
            );
    }


    

    public void EnableControls()
    {
        

        SynchronizeWithCamera();

        controlsEnabled = true;


        Debug.Log(
            "[FloorPlanCameraController] " +
            "Camera controls enabled."
        );
    }


  

    public void DisableControls()
    {
        controlsEnabled = false;
    }



    public void SetTarget(
        Transform newTarget
    )
    {
        target =
            newTarget;


        if (target != null)
        {
            targetPosition =
                target.position;

            SynchronizeWithCamera();
        }
    }
}
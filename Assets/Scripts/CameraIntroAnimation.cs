using System.Collections;
using UnityEngine;

public class CameraIntroAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float animationDuration = 3f;

public float AnimationDuration
{
    get { return animationDuration; }
}

    [SerializeField] private float startingHeightMultiplier = 2.5f;

    [SerializeField] private float startingDistanceMultiplier = 1.5f;

    [SerializeField] private float finalDistanceMultiplier = 1.2f;

    [Header("Camera Angle")]
    [SerializeField] private float finalHeightMultiplier = 0.8f;

    private Camera cam;


    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            cam = Camera.main;
        }
    }


   
    public void PlayIntro(Bounds floorPlanBounds)
    {
        StopAllCoroutines();

        StartCoroutine(
            AnimateCamera(floorPlanBounds)
        );
    }


    private IEnumerator AnimateCamera(
        Bounds bounds
    )
    {
        

        Vector3 center =
            bounds.center;

        float largestSize =
            Mathf.Max(
                bounds.size.x,
                bounds.size.z
            );


      

        float finalDistance =
            largestSize *
            finalDistanceMultiplier;


        finalDistance =
            Mathf.Max(
                finalDistance,
                5f
            );


       

        Vector3 finalPosition =
            center +
            new Vector3(
                0f,
                largestSize *
                finalHeightMultiplier,
                -finalDistance
            );


       

        Vector3 startPosition =
            center +
            new Vector3(
                0f,
                largestSize *
                startingHeightMultiplier,
                -finalDistance *
                startingDistanceMultiplier
            );


       

        Quaternion startRotation =
            LookAt(
                startPosition,
                center
            );


        Quaternion finalRotation =
            LookAt(
                finalPosition,
                center
            );


        

        cam.transform.position =
            startPosition;

        cam.transform.rotation =
            startRotation;


       

        float elapsed = 0f;


        while (elapsed < animationDuration)
        {
            elapsed +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    animationDuration
                );


          
            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            cam.transform.position =
                Vector3.Lerp(
                    startPosition,
                    finalPosition,
                    smoothT
                );


            cam.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    finalRotation,
                    smoothT
                );


            yield return null;
        }


      

        cam.transform.position =
            finalPosition;

        cam.transform.rotation =
            finalRotation;
    }


    private Quaternion LookAt(
        Vector3 cameraPosition,
        Vector3 target
    )
    {
        Vector3 direction =
            target -
            cameraPosition;


        if (direction == Vector3.zero)
        {
            return Quaternion.identity;
        }


        return Quaternion.LookRotation(
            direction.normalized
        );
    }
}
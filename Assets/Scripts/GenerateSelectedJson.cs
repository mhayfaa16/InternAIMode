using System.IO;
using UnityEngine;

public class GenerateSelectedJson : MonoBehaviour
{
    [Header("Generator")]
    [SerializeField] private FloorPlanGenerator generator;

    [Header("Camera")]
    [SerializeField] private CameraIntroAnimation cameraAnimation;
    [SerializeField] private FloorPlanCameraController cameraController;

    
    private Transform cameraPivot;


    private void Start()
    {
        Generate();
    }


    public void Generate()
    {
       

        if (SelectedJsonManager.Instance == null)
        {
            Debug.LogError(
                "[GenerateSelectedJson] " +
                "SelectedJsonManager was not found."
            );

            return;
        }


      

        string jsonPath =
            SelectedJsonManager.Instance.SelectedJsonPath;


        if (string.IsNullOrEmpty(jsonPath))
        {
            Debug.LogError(
                "[GenerateSelectedJson] " +
                "No JSON file was selected."
            );

            return;
        }


        

        if (!File.Exists(jsonPath))
        {
            Debug.LogError(
                "[GenerateSelectedJson] " +
                "JSON file no longer exists:\n" +
                jsonPath
            );

            return;
        }


       

        if (generator == null)
        {
            Debug.LogError(
                "[GenerateSelectedJson] " +
                "FloorPlanGenerator has not been assigned."
            );

            return;
        }



        string jsonText;

        try
        {
            jsonText =
                File.ReadAllText(jsonPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "[GenerateSelectedJson] " +
                "Could not read JSON:\n" +
                e.Message
            );

            return;
        }


  

        Debug.Log(
            "[GenerateSelectedJson] Generating from: " +
            Path.GetFileName(jsonPath)
        );


        FloorPlanGenerationResult result =
            generator.GenerateFromJsonText(
                jsonText,
                Path.GetFileName(jsonPath)
            );


       

        if (!result.success)
        {
            Debug.LogError(
                "[GenerateSelectedJson] " +
                "Generation failed: " +
                result.message
            );

            return;
        }


        Debug.Log(
            $"[GenerateSelectedJson] SUCCESS! " +
            $"Generated {result.wallCount} walls, " +
            $"{result.doorCount} doors, " +
            $"{result.windowCount} windows."
        );


       

        Transform generatedPlan =
            generator.transform.Find(
                "GeneratedFloorPlan"
            );


        if (generatedPlan == null)
        {
            Debug.LogError(
                "[GenerateSelectedJson] " +
                "Could not find GeneratedFloorPlan."
            );

            return;
        }


        

        Bounds floorPlanBounds =
            generator.GetGeneratedFloorPlanBounds();


   

        if (cameraPivot == null)
        {
            cameraPivot =
                new GameObject("CameraPivot").transform;
        }

        cameraPivot.position =
            floorPlanBounds.center;


      

        if (cameraController != null)
        {
            cameraController.SetTarget(
                cameraPivot
            );

            cameraController.DisableControls();
        }


      
        if (cameraAnimation != null)
        {
            Debug.Log(
                "[GenerateSelectedJson] " +
                "Starting camera intro."
            );

            cameraAnimation.PlayIntro(
                floorPlanBounds
            );
        }
        else
        {
            Debug.LogWarning(
                "[GenerateSelectedJson] " +
                "CameraIntroAnimation has not been assigned."
            );

          
            if (cameraController != null)
            {
                cameraController.EnableControls();
            }

            return;
        }



        if (cameraController != null)
        {
            StartCoroutine(
                EnableCameraControlsAfterIntro()
            );
        }
    }


 

    private System.Collections.IEnumerator
        EnableCameraControlsAfterIntro()
    {
   

        float waitTime =
            cameraAnimation.AnimationDuration;

        yield return new WaitForSeconds(
            waitTime
        );


        cameraController.EnableControls();


        Debug.Log(
            "[GenerateSelectedJson] " +
            "Camera controls enabled."
        );
    }
}
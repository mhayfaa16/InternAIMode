using UnityEngine;

public class ArchitecturalEnvironment : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private Color backgroundColor =
        new Color(0.025f, 0.03f, 0.04f);

    [SerializeField] private Color ambientColor =
        new Color(0.12f, 0.14f, 0.18f);

    [Header("Lighting")]
    [SerializeField] private Color mainLightColor =
        new Color(1f, 0.95f, 0.9f);

    [SerializeField] private float mainLightIntensity = 1.2f;

    [SerializeField] private Vector3 mainLightRotation =
        new Vector3(50f, -30f, 0f);

    [Header("Fog")]
    [SerializeField] private bool enableFog = true;

    [SerializeField] private Color fogColor =
        new Color(0.025f, 0.03f, 0.04f);

    [SerializeField] private float fogDensity = 0.008f;


    private void Awake()
    {
        SetupEnvironment();
        SetupLighting();
        SetupFog();
    }


   

    private void SetupEnvironment()
    {
       
        Camera.main.backgroundColor =
            backgroundColor;


       
        RenderSettings.ambientMode =
            UnityEngine.Rendering.AmbientMode.Flat;

        RenderSettings.ambientLight =
            ambientColor;
    }


  

    private void SetupLighting()
    {
      
        Light existingLight =
            FindFirstObjectByType<Light>();


       
        if (existingLight != null)
        {
            existingLight.type =
                LightType.Directional;

            existingLight.color =
                mainLightColor;

            existingLight.intensity =
                mainLightIntensity;

            existingLight.transform.rotation =
                Quaternion.Euler(
                    mainLightRotation
                );

            return;
        }


        GameObject lightObject =
            new GameObject(
                "Architectural Main Light"
            );


        Light light =
            lightObject.AddComponent<Light>();


        light.type =
            LightType.Directional;

        light.color =
            mainLightColor;

        light.intensity =
            mainLightIntensity;

        light.shadows =
            LightShadows.Soft;


        light.transform.rotation =
            Quaternion.Euler(
                mainLightRotation
            );
    }


   

    private void SetupFog()
    {
        RenderSettings.fog =
            enableFog;


        if (!enableFog)
            return;


        RenderSettings.fogMode =
            FogMode.Exponential;


        RenderSettings.fogColor =
            fogColor;


        RenderSettings.fogDensity =
            fogDensity;
    }
}
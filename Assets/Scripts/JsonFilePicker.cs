using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
//using SFB;

public class JsonFilePicker : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text dropText;
    [SerializeField] private TMP_Text browseHint;
    [SerializeField] private GameObject generateButton;

    private string selectedJsonPath;


   

    public void OpenJsonFilePicker()
    {
        var extensions = new[]
        {
            new ExtensionFilter("JSON Files", "json")
        };


        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Select JSON File",
            "",
            extensions,
            false
        );


        if (
            paths == null ||
            paths.Length == 0 ||
            string.IsNullOrEmpty(paths[0])
        )
        {
            return;
        }


        string selectedPath = paths[0];


      

        if (!File.Exists(selectedPath))
        {
            Debug.LogError(
                "Selected JSON file does not exist."
            );

            return;
        }



        selectedJsonPath = selectedPath;


        Debug.Log(
            "Selected JSON: " +
            selectedJsonPath
        );


      

        if (SelectedJsonManager.Instance != null)
        {
            SelectedJsonManager.Instance.SetJsonPath(
                selectedJsonPath
            );
        }
        else
        {
            Debug.LogError(
                "SelectedJsonManager was not found. " +
                "Make sure the SelectedJsonManager GameObject " +
                "exists in the menu scene."
            );

            return;
        }




        if (dropText != null)
        {
            dropText.text =
                Path.GetFileName(selectedJsonPath);
        }




        if (browseHint != null)
        {
            browseHint.gameObject.SetActive(false);
        }


    

        if (generateButton != null)
        {
            generateButton.SetActive(true);
        }
    }


   

    public void Generate3DModel()
    {
    

        if (string.IsNullOrEmpty(selectedJsonPath))
        {
            Debug.LogWarning(
                "No JSON file selected."
            );

            return;
        }


       

        if (!File.Exists(selectedJsonPath))
        {
            Debug.LogError(
                "Selected JSON file no longer exists: " +
                selectedJsonPath
            );

            return;
        }



        if (SelectedJsonManager.Instance == null)
        {
            Debug.LogError(
                "SelectedJsonManager was not found. " +
                "Make sure the SelectedJsonManager GameObject " +
                "exists in the menu scene."
            );

            return;
        }


   
        SelectedJsonManager.Instance.SetJsonPath(
            selectedJsonPath
        );


        Debug.Log(
            "Selected JSON ready for generation: " +
            Path.GetFileName(selectedJsonPath)
        );



        Debug.Log(
            "Loading SampleScene..."
        );


        SceneManager.LoadScene(
            "SampleScene"
        );
    }
}
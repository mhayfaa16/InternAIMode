using UnityEngine;

public class SelectedJsonManager : MonoBehaviour
{
    public static SelectedJsonManager Instance;

    public string SelectedJsonPath { get; private set; }

    private void Awake()
    {
       
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetJsonPath(string path)
    {
        SelectedJsonPath = path;

        Debug.Log("JSON path stored: " + SelectedJsonPath);
    }

    public bool HasJson()
    {
        return !string.IsNullOrEmpty(SelectedJsonPath);
    }
}
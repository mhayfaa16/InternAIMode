using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuReturn : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "AIFloorPlanMain";

    public void ReturnToMenu()
    {
        Debug.Log("[MenuReturn] Returning to main menu.");

        SceneManager.LoadScene(menuSceneName);
    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu
{
    public class UIMenuManager : MonoBehaviour
    {
        private Animator CameraObject;

        // ============================================================
        // MENUS
        // ============================================================

        [Header("MENUS")]

        [Tooltip("Main menu")]
        public GameObject mainMenu;

        [Tooltip("First list of buttons")]
        public GameObject firstMenu;

        [Tooltip("Menu shown when PLAY is clicked")]
        public GameObject playMenu;

        [Tooltip("Menu shown when EXIT is clicked")]
        public GameObject exitMenu;

        [Tooltip("Optional extras menu")]
        public GameObject extrasMenu;


        // ============================================================
        // THEME
        // ============================================================

        public enum Theme
        {
            custom1,
            custom2,
            custom3
        }

        [Header("THEME SETTINGS")]

        public Theme theme;

        private int themeIndex;

        public ThemedUIData themeController;


        // ============================================================
        // PROJECT GUIDE PANELS
        // ============================================================

        [Header("PROJECT GUIDE PANELS")]

        [Tooltip("Panel shown for AI ANALYSIS")]
        public GameObject PanelAI;

        [Tooltip("Panel shown for JSON INPUT")]
        public GameObject PanelJson;

        [Tooltip("Panel shown for 3D RECONSTRUCTION")]
        public GameObject PanelGeneration;


        // ============================================================
        // LOADING SCREEN
        // ============================================================

        [Header("LOADING SCREEN")]

        [Tooltip("If enabled, the loaded scene waits for user input")]
        public bool waitForInput = true;

        [Tooltip("Loading screen GameObject")]
        public GameObject loadingMenu;

        [Tooltip("Loading bar")]
        public Slider loadingBar;

        [Tooltip("Text asking the user to continue")]
        public TMP_Text loadPromptText;

        [Tooltip("Key used to continue loading")]
        public KeyCode userPromptKey;


        // ============================================================
        // SOUND EFFECTS
        // ============================================================

        [Header("SFX")]

        [Tooltip("Hover sound")]
        public AudioSource hoverSound;

        [Tooltip("Slider sound")]
        public AudioSource sliderSound;

        [Tooltip("Swoosh sound when changing menus")]
        public AudioSource swooshSound;


        // ============================================================
        // START
        // ============================================================

        private void Start()
        {
            CameraObject = GetComponent<Animator>();

            // Initial menu state
            if (playMenu != null)
                playMenu.SetActive(false);

            if (exitMenu != null)
                exitMenu.SetActive(false);

            if (extrasMenu != null)
                extrasMenu.SetActive(false);

            if (firstMenu != null)
                firstMenu.SetActive(true);

            if (mainMenu != null)
                mainMenu.SetActive(true);

            // Start with AI Analysis panel
            DisableProjectPanels();

            if (PanelAI != null)
                PanelAI.SetActive(true);

            SetThemeColors();
        }


        // ============================================================
        // THEME COLORS
        // ============================================================

        private void SetThemeColors()
        {
            if (themeController == null)
                return;

            switch (theme)
            {
                case Theme.custom1:

                    themeController.currentColor =
                        themeController.custom1.graphic1;

                    themeController.textColor =
                        themeController.custom1.text1;

                    themeIndex = 0;

                    break;


                case Theme.custom2:

                    themeController.currentColor =
                        themeController.custom2.graphic2;

                    themeController.textColor =
                        themeController.custom2.text2;

                    themeIndex = 1;

                    break;


                case Theme.custom3:

                    themeController.currentColor =
                        themeController.custom3.graphic3;

                    themeController.textColor =
                        themeController.custom3.text3;

                    themeIndex = 2;

                    break;


                default:

                    Debug.LogWarning(
                        "[UIMenuManager] Invalid theme selected."
                    );

                    break;
            }
        }


        // ============================================================
        // PLAY MENU
        // ============================================================

        public void PlayCampaign()
        {
            if (exitMenu != null)
                exitMenu.SetActive(false);

            if (extrasMenu != null)
                extrasMenu.SetActive(false);

            if (playMenu != null)
                playMenu.SetActive(true);
        }


        public void PlayCampaignMobile()
        {
            if (exitMenu != null)
                exitMenu.SetActive(false);

            if (extrasMenu != null)
                extrasMenu.SetActive(false);

            if (playMenu != null)
                playMenu.SetActive(true);

            if (mainMenu != null)
                mainMenu.SetActive(false);
        }


        public void ReturnMenu()
        {
            if (playMenu != null)
                playMenu.SetActive(false);

            if (extrasMenu != null)
                extrasMenu.SetActive(false);

            if (exitMenu != null)
                exitMenu.SetActive(false);

            if (mainMenu != null)
                mainMenu.SetActive(true);
        }


        // ============================================================
        // DISABLE PLAY MENU
        // ============================================================

        public void DisablePlayCampaign()
        {
            if (playMenu != null)
                playMenu.SetActive(false);
        }


        // ============================================================
        // CAMERA MENU POSITIONS
        // ============================================================

        public void Position1()
        {
            if (CameraObject != null)
                CameraObject.SetFloat("Animate", 0);
        }


        public void Position2()
        {
            DisablePlayCampaign();

            if (CameraObject != null)
                CameraObject.SetFloat("Animate", 1);
        }


        // ============================================================
        // PROJECT GUIDE PANELS
        // ============================================================

        private void DisableProjectPanels()
        {
            if (PanelAI != null)
                PanelAI.SetActive(false);

            if (PanelJson != null)
                PanelJson.SetActive(false);

            if (PanelGeneration != null)
                PanelGeneration.SetActive(false);
        }


        // ============================================================
        // AI ANALYSIS BUTTON
        // ============================================================

        public void AIAnalysisPanel()
        {
            DisableProjectPanels();

            if (PanelAI != null)
                PanelAI.SetActive(true);

            PlaySwoosh();
        }


        // ============================================================
        // JSON INPUT BUTTON
        // ============================================================

        public void JsonInputPanel()
        {
            DisableProjectPanels();

            if (PanelJson != null)
                PanelJson.SetActive(true);

            PlaySwoosh();
        }


        // ============================================================
        // 3D RECONSTRUCTION BUTTON
        // ============================================================

        public void GenerationPanel()
        {
            DisableProjectPanels();

            if (PanelGeneration != null)
                PanelGeneration.SetActive(true);

            PlaySwoosh();
        }


        // ============================================================
        // LOAD SCENE
        // ============================================================

        public void LoadScene(string scene)
        {
            if (!string.IsNullOrEmpty(scene))
            {
                StartCoroutine(
                    LoadAsynchronously(scene)
                );
            }
        }


        // ============================================================
        // EXIT CONFIRMATION
        // ============================================================

        public void AreYouSure()
        {
            if (exitMenu != null)
                exitMenu.SetActive(true);

            if (extrasMenu != null)
                extrasMenu.SetActive(false);

            DisablePlayCampaign();
        }


        public void AreYouSureMobile()
        {
            if (exitMenu != null)
                exitMenu.SetActive(true);

            if (extrasMenu != null)
                extrasMenu.SetActive(false);

            if (mainMenu != null)
                mainMenu.SetActive(false);

            DisablePlayCampaign();
        }


        // ============================================================
        // EXTRAS MENU
        // ============================================================

        public void ExtrasMenu()
        {
            if (playMenu != null)
                playMenu.SetActive(false);

            if (extrasMenu != null)
                extrasMenu.SetActive(true);

            if (exitMenu != null)
                exitMenu.SetActive(false);
        }


        // ============================================================
        // QUIT GAME
        // ============================================================

        public void QuitGame()
        {
#if UNITY_EDITOR

            UnityEditor.EditorApplication.isPlaying = false;

#else

            Application.Quit();

#endif
        }


        // ============================================================
        // SOUND
        // ============================================================

        public void PlayHover()
        {
            if (hoverSound != null)
                hoverSound.Play();
        }


        public void PlaySFXHover()
        {
            if (sliderSound != null)
                sliderSound.Play();
        }


        public void PlaySwoosh()
        {
            if (swooshSound != null)
                swooshSound.Play();
        }


        // ============================================================
        // LOADING SCREEN
        // ============================================================

        private IEnumerator LoadAsynchronously(
            string sceneName
        )
        {
            AsyncOperation operation =
                SceneManager.LoadSceneAsync(sceneName);

            if (operation == null)
            {
                Debug.LogError(
                    "[UIMenuManager] Could not load scene: "
                    + sceneName
                );

                yield break;
            }

            operation.allowSceneActivation = false;


            // Hide main menu
            if (mainCanvas != null)
                mainCanvas.SetActive(false);

            // Show loading screen
            if (loadingMenu != null)
                loadingMenu.SetActive(true);


            while (!operation.isDone)
            {
                float progress =
                    Mathf.Clamp01(
                        operation.progress / 0.9f
                    );


                if (loadingBar != null)
                    loadingBar.value = progress;


                // Scene is ready
                if (operation.progress >= 0.9f)
                {
                    if (loadingBar != null)
                        loadingBar.value = 1f;


                    if (waitForInput)
                    {
                        if (loadPromptText != null)
                        {
                            loadPromptText.text =
                                "Press "
                                + userPromptKey
                                    .ToString()
                                    .ToUpper()
                                + " to continue";
                        }


                        if (
                            Input.GetKeyDown(
                                userPromptKey
                            )
                        )
                        {
                            operation.allowSceneActivation =
                                true;
                        }
                    }
                    else
                    {
                        operation.allowSceneActivation =
                            true;
                    }
                }


                yield return null;
            }
        }


        // ============================================================
        // MAIN CANVAS
        // ============================================================

        [Header("MAIN CANVAS")]

        [Tooltip("Main canvas containing the menu")]
        public GameObject mainCanvas;
    }
}
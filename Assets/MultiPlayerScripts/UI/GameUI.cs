using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Gameplay Gameplay;
    [HideInInspector]
    public NetworkRunner Runner;

    public UIPlayerView PlayerView;
    public GameObject ScoreboardView;
    public GameObject MenuView;
    public GameObject DisconnectedView;


    public void OnRunnerShutdown(NetworkRunner runner, ShutdownReason reason)
    {

        ScoreboardView.SetActive(false);
        MenuView.gameObject.SetActive(false);
        DisconnectedView.SetActive(true);
    }

    public void GoToMenu()
    {
        if (Runner != null)
        {
            Runner.Shutdown();
        }

        SceneManager.LoadScene("Startup");
    }

    private void Awake()
    {
        PlayerView.gameObject.SetActive(false);
        MenuView.SetActive(false);
        DisconnectedView.SetActive(false);

        // Make sure the cursor starts unlocked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Application.isBatchMode == true)
            return;

        if (Gameplay.Object == null || Gameplay.Object.IsValid == false)
            return;

        Runner = Gameplay.Runner;

        var keyboard = Keyboard.current;
        bool gameplayActive = Gameplay.State < EGameplayState.Finished;

        ScoreboardView.SetActive(gameplayActive && keyboard != null && keyboard.tabKey.isPressed);

        if (gameplayActive && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            MenuView.SetActive(!MenuView.activeSelf);
        }


        var playerObject = Runner.GetPlayerObject(Runner.LocalPlayer);
        if (playerObject != null)
        {
            var playerData = Gameplay.PlayerData.Get(Runner.LocalPlayer);

            PlayerView.UpdatePlayer(playerData);
            PlayerView.gameObject.SetActive(gameplayActive);
        }
        else
        {
            PlayerView.gameObject.SetActive(false);
        }
    }
}
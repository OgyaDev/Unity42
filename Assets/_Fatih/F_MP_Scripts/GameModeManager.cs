using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager Instance;

    public float gameDuration = 180f; // 3 minutes in seconds
    private float remainingTime;

    public TextMeshProUGUI timerText; // Reference to the UI Text component for the timer

    private void Start()
    {
        if (IsServer)
        {
            remainingTime = gameDuration;
            StartCoroutine(GameTimer());
        }
    }

    private IEnumerator GameTimer()
    {
        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime--;
            UpdateTimerClientRpc(remainingTime);
        }

        EndGame();
    }

    private void EndGame()
    {
        // Logic to end the game, declare winner, etc.
        Debug.Log("Game Over!");
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            // Client-side initialization
            InitializeTimerText();
        }
    }

    private void InitializeTimerText()
    {
        // Bulmak için bir yöntemi var (örneðin, GameObject.Find) veya doðrudan Inspector'dan atanabilir.
        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
    }
}

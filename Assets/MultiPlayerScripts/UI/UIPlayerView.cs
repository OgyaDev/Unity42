using TMPro;
using UnityEngine;

public class UIPlayerView : MonoBehaviour
{
    public TextMeshProUGUI Nickname;

    public void UpdatePlayer(PlayerData playerData)
    {
        Nickname.text = playerData.Nickname;
    }
}
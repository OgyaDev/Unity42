using UnityEngine;
using Fusion;
public struct PlayerData : INetworkStruct
{
    [Networked, Capacity(24)]
    public string Nickname { get => default; set { } }
    public PlayerRef PlayerRef;
    public bool IsAlive;
    public bool IsConnected;
}

public enum EGameplayState
{
    Skirmish = 0,
    Running = 1,
    Finished = 2,
}

public class Gameplay : NetworkBehaviour
{
    public GameUI GameUI;
    private bool _isNicknameSent;

    [Networked]
    [Capacity(32)]
    [HideInInspector]
    public NetworkDictionary<PlayerRef, PlayerData> PlayerData { get; }
    public EGameplayState State { get; set; }


    public override void Render()
    {
        if (Runner.Mode == SimulationModes.Server)
            return;

        if (_isNicknameSent == false)
        {
            RPC_SetPlayerNickname(Runner.LocalPlayer, PlayerPrefs.GetString("Photon.Menu.Username"));
            _isNicknameSent = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    private void RPC_SetPlayerNickname(PlayerRef playerRef, string nickname)
    {
        var playerData = PlayerData.Get(playerRef);
        playerData.Nickname = nickname;
        PlayerData.Set(playerRef, playerData);
    }
}
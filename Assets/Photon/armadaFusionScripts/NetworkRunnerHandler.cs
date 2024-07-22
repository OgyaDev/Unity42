using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using System.Threading.Tasks;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    //Photon Fusion app id ile arasında network bağlantısını kuracak elementler Network Runner ve Network Handler
    [SerializeField] 
    NetworkRunner networkRunnerPrefab;
    NetworkRunner networkRunner;
    void Awake()
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
    }

   
    void Start()
    {
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
        }
        
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient,"testSession", NetAddress.Any(),SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
        
        
        Utils.DebugLog("InitializeNetworkRunner Called1");
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        INetworkSceneManager sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
        { 
            //sahnede zaten networklenmiş olan objeleri yönetme
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }
    
    
    
    protected virtual Task InitializeNetworkRunner(NetworkRunner networkRunner, GameMode gameMode, string sessionName,
        NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        INetworkSceneManager sceneManager = GetSceneManager(networkRunner);
        networkRunner.ProvideInput = true;
        
        return networkRunner.StartGame(new StartGameArgs
            {
               GameMode = gameMode,
               Address = address,
               Scene = scene,
               SessionName = sessionName,
               CustomLobbyName = "Lobby IDmiz",
               SceneManager = sceneManager
            });
    }
    


}

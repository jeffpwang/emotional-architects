using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meta.PP
{
    public class SceneLoader : MonoBehaviour
    {
        private bool _loading = false;
        public Action<string> WhenLoadingScene = delegate { };
        public Action<string> WhenSceneLoaded = delegate { };
        private int _waitingCount = 0;
        
        // ADDED THIS
        private string currentScene;
        // ADDED THIS

        public void Unload(string sceneName)
        {
            if (currentScene != null)
            {
                SceneManager.UnloadSceneAsync(sceneName);   
            }
        }

        public void Load(string sceneName)
        {
            if (_loading) return;
            _loading = true;
            
            // ADDED THIS
            Unload(currentScene);
            
            // make sure we wait for all parties concerned to let us know they're ready to load
            _waitingCount = WhenLoadingScene.GetInvocationList().Length-1;  // remove the count for the blank delegate
            if (_waitingCount == 0)
            {
                // if nobody else cares just set the preload to go directly to the loading of the scene
                HandleReadyToLoad(sceneName);
            }
            else
            {
                WhenLoadingScene.Invoke(sceneName);
            }
        }

        // this should be called after handling any pre-load tasks (e.g. fade to white) by anyone who regsistered with WhenLoadingScene in order for the loading to proceed
        public void HandleReadyToLoad(string sceneName)
        {
            _waitingCount--;
            if (_waitingCount <= 0)
            {
                StartCoroutine(LoadSceneAsync(sceneName));
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            // ADDED THIS
            currentScene = sceneName;
            _loading = false;
            
            WhenSceneLoaded.Invoke(sceneName);
        }
    }
}

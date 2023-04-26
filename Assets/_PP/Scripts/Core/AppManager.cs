using System;
using System.Collections.Generic;
using Buck.MR;
using Oculus.Interaction.PoseDetection;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.PP
{
    public class AppManager : MonoBehaviour
    {
        [Header("Scene Loading")]
        public List<string> sceneNames;
        public SceneLoader sceneLoader;
        public string currentScene;
        
        [Header("Events")]
        public ExampleUsage exampleEvent;
        public static AppManager Instance;

        public SequencePlayer currentSequencePlayer;
        public CustomSequence currentSceneSequence; 
        
        private int currentSequenceIndex = -1;
        // scene events
        public delegate void PlayAction();
        public static event PlayAction OnPlay;
        public delegate void StopAction();
        public static event StopAction OnStop;

        public bool isPlaying = false;
        
        List<IInteractable> Interactables = new List<IInteractable>(); // objects that detect user actions

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if (isPlaying)
            {
                // switch scenes
                if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {
                    EndScene();
                }
                // move sequences
                if (OVRInput.GetDown(OVRInput.RawButton.X) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MoveToNextSequence(true);
                }
                if (OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MoveToNextSequence(false);
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space)) 
                {
                    StartPlaying();
                }
            }
            
            // DEBUG KEYS
            if (Input.GetKeyDown(KeyCode.Keypad0)) // set to welcome
            {
                SetScene(sceneNames[0]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad1)) // set to safe space
            {
                SetScene(sceneNames[1]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2)) // set to calibration
            {
                SetScene(sceneNames[2]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2)) // set to therapy
            {
                SetScene(sceneNames[3]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2)) // set to ending
            {
                SetScene(sceneNames[4]);
            }
        }
        private void OnEnable()
        {

        }
        private void OnDisable()
        {

        }
        
        public void StartPlaying() // play currently selected scene
        {
            if (isPlaying)
            {
                return;
            }
            
            if (currentScene == null)
            {
                Debug.LogError("[AppManager] Scene not set!");
                return;
            }
            
            // reset sequence stuff
            currentSequenceIndex = -1;
            currentScene = null;
            
            isPlaying = true;

            // setup scene
            OnPlay?.Invoke();
            
            Debug.Log($"[AppManager] Now playing: {currentScene}");
        }
        
        public void EndScene()
        {
            if (isPlaying)
            {
                isPlaying = false;
                
                OnStop?.Invoke();

                Debug.Log($"[AppManager] Stopped playing: {currentScene}");
            }
        }
        
        public void SetScene(string newSceneId)
        {
            foreach (var scene in sceneNames)
            {
                if (scene == newSceneId)
                {
                    currentScene = scene;

                    sceneLoader.Load(newSceneId);
                    
                    sceneLoader.WhenSceneLoaded += delegate(string s)
                    {
                        currentSequencePlayer = FindObjectOfType<SequencePlayer>();
                        currentSequencePlayer.SetupScene();
                    };
                    
                    Debug.Log($"[AppManager] Set current scene to: {currentScene}");
                    return;
                }   
            }
            Debug.LogError("[AppManager] NO SCENE FOUND");
        }
        
        public void MoveToNextSequence(bool moveNext, bool endLast = true)
        {
            if (currentSequencePlayer == null)
            {
                Debug.LogError("[AppManager] SEQUENCE PLAYER NOT SET");
                return;
            }
            
            // end the last sequence
            if (currentSceneSequence != null && endLast)
            {
                currentSceneSequence.EndSequence();
            }
            
            if (moveNext)
            {
                currentSequenceIndex++;
                if (currentSequenceIndex > currentSequencePlayer.sequences.Count - 1)
                {
                    currentSequenceIndex = 0;
                }
            }
            else
            {
                currentSequenceIndex--;
                if (currentSequenceIndex < 0)
                {
                    currentSequenceIndex = currentSequencePlayer.sequences.Count - 1;
                }
            }
            
            // play sequence
            currentSceneSequence = currentSequencePlayer.sequences[currentSequenceIndex];
            currentSceneSequence.PlaySequence();
            
        }
    }
}

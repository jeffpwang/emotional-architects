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
        public List<SequencePlayer> sequencePlayers;
        public OVRSceneManager sceneManager;
        public DeskPrefabScript desk;
        public int currentSceneIndex;
        // [Header("Scene Loading")] 
        // public List<string> sceneNames;
        // public SceneLoader sceneLoader;
        // public string currentScene;
        // public int currentSceneIndex;
        
        [Header("Events")]
        public ExampleUsage exampleEvent;
        public static AppManager Instance;

        public SequencePlayer currentSequencePlayer;
        public CustomSequence currentSceneSequence; 
        
        public int currentSequenceIndex = -1;
        // scene events
        public delegate void SceneStartAction();
        public static event SceneStartAction OnSceneStart;
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

        private void Start()
        {
            sceneManager.SceneModelLoadedSuccessfully += StartExperience;
        }

        public void StartExperience()
        {
            Debug.Log("STARTING EXPERIENCE");
            PlayScene(0);
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveToNextSequence(true);
            }
            if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveToNextSequence(false);
            }

            if (Input.GetKeyDown(KeyCode.Keypad0)) // set to welcome
            {
                PlayScene(0);
                // SetScene(sceneNames[0]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad1)) // set to safe space
            {
                PlayScene(1);
                // SetScene(sceneNames[1]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2)) // set to calibration
            {
                PlayScene(2);
                // SetScene(sceneNames[2]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad3)) // set to therapy
            {
                PlayScene(3);
                // SetScene(sceneNames[3]);
            }
            if (Input.GetKeyDown(KeyCode.Keypad4)) // set to ending
            {
                PlayScene(4);
                // SetScene(sceneNames[4]);
            }
        }

        // public void SetScene(string newSceneId)
        // {
        //     foreach (var scene in sceneNames)
        //     {
        //         if (scene == newSceneId)
        //         {
        //             currentScene = scene;
        //             
        //             currentSequenceIndex = -1;
        //             
        //             sceneLoader.Load(newSceneId);
        //             
        //             sceneLoader.WhenSceneLoaded += delegate(string s)
        //             {
        //                 currentSequencePlayer = FindObjectOfType<SequencePlayer>();
        //             };
        //             
        //             Debug.Log($"[AppManager] Set current scene to: {currentScene}");
        //             return;
        //         }   
        //     }
        //     Debug.LogError("[AppManager] NO SCENE FOUND");
        // }

        public void PlayScene(int index)
        {
            currentSceneIndex = index;
            currentSequenceIndex = 0;
            currentSequencePlayer = sequencePlayers[currentSceneIndex];
            
            currentSequencePlayer.StartSequence();
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
                    // GOING TO NEXT SCENE WHEN END OF SEQUENCE HIT
                    currentSceneIndex++;
                    PlayScene(currentSceneIndex);
                    Debug.Log($"GOING TO NEXT SCENE...{sequencePlayers[currentSceneIndex].gameObject.name}");
                    return;
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

        public void EndSequence()
        {
            if (currentSceneSequence != null)
            {
                currentSceneSequence.EndSequence();
            }
        }
    }
}

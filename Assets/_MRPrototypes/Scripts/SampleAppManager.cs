// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.MR
{
    public class SampleAppManager : MonoBehaviour
    {
        public GameObject[] sets;
        // SAMPLE SWITCH SCENES
        public int totalNum = 4;
        int currentSceneIndex = 0;

        static public SampleAppManager Instance = null;

        [Header("Scene Preview")] [SerializeField]
        private OVRSceneManager _sceneManager;

        [SerializeField] private OVRPassthroughLayer _passthroughLayer;
        bool _sceneModelLoaded = false;

        float _floorHeight = 0.0f;

        // after the Scene has been loaded successfuly, we still wait a frame before the data has "settled"
        // e.g. VolumeAndPlaneSwitcher needs to happen first, and script execution order also isn't fixed by default
        int _frameWait = 0;

        [HideInInspector] public OVRSceneAnchor[] _sceneAnchors;

        [Header("Game Pieces")] public VirtualRoom _vrRoom;

        [Header("Overlays")] public Camera _mainCamera;
        PassthroughStylist _passthroughStylist;
        
        public enum SampleScene
        {
            Reset, 
            SceneA, 
            SceneB, 
            SceneC 
        };

        public SampleScene _currentSampleScene { get; private set; }

        private GameObject spawnedSet;
        
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            
            if (!Instance)
            {
                Instance = this;
            }
            
            _currentSampleScene = SampleScene.Reset;

            _passthroughLayer.colorMapEditorType = OVRPassthroughLayer.ColorMapEditorType.None;
            
            _passthroughLayer.textureOpacity = 0;
            _passthroughStylist = this.gameObject.AddComponent<PassthroughStylist>();
            _passthroughStylist.Init(_passthroughLayer);
            
            _passthroughStylist.ResetPassthrough();
        }

        void Start()
        {
            _sceneManager.SceneModelLoadedSuccessfully += SceneModelLoaded;
        }

        private void SceneModelLoaded()
        {
            _sceneModelLoaded = true;
        }
        
        void Update()
        {
            // SWITCH SCENES WITH CONTROLLER
            bool controllersActive = OVRInput.GetActiveController() == OVRInput.Controller.Touch ||
                                     OVRInput.GetActiveController() == OVRInput.Controller.LTouch ||
                                     OVRInput.GetActiveController() == OVRInput.Controller.RTouch;
            
            if (OVRInput.GetUp(OVRInput.Button.Start))
            {
                SwitchScene();
            }
            // END SWITCH SCENES WITH CONTROLLER
        }

        public void SwitchScene()
        {
            currentSceneIndex++;
            if (currentSceneIndex >= totalNum) currentSceneIndex = 0;
            switch (currentSceneIndex)
            {
                case 0:
                    ForceChapter(SampleScene.Reset);
                    break;
                case 1:
                    ForceChapter(SampleScene.SceneA);
                    break; 
                case 2:
                    ForceChapter(SampleScene.SceneB);
                    break;
                case 3:
                    ForceChapter(SampleScene.SceneC);
                    break;
            }
        }
        public void ForceChapter(SampleScene forcedChapter)
        {
            StopAllCoroutines();
            _currentSampleScene = forcedChapter;
            if (spawnedSet) Destroy(spawnedSet);
            switch (_currentSampleScene)
            {
                case SampleScene.Reset:
                    _passthroughStylist.ResetPassthrough(1);
                    break;
                case SampleScene.SceneA:
                    spawnedSet = Instantiate(sets[0]);
                    SetBlackAndWhite();
                    break;
                case SampleScene.SceneB:
                    spawnedSet = Instantiate(sets[1]);
                    SetToWhiteOut();
                    break;
                case SampleScene.SceneC:
                    spawnedSet = Instantiate(sets[2]);
                    _passthroughStylist.ResetPassthrough(1);
                    break;
            }

            Debug.Log("MR SAMPLE APP: started chapter " + _currentSampleScene);
        }

        public void SetBlackAndWhite() //BW
        {
            _passthroughLayer.colorMapEditorType = OVRPassthroughLayer.ColorMapEditorType.ColorAdjustment;
            StartCoroutine(PlayIntroPassthrough());
        }
        public void SetRedEdges() // red edge
        {
            PassthroughStylist.PassthroughStyle weirdPassthrough = new PassthroughStylist.PassthroughStyle(
                1.0f,
                0.0f,
                0.0f,
                0.0f,
                true,
                Color.red,
                Color.black,
                Color.white);
            _passthroughStylist.ShowStylizedPassthrough(weirdPassthrough, 1.0f);
        }
        public void SetToWhiteOut() // white out
        {
            PassthroughStylist.PassthroughStyle normalPassthrough = new PassthroughStylist.PassthroughStyle(
                1.0f,
                0.0f,
                1.0f,
                0.0f,
                false,
                Color.white,
                Color.black,
                Color.white);
            _passthroughStylist.ShowStylizedPassthrough(normalPassthrough, 1.0f);
        }
        public void SetToBlackOut() // white out
        {
            PassthroughStylist.PassthroughStyle normalPassthrough = new PassthroughStylist.PassthroughStyle(
                1.0f,
                0.0f,
                0.0f,
                0.0f,
                false,
                Color.white,
                Color.black,
                Color.white);
            _passthroughStylist.ShowStylizedPassthrough(normalPassthrough, 1.0f);
        }
        IEnumerator PlayIntroPassthrough()
        {
            float timer = 0.0f;
            float lerpTime = 1.0f;
            while (timer <= lerpTime)
            {
                timer += Time.deltaTime;
                float lerpValue = Mathf.Clamp01(timer / lerpTime);
                _passthroughLayer.colorMapEditorSaturation = Mathf.Lerp(0, -1, lerpValue);

                // once lerpTime is over, fade in normal passthrough
                if (timer >= lerpTime)
                {

                }
                yield return null;
            }
        }      
        
        /// <summary>
        /// When the Scene has loaded, instantiate all the wall and furniture items.
        /// OVRSceneManager creates proxy anchors, that we use as parent tranforms for these instantiated items.
        /// </summary>
        void GetRoomFromScene()
        {
            if (_frameWait < 1)
            {
                _frameWait++;
                return;
            }

            try
            {
                // OVRSceneAnchors have already been instantiated from OVRSceneManager
                // to avoid script execution conflicts, we do this once in the Update loop instead of directly when the SceneModelLoaded event is fired
                _sceneAnchors = FindObjectsOfType<OVRSceneAnchor>();

                // WARNING: right now, a Scene is guaranteed to have closed walls
                // if this ever changes, this logic needs to be revisited because the whole game fails (e.g. furniture with no walls)
                _vrRoom.Initialize(_sceneAnchors);

                // even though loading has succeeded to this point, do some sanity checks
                if (!_vrRoom.IsPlayerInRoom())
                {
                    Debug.Log("ERROR: PLAYER NOT INSIDE ROOM!!!");
                }

                SampleEnvironment.Instance.Initialize();
                // ForceChapter(SampleScene.Title);
            }
            catch
            {
                // if initialization messes up for some reason, quit the app
                Debug.Log("ERROR: SCENE MODEL NOT LOADED SUCCESSFULLY!!!");
            }
        }

    }
}

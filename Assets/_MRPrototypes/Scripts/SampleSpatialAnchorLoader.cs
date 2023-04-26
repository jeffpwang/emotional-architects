// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.MR
{
    /// <summary>
    /// Demonstrates loading existing spatial anchors from storage.
    /// </summary>
    /// <remarks>
    /// Loading existing anchors involves two asynchronous methods:
    /// 1. Call <see cref="OVRSpatialAnchor.LoadUnboundAnchors"/>
    /// 2. For each unbound anchor you wish to localize, invoke <see cref="OVRSpatialAnchor.UnboundAnchor.Localize"/>.
    /// 3. Once localized, your callback will receive an <see cref="OVRSpatialAnchor.UnboundAnchor"/>. Instantiate an
    /// <see cref="OVRSpatialAnchor"/> component and bind it to the `UnboundAnchor` by calling
    /// <see cref="OVRSpatialAnchor.UnboundAnchor.BindTo"/>.
    /// </remarks>
    public class SampleSpatialAnchorLoader : MonoBehaviour
    {
        public const string NumUuidsPlayerPref = "numUuids";
        
        [SerializeField]
        private List<GameObject> _prefabsList;
        
        [Serializable]
        public class AnchorData
        {
             /// <summary>
             /// The handle that represents the anchor in this runtime
             /// </summary>
             public Guid spaceHandle;

             /// <summary>
             /// The name of the prefab that should be instantiated for this anchor
             /// </summary>
             public string prefabName;

             /// <summary>
             /// Reference to the gameobject instantiated in scene for this anchor
             /// </summary>
             public GameObject instantiatedObject = null;
             /// <summary>
             /// Reference to the gameobject instantiated in scene 
             /// </summary>
             public GameObject spatialAnchor = null;
        }
        /// <summary>
        /// Dictionary of created anchors. The space handle ID is the key of the dictionary
        /// </summary>
        private Dictionary<Guid, AnchorData> m_createdAnchors;
        
        private OVRSpatialAnchor _anchorPrefab;

        Action<OVRSpatialAnchor.UnboundAnchor, bool> _onLoadAnchor;

        private void Start()
        {
            m_createdAnchors = new Dictionary<Guid, AnchorData>();
        }

        public void LoadAnchorsByUuid()
        {
            // Get number of saved anchor uuids
            if (!PlayerPrefs.HasKey(SampleAnchor.NumUuidsPlayerPref))
            {
                PlayerPrefs.SetInt(SampleAnchor.NumUuidsPlayerPref, 0);
            }

            var playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            Log($"Attempting to load {playerUuidCount} saved anchors.");
            if (playerUuidCount == 0)
                return;

            var uuids = new Guid[playerUuidCount];
            for (int i = 0; i < playerUuidCount; ++i)
            {
                var uuidKey = "uuid" + i;
                var currentUuid = PlayerPrefs.GetString(uuidKey);
                Log("QueryAnchorByUuid: " + currentUuid);

                uuids[i] = new Guid(currentUuid);
                
                // custom anchor data
                var prefabKey = uuidKey + "_prefab";
                if (PlayerPrefs.HasKey(prefabKey))
                {
                    var prefabName = PlayerPrefs.GetString(prefabKey);
                    // add the created anchor to the list of anchors
                    m_createdAnchors[uuids[i]] = new AnchorData()
                    {
                        spaceHandle = uuids[i],
                        prefabName = prefabName
                    };
                }
                else
                {
                    Debug.LogError($"[ERROR] NO PREFAB KEY FOUND FOR {prefabKey}");
                }
            }

            Load(new OVRSpatialAnchor.LoadOptions
            {
                MaxAnchorCount = 100,
                Timeout = 0,
                StorageLocation = OVRSpace.StorageLocation.Local,
                Uuids = uuids
            });
        }

        public void EraseAllAnchors()
        {
            var playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            Log($"Attempting to erase {playerUuidCount} saved anchors.");
            if (playerUuidCount == 0)
                return;

            var uuids = new Guid[playerUuidCount];
            for (int i = 0; i < playerUuidCount; ++i)
            {
                var uuidKey = "uuid" + i;
                var currentUuid = PlayerPrefs.GetString(uuidKey);

                uuids[i] = new Guid(currentUuid);
                if (m_createdAnchors.ContainsKey(uuids[i]))
                {
                    AnchorData anchor = m_createdAnchors[uuids[i]];

                    // remove spatial anchor object
                    SampleAnchor spatialAnchor = anchor.spatialAnchor.GetComponent<SampleAnchor>();
                    spatialAnchor.OnEraseButtonPressed(true);
                    
                    // remove instantiated gameobject
                    Destroy(anchor.instantiatedObject);
                    
                    Debug.Log($"[SampleSpatialAnchorLoader] Erasing...{anchor.prefabName}");
                    m_createdAnchors.Remove(uuids[i]);
                }

            }

            // delete number of IDs from Unity app
            PlayerPrefs.SetInt(SampleAnchor.NumUuidsPlayerPref, 0);
        }
        
        private void Awake()
        {
            _onLoadAnchor = OnLocalized;
        }

        private void Load(OVRSpatialAnchor.LoadOptions options) => OVRSpatialAnchor.LoadUnboundAnchors(options,
            anchors =>
            {
                if (anchors == null)
                {
                    Log("Query failed.");
                    return;
                }

                foreach (var anchor in anchors)
                {
                    if (anchor.Localized)
                    {
                        _onLoadAnchor(anchor, true);
                    }
                    else if (!anchor.Localizing)
                    {
                        anchor.Localize(_onLoadAnchor);
                    }
                }
            });

        private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor, bool success)
        {
            if (!success)
            {
                Log($"{unboundAnchor} Localization failed!");
                return;
            }

            var pose = unboundAnchor.Pose;
            
            if (m_createdAnchors.ContainsKey(unboundAnchor.Uuid))
            {
                AnchorData createdAnchor = m_createdAnchors[unboundAnchor.Uuid];
                var prefabName = createdAnchor.prefabName;
   
                switch (prefabName)
                {
                    case "object_A":
                        _anchorPrefab = _prefabsList[0].GetComponent<OVRSpatialAnchor>();
                        break;
                    case "object_B":
                        _anchorPrefab = _prefabsList[1].GetComponent<OVRSpatialAnchor>();
                        break;
                    case "object_C":
                        _anchorPrefab = _prefabsList[2].GetComponent<OVRSpatialAnchor>();
                        break;
                }
                
                // Instantiate prefab at this spatial anchor location
                var spatialAnchor = Instantiate(_anchorPrefab, pose.position, pose.rotation);
            
                unboundAnchor.BindTo(spatialAnchor);

                if (spatialAnchor.TryGetComponent<SampleAnchor>(out var anchor))
                {
                    // We just loaded it, so we know it exists in persistent storage.
                    anchor.ShowSaveIcon = true;
                    createdAnchor.instantiatedObject = anchor.SpawnObject();
                    createdAnchor.spatialAnchor = anchor.gameObject;
                }
                Debug.Log("PREFAB LOADED: " + prefabName);  
            }
        }

        private static void Log(string message) => Debug.Log($"[SpatialAnchorsUnity]: {message}");
    }

}
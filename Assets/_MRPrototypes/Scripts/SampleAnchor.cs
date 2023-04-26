using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Buck.MR
{
    /// <summary>
    /// Specific functionality for spawned anchors
    /// </summary>
    [RequireComponent(typeof(OVRSpatialAnchor))]
    public class SampleAnchor : MonoBehaviour
    {
        OVRInput.Controller controller = OVRInput.Controller.RTouch;
        public GameObject prefab;
        public string prefabName;
        
        public const string NumUuidsPlayerPref = "numUuids";
        
        public enum AnchorMode
        {
            Idle, // able to be selected
            Selected, // is selected
            Edit, // is being edited
            Play // play mode
        };
        
        private AnchorMode _mode = AnchorMode.Idle;
        
        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private Transform _pivot;

        [SerializeField]
        private GameObject _anchorMenu;
        
        [SerializeField]
        private GameObject _anchorVisual;

        private bool _isHovered;

        [SerializeField]
        private TextMeshProUGUI _anchorName;

        [SerializeField]
        private GameObject _saveIcon;

        [SerializeField]
        private Image _labelImage;

        [SerializeField]
        private Color _labelBaseColor;

        [SerializeField]
        private Color _labelHighlightColor;

        [SerializeField]
        private Color _labelSelectedColor;

        [SerializeField]
        private MeshRenderer[] _renderers;

        [SerializeField]
        private List<Button> _buttonList;

        private OVRSpatialAnchor _spatialAnchor;
        
        private SelectionMenu _anchorSelectionMenu;

        private bool _isLoaded; // is this currently saved in persistent storage
        
        private int _savedIndex = 0; // for persistent storage key 
        
        private string _savedKey; // for persistent storage key 

        private GameObject _placementPreview;
        private GameObject _spawnedObject;
        
        private Vector3 _beforeEditPosition;
        private Quaternion _beforeEditRotation;
        
        #region Monobehaviour Methods

        private void Awake()
        {
            _anchorMenu.SetActive(false);
            _anchorVisual.SetActive(false);
            
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _canvas.worldCamera = Camera.main;
            
            _anchorSelectionMenu = new SelectionMenu(_buttonList, _canvas.gameObject);
            
            _spatialAnchor = GetComponent<OVRSpatialAnchor>();
            
            _savedKey = NumUuidsPlayerPref;
            // _savedKey = NumUuidsPlayerPref + "_" + prefabName;
        }

        private IEnumerator Start()
        {
            while (_spatialAnchor && !_spatialAnchor.Created)
            {
                yield return null;
            }

            if (_spatialAnchor)
            {
                _anchorName.text = _spatialAnchor.Uuid.ToString("D");
            }
            else
            {
                // Creation must have failed
                Destroy(gameObject);
            }
            //_spatialAnchor.enabled = false;
        }
        private void OnEnable()
        {
            SampleAnchorUIManager.OnAnchorSetupComplete += StartSequence;
            SampleAnchorUIManager.OnAnchorSetupReset += EndSequence;
        }

        private void OnDisable()
        {
            RemoveSpawnedObject();
            RemoveAnchorPreview();
            SampleAnchorUIManager.OnAnchorSetupComplete -= StartSequence;
            SampleAnchorUIManager.OnAnchorSetupReset -= EndSequence;
        }
        
        private void StartSequence()
        {
            _mode = AnchorMode.Play;
            _canvas.gameObject.SetActive(false);
            _anchorVisual.SetActive(false);
            //_spatialAnchor.enabled = false;
        }
        
        private void EndSequence()
        {
            _mode = AnchorMode.Idle;
            _canvas.gameObject.SetActive(true);
            //_spatialAnchor.enabled = true;
        }
        
        private void Update()
        {
            if (_mode != AnchorMode.Play)
            {
                // Billboard the boundary
                BillboardPanel(_canvas.transform);

                // Billboard the menu
                BillboardPanel(_pivot);   
                
                // Controller input
                if (_mode == AnchorMode.Edit)
                {
                    HandleEditMode();
                }
                
                if (_mode == AnchorMode.Selected)
                {
                    HandleMenuNavigation();
                }
            }
        }

        #endregion // MonoBehaviour Methods

        #region UI Event Listeners

        /// <summary>
        /// UI callback for the anchor menu's Save button
        /// </summary>
        public void OnSaveLocalButtonPressed()
        {
            if (!_spatialAnchor) return;

            _spatialAnchor.Save((anchor, success) =>
            {
                if (!success) return;

                // Enables save icon on the menu
                ShowSaveIcon = true;

                // Write uuid of saved anchor to file
                if (!PlayerPrefs.HasKey(_savedKey))
                {
                    PlayerPrefs.SetInt(_savedKey, 0);
                }

                int playerNumUuids = PlayerPrefs.GetInt(_savedKey);
                _savedIndex = playerNumUuids;
                PlayerPrefs.SetString("uuid" + playerNumUuids, anchor.Uuid.ToString());
                PlayerPrefs.SetString("uuid" + playerNumUuids + "_prefab", prefabName);
                PlayerPrefs.SetInt(_savedKey, ++playerNumUuids);
            });
        }
        

        /// <summary>
        /// UI callback for the anchor menu's Edit button
        /// </summary>
        public void OnEditButtonPressed()
        {
            _beforeEditPosition = transform.position;
            _beforeEditRotation = transform.rotation;
            
            RemoveSpawnedObject();
            _spatialAnchor.enabled = false;
            
            _mode = AnchorMode.Edit;
            
            _anchorMenu.SetActive(false);
            _anchorVisual.SetActive(true);
            
            ShowPreview();
            
            SampleAnchorUIManager.Instance.ToggleEditingMode();
        }
        
        /// <summary>
        /// UI callback for the anchor menu's Hide button
        /// </summary>
        public void OnHideButtonPressed()
        {
            RemoveAnchorPreview();
            RemoveSpawnedObject();
            Destroy(gameObject);
        }

        /// <summary>
        /// UI callback for the anchor menu's Erase button
        /// </summary>
        public void OnEraseButtonPressed(bool destroyAfter = false)
        {
            if (!_spatialAnchor) return;

            _spatialAnchor.Erase((anchor, success) =>
            {
                if (success)
                {
                    // int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
                    // PlayerPrefs.SetInt(NumUuidsPlayerPref, --playerNumUuids);
                    //
                    PlayerPrefs.DeleteKey("uuid" + _savedIndex);
                    PlayerPrefs.DeleteKey("uuid" + _savedIndex + "_prefab");
                    
                    _saveIcon.SetActive(false);

                    if (destroyAfter)
                    {
                        Destroy(gameObject);
                    }
                }
            });
        }

        #endregion // UI Event Listeners

        #region Public Methods

        public void ShowPreview()
        {
            RemoveAnchorPreview();
            _spatialAnchor.enabled = false;
            _placementPreview = Instantiate(prefab, gameObject.transform);
        }
        public void SaveOverride(int index)
        {
            StartCoroutine(AttemptSaveOverride(index));
            // _spatialAnchor.Save((anchor, success) =>
            // {
            //     if (!success) return;
            //     Debug.Log($"[SampleAnchor] Successfully saved anchor...{prefabName}");
            //     ShowSaveIcon = true;
            //     // Override existing key
            //     PlayerPrefs.SetString("uuid" + index, anchor.Uuid.ToString());
            // });
        }
        
        public bool ShowSaveIcon
        {
            set
            {
                _isLoaded = value; 
                _saveIcon.SetActive(value); 
            }
        }
        
        public void ShowAnchorPreview()
        {
            if (_placementPreview != null)
            {
                _placementPreview.SetActive(true);   
            }
        }

        public void HideAnchorPreview()
        {
            if (_placementPreview != null)
            {
                _placementPreview.SetActive(false);
            }
        }

        public void RemoveAnchorPreview()
        {
            if (_placementPreview)
            {
                Destroy(_placementPreview);   
            }
        }
        public void RemoveSpawnedObject()
        {
            if (_spawnedObject)
            {
                Destroy(_spawnedObject);   
            }
        }
        public GameObject SpawnObject()
        {
            if (prefab != null)
            {
                RemoveAnchorPreview();
                Debug.Log($"[SampleAnchor] Spawning... {prefab.name}");
                _spawnedObject = Instantiate(prefab, transform.position, transform.rotation);
                return _spawnedObject;
            }

            Debug.Log($"[SampleAnchor] No prefab set!");
            return null;
        }
        /// <summary>
        /// Handles interaction when anchor is hovered
        /// </summary>
        public void OnHoverStart()
        {
            if (_isHovered)
            {
                return;
            }

            _isHovered = true;
            _anchorVisual.SetActive(true);

            _labelImage.color = _labelHighlightColor;
        }

        /// <summary>
        /// Handles interaction when anchor is no longer hovered
        /// </summary>
        public void OnHoverEnd()
        {
            if (!_isHovered)
            {
                return;
            }

            _isHovered = false;
            _anchorVisual.SetActive(false);

            if (_mode == AnchorMode.Selected)
            {
                _labelImage.color = _labelSelectedColor;
            }
            else
            {
                _labelImage.color = _labelBaseColor;
            }
        }

        /// <summary>
        /// Handles interaction when anchor is selected
        /// </summary>
        public void OnSelect()
        {
            if (_mode == AnchorMode.Edit)
            {
                return;
            }
            if (_mode == AnchorMode.Selected)
            {
                _spatialAnchor.enabled = false;
                // Hide Anchor menu on deselect
                _anchorMenu.SetActive(false);
                _mode = AnchorMode.Idle;
                _anchorSelectionMenu.Defocus();
                
                if (_isHovered)
                {
                    _labelImage.color = _labelHighlightColor;
                }
                else
                {
                    _labelImage.color = _labelBaseColor;
                }
            }
            else
            {
                _spatialAnchor.enabled = true;
                // Show Anchor Menu on select
                _anchorMenu.SetActive(true);
                _mode = AnchorMode.Selected;
                _anchorSelectionMenu.Refocus();
                _anchorVisual.SetActive(false);
                
                if (_isHovered)
                {
                    _labelImage.color = _labelHighlightColor;
                }
                else
                {
                    _labelImage.color = _labelSelectedColor;
                }
            }
        }

        #endregion // Public Methods

        #region Private Methods
        
        private IEnumerator AttemptSaveOverride(int index)
        {
            while (_spatialAnchor && !_spatialAnchor.Created)
            {
                yield return null;
            }

            _spatialAnchor.Save((anchor, success) =>
            {
                if (!success) return;
                Debug.Log($"[SampleAnchor] Successfully saved anchor...{prefabName}");
                ShowSaveIcon = true;
                // Override existing key
                PlayerPrefs.SetString("uuid" + index, anchor.Uuid.ToString());
            });
        }
        
        private void SaveEditMode()
        {
            SampleAnchorUIManager.Instance.SaveEditedAnchor(transform, prefabName, _isLoaded, _savedIndex);
            SampleAnchorUIManager.Instance.ToggleEditingMode();
            if (_isLoaded)
            {
                _spatialAnchor.Erase((anchor, success) =>
                {
                    if (success)
                    {
                        Destroy(gameObject);
                    }
                });   
            }
            else
            {
                Destroy(gameObject);
            }
            
            // _anchorMenu.SetActive(true);
            // _mode = AnchorMode.Selected;
            // _anchorSelectionMenu.Refocus();
            //
            // SampleAnchorUIManager.Instance.ToggleEditingMode();
            //
            // _spatialAnchor.enabled = true;
            
            // RemoveAnchorPreview();
            // SpawnObject();

            // if (_isLoaded)
            // {
            //     SaveOverride(_savedIndex);
            // }
        }

        private void ExitEditModeWithoutSaving()
        {
            _mode = AnchorMode.Selected;
            
            _anchorMenu.SetActive(true);
            _anchorVisual.SetActive(false);
            
            _anchorSelectionMenu.Refocus();
            
            SampleAnchorUIManager.Instance.ToggleEditingMode();
            
            // reset to previous transform
            gameObject.transform.SetPositionAndRotation(_beforeEditPosition, _beforeEditRotation);
            SpawnObject();
            _spatialAnchor.enabled = true;
        }
        
        private void BillboardPanel(Transform panel)
        {
            // The z axis of the panel faces away from the side that is rendered, therefore this code is actually looking away from the camera
            panel.LookAt(
                new Vector3(panel.position.x * 2 - Camera.main.transform.position.x,
                    panel.position.y * 2 - Camera.main.transform.position.y,
                    panel.position.z * 2 - Camera.main.transform.position.z), Vector3.up);
        }

        private void HandleMenuNavigation()
        {
            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
            {
                _anchorSelectionMenu.NavigateToIndexInMenu(false);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown))
            {
                _anchorSelectionMenu.NavigateToIndexInMenu(true);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                _anchorSelectionMenu.Select();
            }
        }
        
        private void HandleEditMode()
        {
            Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            
            if (OVRInput.Get(OVRInput.RawButton.RHandTrigger))
            {
                RotateObject(gameObject, thumbstick);
                MoveObjectUpAndDown(gameObject, thumbstick);
            }
            else
            {
                ManipulateObject(gameObject, thumbstick);
            }
            
            if (OVRInput.GetDown(OVRInput.RawButton.B))
            {
                ExitEditModeWithoutSaving();
            }
            
            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                SaveEditMode();
            }
        }
        
        void ManipulateObject(GameObject obj, Vector2 vector2)
        { 
            float movementSpeed = 0.25f;
            Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            obj.transform.position += new Vector3(thumbstick.x * movementSpeed * Time.deltaTime, 0, thumbstick.y * movementSpeed * Time.deltaTime);
        }

        void MoveObjectUpAndDown(GameObject obj, Vector2 vector2)
        {
            float movementSpeed = 0.25f;
            Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            obj.transform.position += new Vector3(0, thumbstick.y * movementSpeed * Time.deltaTime, 0);
        }
        
        void RotateObject(GameObject obj, Vector2 vector2)
        {
            Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            obj.transform.Rotate(Vector3.up * -thumbstick.x);
        }
        #endregion // Private Methods
    }
}
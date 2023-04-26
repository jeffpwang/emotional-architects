using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Buck.MR
{
    public class SelectionMenu
    {
        public int MenuIndex
        {
            get { return _menuIndex; }
        }
        private List<Button> _menuButtons;
        private int _menuIndex;
        private Button _selectedButton;
        private GameObject menuRoot;
        
        public SelectionMenu(List<Button> buttons, GameObject parent)
        {
            _menuButtons = buttons;
            _menuIndex = 0;
            
            _selectedButton = _menuButtons[0];
            _selectedButton.OnSelect(null);
            menuRoot = parent;
        }

        public void NavigateToIndexInMenu(bool moveNext)
        {
            if (moveNext)
            {
                _menuIndex++;
                if (_menuIndex > _menuButtons.Count - 1)
                {
                    _menuIndex = 0;
                }
            }
            else
            {
                _menuIndex--;
                if (_menuIndex < 0)
                {
                    _menuIndex = _menuButtons.Count - 1;
                }
            }
            
            // if button is hidden, skip it
            if (!_menuButtons[_menuIndex].gameObject.activeInHierarchy)
            {
                NavigateToIndexInMenu(moveNext);
                return;
            }
            _selectedButton.OnDeselect(null);
            _selectedButton = _menuButtons[_menuIndex];
            _selectedButton.OnSelect(null);
        }

        public void Select()
        {
            _selectedButton.OnSubmit(null);
        }
        public void Refocus()
        {
            _selectedButton.OnSelect(null);
        }
        public void Defocus()
        {
            _selectedButton.OnDeselect(null);
        }

        public void ToggleVisibility(bool isVisible)
        {
            menuRoot.SetActive(isVisible);
        }
        
    }
    
    /// <summary>
    /// Manages UI of anchor sample.
    /// </summary>
    [RequireComponent(typeof(SampleSpatialAnchorLoader))]
    public class SampleAnchorUIManager : MonoBehaviour
    {
        public bool UseSurfaceRotation = false;
        public SampleSpatialAnchorLoader SampleSpatialAnchorLoader;
        public OVRCameraRig CameraRig;
        
        /// <summary>
        /// Anchor UI manager singleton instance
        /// </summary>
        public static SampleAnchorUIManager Instance;
        
        public delegate void AnchorAction(Vector3 position);
        public static event AnchorAction OnPlacement;
        public delegate void DoneSetup();
        public static event DoneSetup OnAnchorSetupComplete;
        public delegate void ResetSetup();
        public static event ResetSetup OnAnchorSetupReset;
        /// <summary>
        /// Anchor Mode switches between create and select
        /// </summary>
        public enum AnchorUIMode
        {
            Placement, // add new objects
            Select, // select anchors
            Edit, // edit specific anchor
            Play
        };
        private AnchorUIMode _uiMode = AnchorUIMode.Placement;

        [SerializeField]
        private GameObject _createModeButton;

        [SerializeField]
        private GameObject _objectSelectionMenu;
        
        [SerializeField]
        private GameObject _mainMenuRoot;

        [SerializeField]
        private Transform _trackedDevice;

        private Transform _raycastOrigin;

        private bool _drawSelectionRaycast = false;
        private bool _drawPlacementRaycast = false;
        
        [SerializeField]
        private LineRenderer _selectionLineRenderer;
        [SerializeField]
        private LineRenderer _placementLineRenderer;
        
        private SampleAnchor _hoveredSampleAnchor;

        private SampleAnchor _selectedSampleAnchor;

        [SerializeField]
        private List<Button> _mainMenuButtonList; // CREATE, LOAD, NAV MESH, PLAY
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _stopButton;
        
        [SerializeField]
        private List<Button> _objectsButtonList; // OBJ A, OBJ B, OBJ C, EXIT
        
        [SerializeField]
        private List<GameObject> _prefabsList; // TODO: Move this elsewhere

        private SelectionMenu _mainMenu;
        private SelectionMenu _objectsMenu;

        private delegate void PrimaryPressDelegate();

        private PrimaryPressDelegate _primaryPressDelegate;

        private bool _isMainMenuFocused = true; // focus is on menu
        
        [SerializeField] private LayerMask _selectionLayerMask; // only select spatial anchors
        // placement 
        [SerializeField] private LayerMask _placementLayerMask; // objects can be placed only on this layer
        
        private SampleAnchor _placementPreview;

        private GameObject _objectToSpawn; // selected from options
        
        // for joystick rotation
        private Vector2 _lastValidDirection; 
        private Quaternion _initialRotation;
        private Quaternion _currentRotation;

        #region Monobehaviour Methods

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
            _raycastOrigin = _trackedDevice;
            
            _mainMenu = new SelectionMenu(_mainMenuButtonList, _mainMenuRoot);
            _objectsMenu = new SelectionMenu(_objectsButtonList, _mainMenuRoot);

            _selectionLineRenderer.startWidth = 0.005f;
            _selectionLineRenderer.endWidth = 0.005f;

            ToggleCreateMode();
        }

        private void Update()
        {
            // disable menu if using hands
            if (!OVRInput.IsControllerConnected(OVRInput.Controller.RTouch))
            {
                _mainMenuRoot.SetActive(false);
                return;
            }
            
            if (_uiMode != AnchorUIMode.Play)
            {
                _mainMenuRoot.SetActive(true);
            }
            
            if (_drawSelectionRaycast && _uiMode == AnchorUIMode.Select)
            {
                SelectionRaycast();
            }
            // PLACEHOLDER PREVIEW MODE
            if (_drawPlacementRaycast && _placementPreview != null)
            {
                PlacementRaycast();
                HandleAnchorManipulation();
                
                // END PLACEMENT MODE
                if (OVRInput.GetDown(OVRInput.RawButton.B))
                {
                    HidePlacementRaycastLine();
                    Destroy(_placementPreview.gameObject);
                }
            }
            
            if (_selectedSampleAnchor == null && _placementPreview == null)
            {
                // Refocus menu
                _mainMenu.Refocus();
                _isMainMenuFocused = true;
            }
            else if (_uiMode == AnchorUIMode.Select)
            {
                // exit from anchor selection menu
                if (OVRInput.GetDown(OVRInput.RawButton.B))
                {
                    ExitAnchorSelection();
                }
            }
            
            // CONTROLLER INPUT
            HandleMenuNavigation();
            
            if (_uiMode == AnchorUIMode.Select || _uiMode == AnchorUIMode.Placement)
            {
                if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
                {
                    _primaryPressDelegate?.Invoke();
                }   
            } 
            else if (_uiMode == AnchorUIMode.Play)
            {
                if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
                {
                    OnStopButtonPressed();
                }   
            }

        }

        #endregion // Monobehaviour Methods
        
        #region Menu UI Callbacks
        /// <summary>
        /// Place Object spatial anchor in scene. 
        /// </summary>
        public void OnObjectButtonPressed(string objectName)
        {
            _objectToSpawn = null;
            foreach (var prefab in _prefabsList)
            {
                if (prefab.GetComponent<SampleAnchor>().prefabName == objectName)
                {
                    _objectToSpawn = prefab;
                }
            }

            if (_objectToSpawn == null)
            {
                Debug.LogError($"OBJECT {objectName} NOT FOUND!");
                return;
            }

            if (_placementPreview != null)
            {
                Destroy(_placementPreview.gameObject);   
            }
            
            // show line
            ShowPlacementRaycastLine();
            
            // show preview
            GameObject placementPreview = Instantiate(_objectToSpawn);
            _placementPreview = placementPreview.GetComponent<SampleAnchor>();
            _placementPreview.ShowPreview();
            // TODO: create preview instance without these components active (prevents glitchiness)
            // _placementPreview.GetComponent<OVRSpatialAnchor>().enabled = false;
            // _placementPreview.GetComponent<SampleAnchor>().enabled = false;
            // _placementPreview.GetComponent<SphereCollider>().enabled = false;
        }
        
        /// <summary>
        /// Create mode button pressed UI callback. Referenced by the Create button in the menu.
        /// </summary>
        public void OnCreateModeButtonPressed()
        {
            ToggleCreateMode();
            var mainMenu = _createModeButton.transform.parent;
            mainMenu.gameObject.SetActive(!mainMenu.gameObject.activeSelf);
            _objectSelectionMenu.SetActive(!_objectSelectionMenu.activeSelf);
        }

        /// <summary>
        /// Load anchors button pressed UI callback. Referenced by the Load Anchors button in the menu.
        /// </summary>
        public void OnLoadAnchorsButtonPressed()
        {
            SampleSpatialAnchorLoader.LoadAnchorsByUuid();
        }

        public void OnEraseAnchorsButtonPressed()
        {
            SampleSpatialAnchorLoader.EraseAllAnchors();
        }

        public void OnPlayButtonPressed()
        {
            _uiMode = AnchorUIMode.Play;

            HideMenu();
            
            OnAnchorSetupComplete?.Invoke();
        }
        
        public void OnStopButtonPressed()
        {
            _uiMode = AnchorUIMode.Select;

            ShowMenu();
            
            OnAnchorSetupReset?.Invoke();
        }
        
        #endregion // Menu UI Callbacks

        #region Mode Handling

        public void ToggleEditingMode()
        {
            if (_uiMode == AnchorUIMode.Select)
            {
                _uiMode = AnchorUIMode.Edit;
            }
            else
            {
                _uiMode = AnchorUIMode.Select;
            }

            Debug.Log($"[SampleAnchorUIManager] {_uiMode}");
        }

        public void SaveEditedAnchor(Transform newTransform, string objectName, bool isSaved, int index = 0)
        {
            GameObject newSpatialAnchor;
            foreach (var prefab in _prefabsList)
            {
                var sampleAnchor = prefab.GetComponent<SampleAnchor>();
                if (sampleAnchor == null)
                {
                    Debug.LogError("NO SAMPLE ANCHOR COMPONENT FOUND");
                    return;
                }
                if (sampleAnchor.prefabName == objectName)
                {
                    newSpatialAnchor = Instantiate(prefab, newTransform.position, newTransform.rotation);
                    sampleAnchor = newSpatialAnchor.GetComponent<SampleAnchor>();
                    if (isSaved)
                    {
                        sampleAnchor.SaveOverride(index);   
                    }
                    sampleAnchor.SpawnObject();
                    return;
                }
            }
        }
        private void ToggleCreateMode()
        {
            if (_uiMode == AnchorUIMode.Select)
            {
                _uiMode = AnchorUIMode.Placement;
                EndSelectMode();
                StartPlacementMode();
            }
            else
            {
                _uiMode = AnchorUIMode.Select;
                EndPlacementMode();
                StartSelectMode();
            }
        }

        private void StartPlacementMode()
        {
            _primaryPressDelegate = PlaceAnchor;
        }

        private void EndPlacementMode()
        {
            HidePlacementRaycastLine();
            HideAnchorPreview();
            _primaryPressDelegate = null;
        }

        private void StartSelectMode()
        {
            ShowSelectionRaycastLine();
            _primaryPressDelegate = SelectAnchor;
        }

        private void EndSelectMode()
        {
            HideSelectionRaycastLine();
            _primaryPressDelegate = null;
        }

        #endregion // Mode Handling

        #region Private Methods
        
        private void ShowMenu()
        {
            _mainMenu.ToggleVisibility(true);
            _isMainMenuFocused = true;
        }

        private void HideMenu()
        {
            _mainMenu.ToggleVisibility(false);
            _isMainMenuFocused = false;
        }
        
        private void HandleMenuNavigation()
        {
            if (!_isMainMenuFocused)
            {
                return;
            }

            SelectionMenu menu = (_uiMode == AnchorUIMode.Placement) ? _objectsMenu : _mainMenu;

            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
            {
                menu.NavigateToIndexInMenu(false);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown))
            {
                menu.NavigateToIndexInMenu(true);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                menu.Select();
            }
            
            if (OVRInput.GetDown(OVRInput.RawButton.B))
            {
                if (_uiMode == AnchorUIMode.Placement)
                {
                    OnCreateModeButtonPressed();
                }
            }
        }

        private void ShowAnchorPreview()
        {
            if (_placementPreview != null)
            {
                _placementPreview.ShowAnchorPreview();   
            }
        }

        private void HideAnchorPreview()
        {
            if (_placementPreview != null)
            {
                _placementPreview.HideAnchorPreview();
            }
        }

        private void PlaceAnchor()
        {
            if (_placementPreview != null)
            {
                _placementPreview.SpawnObject();
                // Place spatial anchor
                // Instantiate(_objectToSpawn, _placementPreview.transform.position, _placementPreview.transform.rotation);
                HidePlacementRaycastLine();
                // Destroy(_placementPreview.gameObject);
            }
        }
        private void HandleAnchorManipulation()
        {
            Vector2 rightDir = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
            float xValue = rightDir.x;
            
            if (Mathf.Abs(xValue) > 0.1)
            {
                _placementPreview.transform.rotation *= Quaternion.AngleAxis(xValue * 3f, Vector3.up);
            }
        }
        
        /// <summary>
        /// Return a quaternion for the Y axis of the HMD's orientation. 
        /// Used by orientation handlers to track the current heading before processing user input to adjust it.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetHeadRotationY()
        {
            Quaternion headRotation = Quaternion.identity;
#if UNITY_2019_1_OR_NEWER
            UnityEngine.XR.InputDevice device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
            if (device.isValid)
            {
                device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out headRotation);
            }
#elif UNITY_2017_2_OR_NEWER
		List<UnityEngine.XR.XRNodeState> nodeStates = new List<UnityEngine.XR.XRNodeState>();
		UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);
		foreach (UnityEngine.XR.XRNodeState n in nodeStates)
		{
			if (n.nodeType == UnityEngine.XR.XRNode.Head)
			{
				n.TryGetRotation(out headRotation);
				break;
			}
		}
#else
		headRotation = InputTracking.GetLocalRotation(VRNode.Head);
#endif
            Vector3 euler = headRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            headRotation = Quaternion.Euler(euler);
            return headRotation;
        }
        
        private void ShowSelectionRaycastLine()
        {
            _drawSelectionRaycast = true;
            _selectionLineRenderer.gameObject.SetActive(true);
        }

        private void HideSelectionRaycastLine()
        {
            _drawSelectionRaycast = false;
            _selectionLineRenderer.gameObject.SetActive(false);
        }
        private void ShowPlacementRaycastLine()
        {
            _drawPlacementRaycast = true;
            _placementLineRenderer.gameObject.SetActive(true);
            _isMainMenuFocused = false;
        }

        private void HidePlacementRaycastLine()
        {
            _drawPlacementRaycast = false;
            _placementLineRenderer.gameObject.SetActive(false);
            _isMainMenuFocused = true;
        }
        
        private void SelectionRaycast()
        {
            Ray ray = new Ray(_raycastOrigin.position, _raycastOrigin.TransformDirection(Vector3.forward));
            _selectionLineRenderer.SetPosition(0, _raycastOrigin.position);
            _selectionLineRenderer.SetPosition(1,
                _raycastOrigin.position + _raycastOrigin.TransformDirection(Vector3.forward) * 10f);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, _selectionLayerMask))
            {
                SampleAnchor sampleAnchorObject = hit.collider.GetComponent<SampleAnchor>();
                if (sampleAnchorObject != null)
                {
                    _selectionLineRenderer.SetPosition(1, hit.point);

                    HoverAnchor(sampleAnchorObject);
                    return;
                }
            }

            UnhoverAnchor();
        }
        
        private void PlacementRaycast()
        {
            Ray ray = new Ray(_raycastOrigin.position, _raycastOrigin.TransformDirection(Vector3.forward));
            _placementLineRenderer.SetPosition(0, _raycastOrigin.position);
            _placementLineRenderer.SetPosition(1,
                _raycastOrigin.position + _raycastOrigin.TransformDirection(Vector3.forward) * 10f);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, _placementLayerMask))
            {
                ShowAnchorPreview();
                _placementLineRenderer.SetPosition(1, hit.point);
                _placementPreview.transform.position = hit.point;

                if (UseSurfaceRotation)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Room_Wall"))
                    {
                        _placementPreview.transform.rotation = Quaternion.FromToRotation(Vector3.back, hit.normal);
                    }
                    else
                    {
                        _placementPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);   
                    }   
                }
            }
            else
            {
                HideAnchorPreview();
            }
        }
        
        private void HoverAnchor(SampleAnchor sampleAnchor)
        {
            _hoveredSampleAnchor = sampleAnchor;
            _hoveredSampleAnchor.OnHoverStart();
        }

        private void UnhoverAnchor()
        {
            if (_hoveredSampleAnchor == null)
            {
                return;
            }

            _hoveredSampleAnchor.OnHoverEnd();
            _hoveredSampleAnchor = null;
        }

        private void SelectAnchor()
        {
            if (_hoveredSampleAnchor != null)
            {
                if (_selectedSampleAnchor != null)
                {
                    // Deselect previous Anchor
                    _selectedSampleAnchor.OnSelect();
                    _selectedSampleAnchor = null;
                }

                // Select new Anchor
                _selectedSampleAnchor = _hoveredSampleAnchor;
                _selectedSampleAnchor.OnSelect();
                
                _mainMenu.Defocus();
                _isMainMenuFocused = false;
            }
            else
            {
                ExitAnchorSelection();
            }
        }

        private void ExitAnchorSelection()
        {
            if (_selectedSampleAnchor != null)
            {
                // Deselect previous Anchor
                _selectedSampleAnchor.OnSelect();
                _selectedSampleAnchor = null;
                

                _mainMenu.Refocus();
                _isMainMenuFocused = true;
            }
        }
        #endregion // Private Methods
    }

}
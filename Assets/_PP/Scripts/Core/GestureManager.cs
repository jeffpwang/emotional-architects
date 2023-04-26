using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Meta.PP
{
    public class GestureManager : MonoBehaviour
    {
        public static GestureManager Instance;

        public HandUI handUI;
        
        [Header("Hands")]
        public HandVisual _leftHandVisual;
        public HandVisual _rightHandVisual;
        [SerializeField]
        private OVRHand _leftHand, _rightHand;

        [SerializeField, Interface(typeof(IHand))]
        private MonoBehaviour _rightHandRef;
        public IHand HandRightRef { get; private set; }
        
        [SerializeField, Interface(typeof(IHand))]
        private MonoBehaviour _leftHandRef;
        public IHand HandLeftRef { get; private set; }
        public bool _usingHands { get; private set; }
        public event Action WhenSwipeLeft = delegate { };
        public event Action WhenSwipeRight = delegate { };
        public event Action WhenPinchSelect = delegate { };
        
        [SerializeField]
        private UnityEvent _whenSwipeLeftActivated;
        [SerializeField]
        private UnityEvent _whenSwipeLeftDeactivated;
        
        [SerializeField]
        private UnityEvent _whenSwipeRightActivated;
        [SerializeField]
        private UnityEvent _whenSwipeRightDeactivated;
        
        [SerializeField]
        private UnityEvent _whenPinchSelectActivated;
        [SerializeField]
        private UnityEvent _whenPinchSelectDeactivated;

        public float pinchTimer = 1.5f;
        public UnityEvent OnSelected;
        
        [SerializeField, Interface(typeof(IActiveState))]
        private MonoBehaviour _swipeGestureLeft, _swipeGestureRight;

        [SerializeField, Interface(typeof(IInteractable))]
        private List<MonoBehaviour> _interactables;
        
        private IActiveState SwipeLeft;
        private IActiveState SwipeRight;

        private bool _savedSwipeLeftState;
        private bool _savedSwipeRightState;
        private bool _isLeftIndexFingerPinching;
        private bool _isRightIndexFingerPinching;

        private Coroutine pinchCouroutine;
        
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
            
            SwipeLeft = _swipeGestureLeft as IActiveState;
            SwipeRight = _swipeGestureRight as IActiveState;
            
            HandLeftRef = _leftHandRef as IHand;
            HandRightRef = _rightHandRef as IHand;
        }

        void Start()
        {
            Assert.IsNotNull(_leftHand);
            Assert.IsNotNull(_rightHand);
            Assert.IsNotNull(SwipeLeft);
            Assert.IsNotNull(SwipeRight);
            Assert.IsNotNull(HandLeftRef);
            Assert.IsNotNull(HandRightRef);
            
            _savedSwipeLeftState = false;
            _savedSwipeRightState = false;

            HandLeftRef.WhenHandUpdated += HandleLeftHandUpdated;
            HandRightRef.WhenHandUpdated += HandleRightHandUpdated;
        }

        private void OnDestroy()
        {
            HandLeftRef.WhenHandUpdated -= HandleLeftHandUpdated;
            HandRightRef.WhenHandUpdated -= HandleRightHandUpdated;
        }

        private void HandleRightHandUpdated()
        {
            var prevPinching = _isRightIndexFingerPinching;
            _isRightIndexFingerPinching = HandRightRef.GetIndexFingerIsPinching();
            if (prevPinching != _isRightIndexFingerPinching)
            {
                if (_isRightIndexFingerPinching)
                {
                    PinchSelectStart();
                }
                else
                {
                    PinchSelectEnd();
                }
            }
        }

        private void HandleLeftHandUpdated()
        {
            var prevPinching = _isLeftIndexFingerPinching;
            _isLeftIndexFingerPinching = HandLeftRef.GetIndexFingerIsPinching();
            if (prevPinching != _isLeftIndexFingerPinching)
            {
                if (_isLeftIndexFingerPinching)
                {
                    PinchSelectStart();
                }
                else
                {
                    PinchSelectEnd();
                }
            }
        }
        
        void Update()
        {
            if (_leftHand.IsDataHighConfidence)
            {
                if (_savedSwipeLeftState != SwipeLeft.Active)
                {
                    _savedSwipeLeftState = SwipeLeft.Active;
                    SwipeLeftEvent();
                }
            }

            if (_rightHand.IsDataHighConfidence)
            {
                if (_savedSwipeRightState != SwipeRight.Active)
                {
                    _savedSwipeRightState = SwipeRight.Active;
                    SwipeRightEvent();
                }
            }

            // debug gestures
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                _savedSwipeLeftState = true;
                SwipeLeftEvent();
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                _savedSwipeRightState = true;
                SwipeRightEvent();
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                PinchSelectStart();
            }
            
            // disable a hand if it's not tracked (avoiding ghost hands)
            if (_leftHand && _rightHand)
            {
                _leftHandVisual.ForceOffVisibility = !_leftHand.IsTracked;
                _rightHandVisual.ForceOffVisibility = !_rightHand.IsTracked;
            }
            
            var usingHands = (
                OVRInput.GetActiveController() == OVRInput.Controller.Hands ||
                OVRInput.GetActiveController() == OVRInput.Controller.LHand ||
                OVRInput.GetActiveController() == OVRInput.Controller.RHand ||
                OVRInput.GetActiveController() == OVRInput.Controller.None);
            
            if (usingHands != _usingHands)
            {
                _usingHands = usingHands;
                if (usingHands)
                {
                    Debug.Log("[BrainGestureManager] SWITCH TO HANDS");
                }
                else
                {
                    Debug.Log("[BrainGestureManager] SWITCH TO CONTROLLERS");
                }
            }
        }
        
        private void SwipeLeftEvent()
        {
            if (_savedSwipeLeftState)
            {
                Debug.Log("Swipe Left");
                _whenSwipeLeftActivated?.Invoke();
                WhenSwipeLeft?.Invoke();
            }
            else
            {
                Debug.Log("Swipe Deactivated");
                _whenSwipeLeftDeactivated?.Invoke();
            }
        }
        private void SwipeRightEvent()
        {
            if (_savedSwipeRightState)
            {
                Debug.Log("Swipe Right");
                _whenSwipeRightActivated?.Invoke();
                WhenSwipeRight?.Invoke();
            }
            else
            {
                Debug.Log("Swipe Deactivated");
                _whenSwipeRightDeactivated?.Invoke();
            }
        }
        private void PinchSelectStart()
        {
            if (pinchCouroutine != null)
            {
                StopCoroutine(pinchCouroutine);
                handUI.SetFillAmount(0);
            }
            pinchCouroutine = StartCoroutine(BeginPinchTimer());
            _whenPinchSelectActivated?.Invoke();
            WhenPinchSelect?.Invoke();
        }

        IEnumerator BeginPinchTimer()
        {
            float time = 0;
            while (time < pinchTimer)
            {
                handUI.SetFillAmount(time / pinchTimer);
                time += Time.deltaTime;
                
                yield return null;
            }
            Debug.Log("Pinch");
            // _whenPinchSelectActivated?.Invoke();
            // WhenPinchSelect?.Invoke();
        }

        private void PinchSelectEnd()
        {
            if (pinchCouroutine != null)
            {
                StopCoroutine(pinchCouroutine);
                handUI.SetFillAmount(0);
            }
            _whenPinchSelectDeactivated?.Invoke();
        }
        
    }
    
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Meta.PP
{
    public class NotificationHandler : MonoBehaviour
    {
        public enum AnimationFrom
        {
            Left, 
            Right, 
            Up, 
            Down
        }
        
        [Header("Animation")]
        public Transform root;

        public List<GameObject> childElements;
        
        public AnimationFrom animateIn = AnimationFrom.Left;
        public float animateInDistance = 2f;
        public float animSpeed = 2f;
        
        [Header("Camera Tracking")]
        public Vector3 cameraOffset;

        [Header("Brain Sequence")] 
        public float holdTime = 3f;
        public float delayBeforeNext = 1f;

        public bool autoGoToNext = true;
        public bool autoTransitionOut = false;
        public float smoothTime = 0.3f;
        Vector3 _velocity = Vector3.zero;
        Vector3 _startPosition, _endPosition;
        Camera _camera;
        public bool isAnimating = false;
        
        void Awake()
        {
            _camera = Camera.main;

            root.gameObject.SetActive(false);
        }

        private void Start()
        {
            transform.position = _camera.transform.TransformPoint(cameraOffset);
            for (int i = 0; i < childElements.Count; i++)
            {
                TrackToCamera trackable = childElements[i].GetComponent<TrackToCamera>();
                if (trackable != null)
                {
                    trackable.SetTargetPosition(cameraOffset);
                    trackable.SetBrainNotification(this);
                }
            }
            AnimateIn();
        }
        
        protected void Update()
        {
            // debug
            if (Input.GetKeyDown(KeyCode.Q))
            {
                AnimateIn();
            }
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                AnimateOut();
            }

            if (!isAnimating) // track to camera when not animating
            {
                var _targetPosition = _camera.transform.TransformPoint(cameraOffset);
                root.transform.position = Vector3.SmoothDamp(root.transform.position, _targetPosition, ref _velocity, smoothTime);
            }
            // face camera
            var lookAtPos = new Vector3(_camera.transform.position.x, transform.position.y, _camera.transform.position.z); 
            transform.LookAt(lookAtPos);   
        }

        IEnumerator StartAnimateIn()
        {
            float time = 0;
            isAnimating = true;
            while (time < animSpeed)
            {
                root.localPosition = Vector3.Lerp(_startPosition, _endPosition, time / animSpeed);
                time += Time.deltaTime;
                yield return null;
            }
            
            root.localPosition = _endPosition;
            isAnimating = false;
            // animate out automatically
            if (autoTransitionOut)
            {
                yield return new WaitForSeconds(holdTime);
                AnimateOut();
            }
        }
        
        IEnumerator StartAnimateOut()
        {
            float time = 0;
            isAnimating = true;

            while (time < animSpeed)
            {
                root.localPosition = Vector3.Lerp(_endPosition, _startPosition, time / animSpeed);
                time += Time.deltaTime;
                yield return null;
            }
            
            root.localPosition = _startPosition;
            isAnimating = false;
            // destroy when animates out
            if (autoGoToNext)
            {
                Debug.Log($"[BrainNotification] Auto go to next from {gameObject.name}");
                AppManager.Instance.MoveToNextSequence(true);
                Destroy(gameObject);
            }
        }
        public void AnimateIn()
        {
            switch (animateIn)
            {
                case AnimationFrom.Left:
                    _startPosition = Vector3.right;
                    // _startPosition = _camera.transform.TransformPoint(cameraOffset + Vector3.right * 2);
                    break;
                case AnimationFrom.Right:
                    _startPosition = Vector3.left;
                    break;
                case AnimationFrom.Up:
                    _startPosition = Vector3.down;
                    break;
                case AnimationFrom.Down:
                    _startPosition = Vector3.up;
                    // _camera.transform.TransformPoint(Vector3.up * 2);
                    // _startPosition = _camera.transform.TransformPoint(cameraOffset + Vector3.up * 2);
                    break;
            }

            _startPosition *= animateInDistance; // apply multiplier
            // var newStartPosition = transform.InverseTransformPoint(_startPosition);
            root.localPosition = _startPosition;
            root.gameObject.SetActive(true);
            
            StartCoroutine(StartAnimateIn());
        }
        public void AnimateOut()
        {           
            // switch (animateIn)
            // {
            //     case AnimationFrom.Left:
            //         _startPosition = Vector3.right;
            //         break;
            //     case AnimationFrom.Right:
            //         _startPosition = Vector3.left;
            //         break;
            //     case AnimationFrom.Up:
            //         _startPosition = Vector3.down;
            //         break;
            //     case AnimationFrom.Down:
            //         _startPosition = Vector3.up;
            //         break;
            // }
            //
            // _startPosition *= animateInDistance; // apply multiplier
            
            StartCoroutine(StartAnimateOut());
        }
    }
}

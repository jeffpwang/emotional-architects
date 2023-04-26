using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.PP
{
    public class TrackToCamera : MonoBehaviour
    {
        // tracking camera smooth
        public float smoothTime = 0.3f;
        public bool stopOnAnimate = false;
        
        private Vector3 _cameraOffset;
        private Vector3 _targetPosition;
        Vector3 _velocity = Vector3.zero;
        Camera _camera;
        private Vector3 originalPosition;
        private NotificationHandler _parentNotificationHandler;
        void Awake()
        {
            _camera = Camera.main;
            originalPosition = transform.localPosition;
        }
        
        private void LateUpdate()
        {
            if (_parentNotificationHandler == null || (_parentNotificationHandler.isAnimating && stopOnAnimate))
            {
                // face camera
                var lookAtPos_2 = new Vector3(_camera.transform.position.x, transform.position.y, _camera.transform.position.z);
                transform.rotation = Quaternion.LookRotation(transform.position - lookAtPos_2);
                
                return;
            }
            
            // track camera with smoothing
            _targetPosition =  _camera.transform.TransformPoint(_cameraOffset + originalPosition); // get world position of local
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, smoothTime);
            
            // face camera
            var lookAtPos = new Vector3(_camera.transform.position.x, transform.position.y, _camera.transform.position.z);
            transform.rotation = Quaternion.LookRotation(transform.position - lookAtPos);
        }

        public void SetTargetPosition(Vector3 targetPosition)
        {
            _cameraOffset = targetPosition;
        }
        
        public void SetBrainNotification(NotificationHandler notificationHandler)
        {
            _parentNotificationHandler = notificationHandler;
        }
    }
}

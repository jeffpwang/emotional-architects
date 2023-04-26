using System;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.PP
{
    public class HandUI : MonoBehaviour
    {
        public GameObject root;
        public Image fill;

        private Camera _camera;

        private void OnEnable()
        {
            _camera = Camera.main;
            fill.fillAmount = 0;
        }

        public void SetFillAmount(float percentage)
        {
            fill.fillAmount = percentage;
        }

        public void Update()
        {
            var lookAtPos = new Vector3(_camera.transform.position.x, _camera.transform.position.y, _camera.transform.position.z); 
            root.transform.LookAt(lookAtPos);
        }
    }

}
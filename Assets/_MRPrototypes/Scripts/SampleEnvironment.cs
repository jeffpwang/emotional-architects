// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.MR
{
    public class SampleEnvironment : MonoBehaviour
    {
        static public SampleEnvironment Instance = null;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
        }

        public void Initialize()
        {
            CullForegroundObjects();
        }

        /// <summary>
        /// If an object contains the ForegroundObject component and is inside the room, destroy it.
        /// </summary>
        void CullForegroundObjects()
        {
            ForegroundObject[] foregroundObjects = GetComponentsInChildren<ForegroundObject>();
            foreach (ForegroundObject obj in foregroundObjects)
            {
                if (VirtualRoom.Instance.IsPositionInRoom(obj.transform.position, 2.0f))
                {
                    Destroy(obj.gameObject);
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
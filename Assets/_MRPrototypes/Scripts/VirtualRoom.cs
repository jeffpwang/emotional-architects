// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.MR
{
    public class VirtualRoom : MonoBehaviour
    {
        static public VirtualRoom Instance = null;
        // List<SampleRoomObject> _roomboxFurnishings = new List<SampleRoomObject>();
        // List<SampleRoomObject> _roomboxWalls = new List<SampleRoomObject>();

        List<Vector3> _cornerPoints = new List<Vector3>();


        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
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

        /// <summary>
        /// Create the actual room surfaces, since the OVRSceneObject array contains just metadata.
        /// Attach the instantiated walls/furniture to the anchors, to ensure they're fixed to the real world.
        /// </summary>
        public void Initialize(OVRSceneAnchor[] sceneAnchors)
        {
        }

        /// <summary>
        /// Simple bounding-box test to see if a position (with buffer radius) is in the room. Ignores height.
        /// </summary>
        public bool IsPositionInRoom(Vector3 pos, float positionBuffer, bool alsoCheckVertical = false)
        {
            bool inRoom = false;
            // float xMin = 0.0f;
            // float xMax = 0.0f;
            // float yMin = 0.0f;
            // float yMax = 0.0f;
            // float zMin = 0.0f;
            // float zMax = 0.0f;
            // int anyWall = 0;
            // for (int i = 0; i < _roomboxWalls.Count; i++)
            // {
            //     if (!_roomboxWalls[i]._isWall)
            //     {
            //         continue;
            //     }
            //     anyWall = i;
            //     Vector3 wallRight = -_roomboxWalls[i].transform.right;
            //     Vector3 wallScale = _roomboxWalls[i]._passthroughMesh.transform.localScale;
            //     Vector3 pos1 = _roomboxWalls[i].transform.position - wallRight * wallScale.x * 0.5f;
            //     Vector3 pos2 = _roomboxWalls[i].transform.position + wallRight * wallScale.x * 0.5f;
            //     if (pos1.x < xMin) xMin = pos1.x;
            //     if (pos1.x > xMax) xMax = pos1.x;
            //     if (pos1.z < zMin) zMin = pos1.z;
            //     if (pos1.z > zMax) zMax = pos1.z;
            // }
            // inRoom = (pos.x > xMin - positionBuffer) && (pos.x < xMax + positionBuffer) && (pos.z > zMin - positionBuffer) && (pos.z < zMax + positionBuffer);
            // if (alsoCheckVertical)
            // {
            //     float floorPos = (_roomboxWalls[anyWall].transform.position - Vector3.up * _roomboxWalls[anyWall]._passthroughMesh.transform.localScale.y * 0.5f).y;
            //     float ceilingPos = (_roomboxWalls[anyWall].transform.position + Vector3.up * _roomboxWalls[anyWall]._passthroughMesh.transform.localScale.y * 0.5f).y;
            //     inRoom &= (pos.y > floorPos + positionBuffer);
            //     inRoom &= (pos.y < ceilingPos - positionBuffer);
            // }
            return inRoom;
        }

        /// <summary>
        /// Point-in-polygon test to see if position is in room
        /// </summary>
        public bool IsPlayerInRoom()
        {
            Vector3 cameraPos = SampleAppManager.Instance._mainCamera.transform.position;
            cameraPos = new Vector3(cameraPos.x, _cornerPoints[0].y, cameraPos.z);
            // Shooting a ray from player to the right (X+), count how many walls it intersects.
            // If the count is odd, the player is in the room
            // Unfortunately we can't use Physics.RaycastAll, because the collision may not match the mesh, resulting in wrong counts
            int lineCrosses = 0;
            for (int i = 0; i < _cornerPoints.Count; i++)
            {
                Vector3 startPos = _cornerPoints[i];
                Vector3 endPos = (i == _cornerPoints.Count - 1) ? _cornerPoints[0] : _cornerPoints[i + 1];

                // get bounding box of line segment
                float xMin = startPos.x < endPos.x ? startPos.x : endPos.x;
                float xMax = startPos.x > endPos.x ? startPos.x : endPos.x;
                float zMin = startPos.z < endPos.z ? startPos.z : endPos.z;
                float zMax = startPos.z > endPos.z ? startPos.z : endPos.z;
                Vector3 lowestPoint = startPos.z < endPos.z ? startPos : endPos;
                Vector3 highestPoint = startPos.z > endPos.z ? startPos : endPos;

                // it's vertically within the bounds, so it might cross
                if (cameraPos.z <= zMax &&
                    cameraPos.z >= zMin)
                {
                    if (cameraPos.x <= xMin)
                    {
                        // it's completely to the left of this line segment's bounds, so must intersect
                        lineCrosses++;
                    }
                    else if (cameraPos.x < xMax)
                    {
                        // it's within the bounds, so further calculation is needed
                        Vector3 lineVec = (highestPoint - lowestPoint).normalized;
                        Vector3 camVec = (cameraPos - lowestPoint).normalized;
                        // polarity of cross product defines which side the point is on
                        if (Vector3.Cross(lineVec, camVec).y < 0)
                        {
                            lineCrosses++;
                        }
                    }
                    // else it's completely to the right of the bounds, so it definitely doesn't cross
                }
            }

            return (lineCrosses % 2) == 1;
        }

    }
}

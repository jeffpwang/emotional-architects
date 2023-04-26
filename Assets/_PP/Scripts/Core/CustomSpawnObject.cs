using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.PP
{
    public class CustomSpawnObject : CustomSequence
    {
        // public enum SpawnBehaviour
        // {
        //     SetAsChild, 
        //     UseTransform, 
        //     UsePositionOnly, 
        //     UseOriginal
        // }
        // public GameObject prefabToSpawn;
        //
        // public SpawnBehaviour spawnBehaviour;
        //
        // public UnityEvent OnPlaySequence;
        // public UnityEvent OnEndSequence;
        //
        // private GameObject _instantiatedObject;
        //
        // protected override void CustomAction()
        // {
        //     switch (spawnBehaviour)
        //     {
        //         case SpawnBehaviour.SetAsChild:
        //             _instantiatedObject = Instantiate(prefabToSpawn, gameObject.transform);
        //             break;
        //         case SpawnBehaviour.UseTransform:
        //             _instantiatedObject = Instantiate(prefabToSpawn, gameObject.transform.position, gameObject.transform.rotation);
        //             break;
        //         case SpawnBehaviour.UsePositionOnly:
        //             _instantiatedObject = Instantiate(prefabToSpawn, gameObject.transform.position, Quaternion.identity);
        //             break;
        //         case SpawnBehaviour.UseOriginal:
        //             _instantiatedObject = Instantiate(prefabToSpawn);
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        //     OnPlaySequence?.Invoke();
        //     Debug.Log($"[BrainSequence] Spawn object... {_instantiatedObject.name}");
        // }
        //
        // public override void EndSequence()
        // {
        //     base.EndSequence();
        //     
        //     OnEndSequence?.Invoke();
        //     
        //     if (_instantiatedObject)
        //     {
        //         Destroy(_instantiatedObject);
        //     }
        // }
        
    }
}

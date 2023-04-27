using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _damping = 5;

    private void Update()
    {
        if (_target != null)
        {
            var lookPos = _target.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * _damping);
        }
    }
}
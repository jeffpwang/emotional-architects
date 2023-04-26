using Meta.PP;
using UnityEngine;

public class CombinedCubeVoiceController : MonoBehaviour
{
    [SerializeField] private float _maximumScale = 2.0f;

    private AudioManager _audioManager;
    private float _currentScale;

    private void Awake()
    {
        _audioManager = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        if (_audioManager.IsPlaying())
        {
            Scale();
        }
    }

    private void Scale()
    {
        _currentScale = Time.time;
        Vector3 nextScale = new Vector3(Mathf.PingPong(_currentScale, _maximumScale - 1) + 1, Mathf.PingPong(_currentScale, _maximumScale - 1) + 1, 0);
        transform.localScale = nextScale;
    }
}

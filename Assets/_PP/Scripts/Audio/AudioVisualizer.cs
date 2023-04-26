using UnityEngine;

namespace Meta.PP
{
    public class AudioVisualizer : MonoBehaviour
    {
        [SerializeField] private APIIntegration APIIntegration;

        private void OnEnable()
        {
            Events.AddListener<AudioVisualizerEvent>(Visualize);
        }

        private void OnDisable()
        {
            Events.RemoveListener<AudioVisualizerEvent>(Visualize);
        }

        private void Visualize(AudioVisualizerEvent audioVisualizerEvent)
        {
            StartCoroutine(APIIntegration.GenerateImage(audioVisualizerEvent.Text, gameObject));
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ImageGenerationParameters
{
    public string key;
    public string prompt;
    public string negative_prompt;
    public string height;
    public string width;
    public string samples;
    public string seed;
    public string num_inference_steps;
    public double guidance_scale;
    public string safety_checker;
    public string webhook;
    public string track_id;
}

public class APIIntegration : MonoBehaviour
{
    // Set up variables for API keys and endpoints
    private string stableDiffusionApiKey = "tAfSlw05pS52QWvw8KcnnSRwSJamxWdhuKXT5g39BrhINiQqMk7FgpPc1CO0";
    private string stableDiffusionEndpoint = "https://stablediffusionapi.com/api/v3/text2img";

    public IEnumerator GenerateImage(string prompt, GameObject sphere)
    {
        // Create an instance of ImageGenerationParameters and set its values
        ImageGenerationParameters parameters = new ImageGenerationParameters();
        parameters.key = stableDiffusionApiKey;
        parameters.prompt = $"{prompt} panorama 4k photorealistic aesthetic";
        parameters.negative_prompt = "";
        parameters.width = "1024";
        parameters.height = "1024";
        parameters.samples = "1";
        parameters.seed = null;
        parameters.num_inference_steps = "20";
        parameters.guidance_scale = 7.5;
        parameters.safety_checker = "yes";
        parameters.webhook = null;
        parameters.track_id = null;

        // Serialize the ImageGenerationParameters object to a JSON string
        string jsonStr = JsonUtility.ToJson(parameters);
        //Debug.Log("jsonStr: '" + jsonStr + "'");

        // Convert the JSON string to a byte array
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonStr);

        // Create the UnityWebRequest object
        UnityWebRequest request = UnityWebRequest.Post(stableDiffusionEndpoint, "POST");

        // Set the request's headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");

        // Set the request's body
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        // Send the request and wait for a response
        yield return request.SendWebRequest();


        // Handle the response
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {

            // Get the URL and time estimate from the response
            string responseText = request.downloadHandler.text;
            Debug.Log(responseText);
            StartCoroutine(ProcessJsonResponse(responseText, sphere));
        }
    }

    IEnumerator ProcessJsonResponse(string json, GameObject sphere)
    {
        // Parse JSON response
        var parsedJson = JsonUtility.FromJson<JsonResponse>(json);
        float generationTime = parsedJson.generationTime;

        // Wait for the estimated time before making the request

        while (parsedJson.output[0] == null)
        {
            yield return new WaitForEndOfFrame();
        }

        string imageUrl = parsedJson.output[0];
        // Download the image and apply it as a texture
        StartCoroutine(DownloadAndSetTexture(imageUrl, sphere));
    }

    IEnumerator DownloadAndSetTexture(string url, GameObject sphere)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Retrieve the downloaded texture
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                // Find the sphere called "360Env" and set the texture
                if (sphere != null)
                {
                    Renderer renderer = sphere.GetComponent<Renderer>();
                    renderer.material.mainTexture = texture;
                    // renderer.enabled = true;
                }
                else
                {
                    Debug.LogError("Could not find a GameObject named '360Env'");
                }
            }
        }
    }

    [System.Serializable]
    public class JsonResponse
    {
        public string status;
        public float generationTime;
        public int id;
        public string[] output;
        public Meta meta;
    }

    [System.Serializable]
    public class Meta
    {
        public int H;
        public int W;
        // ... add other properties if needed
    }
}
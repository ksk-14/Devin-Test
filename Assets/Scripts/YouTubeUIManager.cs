using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshPro for better text rendering

public class YouTubeUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField urlInputField;
    [SerializeField] private Button playButton;
    [SerializeField] private YouTubePlayerController playerController; // Reference to the player script

    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("Play Button is not assigned in the inspector!");
        }

        if (playerController == null)
        {
            Debug.LogError("YouTubePlayerController is not assigned in the inspector!");
        }
         if (urlInputField == null)
        {
            Debug.LogError("URL Input Field is not assigned in the inspector!");
        }
    }

    void OnPlayButtonClicked()
    {
        if (playerController != null && urlInputField != null)
        {
            string url = urlInputField.text;
            if (!string.IsNullOrWhiteSpace(url))
            {
                Debug.Log($"Play button clicked. Attempting to play URL: {url}");
                playerController.PlayVideo(url);
            }
            else
            {
                 Debug.LogWarning("URL Input Field is empty.");
            }
        }
         else
        {
             Debug.LogError("Cannot play video. PlayerController or URL Input Field is missing.");
        }
    }

}

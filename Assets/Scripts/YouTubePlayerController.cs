using UnityEngine;
using UnityEngine.Video; // Unity's built-in VideoPlayer
using YoutubePlayer; // From iBicha.YoutubePlayer package
using UnityEngine.UI; // For RawImage

[RequireComponent(typeof(VideoPlayer))]
public class YouTubePlayerController : MonoBehaviour
{
    [SerializeField] private RawImage videoOutputImage; // Assign a RawImage UI element here
    private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.errorReceived += VideoPlayer_errorReceived;
        videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;

        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.renderMode = VideoRenderMode.APIOnly; // We'll render to RawImage manually or via targetTexture
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource; // Requires an AudioSource component
        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
        }
        videoPlayer.SetTargetAudioSource(0, GetComponent<AudioSource>());

        if (videoOutputImage != null)
        {
             if (videoPlayer.targetTexture == null)
             {
                 RenderTexture renderTexture = new RenderTexture(1280, 720, 24); // Example resolution
                 videoPlayer.targetTexture = renderTexture;
                 Debug.Log("Created and assigned RenderTexture for VideoPlayer.");
             }
             videoOutputImage.texture = videoPlayer.targetTexture;
             videoOutputImage.color = Color.white; // Ensure RawImage is visible
             Debug.Log("Assigned VideoPlayer texture to RawImage.");
        }
        else
        {
            Debug.LogError("Video Output RawImage is not assigned in the inspector!");
        }
    }

    private void VideoPlayer_prepareCompleted(VideoPlayer source)
    {
         Debug.Log("Video prepared. Starting playback.");
         source.Play();
         if (videoOutputImage != null) videoOutputImage.color = Color.white; // Make sure it's visible
    }

    private void VideoPlayer_errorReceived(VideoPlayer source, string message)
    {
        Debug.LogError($"VideoPlayer Error: {message}");
        if (videoOutputImage != null) videoOutputImage.color = Color.clear; // Hide image on error
    }

    public async void PlayVideo(string youtubeUrl)
    {
        Debug.Log($"Attempting to resolve YouTube URL: {youtubeUrl}");
        if (videoOutputImage != null) videoOutputImage.color = Color.clear; // Hide image while loading

        try
        {
            var videoUrl = await YoutubePlayer.YoutubeDl.GetVideoUrlAsync(youtubeUrl, YoutubeDlOptions.Default);

            if (!string.IsNullOrEmpty(videoUrl))
            {
                Debug.Log($"Resolved video URL: {videoUrl}. Preparing VideoPlayer...");
                videoPlayer.url = videoUrl;
                videoPlayer.Prepare(); // Asynchronously prepares the video
            }
            else
            {
                Debug.LogError("Failed to resolve YouTube video URL. The URL might be invalid, private, or require different extraction methods.");
                 if (videoOutputImage != null) videoOutputImage.color = Color.clear;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting video URL: {e.Message}");
             if (videoOutputImage != null) videoOutputImage.color = Color.clear;
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("Video paused.");
        }
    }

    public void StopVideo()
    {
        if (videoPlayer.isPlaying || videoPlayer.isPaused)
        {
            videoPlayer.Stop();
             if (videoOutputImage != null) videoOutputImage.color = Color.clear; // Hide image on stop
            Debug.Log("Video stopped.");
        }
    }
}

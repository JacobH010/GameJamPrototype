using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public GameObject videoPlayerObject; // The GameObject with the VideoPlayer component
    public GameObject playButton;       // Play Button GameObject
    public GameObject closeButton;      // Close Button GameObject

    public void PlayVideo()
    {
        // Enable the VideoPlayerObject
        videoPlayerObject.SetActive(true);

        // Get the VideoPlayer component and start playback
        VideoPlayer videoPlayer = videoPlayerObject.GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        // Show the Close Button and hide the Play Button
        playButton.SetActive(false);
        closeButton.SetActive(true);
    }

    public void StopVideo()
    {
        // Get the VideoPlayer component and stop playback
        VideoPlayer videoPlayer = videoPlayerObject.GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        // Disable the VideoPlayerObject
        videoPlayerObject.SetActive(false);

        // Show the Play Button and hide the Close Button
        playButton.SetActive(true);
        closeButton.SetActive(false);
    }
}

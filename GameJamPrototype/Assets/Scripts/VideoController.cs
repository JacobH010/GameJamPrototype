using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Assign the VideoPlayer
    public GameObject playButton;  // Assign the Play Button (to hide it when playing)

    public void PlayVideo()
    {
        videoPlayer.Play();        // Play the video
        playButton.SetActive(false); // Hide the button while the video is playing
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SpotifyScrollerAPI : MonoBehaviour
{
    public string _accessToken;
    public string trackID;
    public bool continueAPICalls;
    public bool updateFeatures;
    public bool firstCall;

    // Unity UI elements 
    public Text title;
    public Text artist;
    public Text album;
    public Text progress;
    public Text energy;
    public Text valence;
    public Text tempo;
    public Text duration;
    public UnityEngine.UI.Image albumCover;
    public Texture2D textureToUse;

    // Use this for initialization
    void Start()
    {
        // assign access token for spotify api
        _accessToken = "BQDo3XDvLjGUW9iDy9fBEczFz_zXVGpK1sW8_8UQCsSaP1q0ZFy1HYpAKHBCqDhdH-DSvT0xuzNUftaQYP2QKWqNzG9TZy8Zzrx89GmS0umyzqSDFhv7CTmBzD8etdH_Jwm6v-KqMaZ6OJb00UyhvNr67pRGnfKPxUT2xROSScM92QZ3";
        SpotifyVals.AccessCode = _accessToken;

        trackID = "";
        continueAPICalls = true;
        updateFeatures = true;
        firstCall = true;

        title.text = "Loading Song...";
        artist.text = "Loading Artist...";
        album.text = "Loading Album...";
        progress.text = "Loading Progress...";
        energy.text = "Loading Energy...";
        valence.text = "Loading Valence...";
        tempo.text = "Loading Tempo...";
        duration.text = "Loading Duration...";

        // Start API calls for data
        StartCoroutine(GetSpotifyData());
    }

    IEnumerator GetSpotifyData()
    {
        while (continueAPICalls)
        {
            // only check for song updates once a second
            yield return new WaitForSeconds(1f);

            //Debug.Log("Calling Spotify Data");
            updateFeatures = false;

            string currentlyPlayingURL = "https://api.spotify.com/v1/me/player/currently-playing";
            UnityWebRequest reqCurrTrack = new UnityWebRequest(currentlyPlayingURL);
            reqCurrTrack.SetRequestHeader("Authorization", "Bearer " + _accessToken);
            reqCurrTrack.SetRequestHeader("Accept", "application/json");
            reqCurrTrack.SetRequestHeader("Content-Type", "application/json");
            reqCurrTrack.downloadHandler = new DownloadHandlerBuffer();

            using (reqCurrTrack)
            {
                // wait until the download is done
                yield return reqCurrTrack.SendWebRequest();

                if (reqCurrTrack.isNetworkError || reqCurrTrack.isHttpError)
                {
                    // get data failed, log error
                    Debug.Log("CurrPlaying Web Request Failed: " + reqCurrTrack.error + ". Try a new access token or make sure music is playing.");
                }
                else
                {
                    // get data is complete, parse json results
                    if (reqCurrTrack.isDone)
                    {
                        // parse Json text into Unity object
                        //Debug.Log("Currently Playing request successful!");
                        string trackJson = reqCurrTrack.downloadHandler.text;
                        CurrentlyPlayingTrack trackData = JsonUtility.FromJson<CurrentlyPlayingTrack>(trackJson);

                        // update UI values
                        title.text = "Song: " + trackData.item.name;
                        artist.text = "Artist: " + trackData.item.artists[0].name;
                        album.text = "Album: " + trackData.item.album.name;

                        // calculate and display progress
                        int progTime = trackData.progress_ms;
                        float progS = Mathf.Floor((progTime / 1000) % 60);
                        float progM = Mathf.Floor((progTime / 1000) / 60);
                        progress.text = "Progress: " + progM + "m:" + progS + "s";

                        // calculate and display progress
                        int durTime = trackData.item.duration_ms;
                        float durS = Mathf.Floor((durTime / 1000) % 60);
                        float durM = Mathf.Floor((durTime / 1000) / 60);
                        duration.text = "Duration: " + durM + "m:" + durS + "s";

                        // update global saved values
                        SpotifyVals.Title = trackData.item.name;
                        SpotifyVals.Artist = trackData.item.artists[0].name;
                        SpotifyVals.Album = trackData.item.album.name;
                        SpotifyVals.Progress = trackData.progress_ms;
                        SpotifyVals.Duration = trackData.item.duration_ms;
                        SpotifyVals.ID = trackData.item.id;

                        // save track id for features api call, check if update needed
                        if ((trackData.item.id != trackID) || (firstCall == true))
                        {
                            firstCall = false;
                            updateFeatures = true;
                            trackID = trackData.item.id;
                        }
                        else if (trackData.item.id == trackID)
                        {
                            updateFeatures = false;
                        }

                        // download image of cover art
                        string imageURL = trackData.item.album.images[0].url;
                        // Start a download of the given URL
                        var www = new WWW(imageURL);
                        // wait until the download is done
                        yield return www;
                        // Create a texture in DXT1 format
                        Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);
                        // assign the downloaded image to sprite
                        www.LoadImageIntoTexture(texture);
                        textureToUse = texture;
                        albumCover.GetComponent<Renderer>().material.mainTexture = textureToUse;
                        www.Dispose();
                        www = null;
                    }
                }
            }

            // only update track features if you need to, don't make unnecesary calls
            if (updateFeatures == true)
            {
                // Set up API request for track features
                string trackURL = "https://api.spotify.com/v1/audio-features/" + trackID;
                UnityWebRequest reqFeatures = new UnityWebRequest(trackURL);
                reqFeatures.SetRequestHeader("Authorization", "Bearer " + _accessToken);
                reqFeatures.SetRequestHeader("Accept", "application/json");
                reqFeatures.SetRequestHeader("Content-Type", "application/json");
                reqFeatures.downloadHandler = new DownloadHandlerBuffer();

                // Make request for track features
                using (reqFeatures)
                {
                    // wait until the download is done
                    yield return reqFeatures.SendWebRequest();

                    if (reqFeatures.isNetworkError || reqFeatures.isHttpError)
                    {
                        // get data failed, log error
                        Debug.Log("Features Web Request Failed: " + reqFeatures.error + ". Try a new access token or make sure music is playing.");
                    }
                    else
                    {
                        // parse Json text into Unity object
                        //Debug.Log("Features request successful!");
                        string featuresJson = reqFeatures.downloadHandler.text;
                        AudioFeature featuresData = JsonUtility.FromJson<AudioFeature>(featuresJson);

                        // update UI values
                        energy.text = "Energy: " + featuresData.energy;
                        valence.text = "Valence: " + featuresData.valence;
                        tempo.text = "Tempo: " + featuresData.tempo;

                        // update global saved values
                        SpotifyVals.Energy = featuresData.energy;
                        SpotifyVals.Valence = featuresData.valence;
                        SpotifyVals.Tempo = featuresData.tempo;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // dont make Spotify api calls here, update is 60 times/second
        // fill with loading text
        if (firstCall == true)
        {
            title.text = "Loading Song...";
            artist.text = "Loading Artist...";
            album.text = "Loading Album...";
            progress.text = "Loading Progress...";
            energy.text = "Loading Energy...";
            valence.text = "Loading Valence...";
            tempo.text = "Loading Tempo...";
            duration.text = "Loading Duration...";

            firstCall = false;
        }
        else
        {
            title.text = "Song: " + SpotifyVals.Title;
            artist.text = "Artist: " + SpotifyVals.Artist;
            album.text = "Album: " + SpotifyVals.Album;
            progress.text = "Progress: " + SpotifyVals.Progress;
            energy.text = "Energy: " + SpotifyVals.Energy;
            valence.text = "Valence: " + SpotifyVals.Valence;
            tempo.text = "Tempo: " + SpotifyVals.Tempo;
            duration.text = "Duration: " + SpotifyVals.Duration;

            // calculate and display progress
            int progTime = SpotifyVals.Progress;
            float progS = Mathf.Floor((progTime / 1000) % 60);
            float progM = Mathf.Floor((progTime / 1000) / 60);
            int durTime = SpotifyVals.Duration;
            float durS = Mathf.Floor((durTime / 1000) % 60);
            float durM = Mathf.Floor((durTime / 1000) / 60);
            progress.text = "Progress: " + progM + "m:" + progS + "s";
            duration.text = "Duration: " + durM + "m:" + durS + "s";
        }
    }
}
 
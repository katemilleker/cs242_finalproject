using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class LevelGenerator : MonoBehaviour
{
    // variables for gameplay
    public int Seed;
    public int x;
    public float y;
    public int oldx;
    public int n;
    public int m;
    public int dif;
    public int freq;

    // variables for Spotify API
    public bool continueAPICalls;
    public bool updateFeatures;
    public bool firstCall;
    public string trackID;
    public string _accessToken;

    // unity game elements
    public Transform Brick;
    public Transform Note;
    public Transform Tiny;
    public Transform Double;
    public Text title;
    public Text artist;

    // Use this for initialization
    void Start()
    {
        // SPOTIFY: parse song ID for numeric seed value
        string parsedID = string.Empty;
        for (int i = 0; i < SpotifyVals.ID.Length; i++)
        {
            if (Char.IsDigit(SpotifyVals.ID[i]))
                parsedID += SpotifyVals.ID[i];
        }
        if (parsedID.Length > 0)
            Seed = int.Parse(parsedID);

        x = 0;          // x position 
        y = 0;          // y position
        oldx = -1;      // x position from last update
        n = 1;          // number of updates so far, used to generate game elements

        // SPOTIFY: frequency of obstacles appears affected by energy of track
        dif = (int)(3 + SpotifyVals.Energy);

        // SPOTIFY: frequency of note objects affected by positivity of track 
        freq = (int)(4 + SpotifyVals.Valence);

        // initial obstacles
        for (int i = 0; i < 12; i++)
        {
            Instantiate(Brick, new Vector3(i, 6, 0), Quaternion.identity);
            Instantiate(Brick, new Vector3(i, -6, 0), Quaternion.identity);
        }

        // set up spotify api request
        _accessToken = SpotifyVals.AccessCode;
        continueAPICalls = true;
        updateFeatures = true;
        firstCall = true;

        // fetch spotify data into scene
        StartCoroutine(GetSpotifyData());
    }

    // Update is called once per frame
    void Update()
    {
        // update UI with current song information
        title.text = "Song: " + SpotifyVals.Title;
        artist.text = "Artist: " + SpotifyVals.Artist;

        // create game elements
        x = (int)this.transform.position.x;
        y = this.transform.position.y;
        if (x > oldx)
        {
            n++;
            Instantiate(Brick, new Vector3(x + 12, y + 6, 0), Quaternion.identity);
            Instantiate(Brick, new Vector3(x + 12, y - 6, 0), Quaternion.identity);

            // generate brick obstacles
            if ((n % dif) == 0)
            {
                m = Math.Abs(((Seed - n) % 10));
                Instantiate(Brick, new Vector3(x + 12, y - 5 + m, 0), Quaternion.identity);
            }
            // generate notes for points
            else if ((n % freq) == 0)
            {
                m = Math.Abs(((Seed - n - 2) % 10));
                Instantiate(Note, new Vector3(x + 12, y - 5 + m, 0), Quaternion.identity);
            }
            // generate tiny powerup
            if (n % 77 == 0)
            {
                Instantiate(Tiny, new Vector3(x + 12, y, 0), Quaternion.identity);
            }
            // generate double points powerup
            if (n % 104 == 0)
            {
                Instantiate(Double, new Vector3(x + 12, y, 0), Quaternion.identity);
            }

            // update x position
            oldx = x;
        }
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

                        // update global saved values
                        SpotifyVals.Energy = featuresData.energy;
                        SpotifyVals.Valence = featuresData.valence;
                        SpotifyVals.Tempo = featuresData.tempo;

                        // update UI values and game changing values
                        // update game features with song information
                        dif = 4 + (int)(SpotifyVals.Energy);
                        freq = 3 + (int)(SpotifyVals.Valence);

                        string parsedID = string.Empty;
                        for (int i = 0; i < SpotifyVals.ID.Length; i++) {
                            if (Char.IsDigit(SpotifyVals.ID[i]))
                                parsedID += SpotifyVals.ID[i];
                        }
                        if (parsedID.Length > 0)
                            Seed = int.Parse(parsedID);
                    }
                }
            }
        }
    }
}

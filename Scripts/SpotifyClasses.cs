using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Get the user's currently playing track
// GET https://api.spotify.com/v1/me/player/currently-playing
[System.Serializable]
public class ExternalUrls
{
    public string spotify;
}

[System.Serializable]
public class Artist
{
    public ExternalUrls external_urls;
    public string href;
    public string id;
    public string name;
    public string type;
    public string uri;
}

[System.Serializable]
public class Image
{
    public int height;
    public string url;
    public int width;
}

[System.Serializable]
public class Album
{
    public string album_type;
    public List<Artist> artists;
    public List<string> available_markets;
    public ExternalUrls external_urls;
    public string href;
    public string id;
    public List<Image> images;
    public string name;
    public string release_date;
    public string release_date_precision;
    public string type;
    public string uri;
}

[System.Serializable]
public class ExternalIds
{
    public string isrc;
}

[System.Serializable]
public class Item
{
    public Album album;
    public List<Artist> artists;
    public List<string> available_markets;
    public int disc_number;
    public int duration_ms;
    public bool @explicit;
    public ExternalIds external_ids;
    public ExternalUrls external_urls;
    public string href;
    public string id;
    public bool is_local;
    public string name;
    public int popularity;
    public string preview_url;
    public int track_number;
    public string type;
    public string uri;
}

[System.Serializable]
public class Context
{
    public ExternalUrls external_urls;
    public string href;
    public string type;
    public string uri;
}

[System.Serializable]
public class CurrentlyPlayingTrack
{
    public long timestamp;
    public int progress_ms;
    public bool is_playing;
    public Item item;
    public Context context;
}

// Get Audio Features for a Track
// GET https://api.spotify.com/v1/audio-features/{id}
[System.Serializable]
public class AudioFeature
{
    public double danceability;
    public double energy;
    public int key;
    public double loudness;
    public int mode;
    public double speechiness;
    public double acousticness;
    public int instrumentalness;
    public double liveness;
    public double valence;
    public double tempo;
    public string type;
    public string id;
    public string uri;
    public string track_href;
    public string analysis_url;
    public int duration_ms;
    public int time_signature;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Access song data globally across multiple scenes
public static class SpotifyVals
{
    private static string title, artist, album, accessCode, id;
    private static double energy, valence, tempo;
    private static int progress, duration;
    private static bool firstCall;

    // active access code
    public static string AccessCode
    {
        get { return accessCode; }
        set { accessCode = value; }
    }

    // first call for SpotifyScrollerAPI.cs
    public static bool FirstCall
    {
        get { return firstCall; }
        set { firstCall = value; }
    }

    // currently playing song title
    public static string Title
    {
        get{ return title; }
        set{ title = value; }
    }

    // currently playing song artist
    public static string Artist
    {
        get { return artist; }
        set { artist = value; }
    }

    // currently playing song album
    public static string Album
    {
        get { return album; }
        set { album = value; }
    }

    // currently playing song energy
    public static double Energy
    {
        get { return energy; }
        set { energy = value; }
    }

    // currently playing song valence
    public static double Valence
    {
        get { return valence; }
        set { valence = value; }
    }

    // currently playing song tempo
    public static double Tempo
    {
        get { return tempo; }
        set { tempo = value; }
    }

    // currently playing song progress
    public static int Progress
    {
        get { return progress; }
        set { progress = value; }
    }

    // currently playing song duration
    public static int Duration
    {
        get { return duration; }
        set { duration = value; }
    }

    // currently playing song ID
    public static string ID
    {
        get { return id; }
        set { id = value; }
    }
}

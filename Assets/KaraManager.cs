using System;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Script class managing the program globaly
/// </summary>
public class KaraManager : MonoBehaviour
{
    private VideoPlayer videoObject;
    private AudioSource audioObject;
    private TextScript textScript;
    private GameObject uiObject;
    private bool wasKaraStared = false;
    private bool isKaraPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        videoObject = GetComponentInChildren<VideoPlayer>();
        audioObject = GetComponentInChildren<AudioSource>();
        textScript = GetComponent<TextScript>();
        uiObject = transform.parent.Find("UI Manager").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (wasKaraStared)
        {
            if (textScript.IsVideo)
            {
                if(!videoObject.isPlaying && !isKaraPaused)
                {
                    StopKara();
                    uiObject.SetActive(true);
                }
            }
            else
            {
                if (!audioObject.isPlaying && !isKaraPaused)
                {
                    StopKara();
                    uiObject.SetActive(true);
                }
            }
        }
    }

    public void StartKara()
    {
        videoObject.Play();
        audioObject.Play();
        textScript.Kara_started = true;
        wasKaraStared = true;
        isKaraPaused = false;
    }

    public void PauseKara()
    {
        videoObject.Pause();
        audioObject.Pause();
        textScript.Kara_started = false;
        isKaraPaused = true;
    }

    public void StopKara()
    {
        videoObject.Stop();
        audioObject.Stop();
        textScript.Kara_started = false;
        wasKaraStared = false;
        isKaraPaused = false;
        uiObject.SetActive(true);
        textScript.ResetKara();
    }

    public void LoadLocal(Kara kara)
    {
        TextAsset subfile = Resources.Load<TextAsset>(kara.Subfile.Remove(kara.Subfile.Length - 4));
        textScript.LoadKara(subfile.text);
        if (kara.Mediafile.EndsWith(".mp4"))
        {
            textScript.IsVideo = true;
            videoObject.enabled = true;
            videoObject.clip = Resources.Load<VideoClip>(kara.Mediafile.Remove(kara.Mediafile.Length - 4));
            audioObject.clip = null;
        }
        else
        {
            textScript.IsVideo = false;
            videoObject.clip = null;
            videoObject.enabled = false;
            audioObject.clip = Resources.Load<AudioClip>(kara.Mediafile.Remove(kara.Mediafile.Length - 4));
        }
    }

    public void Load(string subs, string url)
    {
        textScript.LoadKara(subs);
        textScript.IsVideo = true;
        videoObject.enabled = true;
        videoObject.url = url;
        audioObject.clip = null;
    }

    public void Load(string subs, AudioClip audio)
    {
        textScript.LoadKara(subs);
        textScript.IsVideo = false;
        videoObject.clip = null;
        videoObject.enabled = false;
        audioObject.clip = audio;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && wasKaraStared) StartKara();
        else PauseKara();
    }
}


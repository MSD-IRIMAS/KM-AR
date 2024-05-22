using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script class that manages the network part of the program 
/// </summary>
public class NetManager : MonoBehaviour
{
    private static readonly string PATH = "Assets/Resources/";
    private static readonly string API_PATH = "https://kara.moe/api/karas/";
    private static readonly string MEDIA_PATH = "https://kara.moe/downloads/";
    private static readonly HttpClient CLIENT_API = new()
    {
        BaseAddress = new Uri(API_PATH)
    };
    private static readonly HttpClient CLIENT_MEDIA = new()
    {
        BaseAddress = new Uri(MEDIA_PATH)
    };
    public List<Kara> Karas { get; protected set; }
    public string Subs { get; protected set; }
    public bool IsVideo { get; protected set; }
    public string MediaUrl { get; protected set; }
    public AudioClip Audio { get; protected set; }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    /// <summary>
    /// Search for karas and store the result in NetManager.Karas (C# native version)
    /// </summary>
    /// <param name="filter">The query filter</param>
    public void Search(string filter)
    {
        if (filter == null || filter == string.Empty) throw new ArgumentException("Filter can't be empty");
        using HttpRequestMessage message = new(HttpMethod.Get, "search?filter=" + filter.Sanitize());
        using Task<HttpResponseMessage> responceTask = CLIENT_API.SendAsync(message);
        while (!responceTask.IsCompleted) print("Waiting for responce...");
        if (!responceTask.IsCompletedSuccessfully) throw new ApplicationException("Responce Task was not successfull");
        using HttpResponseMessage responce = responceTask.Result;
        if (responce.IsSuccessStatusCode)
        {
            using Task<string> readTask = responce.Content.ReadAsStringAsync();
            while (!readTask.IsCompleted) print("Reading responce...");
            if (!readTask.IsCompletedSuccessfully) throw new ApplicationException("Reading task was not successfull");
            Karas = JsonSerializer.Deserialize<SearchResult>(readTask.Result).Content;
        }
        else throw new HttpRequestException("Request was not successfull : "+responce.StatusCode.ToString());
    }

    /// <summary>
    /// Search for karas and store the result in NetManager.Karas (Unity version)
    /// </summary>
    /// <param name="filter">The query filter</param>
    public void SearchNew(string filter)
    {
        using UnityWebRequest textRequest = UnityWebRequest.Get(API_PATH + "search?filter="+filter.Sanitize());
        textRequest.downloadHandler = new DownloadHandlerBuffer();
        textRequest.SendWebRequest();
        while (textRequest.result == UnityWebRequest.Result.InProgress) print("Text download at " + textRequest.downloadProgress);
        switch (textRequest.result)
        {
            case UnityWebRequest.Result.InProgress:
                print("Text request in progress...");
                break;

            case UnityWebRequest.Result.Success:
                string json = DownloadHandlerBuffer.GetContent(textRequest);
                Karas = JsonSerializer.Deserialize<SearchResult>(json).Content;
                break;

            case UnityWebRequest.Result.ConnectionError:
                throw new HttpRequestException("Error while connecting to the server: " + textRequest.error);

            case UnityWebRequest.Result.ProtocolError:
                throw new HttpRequestException("Error while from the server: " + textRequest.responseCode);

            case UnityWebRequest.Result.DataProcessingError:
                throw new ApplicationException("Error while processing the data");

        }
    }

    public void SearchLocal()
    {
        TextAsset localSongs = Resources.Load<TextAsset>("LocalSongs");
        if (localSongs == null || localSongs.text == string.Empty) throw new FileNotFoundException("Local Kara file not found.")
        Karas = JsonSerializer.Deserialize<SearchResult>(localSongs.text).Content;
    }

    public static bool IsValid(Kara kara)
    {
        if (
            kara == null ||
            kara.Mediafile == null ||
            kara.Subfile == null ||
            kara.Mediafile == string.Empty ||
            kara.Subfile == string.Empty
           ) return false;
        else return true;
    }

    [Obsolete("Use DownloadNew instead")]
    /// <summary>
    /// Downloads the media file and the sub (text) file of a Kara
    /// </summary>
    /// <param name="kara"></param>
    public static void Download(Kara kara)
    {
        if (!IsValid(kara)) throw new ArgumentException("Invalid kara");

        using HttpRequestMessage sub_message = new(HttpMethod.Get, "lyrics/" + kara.Subfile);
        using Task<HttpResponseMessage> responceTask = CLIENT_MEDIA.SendAsync(sub_message);
        while (!responceTask.IsCompleted) print("Waiting for responce...");
        if (!responceTask.IsCompletedSuccessfully) throw new ApplicationException("Responce Task was not successfull");
        using HttpResponseMessage responce = responceTask.Result;
        if (responce.IsSuccessStatusCode)
        {
            using Task<string> readTask = responce.Content.ReadAsStringAsync();
            while (!readTask.IsCompleted) print("Reading responce...");
            if (!readTask.IsCompletedSuccessfully) throw new ApplicationException("Reading task was not successfull");
            File.WriteAllText(PATH + kara.Subfile.Remove(kara.Subfile.Length - 3) + "txt", readTask.Result);
        }
        else throw new HttpRequestException("Request was not successfull : " + responce.StatusCode.ToString());

        using HttpRequestMessage media_message = new(HttpMethod.Get, "medias/" + kara.Mediafile);
        using Task<HttpResponseMessage> responceTask2 = CLIENT_MEDIA.SendAsync(media_message);
        while (!responceTask2.IsCompleted) print("Waiting for responce...");
        if (!responceTask2.IsCompletedSuccessfully) throw new ApplicationException("Responce Task was not successfull");
        using HttpResponseMessage responce2 = responceTask2.Result;
        if (responce2.IsSuccessStatusCode)
        {
            using Task<byte[]> readTask = responce2.Content.ReadAsByteArrayAsync();
            while (!readTask.IsCompleted) print("Reading responce...");
            if (!readTask.IsCompletedSuccessfully) throw new ApplicationException("Reading task was not successfull");
            File.WriteAllBytes(PATH + kara.Mediafile, readTask.Result);
        }
        else throw new HttpRequestException("Request was not successfull : " + responce.StatusCode.ToString());
    }

    /// <summary>
    /// Downloads the media file and the sub (text) file of a Kara and sets the parameters acordingly.
    /// If the media file is a video, sets IsVideo to true and MediaUri to the URI to the video.
    /// If the media fils is a sound, sets IsVideo to false and Audio to a AudioClip made from the sound file.
    /// </summary>
    /// <param name="kara"></param>
    public void DownloadNew(Kara kara)
    {
        // Downloads the sub (text) file
        using UnityWebRequest textRequest = UnityWebRequest.Get(MEDIA_PATH + "lyrics/" + kara.Subfile);
        textRequest.downloadHandler = new DownloadHandlerBuffer();
        textRequest.SendWebRequest();
        while (textRequest.result == UnityWebRequest.Result.InProgress) print("Text download at " + textRequest.downloadProgress);
        switch (textRequest.result)
        {
            case UnityWebRequest.Result.InProgress:
                print("Text request in progress...");
                break;

            case UnityWebRequest.Result.Success:
                Subs = DownloadHandlerBuffer.GetContent(textRequest);
                break;

            case UnityWebRequest.Result.ConnectionError:
                throw new HttpRequestException("Error while connecting to the server: " + textRequest.error);

            case UnityWebRequest.Result.ProtocolError:
                throw new HttpRequestException("Error while from the server: " + textRequest.responseCode);

            case UnityWebRequest.Result.DataProcessingError:
                throw new ApplicationException("Error while processing the data");

        }

        if (kara.Mediafile.EndsWith(".mp4")) //if the media file is a video
        {
            IsVideo = true;
            MediaUrl = MEDIA_PATH + "medias/" + kara.Mediafile; //sends the url to the video player
            Audio = null;
        }
        else
        {
            IsVideo = false;
            MediaUrl = null;

            //Downloads the sound file
            using UnityWebRequest mediaRequest = UnityWebRequestMultimedia.GetAudioClip(MEDIA_PATH + "medias/" + kara.Mediafile, AudioType.MPEG);
            mediaRequest.SendWebRequest();
            while (mediaRequest.result == UnityWebRequest.Result.InProgress) print("Sound download at " + mediaRequest.downloadProgress);
            switch (mediaRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    throw new HttpRequestException("Error while connecting to the server: " + mediaRequest.error);

                case UnityWebRequest.Result.ProtocolError:
                    throw new HttpRequestException("Error while from the server: " + mediaRequest.responseCode);

                case UnityWebRequest.Result.DataProcessingError:
                    throw new ApplicationException("Error while processing the data");

                case UnityWebRequest.Result.Success:
                    Audio = DownloadHandlerAudioClip.GetContent(mediaRequest);
                    Audio.name = kara.Mediafile.Remove(kara.Mediafile.Length - 4);
                    break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        CLIENT_API.Dispose();
        CLIENT_MEDIA.Dispose();
    }
}

/// <summary>
/// JSON Class for Karaokes
/// </summary>
public class Kara
{
    /// <summary>
    /// The Kara's ID
    /// </summary>
    [JsonPropertyName("kid")]
    public string Kid { get; set; }

    /// <summary>
    /// The name of the Sub file
    /// </summary>
    [JsonPropertyName("subfile")]
    public string Subfile { get; set; }

    /// <summary>
    /// The name of the Media file
    /// </summary>
    [JsonPropertyName("mediafile")]
    public string Mediafile { get; set; }

    /// <summary>
    /// A dictionary containing the song's title in different languages
    /// </summary>
    [JsonPropertyName("titles")]
    public Dictionary<string, string> Titles { get; set; }

    /// <summary>
    /// The song's aliases
    /// </summary>
    [JsonPropertyName("titles_aliases")]
    public List<string> Aliases { get; set; }

    /// <summary>
    /// The duration of the kara in seconds
    /// </summary>
    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}

/// <summary>
/// JSON Class containing Search Metadata
/// </summary>
public class SearchInfos
{
    /// <summary>
    /// The total number of found Karas
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// The index number of the first Kara
    /// </summary>
    [JsonPropertyName("from")]
    public int From { get; set; }

    /// <summary>
    /// The index number of the last Kara
    /// </summary>
    [JsonPropertyName("to")]
    public int To { get; set; }
}

/// <summary>
/// JSON Class warping the two previous classes
/// </summary>
public class SearchResult
{
    [JsonPropertyName("infos")]
    public SearchInfos Infos { get; set; }

    [JsonPropertyName("content")]
    public List<Kara> Content { get; set; }
}
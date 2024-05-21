using System;
using System.Collections.Generic;
using System.Text.Json;
using TMPro;
using UnityEngine;

/// <summary>
/// Script class managing the UI
/// </summary>
public class UIManager : MonoBehaviour
{
    public static readonly int FIELDS_BY_PAGE = 4;

    [SerializeField]
    private TouchScreenKeyboard Keyboard;

    private TextMeshPro text;
    private NetManager netManager;
    private KaraManager karaManager;
    private Transform root;
    private bool locked = false;
    private int page = 0;
    private int maxPage = -1;
    private bool isLocal = false;

    public int SelectedKara { get; set; } = 0;

    // Start is called before the first frame update
    void Start()
    {
        text = transform.Find("Search Field").GetComponent<TextMeshPro>();
        netManager = GetComponent<NetManager>();
        karaManager = transform.parent.Find("Kara Manager").GetComponent<KaraManager>();
        root = transform.Find("Kara Selector Holder").Find("Kara Selector").GetChild(0).GetChild(0).Find("ListItems");
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard != null && !locked)
        {
            text.text = Keyboard.text;
            if (Keyboard.status == TouchScreenKeyboard.Status.Done && Keyboard.text.Sanitize() != string.Empty)
            {
                isLocal = false;
                try
                {
                    netManager.SearchNew(text.text);
                    ShowResults(netManager.Karas);
                }
                catch (Exception e)
                {
                    text.text = e.Message;
                }

                locked = true;
            }
        }
    }

    /// <summary>
    /// Show the search results on the UI
    /// </summary>
    /// <param name="karas">The results to show</param>
    protected void ShowResults(List<Kara> karas)
    {
        transform.Find("Kara Selector Holder").gameObject.SetActive(true);
        foreach (Kara kara in karas)
        {
            print(kara.Subfile);
        }
        Kara[] karas_subset;
        int count = 0;
        int start_index = page * FIELDS_BY_PAGE;
        int end_index = start_index + FIELDS_BY_PAGE;
        maxPage = karas.Count / FIELDS_BY_PAGE;
        if (end_index >= karas.Count) karas_subset = karas.ToArray()[start_index..];
        else karas_subset = karas.ToArray()[start_index..end_index];
        
        foreach (Kara kara in karas_subset)
        {
            GameObject item = root.Find("Choice " + count).gameObject;
            TextMeshPro itemText = item.transform.Find("CompressableButtonVisuals").Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>();
            item.SetActive(true);
            string eng_title = kara.Titles.GetValueOrDefault("eng");
            if (eng_title == null || eng_title == string.Empty) itemText.text = kara.Mediafile.Remove(kara.Mediafile.Length - 4);
            else itemText.text = eng_title;
            count++;
        }
    }

    public void PreviousPage()
    {
        if (page > 0) page--;
        ResetFields();
        ShowResults(netManager.Karas);
    }

    public void NextPage()
    {
        if (maxPage != -1 && page < maxPage) page++;
        ResetFields();
        ShowResults(netManager.Karas);
    }

    void ResetFields()
    {
        for (int i = 0; i < FIELDS_BY_PAGE; i++)
        {
            root.Find("Choice " + i).gameObject.SetActive(false);
        }
    }

    public void Confirm()
    {
        Kara selectedKara = netManager.Karas.ToArray()[page * FIELDS_BY_PAGE + SelectedKara];
        print("Kara "+(page* FIELDS_BY_PAGE + SelectedKara)+" selected");
        if(!isLocal)
        {
            netManager.DownloadNew(selectedKara);
            if (netManager.IsVideo)
            {
                print("Is Video: " + netManager.MediaUrl);
                karaManager.Load(netManager.Subs, netManager.MediaUrl);
            }
            else
            {
                print("Is Sound: " + netManager.Audio.name);
                karaManager.Load(netManager.Subs, netManager.Audio);
            }
        }
        else
        {
            karaManager.LoadLocal(selectedKara);
        }
        string eng_title = selectedKara.Titles.GetValueOrDefault("eng");
        if (eng_title == null || eng_title == string.Empty) text.text = "Song \""+selectedKara.Mediafile.Remove(selectedKara.Mediafile.Length - 4)+"\" loaded";
        else text.text = "Song \""+eng_title+"\" loaded";
    }

    public void OpenSystemKeyboard()
    {
        locked = false;
        transform.Find("Kara Selector Holder").gameObject.SetActive(false);
        Keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "Type Query here");
    }

    public void OpenLocalSongs()
    {
        isLocal = true;
        netManager.SearchLocal();
        ShowResults(netManager.Karas);
    }
}

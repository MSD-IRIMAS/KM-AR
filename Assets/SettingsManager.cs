using Microsoft.MixedReality.Toolkit.UX;
using System;
using TMPro;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private Transform root;
    private Signposting signposting;
    private AudioSource audioSource;
    private Slider volumeSlider;
    private Slider distanceSlider;
    private TMP_Text volumeText;
    private TMP_Text distanceText;
    // Start is called before the first frame update
    void Start()
    {
        root = transform.parent.parent.Find("Kara Manager");
        signposting = root.Find("Text").GetComponent<Signposting>();
        audioSource = root.Find("Sound source").GetComponent<AudioSource>();
        volumeSlider = transform.Find("Volume Slider").GetComponent<Slider>();
        distanceSlider = transform.Find("Distance Slider").GetComponent<Slider>();
        volumeText = transform.Find("Volume Text").GetComponent<TMP_Text>();
        distanceText = transform.Find("Distance Text").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        float volume = volumeSlider.Value;
        float distance = distanceSlider.Value;

        audioSource.volume = volume;
        signposting.distance = distance * 100 ;

        volumeText.text = "Volume : " + Convert.ToInt32(volume * 100);
        distanceText.text = "Distance : " + Convert.ToInt32(distance * 100);
    }
}

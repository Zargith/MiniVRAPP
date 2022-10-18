using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNighCycleManager : MonoBehaviour
{
    [Header("Day night elements")]
    [SerializeField] Skybox skybox;
    [SerializeField] Material daySkybox;
    [SerializeField] Material nightSkybox;
    [SerializeField] GameObject nightPanel;
    [SerializeField] AudioClip dayAmbiance;
    [SerializeField] AudioClip nightAmbiance;
    bool _dayNightCycle = true; // false = day, true = night


    [Header("Lightning elements")]
    [SerializeField] GameObject campfireFire;
    [SerializeField] GameObject sunlight;


    public void changeDayNightCycle(AudioSource audioSource)
    {
        _dayNightCycle = !_dayNightCycle;
        if (_dayNightCycle) {
            skybox.material = nightSkybox;
            campfireFire.SetActive(true);
            sunlight.SetActive(false);
            nightPanel.SetActive(true);
            audioSource.clip = nightAmbiance;
            audioSource.Play();
        } else {
            skybox.material = daySkybox;
            campfireFire.SetActive(false);
            sunlight.SetActive(true);
            nightPanel.SetActive(false);
            audioSource.clip = dayAmbiance;
            audioSource.Play();
        }
    }

    public bool getDayNightCycle()
    {
        return _dayNightCycle; // false = day, true = night
    }

    public void activateNightPanel(bool active)
    {
        nightPanel.SetActive(active);
    }

}

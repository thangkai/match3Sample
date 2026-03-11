using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Start()
    {
        AdsManager.Initialize(new AdmobAdsService());
    }

    public void showInterstitial()
    {
        Debug.LogError("ShowInterstitial");
        AdsManager.ShowInterstitial();
    }
    
}

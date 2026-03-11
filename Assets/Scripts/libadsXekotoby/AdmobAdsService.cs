using GoogleMobileAds.Api;
using UnityEngine;

public class AdmobAdsService : IAdsService
{
    public void Initialize()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Admob Initialized");
        });
    }

    public void ShowInterstitial()
    {
        Debug.Log("Show Admob Interstitial");
        // interstitialAd.Show();
    }

    public void ShowRewarded()
    {
        Debug.Log("Show Rewarded Ad");
        // rewardedAd.Show();
    }

    public bool IsRewardedReady()
    {
        return true;
    }
}
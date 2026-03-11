public interface IAdsService
{
    void Initialize();
    void ShowInterstitial();
    void ShowRewarded();
    bool IsRewardedReady();
}
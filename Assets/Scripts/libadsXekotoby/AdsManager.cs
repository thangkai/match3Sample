public static class AdsManager
{
    static IAdsService adsService;

    public static void Initialize(IAdsService service)
    {
        adsService = service;
        adsService.Initialize();
    }

    public static void ShowInterstitial()
    {
        adsService?.ShowInterstitial();
    }

    public static void ShowRewarded()
    {
        adsService?.ShowRewarded();
    }
}
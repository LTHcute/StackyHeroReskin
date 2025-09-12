//using System;
//using System.Collections;
//using UnityEngine;
//using GoogleMobileAds.Api;
//using GoogleMobileAds.Common;

//public class AdmobController : MonoBehaviour
//{
//    public static AdmobController Instance { private set; get; }

//    [Header("Banner ID")]
//#if UNITY_ANDROID
//    [SerializeField] private string androidBannerID = "ca-app-pub-1064078647772222/9329609006";
//#elif UNITY_IOS
//        [SerializeField] private string iOSBannerID = "ca-app-pub-1064078647772222/9329609006";
//#endif
//    [SerializeField] private AdPosition bannerPosition = AdPosition.Bottom;


//    [Header("Interstitial Ad ID")]
//#if UNITY_ANDROID
//    [SerializeField] private string androidInterstitialAdID = "ca-app-pub-1064078647772222/2139808686";
//#elif UNITY_IOS
//        [SerializeField] private string iOSInterstitialAdID = "ca-app-pub-1064078647772222/2139808686";
//#endif

//    //        [Header("Rewarded Ad ID")]
//    //#if UNITY_ANDROID
//    //        [SerializeField] private string androidRewardedAdID = "ca-app-pub-1064078647772222/9919321234";

//    //#elif UNITY_IOS
//    //        [SerializeField] private string iOSRewardedAdID = "ca-app-pub-1064078647772222/9919321234";
//    //#endif

//    private BannerView bannerView = null;
//    private InterstitialAd interstitialAd = null;
//    //private RewardedAd rewardedAd = null;
//    private int interstitialLoadCount = 0;
//    //private int rewardedAdLoadCount = 0;
//    private bool isInitializeCompleted = false;



//    private void Awake()
//    {
//        if (Instance)
//        {
//            DestroyImmediate(gameObject);
//        }
//        else
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);

//            MobileAds.SetiOSAppPauseOnBackground(true);
//            MobileAds.Initialize((initStatus) =>
//            {
//                // Callbacks from GoogleMobileAds are not guaranteed to be called on
//                // main thread.
//                // In this example we use MobileAdsEventExecutor to schedule these calls on
//                // the next Update() loop.
//                MobileAdsEventExecutor.ExecuteInUpdate(() =>
//                {
//                    isInitializeCompleted = true;
//                    interstitialLoadCount = 3;
//                    //rewardedAdLoadCount = 3;
//                    LoadInterstitialAd();
//                    //LoadRewardedAd();
//                });
//            });
//        }
//    }


//    /// <summary>
//    /// Load and show a banner ad.
//    /// </summary>
//    public void ShowBannerAd(float delay)
//    {
//        StartCoroutine(CRLoadAndShowBanner(delay));
//    }


//    /// <summary>
//    /// Coroutine load and show banner.
//    /// </summary>
//    /// <param name="delay"></param>
//    /// <returns></returns>
//    private IEnumerator CRLoadAndShowBanner(float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        while (!isInitializeCompleted)
//        {
//            yield return null;
//        }

//        // Clean up banner ad before creating a new one.
//        if (bannerView != null)
//        {
//            bannerView.Destroy();
//        }

//        // Create a banner
//        AdSize adSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
//#if UNITY_ANDROID
//        bannerView = new BannerView(androidBannerID, adSize, bannerPosition);
//#elif UNITY_IOS
//            bannerView = new BannerView(iOSBannerID, adSize, bannerPosition);
//#endif
//        // Load banner ad.
//        AdRequest adRequest = new AdRequest();
//        bannerView.LoadAd(adRequest);
//    }

//    /// <summary>
//    /// Hide the current banner ad.
//    /// </summary>
//    public void HideBannerAd()
//    {
//        if (bannerView != null)
//        {
//            bannerView.Hide();
//        }
//    }


//    /// <summary>
//    /// Determine whether the interstitial ad is ready.
//    /// </summary>
//    /// <returns></returns>
//    public bool IsInterstitialAdReady()
//    {
//        if (!isInitializeCompleted)
//            return false;

//        if (interstitialAd != null && interstitialAd.CanShowAd())
//        {
//            return true;
//        }
//        else
//        {
//            interstitialLoadCount--;
//            if (interstitialLoadCount <= 0)
//            {
//                interstitialLoadCount = 3;
//                LoadInterstitialAd();
//            }
//            return false;
//        }
//    }


//    /// <summary>
//    /// Show interstitial ad with given delay time.
//    /// </summary>
//    /// <param name="delay"></param>
//    public void ShowInterstitialAd(float delay)
//    {
//        StartCoroutine(CRShowInterstitialAd(delay));
//    }


//    /// <summary>
//    /// Coroutine show interstitial ad.
//    /// </summary>
//    /// <param name="delay"></param>
//    /// <returns></returns>
//    private IEnumerator CRShowInterstitialAd(float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        if (interstitialAd != null && interstitialAd.CanShowAd())
//        {
//            interstitialAd.Show();
//        }
//        else if (isInitializeCompleted)
//        {
//            LoadInterstitialAd();
//        }
//    }



//    /// <summary>
//    /// Load the interstitial ad.
//    /// </summary>
//    private void LoadInterstitialAd()
//    {
//        if (interstitialAd != null)
//        {
//            interstitialAd.Destroy();
//            interstitialAd = null;
//        }

//        // Create our request used to load the ad.
//        AdRequest adRequest = new AdRequest();

//        //Define the interstitial ad id
//        string interstitialAdId = string.Empty;
//#if UNITY_ANDROID
//        interstitialAdId = androidInterstitialAdID;
//#elif UNITY_IOS
//            interstitialAdId = iOSInterstitialAdID;
//#endif
//        //Send the request to load the ad.
//        InterstitialAd.Load(interstitialAdId, adRequest, (InterstitialAd ad, LoadAdError error) =>
//        {
//            MobileAdsEventExecutor.ExecuteInUpdate(() =>
//            {
//                // If error is not null, the load request failed.
//                if (error != null || ad == null)
//                {
//                    return;
//                }
//                interstitialAd = ad;

//                //Handle interstitial ad closed event
//                interstitialAd.OnAdFullScreenContentClosed += () =>
//                {
//                    MobileAdsEventExecutor.ExecuteInUpdate(() =>
//                    {
//                        interstitialLoadCount = 3;
//                        LoadInterstitialAd();
//                    });
//                };
//            });
//        });
//    }












//    ///// <summary>
//    ///// Determine whether the rewarded ad is ready.
//    ///// </summary>
//    ///// <returns></returns>
//    //public bool IsRewardedAdReady()
//    //{
//    //    if (!isInitializeCompleted)
//    //        return false;

//    //    if (rewardedAd != null && rewardedAd.CanShowAd())
//    //    {
//    //        return true;
//    //    }
//    //    else
//    //    {
//    //        rewardedAdLoadCount--;
//    //        if (rewardedAdLoadCount <= 0)
//    //        {
//    //            rewardedAdLoadCount = 3;
//    //            LoadRewardedAd();
//    //        }
//    //        return false;
//    //    }
//    //}

//    ///// <summary>
//    ///// Show rewarded ad with given delay time 
//    ///// </summary>
//    ///// <param name="delay"></param>
//    //public void ShowRewardedAd(float delay)
//    //{
//    //    StartCoroutine(CRShowRewardedAd(delay));
//    //}


//    ///// <summary>
//    ///// Coroutine show rewarded ad.
//    ///// </summary>
//    ///// <param name="delay"></param>
//    ///// <returns></returns>
//    //IEnumerator CRShowRewardedAd(float delay)
//    //{
//    //    yield return new WaitForSecondsRealtime(delay);
//    //    if (rewardedAd != null && rewardedAd.CanShowAd())
//    //    {
//    //        rewardedAd.Show((Reward reward) =>
//    //        {                   
//    //        });
//    //    }
//    //    else
//    //    {
//    //        rewardedAdLoadCount--;
//    //        if (rewardedAdLoadCount <= 0)
//    //        {
//    //            rewardedAdLoadCount = 3;
//    //            LoadRewardedAd();
//    //        }
//    //    }
//    //}


//    //        /// <summary>
//    //        /// Load the rewarded ad.
//    //        /// </summary>
//    //        private void LoadRewardedAd()
//    //        {
//    //            // Clean up the old ad before loading a new one.
//    //            if (rewardedAd != null)
//    //            {
//    //                rewardedAd.Destroy();
//    //                rewardedAd = null;
//    //            }


//    //            // Create our request used to load the ad.
//    //            AdRequest adRequest = new AdRequest();

//    //            //Define the rewarded ad id
//    //            string rewardedAdId = string.Empty;
//    //#if UNITY_ANDROID
//    //            rewardedAdId = androidRewardedAdID;
//    //#elif UNITY_IOS
//    //            rewardedAdId = iOSRewardedAdID;
//    //#endif

//    //            //Send the request to load the ad.
//    //            RewardedAd.Load(rewardedAdId, adRequest, (RewardedAd ad, LoadAdError error) =>
//    //            {
//    //                MobileAdsEventExecutor.ExecuteInUpdate(() =>
//    //                {
//    //                    // If error is not null, the load request failed.
//    //                    if (error != null || ad == null)
//    //                    {
//    //                        return;
//    //                    }
//    //                    rewardedAd = ad;


//    //                    //Handle rewarded ad failed to show
//    //                    rewardedAd.OnAdFullScreenContentFailed += (advalue) =>
//    //                    {
//    //                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
//    //                        {
//    //                            rewardedAdLoadCount = 3;
//    //                            LoadRewardedAd();
//    //                        });
//    //                    };


//    //                    //Handle user close rewarded ad
//    //                    rewardedAd.OnAdFullScreenContentClosed += () =>
//    //                    {
//    //                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
//    //                        {
//    //                            rewardedAdLoadCount = 3;
//    //                            LoadRewardedAd();
//    //                        });
//    //                    };

//    //                });
//    //            });
//    //        }
//}


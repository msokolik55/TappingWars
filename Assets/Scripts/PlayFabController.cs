﻿using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PlayFab.Json;
using PlayFab.PfEditor.Json;
using System.Collections;

public class PlayFabController : MonoBehaviour
{
    public static PlayFabController PFC;

    private string userEmail;
    private string userPassword;
    private string username;
    private string myID;
    
    //public GameObject loginPanel;
    public GameObject addLoginPanel;
    public GameObject recoverButton;

    private void OnEnable()
    {
        if (PFC == null)
        {
            PFC = this;
        }
        else
        {
            if (PFC != this)
            {
                Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        //Note: Setting title Id here can be skipped if you have set the value in Editor Extensions already.
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "6CF00"; // Please change this value to your own titleId from PlayFab Game Manager
        }

        //PlayerPrefs.DeleteAll();
        //var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
        //PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

        if (PlayerPrefs.HasKey("EMAIL"))
        {
            userEmail = PlayerPrefs.GetString("EMAIL");
            userPassword = PlayerPrefs.GetString("PASSWORD");
            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }
        else
        {
            #if UNITY_ANDROID
            var requestAndroid = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileID(), CreateAccount = true };
            PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginMobileSuccess, DisplayPlayFabError);
            #endif

            #if UNITY_IOS
            var requestIOS = new LoginWithIOSDeviceIDRequest { DeviceId = ReturnMobileID(), CreateAccount = true };
            PlayFabClientAPI.LoginWithIOSDeviceID(requestIOS, OnLoginMobileSuccess, OnLoginMobileFailure);
            #endif
        }
    }

    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    void DisplayError(string error)
    {
        Debug.LogError(error);
    }

    #region Login
    private void OnLoginSuccess(LoginResult result)
    {
        //Debug.Log("Congratulations, you made your first successful API call!");
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
        //loginPanel.SetActive(false);
        recoverButton.SetActive(false);
        GetStats();

        myID = result.PlayFabId;
        GetPlayerData();

        SceneManager.LoadScene("Menu");
    }
    private void OnLoginMobileSuccess(LoginResult result)
    {
        //Debug.Log("Congratulations, you made your first successful API call!");
        GetStats();
        //loginPanel.SetActive(false);

        myID = result.PlayFabId;
        GetPlayerData();

        SceneManager.LoadScene("Menu");
    }
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        //Debug.Log("Congratulations, you made your first successful API call!");
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);

        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = username }, OnDisplayName, DisplayPlayFabError);
        GetStats();
        //loginPanel.SetActive(false);

        myID = result.PlayFabId;
        GetPlayerData();

        SceneManager.LoadScene("Menu");
    }
    void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        //Debug.Log(result.DisplayName + " is your new display name");
    }
    private void OnLoginFailure(PlayFabError error)
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, DisplayPlayFabError);
    }
    public void GetUserEmail(string emailIn)
    {
        userEmail = emailIn;
    }
    public void GetUserPassword(string passwordIn)
    {
        userPassword = passwordIn;
    }
    public void GetUsername(string usernameIn)
    {
        username = usernameIn;
    }
    public void OnClickLogin()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, DisplayPlayFabError);
    }
    public static string ReturnMobileID()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        return deviceID;
    }
    public void OpenAddLogin()
    {
        addLoginPanel.SetActive(true);
    }
    public void OnClickAddLogin()
    {
        var addLoginRequest = new AddUsernamePasswordRequest { Email = userEmail, Password = userPassword, Username = username };
        PlayFabClientAPI.AddUsernamePassword(addLoginRequest, OnAddLoginSuccess, DisplayPlayFabError);
    }
    private void OnAddLoginSuccess(AddUsernamePasswordResult result)
    {
        //Debug.Log("Congratulations, you made your first successful API call!");
        GetStats();
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
        addLoginPanel.SetActive(false);

        SceneManager.LoadScene("Menu");
    }
    #endregion Login

    public int playerLevel;
    public int gameLevel;
    public int playerHealth;
    public int playerDamage;
    public int playerHighScore;
    
    #region PlayerStats
    public void SetStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "PlayerLevel", Value = playerLevel },
                new StatisticUpdate { StatisticName = "GameLevel", Value = gameLevel },
                new StatisticUpdate { StatisticName = "PlayerHealth", Value = playerHealth },
                new StatisticUpdate { StatisticName = "PlayerDamage", Value = playerDamage },
                new StatisticUpdate { StatisticName = "PlayerHighScore", Value = playerHighScore },

            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStats(GetPlayerStatisticsResult result)
    {
        //Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
        {
            //Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
            switch (eachStat.StatisticName)
            {
                case "PlayerLevel":
                    playerLevel = eachStat.Value;
                    break;

                case "GameLevel":
                    gameLevel = eachStat.Value;
                    break;

                case "PlayerHealth":
                    playerHealth = eachStat.Value;
                    break;

                case "PlayerDamage":
                    playerDamage = eachStat.Value;
                    break;

                case "PlayerHighScore":
                    playerHighScore = eachStat.Value;
                    break;
            }
        }
    }
    
    // Build the request object and access the API
    public void StartCloudUpdatePlayerStats()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStats", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { Level = playerLevel, highScore = playerHighScore }, // The parameter provided to your function
            GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
        }, OnCloudUpdateStats, DisplayPlayFabError);
    }
    
    // OnCloudHelloWorld defined in the next code block
    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result)
    {
        // Cloud Script returns arbitrary results, so you have to evaluate them one step and one parameter at a time
        //Debug.Log(JsonWrapper.SerializeObject(result.FunctionResult));
        PlayFab.Json.JsonObject jsonResult = (PlayFab.Json.JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in Cloud Script
        //Debug.Log((string)messageValue);
    }
    #endregion PlayerStats

    //public GameObject leaderboardPanel;
    public GameObject listingPrefab;
    public Transform listingContainer;

    #region Leaderboard
    public void GetLeaderboarder()
    {
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "PlayerHighScore", MaxResultsCount = 20 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeadboard, DisplayPlayFabError);
    }
    void OnGetLeadboard(GetLeaderboardResult result)
    {
        //leaderboardPanel.SetActive(true);
        //Debug.Log(result.Leaderboard[0].StatValue);
        foreach (PlayerLeaderboardEntry player in result.Leaderboard)
        {
            GameObject tempListing = Instantiate(listingPrefab, listingContainer);
            ListingPrefab LL = tempListing.GetComponent<ListingPrefab>();
            LL.playerNameText.text = player.DisplayName;
            LL.playerScoreText.text = player.StatValue.ToString();
            //Debug.Log(player.DisplayName + ": " + player.StatValue);
        }
    }
    public void CloseLeaderboardPanel()
    {
        //leaderboardPanel.SetActive(false);
        for (int i = listingContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(listingContainer.GetChild(i).gameObject);
        }
    }
    #endregion Leaderboard

    #region PlayerData
    //sends a request to get the player data from the playfab cloud
    public void GetPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = myID,
            Keys = null
        }, UserDataSuccess, DisplayPlayFabError);
    }
    
    //the return callback function for success.
    void UserDataSuccess(GetUserDataResult result)
    {
        if (result.Data == null || !result.Data.ContainsKey("Skins"))
        {
            //Debug.Log("Skins not set");
        }
        else
        {
            //Get the resutls of the requests and sends it to be converted to the all skins array.
            PersistentData.PD.SkinsStringToData(result.Data["Skins"].Value);
        }
    }
    
    //Sends a request to save the new player data to the playfab cloud
    public void SetUserData(string SkinsData)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                //key value pair, saving the allskins array as a string to the playfab cloud
                {"Skins", SkinsData}
            }
        }, SetDataSuccess, DisplayPlayFabError);
    }
    
    //return callback function for a successful request
    void SetDataSuccess(UpdateUserDataResult result)
    {
        //Debug.Log(result.DataVersion);
    }
    #endregion PlayerData

    #region Friends
    [SerializeField]
    public Transform friendListing;
    List<FriendInfo> myFriends;
    void DisplayFriends(List<FriendInfo> friendsCache)
    {
        foreach (FriendInfo f in friendsCache)
        {
            bool isFound = false;
            if (myFriends != null)
            {
                foreach (FriendInfo g in myFriends)
                {
                    if (f.FriendPlayFabId == g.FriendPlayFabId)
                        isFound = true;
                }
            }

            if (isFound == false)
            {
                GameObject listing = Instantiate(listingPrefab, friendListing);
                ListingPrefab tempListing = listing.GetComponent<ListingPrefab>();
                //Debug.Log(tempListing.playerNameText);
                //Debug.Log(f.TitleDisplayName);
                tempListing.playerNameText.text = f.TitleDisplayName;
            }
        }
        myFriends = friendsCache;
    }

    IEnumerator WaitForFriend()
    {
        yield return new WaitForSeconds(2);
        GetFriends();
    }

    public void RunWaitFunction()
    {
        StartCoroutine(WaitForFriend());
    }

    List<FriendInfo> _friends = null;
    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false
        }, result => {
            _friends = result.Friends;
            DisplayFriends(_friends); // triggers your UI
        }, DisplayPlayFabError);
    }

    public enum FriendIdType { PlayFabId, Username, Email, DisplayName };
    public void AddFriend(FriendIdType idType, string friendId)
    {
        var request = new AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => {
            Debug.Log("Friend added successfully!");
        }, DisplayPlayFabError);
    }

    public string friendSearch;
    [SerializeField]
    //GameObject friendPanel;

    public void InputFriendID(string idIn)
    {
        friendSearch = idIn;
    }

    public void SubmitFriendRequest()
    {
        AddFriend(FriendIdType.PlayFabId, friendSearch);
    }

    //public void OpenCloseFriends()
    //{
    //    friendPanel.SetActive(!friendPanel.activeInHierarchy);
    //}
    #endregion Friends
}
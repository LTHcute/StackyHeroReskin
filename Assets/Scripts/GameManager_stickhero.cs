using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;
using UniPay;
using UnityEngine.UI;
using UnityEngine.Rendering;

public enum GameState
{
    START,INPUT,GROWING,NONE
}

public class GameManager_stickhero : MonoBehaviour
{
    public static string DIAMOND = "gem";

    [SerializeField]
    private Vector3 startPos;

    [SerializeField]
    private Vector2 minMaxRange, spawnRange;

    [SerializeField]
    private GameObject pillarPrefab, playerPrefab, stickPrefab, diamondPrefab, currentCamera;

    [SerializeField]
    private Transform rotateTransform, endRotateTransform;

    [SerializeField]
    private GameObject scorePanel, startPanel, endPanel, storeMenu;

    [SerializeField]
    private TMP_Text scoreText, scoreEndText, diamondsText, highScoreText;

    private GameObject currentPillar, nextPillar, currentStick, player;

    private int score, highScore;

    private float cameraOffsetX;

    private GameState currentState;

    [SerializeField]
    private float stickIncreaseSpeed, maxStickSize;
    public  AudioManager audio;
    public static GameManager_stickhero instance;

    public AudioSource audioSource;
    public GameObject panel_loading;

    public GameObject intro;
    public GameObject topBar, panelScore, start, storeButton, notification;
    public GameObject sharkPrefab;
    public GameObject wave;

    private void OnEnable()
    {
        IAPManager.purchaseSucceededEvent += UpdateValue;
    }

    private void OnDisable()
    {
        IAPManager.purchaseSucceededEvent -= UpdateValue;
    }

    private void Awake()
    {
        AudioManager.instance.PlayMusic();
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentState = GameState.START;

        endPanel.SetActive(false);
        scorePanel.SetActive(false);
        startPanel.SetActive(false);
        storeMenu.SetActive(false);
        storeButton.SetActive(false);



        StartCoroutine(On());
        
       


    }

    IEnumerator On()
    {
        yield return StartCoroutine(HideIntro());
       // Debug.Log("continue");
        yield return StartCoroutine(CreateMenu());

    }

    IEnumerator CreateMenu()
    {
       // Debug.Log("Create menu");

       

        score = 0;
        highScore = PlayerPrefs.HasKey("HighScore_stickhero") ? PlayerPrefs.GetInt("HighScore_stickhero") : 0;
        // tạo cá mập
        //BoxCollider waveCollider = wave.GetComponent<BoxCollider>();
        //Bounds bounds = waveCollider.bounds;

      
        //float randomX = Random.Range(bounds.min.x, bounds.max.x);
        //float randomY = Random.Range(bounds.min.y, bounds.max.y);

        //Vector3 spawnPos = new Vector3(randomX, randomY, 0f);
        //Instantiate(sharkPrefab, spawnPos, Quaternion.identity);
        // GameObject sharks = Instantiate(sharkPrefab,wave.);
        scoreText.text = score.ToString();
        int diamond = DBManager.GetCurrency(DIAMOND);
        diamondsText.text = diamond.ToString();
        highScoreText.text = highScore.ToString();

        CreateStartObjects();
        cameraOffsetX = currentCamera.transform.position.x - player.transform.position.x;
        yield return null;
      //  Debug.Log("Done menu");
    }    

    private void Start()
    {
       
        //AdmobController.Instance.ShowBannerAd(0.5f);
    }

    private void Update()
    {
        int diamond = DBManager.GetCurrency(DIAMOND);
        diamondsText.text = diamond.ToString();
        if (currentState == GameState.INPUT)
        {

            if (Input.GetMouseButton(0))
            {
                currentState = GameState.GROWING;
                ScaleStick();
            }
        }

        if(currentState == GameState.GROWING)
        {
           
            if (StateManager.instance.hasSceneStarted)
            {
                GameStart();
            }
            if (Input.GetMouseButton(0))
            {
                ScaleStick();
                audioSource.Play();
            }
            else
            {
                StartCoroutine(FallStick());
            }
        }
    }

    void ScaleStick()
    {
        Vector3 tempScale = currentStick.transform.localScale;
        tempScale.y += Time.deltaTime * stickIncreaseSpeed;
        if (tempScale.y > maxStickSize)
            tempScale.y = maxStickSize;
        currentStick.transform.localScale = tempScale;
        
    }

    IEnumerator FallStick()
    {
        currentState = GameState.NONE;
        var x = Rotate(currentStick.transform, rotateTransform, 0.4f);
        yield return x;

        Vector3 movePosition = currentStick.transform.position + new Vector3(currentStick.transform.localScale.y,0,0);
        movePosition.y = player.transform.position.y;
        x = Move(player.transform,movePosition,0.5f);
        yield return x;

        var results = Physics2D.RaycastAll(player.transform.position,Vector2.down);
        
        var result = Physics2D.Raycast(player.transform.position, Vector2.down);
        foreach (var temp in results)
        {
            //Debug.Log("1");
            //Debug.Log(temp.collider);
            if(temp.collider.CompareTag("Platform"))
            {
                result = temp;
            }
        }

        if(!result || !result.collider.CompareTag("Platform"))
        {
            AudioManager.instance.PlayGameOver();
            player.GetComponent<Rigidbody2D>().gravityScale = 1f;
            x = Rotate(currentStick.transform, endRotateTransform, 0.5f);
            yield return x;
            GameOver();
        }
        else
        {
            UpdateScore();

            movePosition = player.transform.position;
            movePosition.x = nextPillar.transform.position.x + nextPillar.transform.localScale.x * 0.5f - 0.35f;
            x = Move(player.transform, movePosition, 0.2f);
            yield return x;

            movePosition = currentCamera.transform.position;
            movePosition.x = player.transform.position.x + cameraOffsetX;
            x = Move(currentCamera.transform, movePosition, 0.5f);
            yield return x;

            CreatePlatform();
           // SetRandomSize(nextPillar);
            currentState = GameState.INPUT;
            Vector3 stickPosition = currentPillar.transform.position;
           
            stickPosition.x += currentPillar.transform.localScale.x * 0.5f - 0.05f;
            stickPosition.y = currentStick.transform.position.y;
            stickPosition.z = currentStick.transform.position.z;
           
            Vector3 newPoss = new Vector3(stickPosition.x, stickPosition.y, stickPosition.z);
            currentStick = Instantiate(stickPrefab, newPoss, Quaternion.identity);

            Vector3 colliderOfSticj = currentPillar.GetComponent<BoxCollider2D>().transform.position;
            Vector3 size = currentPillar.GetComponent<BoxCollider2D>().bounds.size;
            Debug.Log("size:" + size);
            Debug.Log("pos:" + colliderOfSticj);
        }
    }


    void CreateStartObjects()
    {
        CreatePlatform();

        Vector3 playerPos = playerPrefab.transform.position;
        playerPos.x += (currentPillar.transform.localScale.x * 0.5f - 0.35f);
        playerPos.y = currentPillar.transform.position.y + currentPillar.transform.localScale.y * 0.5f + 2f; //Thêm dòng này

        player = Instantiate(playerPrefab,playerPos,Quaternion.identity);
        player.name = "Player";

        Vector3 stickPos = stickPrefab.transform.position;
        stickPos.x += (currentPillar.transform.localScale.x +currentPillar.transform.localScale.x * 0.5f - 0.05f);
        stickPos.y = currentPillar.transform.position.y + currentPillar.transform.localScale.y; //Thêm dòng này
        //stickPos.x += currentPillar.transform.localScale.x * 0.5f - 0.05f;
        //stickPos.y = currentPillar.transform.position.y;
        //stickPos.z = currentPillar.transform.position.z;
        currentStick = Instantiate(stickPrefab, stickPos, Quaternion.identity);
    }

    void CreatePlatform()
    {
        Debug.Log("tạo platform");
        var currentPlatform = Instantiate(pillarPrefab);
        currentPlatform.transform.localScale = new Vector3(
        currentPlatform.transform.localScale.x,   // giữ nguyên chiều ngang   Thêm dòng này 
        currentPlatform.transform.localScale.y * 2f, // tăng chiều cao gấp đôi
        currentPlatform.transform.localScale.z    // giữ nguyên chiều sâu
);
       // Debug.Log(currentPlatform.transform.localScale);
        currentPillar = nextPillar == null ? currentPlatform : nextPillar;
        nextPillar = currentPlatform;
        currentPlatform.transform.position = pillarPrefab.transform.position + startPos;
        Vector3 tempDistance = new Vector3(Random.Range(spawnRange.x,spawnRange.y) + currentPillar.transform.localScale.x*0.5f,0,0);
        startPos += tempDistance;

        if(Random.Range(0,20) == 0)
        {
            var tempDiamond = Instantiate(diamondPrefab);
            Vector3 tempPos = currentPlatform.transform.position;
            tempPos.y = currentPlatform.transform.position.y - currentPlatform.transform.position.y/1.5f;
           // tempPos.x = currentPlatform.transform.position.x - currentPlatform.transform.position.x/2;
            tempDiamond.transform.position = tempPos;
        }
    }

    void SetRandomSize(GameObject pillar)
    {
        var newScale = pillar.transform.localScale;
        var allowedScale = nextPillar.transform.position.x - currentPillar.transform.position.x
            - currentPillar.transform.localScale.x * 0.5f - 0.4f;
        newScale.x = Mathf.Max(minMaxRange.x,Random.Range(minMaxRange.x,Mathf.Min(allowedScale,minMaxRange.y)));
        pillar.transform.localScale = newScale;
    }

    void UpdateScore()
    {
        score++;
        scoreText.text = score.ToString();
    }

    void GameOver()
    {
        endPanel.SetActive(true);
        scorePanel.SetActive(false);

        if(score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore_stickhero", highScore);
        }

        scoreEndText.text = score.ToString();
        highScoreText.text = highScore.ToString();
        //AdmobController.Instance.ShowInterstitialAd(0.5f);
    }

    public void UpdateDiamonds()
    {
        DBManager.AddCurrency(DIAMOND, 1);
        int diamond = DBManager.GetCurrency(DIAMOND);
        diamondsText.text = diamond.ToString();
    }


    public void UpdateValue(string obj)
    {
        int diamond = DBManager.GetCurrency(DIAMOND);
        diamondsText.text = diamond.ToString();
    }

    public void GameStart()
    {
       // audio.PlayMusic();
        int diamond = DBManager.GetCurrency(DIAMOND);
       
        if (diamond > 1)
        {
            DBManager.SetCurrency(DIAMOND, diamond - 1);
         //   DBManager.ConsumeCurrency(DIAMOND, 1);

            startPanel.SetActive(false);
            scorePanel.SetActive(true);
            storeButton.SetActive(false);
          //  CreateShark();
            CreatePlatform();
          //  SetRandomSize(nextPillar);
            currentState = GameState.INPUT;
        }
        else
        {
            if (UIShopFeedback.GetInstance() != null)
                UIShopFeedback.ShowMessage("Do not enough energy to start game! please buy more energy in the store.");
        }
        
    }


    public void ShowNotification()
    {
        StartCoroutine(HideNotification());
    }
    

    IEnumerator HideNotification()
    {
        notification.SetActive(true);
        yield return new WaitForSeconds(1);
        notification.SetActive(false);
    }    
    public void GameRestart()
    {
        panel_loading.SetActive(true); 
        StateManager.instance.hasSceneStarted = false;
         SceneManager.LoadScene(0);
     //   StartCoroutine(CreateMenu());
    }

    public void SceneRestart()
    {
        StateManager.instance.hasSceneStarted = true;
        SceneManager.LoadScene(16);
    }

    public void OpenStore()
    {
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();

        Color c = playerSprite.color; 
        c.a = 0f;                    
        playerSprite.color = c;
        storeMenu.SetActive(true);
    }
    public void CloseStore()
    {
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();

        Color c = playerSprite.color;
        c.a = 255f;
        playerSprite.color = c;
        storeMenu.SetActive(false);
    }

    //Helper Functions
    IEnumerator Move(Transform currentTransform,Vector3 target,float time)
    {
        var passed = 0f;
        var init = currentTransform.transform.position;
        while(passed < time)
        {
            //var x = Rotate(currentStick.transform, rotateTransform, 0.4f);
            //yield return x;
            var results = Physics2D.RaycastAll(player.transform.position, Vector2.down);

            var result = Physics2D.Raycast(player.transform.position, Vector2.down);
            foreach (var temp in results)
            {
                Debug.Log("1");
                Debug.Log(temp.collider);
                if (temp.collider.CompareTag("Stick"))
                {
                    result = temp;
                    Debug.Log(result);
                }
            }
            //if (!result || !result.collider.CompareTag("Stick") || !result.collider.CompareTag("Platform"))
            //{
            //    AudioManager.instance.PlayGameOver();
            //    player.GetComponent<Rigidbody2D>().gravityScale = 1f;
            //     var x = Rotate(currentStick.transform, endRotateTransform, 0.5f);
            //    yield return x;
            //    GameOver();
            //    break;
            //}
            passed += Time.deltaTime;
            var normalized = passed / time;
            var current = Vector3.Lerp(init, target, normalized);
            currentTransform.position = current;
            yield return null;
        }
    }

    IEnumerator Rotate(Transform currentTransform, Transform target, float time)
    {
        var passed = 0f;
        var init = currentTransform.transform.rotation;
        while (passed < time)
        {
            passed += Time.deltaTime;
            var normalized = passed / time;
            var current = Quaternion.Slerp(init, target.rotation, normalized);
            currentTransform.rotation = current;
            yield return null;
        }
    }



    IEnumerator HideIntro()
    {
        TextMeshProUGUI[] texts = intro.GetComponentsInChildren<TextMeshProUGUI>(true);
        Image []playerIntro = intro.GetComponentsInChildren<Image>(true);
         
        Debug.Log($"objs:{texts.Length}");
        Debug.Log($"objs:{playerIntro.Length}");
        float elapsed = 0f;
        float fadeDuration = 1.5f;
        while(elapsed<fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            foreach (TextMeshProUGUI g in texts)
            {
                Color c = g.color;
                c.a = alpha;
                g.color = c;
            //     Color playerColor = playerIntro.color;
            //playerColor.a = alpha;
            //playerIntro.color = playerColor;
            }
            foreach (Image g in playerIntro)
            {
                Color c = g.color;
                c.a = alpha;
                g.color = c;
                //     Color playerColor = playerIntro.color;
                //playerColor.a = alpha;
                //playerIntro.color = playerColor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }    

        intro.SetActive(false);
        topBar.SetActive(true);
        panelScore.SetActive(true);
        start.SetActive(true);
        storeButton.SetActive(true);    
    }    


    public void CreateShark()
    {
        Debug.Log("tạo shark");
        Vector3 center = wave.transform.position;
        Vector3 size = wave.transform.localScale;

        int sharkAmount = Random.Range(1, 3);
        for (int i = 0; i < sharkAmount; i++)
        {
            float randomX = center.x + Random.Range(-size.x / 2, size.x / 2);
            float randomY = center.y + Random.Range(-size.y / 2, size.y / 2);
            float randomZ = center.z + Random.Range(-size.z / 2, size.z / 2);

            Vector3 spawnPos = new Vector3(randomX, randomY, randomZ);

            GameObject shark = Instantiate(sharkPrefab, spawnPos, Quaternion.identity, wave.transform);
        }

    }
}

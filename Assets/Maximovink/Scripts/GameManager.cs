
    using System.Collections;
    using MaximovInk.AI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    namespace MaximovInk
    {
        public class GameManager : MonoBehaviour
        {
            public static GameManager Instance;
            [Header("Paricles")]
            public ParticleSystem Trash;
            public ParticleSystem Metal;
            public ParticleSystem Wood;
            public ParticleSystem Glass;
            [Space] public PlayerInventory mainInventory;
            public RobotPartEditor partsEditor;
            public Tooltip tooltip;
            public DroppedItem DroppedItemPrefab;
            public Player player;
            public Bullet bullet;
            public Text ammoText;

            public TextMesh TextMesh;

            private string lastText = string.Empty;

            private float TextCheckerTimer;
            
            public Text Fps;

            public GameObject InventoryPanel, RobotPartsPanel,WorkbenchPanel,OpenWB,OpenInv,OpenRE;

            public GameObject LoadingPanel,MainMenu,PauseMenu,LoadMenu,SaveMenu,BlackBackground;
            public Slider LoadingSlider;
            public Text LoadingText;

            public bool ISPause
            {
                get { return Time.timeScale == 0; }
                set { Time.timeScale = value ? 0 : 1; PauseMenu.SetActive(value); BlackBackground.SetActive(ISPause); }
            }

            public void SetTextMesh(string text)
            {
                if(!TextMesh.gameObject.activeSelf)
                    TextMesh.gameObject.SetActive(true);
                TextMesh.text = text;
            }

            public static int loadingPercent = 0;
            
            public bool MouseIsFree => !EventSystem.current.IsPointerOverGameObject() /*&&
            EventSystem.current.currentSelectedGameObject == null*/;
            
            private void Awake()
            {
                if (Instance != null && Instance != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Instance = this;
                    SceneManager.sceneLoaded += OnLoadScene;
                    DontDestroyOnLoad(Instance);
                }
                
               
            }

            public void LoadScene(int index)
            {
                StartCoroutine(nameof(LoadAsyncronously), index);
            }

            public void Exit()
            {
                Application.Quit();
            }

            private IEnumerator LoadAsyncronously(int index)
            {
                LoadingPanel.gameObject.SetActive(true);
                var operation = SceneManager.LoadSceneAsync(index);
                while (!operation.isDone)
                {
                    var progress = Mathf.Clamp01(operation.progress / .9f);
                    LoadingSlider.value = progress;
                    loadingPercent = (int)(progress * 100);
                    LoadingText.text = LanguageManager.instance.GetValueByKey("_loading");
                    
                    yield return null;
                }
                //LoadingPanel.gameObject.SetActive(false);
            }

            IEnumerator waitingFor()
            {
                ISPause = true;
                while (!ChunkManager.instance.generateComplete)
                {
                    LoadingSlider.value = ChunkManager.instance.generationProgress;
                    LoadingText.text = "Generation terrain " + (int) (ChunkManager.instance.generationProgress * 100) +
                                       "%";
                    yield return null;
                }
                ChunkManager.instance.OnEndGeneration();
                PathfindingManager.instance.GenerateMap();
                
                
                
                while (!PathfindingManager.instance.generateComplete)
                {
                    LoadingSlider.value = PathfindingManager.instance.generationProgress;
                    var progress = (int) (PathfindingManager.instance.generationProgress * 100);
                    LoadingText.text = "Generation grid for ai " +progress +
                                       "%";

                    PathfindingManager.instance.generateComplete = progress == 100;
                    
                    yield return null;
                }
                ChunkManager.instance.Clear();
                PathfindingManager.instance.Clear();
                ISPause = false;
                LoadingPanel.SetActive(false);
                
            }

            
            private void OnLoadScene(Scene arg0, LoadSceneMode arg1)
            {
                LoadMenu.SetActive(false);
                SaveMenu.SetActive(false);
                if (arg0.name != "Menu")
                {
                    player = FindObjectOfType<Player>();
                    OpenRE.SetActive(true);
                    OpenInv.SetActive(true);
                    MainMenu.SetActive(false);
                    BlackBackground.SetActive(false);
                    StartCoroutine(waitingFor());
                }
                else
                {
                    ISPause = false;
                    BlackBackground.SetActive(true);
                    MainMenu.SetActive(true);
                    OpenRE.SetActive(false);
                    OpenInv.SetActive(false);
                    OpenWB.SetActive(false);
                    WorkbenchPanel.SetActive(false);
                    InventoryPanel.SetActive(false);
                    RobotPartsPanel.SetActive(false);
                    LoadingPanel.SetActive(false);

                }
            }

            
            
            public void WorkbenchChanged(bool active)
            {
                OpenWB.SetActive(active);
                if (!active)
                {
                    WorkbenchPanel.SetActive(false);
                }
            }
            

            public void Update()
            {
                if (SceneManager.GetActiveScene().name != "Menu" && !ISPause)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ISPause = true;
                    }

                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        InventoryPanel.SetActive(!InventoryPanel.activeSelf);
                    }

                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        RobotPartsPanel.SetActive(!RobotPartsPanel.activeSelf);
                    }

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (player.workbench)
                        {
                            WorkbenchPanel.SetActive(!WorkbenchPanel.activeSelf);
                            OpenWB.SetActive(WorkbenchPanel.activeSelf);
                        }
                    }
                }
                if(!ISPause)
                    Fps.text = "FPS:" + (1 / Time.smoothDeltaTime).ToString("0.00") + "\n (" + (Time.smoothDeltaTime).ToString("0.00") + ")";
            }


            private void LateUpdate()
            {
                if (TextMesh.gameObject.activeSelf)
                {
                    TextCheckerTimer += Time.deltaTime;

                    if (lastText != TextMesh.text)
                    {
                        TextCheckerTimer = 0;
                        lastText = TextMesh.text;
                    }

                    if (TextCheckerTimer > 10)
                    {
                        TextMesh.gameObject.SetActive(false);
                        
                        TextCheckerTimer = 0;
                    }
                }
            }

            public void DropItem(Inventory.Slot slot)
            {
                var d = Instantiate(DroppedItemPrefab);
                d.transform.position = player.transform.position;
                d.item = slot.item;
                d.UpdateSprite();
            }

            public void MakeParticleAt(GameObject go, Vector3 position)
            {
                ParticleSystem ps = null;
                switch (go.tag.ToLower())
                {
                    case "trash":
                        ps = Instantiate(Instance.Trash, position,
                            Quaternion.identity);
                        break;
                    default:
                        ps = Instantiate(Instance.Metal, position,
                            Quaternion.identity);
                        break;
                }
                Destroy(ps.gameObject, ps.main.startLifetime.constant);
                
    
            }
            
            public void CloseLoadSaveMenu()
            {
                LoadMenu.SetActive(false);
                SaveMenu.SetActive(false);
                PauseMenu.SetActive(SceneManager.GetActiveScene().name != "Menu");
                MainMenu.SetActive(SceneManager.GetActiveScene().name == "Menu");
            }
        }
    }
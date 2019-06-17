
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.Serialization;
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

            public GameObject InventoryPanel, RobotPartsPanel;

            private void Awake()
            {
                if (Instance != null && Instance != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Instance = this;
                    DontDestroyOnLoad(Instance);
                }

                SceneManager.sceneLoaded += OnLoadScene;
            }

            private void OnLoadScene(Scene arg0, LoadSceneMode arg1)
            {
                if (arg0.name != "Menu")
                {
                    player = FindObjectOfType<Player>();
                }
            }

            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    InventoryPanel.SetActive(!InventoryPanel.activeSelf);
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    RobotPartsPanel.SetActive(!RobotPartsPanel.activeSelf);
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
                switch (go.tag)
                {
                    case "Trash":
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
        }
    }
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System.Linq;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#elif UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Game Controller.
/// </summary>
/// <remarks>
/// Calls the settings loader. Spawns new chests. Returns new loot. Generates tiered materials.
/// Updates GUI, Tooltip and Tier Counters.
/// </remarks>
public class GameController : MonoBehaviour
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void HelloString(string str);
#endif
    private GraphicRaycaster canvasRaycaster;
    private EventSystem canvasEventSystem;
    public RectTransform tooltipRect;
    private Text tooltipText;
    public Text[] counterText;
    private List<int> tierCounter;
    public static GameController instance;
    public static Settings Settings;
    public Inventory inventory;
    public GameObject chestPrefab;
    private GameObject activeChest;
    public AudioSource audioSrc0;
    public AudioSource audioSrc1;
    public AudioMixer audioMixer;
    private List<GameObject> mLootObjects;
    private List<List<Material>> mLootObjectsMaterials;
    //common-white, uncommon-green, rare-yellow, epic-purple, legendary-orange
    public List<Color> tierColors = new List<Color>();
    public List<string> tierNames = new List<string>();
    private Material auraMat;
    public Material AuraMat
    {
        get
        {
            return auraMat;
        }
    }
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        tierColors.Add(new Color(1, 1, 1, 1));
        tierColors.Add(new Color(0, 1, 0, 1));
        tierColors.Add(new Color(1, 1, 0, 1));
        tierColors.Add(new Color(1, 0, 1, 1));
        tierColors.Add(new Color(1, 0.5f, 0, 1));
        tierNames.Add("Common");
        tierNames.Add("Uncommon");
        tierNames.Add("Rare");
        tierNames.Add("Epic");
        tierNames.Add("Legendary");
        tierCounter = Enumerable.Repeat<int>(0, 5).ToList();
        auraMat = new Material(Shader.Find("Mobile/Particles/Additive"));
    }
    public Dictionary<string, Sprite> imgDict; //Inventory icons accessed by prefab name.
    IEnumerator Start()
    {
        
        var canv = GameObject.Find("Canvas");
        canvasRaycaster = canv.GetComponent<GraphicRaycaster>();
        canvasEventSystem = canv.GetComponent<EventSystem>();
        tooltipText = tooltipRect.GetChild(0).GetComponent<Text>();
        Settings = new Settings();
        yield return Settings.init("Settings.ini");

        mLootObjects = Resources.LoadAll("LootObjectsActive").OfType<GameObject>().ToList();
        // foreach (var item in mLootObjects) Debug.Log(item);
        new System.Random().Shuffle(mLootObjects);
        int idx = Settings.Variety;
        if (idx > mLootObjects.Count) idx = mLootObjects.Count;
        mLootObjects.RemoveRange(idx, mLootObjects.Count - idx); //leave only Variety number of possible objects
        mLootObjectsMaterials = new List<List<Material>>();
        for (int i = 0; i < mLootObjects.Count; i++) mLootObjectsMaterials.Add(null);

        imgDict = new Dictionary<string, Sprite>(mLootObjects.Count);
        var imgs = Resources.LoadAll("InventoryIcons").OfType<Sprite>().ToList();
        foreach (var s in imgs) imgDict.Add(s.name, s); //maybe should add a check if the object is present in mLootObjects after trimming

        var audios = GetComponents<AudioSource>();
        audioSrc0 = audios[0];
        audioSrc1 = audios[1];
        audioMixer.SetFloat("ReverbRoomVol", Settings.AudioReverbVol);
        audioMixer.SetFloat("ReverbDecayTime", Settings.AudioReverbDecay);
        audioMixer.SetFloat("FlangeWet", Settings.AudioFlangeMix);
        audioMixer.SetFloat("FlangeDry", 1 - Settings.AudioFlangeMix);

        spawnNewChest();
        setCounterTexts();
        Shader.WarmupAllShaders();
    }
    private int mLootCounter = 0;
    private int mRigCounter = 0;
    /// <summary>
    /// Returns next loot.
    /// </summary>
    /// <remarks>
    /// Returns a random item from LootObjectsActive folder with random or rigged tier.
    /// </remarks>
    public GameObject getNextLoot()
    {
        var idx = Random.Range(0, mLootObjects.Count);
        var lootPrefab = mLootObjects[idx];
        var go = Instantiate(lootPrefab);
        var loot3d = go.GetComponent<Loot3D>();
        if (!loot3d) Debug.Log("Loot3D component is not present in the prefab." + go.name);
        //check if rigged
        bool isRigged = false;
        if (Settings.RiggedItems.Length > mRigCounter)
        {
            var rigItem = Settings.RiggedItems[mRigCounter];
            if (rigItem.number == mLootCounter)
            {
                loot3d.lootTier = rigItem.tier;
                mRigCounter++;
                isRigged = true;
            }

        }
        if (!isRigged)
        {
            var tierRnd = Random.value;
            for (int i = 0; i < Settings.Probabilities.Count; i++)
                if (tierRnd < Settings.Probabilities[i])
                {
                    loot3d.lootTier = i;
                    break;
                }
        }
        tierCounter[loot3d.lootTier]++;
        setCounterTexts(); 
        loot3d.iconName = lootPrefab.name;
        go.name = tierNames[loot3d.lootTier] + " " + loot3d.Name;

        if (mLootObjectsMaterials[idx] == null)
            mLootObjectsMaterials[idx] = generateMaterialTiers(lootPrefab.GetComponent<Renderer>().sharedMaterial);
        go.GetComponent<Renderer>().material = mLootObjectsMaterials[idx][loot3d.lootTier];
        mLootCounter++;
        return go;
    }
    private int mHoverCounter = 0;
    void Update()
    {
        //update tooltip
        Loot2D hitLoot = null;
        var rcObj = RaycastScreenPos(Input.mousePosition);
        if (rcObj) hitLoot = rcObj.GetComponent<Loot2D>();
        if (hitLoot)
        {
            if (!tooltipRect.gameObject.activeSelf) mHoverCounter++;
            tooltipRect.gameObject.SetActive(true);
            tooltipText.text = hitLoot.lootName;
            tooltipText.color = tierColors[hitLoot.lootTier];
            tooltipRect.position = Input.mousePosition;
            return;
        }
        CounterLabel label = null;
        if (rcObj) label = rcObj.GetComponent<CounterLabel>();
        if (label)
        {
            var txt = label.GetComponent<Text>();
            tooltipRect.gameObject.SetActive(true);
            tooltipText.text = label.gameObject.name;
            tooltipText.color = txt.color;
            tooltipRect.position = Input.mousePosition - new Vector3(0, tooltipRect.rect.height, 0);
            return;
        }
        else
        {
            tooltipRect.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Updates the counters on top of the inventory panel.
    /// </summary>
    void setCounterTexts()
    {
        counterText[0].text = "C:" + tierCounter[0];
        counterText[1].text = "U:" + tierCounter[1];
        counterText[2].text = "R:" + tierCounter[2];
        counterText[3].text = "E:" + tierCounter[3];
        counterText[4].text = "L:" + tierCounter[4];
        counterText[5].text = "T:" + tierCounter.Sum();
    }
    
    /// <summary>
    /// Wrapper for the coroutine chest spawn call.
    /// </summary>
    public void spawnNewChest(float delay = 0)
    {
        StartCoroutine(spawnNewChestAfter(delay));
    }
    public IEnumerator spawnNewChestAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        activeChest = Instantiate(chestPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
    }
    List<RaycastResult> results = new List<RaycastResult>();

    
    GameObject RaycastScreenPos(Vector2 sp)
    {
        results.Clear();
        var pointerEventData = new PointerEventData(canvasEventSystem);
        pointerEventData.position = sp;
        canvasRaycaster.Raycast(pointerEventData, results);
        if (results.Count() > 0)
            return results[0].gameObject;
        return null;
    }
    
    /// <summary>
    /// Saves data to clipboard if in Editor, 
    /// or calls DataAlert.jslib and shows window.prompt with the Save data if in WebGL.
    /// </summary>
    public void saveDataToClipboard()
    {
        var gameStr = mLootCounter + "\t" + Time.time.ToString("0.000") + "\t" + mHoverCounter + "\t" + inventory.SortCounter;
        #if UNITY_WEBGL
                HelloString(gameStr);
        #elif UNITY_EDITOR
                    EditorGUIUtility.systemCopyBuffer = gameStr; 
        #endif
    }
    /// <summary>
    /// Generate and return 5 tiered versions of the input material.
    /// </summary>
    private List<Material> generateMaterialTiers(Material mat)
    {
        var ret = new List<Material>();
        var propName = "_Color";

        for (int i = 0; i < tierColors.Count; i++)
        {
            ret.Add(new Material(mat));
            ret.Last().SetColor(propName, tierColors[i]);
        }

        return ret;
    }
}
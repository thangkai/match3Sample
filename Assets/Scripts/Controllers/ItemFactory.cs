using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFactory : MonoBehaviour
{
    public static ItemFactory Instance { get; private set; }

    private Dictionary<string, GameObject> m_prefabsDict = new Dictionary<string, GameObject>();
    private Dictionary<string, Queue<Item>> m_pools = new Dictionary<string, Queue<Item>>();

    private Transform m_poolContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeFactory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFactory()
    {
        m_poolContainer = new GameObject("ItemPool").transform;
        m_poolContainer.SetParent(this.transform);

        // Load all prefabs once
        LoadPrefab(Constants.PREFAB_NORMAL_TYPE_ONE);
        LoadPrefab(Constants.PREFAB_NORMAL_TYPE_TWO);
        LoadPrefab(Constants.PREFAB_NORMAL_TYPE_THREE);
        LoadPrefab(Constants.PREFAB_NORMAL_TYPE_FOUR);
        LoadPrefab(Constants.PREFAB_NORMAL_TYPE_FIVE);
        LoadPrefab(Constants.PREFAB_NORMAL_TYPE_SIX);
        LoadPrefab(Constants.PREFAB_NORMAL_TYPE_SEVEN);

        LoadPrefab(Constants.PREFAB_BONUS_HORIZONTAL);
        LoadPrefab(Constants.PREFAB_BONUS_VERTICAL);
        LoadPrefab(Constants.PREFAB_BONUS_BOMB);
    }

    private void LoadPrefab(string path)
    {
        if (!m_prefabsDict.ContainsKey(path))
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                m_prefabsDict.Add(path, prefab);
                m_pools.Add(path, new Queue<Item>());
            }
        }
    }

    public NormalItem GetNormalItem(NormalItem.eNormalType type)
    {
        string prefabPath = GetNormalPrefabPath(type);
        return GetItem<NormalItem>(prefabPath);
    }

    public BonusItem GetBonusItem(BonusItem.eBonusType type)
    {
        string prefabPath = GetBonusPrefabPath(type);
        return GetItem<BonusItem>(prefabPath);
    }

    private T GetItem<T>(string prefabPath) where T : Item, new()
    {
        if (!m_pools.ContainsKey(prefabPath))
        {
            Debug.LogError("Prefab not found for path: " + prefabPath);
            return null;
        }

        Queue<Item> pool = m_pools[prefabPath];
        T item = null;

        if (pool.Count > 0)
        {
            item = pool.Dequeue() as T;
        }
        else
        {
            item = new T();
            GameObject go = Instantiate(m_prefabsDict[prefabPath]);
            item.SetView(go.transform);
            item.PrefabPath = prefabPath; // Store path to know where to return
        }

        item.OnGetFromPool();
        return item;
    }

    public void ReturnItem(Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.PrefabPath)) return;

        item.OnReturnToPool();
        
        if (m_pools.ContainsKey(item.PrefabPath))
        {
            item.View.SetParent(m_poolContainer);
            m_pools[item.PrefabPath].Enqueue(item);
        }
        else
        {
            // Should not happen if logic is correct
            GameObject.Destroy(item.View.gameObject);
        }
    }

    private string GetNormalPrefabPath(NormalItem.eNormalType type)
    {
        switch (type)
        {
            case NormalItem.eNormalType.TYPE_ONE: return Constants.PREFAB_NORMAL_TYPE_ONE;
            case NormalItem.eNormalType.TYPE_TWO: return Constants.PREFAB_NORMAL_TYPE_TWO;
            case NormalItem.eNormalType.TYPE_THREE: return Constants.PREFAB_NORMAL_TYPE_THREE;
            case NormalItem.eNormalType.TYPE_FOUR: return Constants.PREFAB_NORMAL_TYPE_FOUR;
            case NormalItem.eNormalType.TYPE_FIVE: return Constants.PREFAB_NORMAL_TYPE_FIVE;
            case NormalItem.eNormalType.TYPE_SIX: return Constants.PREFAB_NORMAL_TYPE_SIX;
            case NormalItem.eNormalType.TYPE_SEVEN: return Constants.PREFAB_NORMAL_TYPE_SEVEN;
            default: return string.Empty;
        }
    }

    private string GetBonusPrefabPath(BonusItem.eBonusType type)
    {
        switch (type)
        {
            case BonusItem.eBonusType.HORIZONTAL: return Constants.PREFAB_BONUS_HORIZONTAL;
            case BonusItem.eBonusType.VERTICAL: return Constants.PREFAB_BONUS_VERTICAL;
            case BonusItem.eBonusType.ALL: return Constants.PREFAB_BONUS_BOMB;
            default: return string.Empty;
        }
    }
}

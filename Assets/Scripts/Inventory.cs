using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Inventory grid behaviour.
/// </summary>
public class Inventory : MonoBehaviour
{
    public RectTransform emptySlot;
    private List<GameObject> items = new List<GameObject>();
    public void AddItem(Loot2D item)
    {
        var es = Instantiate(emptySlot);
        es.transform.SetParent(transform, false);
        item.transform.position = Vector3.zero;
        item.transform.SetParent(es, false);

        //  maybe should create empty slots, calculate from the last cell:
        //  the amount of possible full rows + cells in the current row

    }
    private int mSortCounter = 0;
    public void SortItems()
    {
        mSortCounter++;
        var tmpChildrenLoot2D = new List<Loot2D>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var ch = transform.GetChild(i);
            Loot2D item = ch.GetComponentInChildren<Loot2D>();
            if (item) tmpChildrenLoot2D.Add(item);
        }

        transform.DetachChildren();
        
        tmpChildrenLoot2D.Sort((a, b) => b.lootTier.CompareTo(a.lootTier));

        foreach (var item in tmpChildrenLoot2D) 
            item.gameObject.transform.parent.SetParent(transform);
    }
    GridLayoutGroup mGridLG;
    RectTransform inventoryRT;
    public int SortCounter
    {
        get
        {
            return mSortCounter;
        }
    }
    void Start()
    {
        mGridLG = GetComponent<GridLayoutGroup>();
        inventoryRT = GetComponent<RectTransform>();
    }
    
    void Update()
    {
        //shrink if last child is lower than the bottom of the inventory panel
        if (transform.childCount > 0 && transform.GetChild(transform.childCount - 1).transform.position.y < inventoryRT.position.y)
            mGridLG.cellSize = mGridLG.cellSize * 0.995f; 
    }


}

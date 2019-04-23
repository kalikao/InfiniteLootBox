using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loot representation in 2D, in the inventory.
/// </summary>
public class Loot2D : MonoBehaviour
{
    public string lootName;
    public int lootTier;

    /// <summary>
    /// Constructor.
    /// </summary>
    public static Loot2D Create(Sprite icon, string name, int tier){
        var go = new GameObject();
        go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        var img = go.AddComponent<Image>();
        img.sprite = icon;
        img.color = GameController.instance.tierColors[tier];
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        var pad = 8; 
        img.rectTransform.offsetMin = new Vector2(pad, pad);
        img.rectTransform.offsetMax = new Vector2(-pad, -pad);
        var l2d = go.AddComponent<Loot2D>();
        l2d.lootName = name;
        l2d.lootTier = tier;
        go.name = name;
        GameController.instance.inventory.AddItem(l2d);
        return l2d;
    }
}

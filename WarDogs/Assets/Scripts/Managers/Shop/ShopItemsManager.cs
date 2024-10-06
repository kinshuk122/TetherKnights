using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemManager : MonoBehaviour
{
    public enum  ItemType
    {
        GunLMG,
        GunSMG,
        GunRevolver,
        TurretAutomatic,
        TurretSemiAutomatic,
    }

    public static int GetCost(ItemType itemType)
    {
        switch (itemType)
        {
            default:
                case ItemType.GunLMG: return 200;
                case ItemType.GunSMG: return 150;
                case ItemType.GunRevolver: return 100;
                case ItemType.TurretAutomatic: return 300;
                case ItemType.TurretSemiAutomatic: return 250;
        }
    }
}

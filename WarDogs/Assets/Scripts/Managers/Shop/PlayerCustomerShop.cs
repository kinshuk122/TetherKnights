using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlayerCustomerShop
{
    void BoughtItem(ShopItemManager.ItemType itemType);
    
    bool TrySpendCandy(int amount);
}


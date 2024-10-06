using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : NetworkBehaviour
{
    [Header("References")]
    public Transform shopItemTemplate;
    public Transform container;
    private PlayerCustomerShop shopCustomer;

    private void Start()
    {
        CreateItemButton(ShopItemManager.ItemType.GunLMG, ShopItemManager.ItemType.GunLMG.ToString(), ShopItemManager.GetCost(ShopItemManager.ItemType.GunLMG), 0);
        CreateItemButton(ShopItemManager.ItemType.GunSMG, ShopItemManager.ItemType.GunSMG.ToString(), ShopItemManager.GetCost(ShopItemManager.ItemType.GunSMG), 1);
        CreateItemButton(ShopItemManager.ItemType.GunRevolver, ShopItemManager.ItemType.GunRevolver.ToString(), ShopItemManager.GetCost(ShopItemManager.ItemType.GunRevolver), 2);
        CreateItemButton(ShopItemManager.ItemType.TurretAutomatic, ShopItemManager.ItemType.TurretAutomatic.ToString(), ShopItemManager.GetCost(ShopItemManager.ItemType.TurretAutomatic), 3);
        CreateItemButton(ShopItemManager.ItemType.TurretSemiAutomatic, ShopItemManager.ItemType.TurretSemiAutomatic.ToString(), ShopItemManager.GetCost(ShopItemManager.ItemType.TurretSemiAutomatic), 4);
        
        Hide();
    }

    private void CreateItemButton(ShopItemManager.ItemType itemType ,string itemName, int itemCost, int positionIndex)
    {
        Transform shopItemTransform = Instantiate(shopItemTemplate, container);
        RectTransform shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();
        
        float shopItemHeight = 40f;
        shopItemRectTransform.anchoredPosition = new Vector2(0, -shopItemHeight * positionIndex);
        
        shopItemTransform.Find("ItemText").GetComponent<TMPro.TextMeshProUGUI>().text = itemName;
        shopItemTransform.Find("CostText").GetComponent<TMPro.TextMeshProUGUI>().text = itemCost.ToString();
        
        shopItemTransform.GetComponent<Button>().onClick.AddListener(() =>
        {
            TryBuyItem(itemType);
        });
    }
    
    private void TryBuyItem(ShopItemManager.ItemType itemType)
    {
        if (shopCustomer.TrySpendCandy(ShopItemManager.GetCost(itemType)))
        {
            shopCustomer.BoughtItem(itemType); 
        }
    }
    
    public void Show(PlayerCustomerShop shopCustomer)
    { 
        this.shopCustomer = shopCustomer;
        container.gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        container.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCustomerShop shopCustomer = other.GetComponent<PlayerCustomerShop>();
            NetworkBehaviour networkBehaviour = other.GetComponent<NetworkBehaviour>();

            if (networkBehaviour.IsLocalPlayer)
            {
                Show(shopCustomer);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkBehaviour networkBehaviour = other.GetComponent<NetworkBehaviour>();

            if (networkBehaviour.IsLocalPlayer)
            {
                Hide();
            }
        }
    }
}

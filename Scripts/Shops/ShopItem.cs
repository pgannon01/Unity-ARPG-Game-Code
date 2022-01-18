using System;
using GameDevTV.Inventories;
using UnityEngine;

namespace RPG.Shops
{
    public class ShopItem
    {
        // Why a class and not a struct?
        // Could go with a struct, but sticking with a class for now for simplicity
        // Do need a class because we need to hold more information than a simple variable
        InventoryItem item;
        int availability;
        float price;
        int quantityInTransaction;

        // Need a constructor, which we often don't need in Unity C#
        public ShopItem(InventoryItem item, int availability, float price, int quantityInTransaction)
        {
            // Going to initialize those variables
            this.item = item;
            this.availability = availability;
            this.price = price;
            this.quantityInTransaction = quantityInTransaction;
        }

        public float GetPrice()
        {
            return price;
        }

        public InventoryItem GetInventoryItem()
        {
            return item;
        }

        public int GetAvailability()
        {
            return availability;
        }

        public string GetName()
        {
            return item.GetDisplayName();
        }

        public Sprite GetIcon()
        {
            return item.GetIcon();
        }

        public int GetQuantityInTransaction()
        {
            return quantityInTransaction;
        }
    }
}
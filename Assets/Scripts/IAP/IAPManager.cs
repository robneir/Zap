﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using GameCritical;
using Utility;

// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager, 
// one of the existing Survival Shooter scripts.
namespace IAP
{
    // Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        public static IAPManager Instance;

        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        public static string PRODUCT_50_ZAPS = /*"com.rgs.zap."*/"zaps50";
        public static string PRODUCT_100_ZAPS = /*"com.rgs.zap."*/"zaps100";
        public static string PRODUCT_250_ZAPS = /*"com.rgs.zap.*/"zaps250";
        public static string PRODUCT_1000_ZAPS = /*"com.rgs.zap.*/"zaps1000";

        private AudioClip successfulPurchaseClip;

        /*public static string PRODUCT_50_ZAPS_GooglePlay = "zaps50";
        public static string PRODUCT_100_ZAPS_GooglePlay = "zaps100";
        public static string PRODUCT_250_ZAPS_GooglePlay = "zaps250";
        public static string PRODUCT_1000_ZAPS_GooglePlay = "zaps1000";

        public static string PRODUCT_50_ZAPS_Apple = "zaps50";
        public static string PRODUCT_100_ZAPS_Apple = "zaps100";
        public static string PRODUCT_250_ZAPS_Apple = "zaps250";
        public static string PRODUCT_1000_ZAPS_Apple = "zaps1000";*/

        private void Awake()
        {
            // create static instance if there is not one
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                }
            }

            DontDestroyOnLoad(this);

            successfulPurchaseClip = Resources.Load<AudioClip>(PrefabFinder.AUDIO + "Sounds/Cash register");
        }

        private void Start()
        {
            // If we haven't set up the Unity Purchasing reference
            if (m_StoreController == null)
            {
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
            }
        }
        public void InitializePurchasing()
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add products
            builder.AddProduct(PRODUCT_50_ZAPS, ProductType.Consumable);
            builder.AddProduct(PRODUCT_100_ZAPS, ProductType.Consumable);
            builder.AddProduct(PRODUCT_250_ZAPS, ProductType.Consumable);
            builder.AddProduct(PRODUCT_1000_ZAPS, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }
        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void Buy50Zaps()
        {
            BuyProductID(PRODUCT_50_ZAPS);
        }
        public void Buy100Zaps()
        {
            BuyProductID(PRODUCT_100_ZAPS);
        }
        public void Buy250Zaps()
        {
            BuyProductID(PRODUCT_250_ZAPS);
        }
        public void Buy1000Zaps()
        {
            BuyProductID(PRODUCT_1000_ZAPS);
        }

        private void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        //  
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            Debug.Log("IAPInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;
        }
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        private void AddZaps(int numZaps)
        {
            StatsManager statsManager = GameMaster.Instance.m_StatsManager;
            if (statsManager == null)
            {
                statsManager = FindObjectOfType<StatsManager>();
            }

            if (statsManager != null)
            {
                statsManager.AddZaps(numZaps);
            }
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            // A consumable product has been purchased by this user.
            if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_50_ZAPS, StringComparison.Ordinal))
            {
                Debug.Log("You've just bought 50 zaps! good times!");
                AddZaps(50);
                AudioManager.Instance.Spawn2DAudio(successfulPurchaseClip, true);
                GameMaster.Instance.m_UIManager.m_ShopCanvas.m_WarpStorePanel.m_SuccessfulPurchaseAnimation.Play();
            }
            else if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_100_ZAPS, StringComparison.Ordinal))
            {
                Debug.Log("You've just bought 100 zaps! good times!");
                AddZaps(100);
                AudioManager.Instance.Spawn2DAudio(successfulPurchaseClip, true);
                GameMaster.Instance.m_UIManager.m_ShopCanvas.m_WarpStorePanel.m_SuccessfulPurchaseAnimation.Play();
            }
            else if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_250_ZAPS, StringComparison.Ordinal))
            {
                Debug.Log("You've just bought 250 zaps! good times!");
                AddZaps(250);
                AudioManager.Instance.Spawn2DAudio(successfulPurchaseClip, true);
                GameMaster.Instance.m_UIManager.m_ShopCanvas.m_WarpStorePanel.m_SuccessfulPurchaseAnimation.Play();
            }
            else if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_1000_ZAPS, StringComparison.Ordinal))
            {
                Debug.Log("You've just bought 1000 zaps! good times!");
                AddZaps(1000);
                AudioManager.Instance.Spawn2DAudio(successfulPurchaseClip, true);
                GameMaster.Instance.m_UIManager.m_ShopCanvas.m_WarpStorePanel.m_SuccessfulPurchaseAnimation.Play();
            }
            else
            {
                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            }

            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }
    }
}
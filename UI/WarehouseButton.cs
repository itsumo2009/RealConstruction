using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using RealConstruction.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RealConstruction.UI
{
    [HarmonyPatch]
    public class WarehouseWorldInfoPanelDropDownIndexChangedPatch
    {   public static MethodBase TargetMethod()
        {
            return typeof(WarehouseWorldInfoPanel).GetMethod("OnDropdownResourceChanged", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { 
                typeof(UIComponent),
                typeof(int) }, null);
        }
        public static bool Prefix(UIComponent component, int index)
        {
            
            return true;
        }

    }

    [HarmonyPatch]
    public class WarehouseAISetTransferPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(WarehouseAI).GetMethod("SetContentFlags", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {
                typeof(ushort),
                typeof(Building).MakeByRefType(),
                typeof(TransferManager.TransferReason) 
            }, null);
        }
        public static void Postfix(ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            if (material == (TransferManager.TransferReason)124)
                data.m_flags |= Building.Flags.IncomingOutgoing;
        }

    }

    [HarmonyPatch]
    public class WarehouseWorldInfoPanelGenerateDescriptionPathc
    {
        public static MethodBase TargetMethod()
        {
            return typeof(WarehouseWorldInfoPanel).GetMethod("GenerateResourceDescription", BindingFlags.Public | BindingFlags.Static, null, new Type[]
                {
                    typeof(TransferManager.TransferReason),
                    typeof(bool)
                }, null);
        }
        public static bool Prefix(TransferManager.TransferReason resource, bool isForWarehousePanel, ref String __result)
        {
            string str = ColossalFramework.Globalization.Locale.Get("RESOURCEDESCRIPTION", resource.ToString());
            if (resource == (TransferManager.TransferReason)124)
            {
                str = str + System.Environment.NewLine + System.Environment.NewLine;
                str = str + System.Environment.NewLine;
                str = str + "- " + ColossalFramework.Globalization.Locale.Get("RESOURCE_CANBEEXPORTED_COST");
                if (isForWarehousePanel)
                {
                    __result = str;
                    return false;
                }
                str = str + System.Environment.NewLine + System.Environment.NewLine;
                str = str + "- " + ColossalFramework.Globalization.Locale.Get("RESOURCE_STOREINWAREHOUSE");
                object[] p = new object[] { ColossalFramework.Globalization.Locale.Get("WAREHOUSEPANEL_RESOURCE", resource.ToString()) };
                str = str + "- " + LocaleFormatter.FormatGeneric("RESOURCE_STOREINSTORAGEBUILDING", p);
                
                __result = str;
                return false;
            }

            return true;
        }
    }
    public class WarehouseButton : UIButton
    {
        public static bool refeshOnce = false;
        private UIPanel playerBuildingInfo;
        private WareHouseUI wareHouseUI;
        private InstanceID BuildingID = InstanceID.Empty;
        public void PlayerBuildingUIToggle()
        {
            if (!wareHouseUI.isVisible && (BuildingID != InstanceID.Empty))
            {
                WareHouseUI.refeshOnce = true;
                wareHouseUI.position = new Vector3(playerBuildingInfo.size.x, playerBuildingInfo.size.y);
                wareHouseUI.size = new Vector3(playerBuildingInfo.size.x, playerBuildingInfo.size.y);
                wareHouseUI.Show();
            }
            else
            {
                wareHouseUI.Hide();
            }
        }

        public override void Start()
        {
            base.normalBgSprite = "ToolbarIconGroup1Nomarl";
            base.hoveredBgSprite = "ToolbarIconGroup1Hovered";
            base.focusedBgSprite = "ToolbarIconGroup1Focused";
            base.pressedBgSprite = "ToolbarIconGroup1Pressed";
            base.playAudioEvents = true;
            base.name = "PBButton";
            base.relativePosition = new Vector3(90f, 0f);
            UISprite internalSprite = base.AddUIComponent<UISprite>();
            internalSprite.atlas = SpriteUtilities.GetAtlas(Loader.m_atlasName);
            internalSprite.spriteName = "Pic";
            internalSprite.relativePosition = new Vector3(0, 0);
            internalSprite.width = internalSprite.height = 40f;
            base.size = new Vector2(40f, 40f);
            //Setup UniqueFactoryUI
            var buildingWindowGameObject = new GameObject("buildingWindowObject");
            wareHouseUI = (WareHouseUI)buildingWindowGameObject.AddComponent(typeof(WareHouseUI));
            playerBuildingInfo = UIView.Find<UIPanel>("(Library) WarehouseWorldInfoPanel");
            if (playerBuildingInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): (Library) WarehouseWorldInfoPanel\nAvailable panels are:\n");
            }
            wareHouseUI.transform.parent = playerBuildingInfo.transform;
            wareHouseUI.baseBuildingWindow = playerBuildingInfo.gameObject.transform.GetComponentInChildren<WarehouseWorldInfoPanel>();
            base.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                PlayerBuildingUIToggle();
            };

            var inst = wareHouseUI.baseBuildingWindow;
            var transferReasons = typeof(WarehouseWorldInfoPanel).GetField("m_transferReasons", BindingFlags.NonPublic | BindingFlags.Instance);
            var transferReasons1 = transferReasons.GetValue(inst) as TransferManager.TransferReason[];
            transferReasons1 = transferReasons1.AddToArray((TransferManager.TransferReason)124);
            transferReasons.SetValue(inst, transferReasons1);

            var refreshDropDown_ = typeof(WarehouseWorldInfoPanel).GetMethod("RefreshDropdownLists", BindingFlags.NonPublic | BindingFlags.Instance);

            refreshDropDown_.Invoke(inst, null);

            ColossalFramework.Globalization.Locale.Get(transferReasons1.ToString(), transferReasons1.Length);
            {
                var dd_res = typeof(WarehouseWorldInfoPanel).GetField("m_dropdownResource", BindingFlags.NonPublic | BindingFlags.Instance);

                var dd_res_var = dd_res.GetValue(inst) as UIDropDown;
                dd_res_var.AddItem("Стройматериалы");
            }
        }

        public override void Update()
        {
            if (Loader.isGuiRunning)
            {
                if (WorldInfoPanel.GetCurrentInstanceID() != InstanceID.Empty)
                {
                    BuildingID = WorldInfoPanel.GetCurrentInstanceID();
                }
                base.Show();
            }
            else
            {
                base.Hide();
            }
            base.Update();
        }
    }
}
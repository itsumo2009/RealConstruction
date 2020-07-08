using ColossalFramework;
using ColossalFramework.Globalization;
using HarmonyLib;
using RealConstruction.CustomAI;
using RealConstruction.CustomManager;
using RealConstruction.NewAI;
using RealConstruction.Util;
using System;
using System.Reflection;

namespace RealConstruction.Patch
{
    [HarmonyPatch]
    public static class CargoTruckAIArriveAtTargetPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CargoTruckAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        }
        public static void Prefix(ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                // NON-STOCK CODE START
                CargoTruckAIArriveAtTargetForRealConstruction(vehicleID, ref data);
            }
        }

        public static void CargoTruckAIArriveAtTargetForRealConstruction(ushort vehicleID, ref Vehicle vehicleData)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (vehicleData.m_targetBuilding != 0)
            {
                Building buildingData = instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];

                
                if (!(buildingData.Info.m_buildingAI is OutsideConnectionAI))
                {
                    if (buildingData.m_flags.IsFlagSet(Building.Flags.Created) && (!buildingData.m_flags.IsFlagSet(Building.Flags.Completed)) && (!buildingData.m_flags.IsFlagSet(Building.Flags.Deleted)))
                    {
                        Locale.Get("ZED", (int)buildingData.Info.m_mesh.bounds.size.z);
                        if (vehicleData.m_transferType == 124 || (TransferManager.TransferReason)(vehicleData.m_transferType) == TransferManager.TransferReason.Goods)
                        {
                            ushort square = (ushort)(buildingData.Info.m_cellLength * buildingData.Info.m_cellWidth * 10);
                            square -= MainDataStore.constructionResourceBuffer[vehicleData.m_targetBuilding];
                            if (vehicleData.m_transferSize > square * 1000)
                            {
                                MainDataStore.constructionResourceBuffer[vehicleData.m_targetBuilding] += square;
                                vehicleData.m_transferSize -= (ushort)(square * 1000);
                            }
                            else
                            {
                                MainDataStore.constructionResourceBuffer[vehicleData.m_targetBuilding] += (ushort)(vehicleData.m_transferSize * 100);
                                vehicleData.m_transferSize = 0;
                            }

                            MainDataStore.vehicleFree[vehicleID] = true;
                        }
                    }
                    else
                    {
                        if (ResourceBuildingAI.IsSpecialBuilding(vehicleData.m_targetBuilding) == true)
                        {
                            switch ((TransferManager.TransferReason)vehicleData.m_transferType)
                            {
                                case TransferManager.TransferReason.Goods:
                                    if (MainDataStore.constructionResourceBuffer[vehicleData.m_targetBuilding] + vehicleData.m_transferSize < 64000)
                                        MainDataStore.constructionResourceBuffer[vehicleData.m_targetBuilding] += vehicleData.m_transferSize;
                                    else
                                        MainDataStore.constructionResourceBuffer[vehicleData.m_targetBuilding] = 64000;

                                    vehicleData.m_transferSize = 0;
                                    break;
                                case TransferManager.TransferReason.Food:
                                    if (MainDataStore.foodBuffer[vehicleData.m_targetBuilding] < 57000)
                                    {
                                        MainDataStore.foodBuffer[vehicleData.m_targetBuilding] += vehicleData.m_transferSize;
                                    }
                                    if (Loader.isRealCityRunning)
                                    {
                                        RealCityUtil.InitDelegate();
                                        float productionValue1 = vehicleData.m_transferSize * RealCityUtil.GetResourcePrice((TransferManager.TransferReason)vehicleData.m_transferType);
                                        Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, (int)productionValue1, ItemClass.Service.PlayerIndustry, ItemClass.SubService.PlayerIndustryFarming, ItemClass.Level.Level1);
                                    }
                                    vehicleData.m_transferSize = 0;
                                    break;
                                case TransferManager.TransferReason.Lumber:

                                    if (MainDataStore.lumberBuffer[vehicleData.m_targetBuilding] < 57000)
                                    {
                                        MainDataStore.lumberBuffer[vehicleData.m_targetBuilding] += vehicleData.m_transferSize;
                                    }
                                    if (Loader.isRealCityRunning)
                                    {
                                        RealCityUtil.InitDelegate();
                                        float productionValue1 = vehicleData.m_transferSize * RealCityUtil.GetResourcePrice((TransferManager.TransferReason)vehicleData.m_transferType);
                                        Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, (int)productionValue1, ItemClass.Service.PlayerIndustry, ItemClass.SubService.PlayerIndustryForestry, ItemClass.Level.Level1);
                                    }
                                    vehicleData.m_transferSize = 0;
                                    break;
                                case TransferManager.TransferReason.Coal:
                                    if (MainDataStore.coalBuffer[vehicleData.m_targetBuilding] < 57000)
                                    {
                                        MainDataStore.coalBuffer[vehicleData.m_targetBuilding] += vehicleData.m_transferSize;
                                    }
                                    if (Loader.isRealCityRunning)
                                    {
                                        RealCityUtil.InitDelegate();
                                        float productionValue1 = vehicleData.m_transferSize * RealCityUtil.GetResourcePrice((TransferManager.TransferReason)vehicleData.m_transferType);
                                        Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, (int)productionValue1, ItemClass.Service.PlayerIndustry, ItemClass.SubService.PlayerIndustryOre, ItemClass.Level.Level1);
                                    }
                                    vehicleData.m_transferSize = 0;
                                    break;
                                case TransferManager.TransferReason.Petrol:
                                    if (MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] < 57000)
                                    {
                                        MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] += vehicleData.m_transferSize;
                                    }
                                    if (Loader.isRealCityRunning)
                                    {
                                        RealCityUtil.InitDelegate();
                                        float productionValue1 = vehicleData.m_transferSize * RealCityUtil.GetResourcePrice((TransferManager.TransferReason)vehicleData.m_transferType);
                                        Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, (int)productionValue1, ItemClass.Service.PlayerIndustry, ItemClass.SubService.PlayerIndustryOil, ItemClass.Level.Level1);
                                    }
                                    vehicleData.m_transferSize = 0;
                                    break;
                                default:
                                    //DebugLog.LogToFileOnly("Error: Unknow m_transferType in realconstruction = " + vehicleData.m_transferType.ToString()); 
                                    break;
                            }
                        }
                        else
                        {
                            if (vehicleData.m_transferType == 125)
                            {
                                vehicleData.m_transferSize = 0;
                                MainDataStore.operationResourceBuffer[vehicleData.m_targetBuilding] += 8000;
                            }
                        }
                    }
                }
            }
        }
    }
}

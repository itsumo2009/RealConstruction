using ColossalFramework;
using ColossalFramework.Globalization;
using RealConstruction.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RealConstruction.NewAI
{
    public class ConstructionAI
    {
        public static void ProcessBuildingConstruction(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            var ml = buildingData.Info.m_cellLength;
            var mw = buildingData.Info.m_cellWidth;

            int square = mw * ml;

            int total_resources_need = 10 * square;

            int current_resources = MainDataStore.constructionResourceBuffer[buildingID];
            if (current_resources < total_resources_need && (!ResourceBuildingAI.IsSpecialBuilding(buildingID)))
            {
                System.Random rand = new System.Random();
                if (buildingData.m_flags.IsFlagSet(Building.Flags.Created) && (!buildingData.m_flags.IsFlagSet(Building.Flags.Completed)) && (!buildingData.m_flags.IsFlagSet(Building.Flags.Deleted)))
                {
                    byte x = (byte)(current_resources * 255 / total_resources_need) ;
                    frameData.m_constructState = Math.Min(x, frameData.m_constructState);
                    int num27 = 0;
                    int num28 = 0;
                    int num29 = 0;
                    int value = 0;
                    int num34 = 0;
                    TransferManager.TransferReason incomingTransferReason = default(TransferManager.TransferReason);
                    //construction resource
                    incomingTransferReason = (TransferManager.TransferReason)124;
                    num27 = 0;
                    num28 = 0;
                    num29 = 0;
                    value = 0;
                    num34 = 0;
                    if (incomingTransferReason != TransferManager.TransferReason.None)
                    {
                        CaculationVehicle.CustomCalculateGuestVehicles(buildingID, ref buildingData, incomingTransferReason, ref num27, ref num28, ref num29, ref value);
                        buildingData.m_tempImport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempImport, 255);
                    }

                    num34 = total_resources_need - current_resources - num29;
                    if (num34 > 0)
                    {
                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                        offer.Priority = rand.Next(8);
                        if ((buildingData.Info.m_class.m_service != ItemClass.Service.Residential) && (buildingData.Info.m_class.m_service != ItemClass.Service.Industrial) && (buildingData.Info.m_class.m_service != ItemClass.Service.Commercial) && (buildingData.Info.m_class.m_service != ItemClass.Service.Office))
                        {
                            offer.Priority = 7;
                        }
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = 1;
                        offer.Active = false;
                        Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                    }
                }
            }
            else
            {
                if (!ResourceBuildingAI.IsSpecialBuilding(buildingID) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
                {
                    MainDataStore.constructionResourceBuffer[buildingID] = 0;
                }
            }
        }
    }
}

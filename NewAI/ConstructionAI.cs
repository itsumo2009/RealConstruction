using ColossalFramework;
using ColossalFramework.Globalization;
using RealConstruction.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace RealConstruction.NewAI
{
    public class ConstructionAI
    {
        public static ushort ConstructionResourcesNeed(ref Building buildingData)
        {
            var ml = buildingData.Info.m_cellLength;
            var mw = buildingData.Info.m_cellWidth;

            int square = mw * ml;
            int height = ((int)(buildingData.Info.m_collisionHeight / 8) + 1);
            int factor;
            switch (height)
            {
                case 1:
                    factor = 500;
                    break;
                case 2:
                    factor = 750;
                    break;
                case 3:
                    factor = 900;
                    break;
                default:
                    factor = height / 4 * 1250;
                    break;
            }
            
            return (ushort)((square + 1) * factor);
        }

        public static int ConstructionResourcesInDelivery(ushort buildingID, ref Building buildingData)
        {
            int count = 0;
            int cargo = 0;
            int capacity = 0;
            int outside = 0;
            CaculationVehicle.CustomCalculateGuestVehicles(buildingID, ref buildingData, (TransferManager.TransferReason)(124), ref count, ref cargo, ref capacity, ref outside);
            return cargo;
        }

        public static void RequestResources(ushort buildingID, ref Building buildingData, TransferManager.TransferReason reason)
        {               
            int current_resources = MainDataStore.Current(buildingID, true) + ConstructionResourcesInDelivery(buildingID, ref buildingData);

            TransferManager.TransferReason incomingTransferReason = reason;
            
            if (current_resources < 0)
            {
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Priority = UnityEngine.Random.Range(0, 8);
                if (
                    (buildingData.Info.m_class.m_service != ItemClass.Service.Residential) &&
                    (buildingData.Info.m_class.m_service != ItemClass.Service.Industrial)  &&
                    (buildingData.Info.m_class.m_service != ItemClass.Service.Commercial)  &&
                    (buildingData.Info.m_class.m_service != ItemClass.Service.Office))
                {
                    offer.Priority = 7;
                }

                offer.Building = buildingID;
                offer.Position = buildingData.m_position;
                offer.Amount = -current_resources;
                offer.Active = false;
                Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
            }

        }
        public static void ProcessBuildingConstruction(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            MainDataStore.CheckStart(buildingID, ref buildingData);

            if ( buildingData.m_flags.IsFlagSet(Building.Flags.Created)   && 
                !buildingData.m_flags.IsFlagSet(Building.Flags.Completed) &&
               (!buildingData.m_flags.IsFlagSet(Building.Flags.Deleted)))
            {
                ushort total_resources_need = ConstructionResourcesNeed(ref buildingData);
                int current_resources = MainDataStore.Current(buildingID);

                byte x = (byte)(Math.Min(current_resources * 255 / total_resources_need, 255));
                frameData.m_constructState = Math.Min(x, frameData.m_constructState);
                

                RequestResources(buildingID, ref buildingData, (TransferManager.TransferReason)124);
            }

            MainDataStore.CheckComplete(buildingID, ref buildingData);
        }
    }
}

using ColossalFramework.Globalization;
using RealConstruction.NewAI;
using RealConstruction.Util;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace RealConstruction.CustomAI
{
    public class CustomCargoTruckAI: CargoTruckAI
    {
        public static void CargoTruckAISetSourceForRealConstruction(ushort vehicleID, ref Vehicle data, ushort sourceBuilding)
        {
            CargoTruckAI AI = data.Info.m_vehicleAI as CargoTruckAI;
            
            //new added begin
            if (ResourceBuildingAI.IsSpecialBuilding(sourceBuilding))
            {
                if ((TransferManager.TransferReason)data.m_transferType == (TransferManager.TransferReason)124)
                {
                    if (MainDataStore.constructionResourceBuffer[sourceBuilding] < data.m_transferSize / 100)
                        data.m_transferSize = (ushort) (MainDataStore.constructionResourceBuffer[sourceBuilding] * 100);
                    
                    MainDataStore.constructionResourceBuffer[sourceBuilding] -= (ushort)(data.m_transferSize / 100);
                }
                else if ((TransferManager.TransferReason)data.m_transferType == (TransferManager.TransferReason)125)
                {
                    MainDataStore.operationResourceBuffer[sourceBuilding] -= 8000;
                }
                else
                {
                    DebugLog.LogToFileOnly("find unknow transfor for SpecialBuilding " + data.m_transferType.ToString());
                }
            }
        }
    }
}

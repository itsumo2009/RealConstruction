using ColossalFramework;
using HarmonyLib;
using RealConstruction.CustomAI;
using RealConstruction.NewAI;
using RealConstruction.Util;
using System;
using System.Reflection;

namespace RealConstruction.Patch
{
    [HarmonyPatch]
    public class OutsideConnectionAISimulationStepPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(OutsideConnectionAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType() }, null);
        }
		
        public static void Postfix(ushort buildingID, ref Building data)
        {
            TransferManager.TransferReason outgoingTransferReason = default(TransferManager.TransferReason); 
            outgoingTransferReason = (TransferManager.TransferReason)124;
            int count = 0;
            int cargo = 0;
            int max = 0;
            int outside = 0;
            CaculationVehicle.CustomCalculateOwnVehicles(buildingID, ref data, outgoingTransferReason, ref count, ref cargo, ref max, ref outside);

            if (cargo > 8000)
                return;

            TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
            
            offer2.Building = buildingID;
            offer2.Priority = 1;
            offer2.Position = data.m_position;
            offer2.Amount = 1;
            offer2.Active = true;
            Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer2);
		}
    }
}

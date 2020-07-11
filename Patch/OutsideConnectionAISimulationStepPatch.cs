using ColossalFramework;
using HarmonyLib;
using RealConstruction.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RealConstruction.Patch
{
    [HarmonyPatch]
    class OutsideConnectionAISimulationStepPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(OutsideConnectionAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType() }, null);
        }
        public static void Postfix(ushort buildingID, ref Building data)
        {
            int num27 = 0;
            int num28 = 0;
            int num29 = 0;
            int value = 0;
            TransferManager.TransferReason outgoingTransferReason = default(TransferManager.TransferReason);

            //constructionResource
            System.Random rand = new System.Random();
            outgoingTransferReason = (TransferManager.TransferReason)124;
            if (outgoingTransferReason != TransferManager.TransferReason.None)
            {
                CaculationVehicle.CustomCalculateOwnVehicles(buildingID, ref data, outgoingTransferReason, ref num27, ref num28, ref num29, ref value);
            }

            int num36 = 10;
            if (num36 < 100000)
            {
                TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
                offer2.Priority = 6;
                offer2.Building = buildingID;
                offer2.Position = data.m_position;
                offer2.Amount = 40000;
                offer2.Active = true;
                Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer2);
            }
        }
    }
}

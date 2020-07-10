using ColossalFramework;
using RealConstruction.NewAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace RealConstruction.Util
{
    public class MainDataStore
    {
        public static ulong  constructionResourcesTotal;
        public static ushort[] foodBuffer = new ushort[49152];
        public static ushort[] lumberBuffer = new ushort[49152];
        public static ushort[] coalBuffer = new ushort[49152];
        public static ushort[] petrolBuffer = new ushort[49152];
        private static ushort[] constructionResourceBuffer = new ushort[49152];
        private static byte[] constructionResourceBufferW = new byte[49152];
        public static ushort[] reservedResourceBuffer = new ushort[49152];

        public static ushort[] operationResourceBuffer = new ushort[49152];
        public static byte[] resourceCategory = new byte[49152];
        public static ushort lastBuildingID = 0;
        public static ushort[,] canNotConnectedBuildingID = new ushort[49152, 8];
        public static byte[] refreshCanNotConnectedBuildingIDCount = new byte[49152];
        public static byte[] canNotConnectedBuildingIDCount = new byte[49152];

        public static void CheckStart(ushort buildingID, ref Building buildingData)
        {
            if (!buildingData.m_flags.IsFlagSet(Building.Flags.Created))
                return;
            if (buildingData.m_flags.IsFlagSet(Building.Flags.Deleted))
                return;

            if (reservedResourceBuffer[buildingID] == 0)
            {
                buildingData.m_flags.ClearFlags(Building.Flags.Completed);
                reservedResourceBuffer[buildingID] = ConstructionAI.ConstructionResourcesNeed(ref buildingData);
            }
        }

        public static void CheckComplete(ushort buildingID, ref Building buildingData)
        {
            if (!buildingData.m_flags.IsFlagSet(Building.Flags.Created))
                return;
            if (buildingData.m_flags.IsFlagSet(Building.Flags.Deleted))
                return;

            if (Current(buildingID) >= 0)
            {
                buildingData.m_flags.SetFlags(Building.Flags.Completed);
            }
        }
        public static void Init(ushort buildingId)
        {
            constructionResourceBuffer[buildingId] = 0;
            constructionResourceBufferW[buildingId] = 0;
            reservedResourceBuffer[buildingId] = 0;
        }

        public static void Release(ushort buildingId)
        {
            constructionResourceBuffer[buildingId] = 0;
            constructionResourceBufferW[buildingId] = 0;
            reservedResourceBuffer[buildingId] = 0;
        }

        public static int Current(ushort buildingId, bool include_reserved = false)
        {
            if (include_reserved)
                return constructionResourceBufferW[buildingId] * 50000 + constructionResourceBuffer[buildingId] - reservedResourceBuffer[buildingId];
            else
                return constructionResourceBufferW[buildingId] * 50000 + constructionResourceBuffer[buildingId];
        }
        public static void Decrement(ushort buildingId, ref ushort dec)
        {
            if (dec < constructionResourceBuffer[buildingId])
            {
                constructionResourceBuffer[buildingId] -= dec;
                return;
            }

            if (constructionResourceBufferW[buildingId] == 0)
            {
                dec = constructionResourceBuffer[buildingId];
                constructionResourceBuffer[buildingId] = 0;
                return;
            }

            --constructionResourceBufferW[buildingId];
            constructionResourceBuffer[buildingId] += 50000;
            constructionResourceBuffer[buildingId] -= dec;
        }
        public static void Increment(ushort buildingId, ushort inc)
        {
            if (constructionResourceBuffer[buildingId] > 50000)
            {
                constructionResourceBuffer[buildingId] -= 50000;
                ++constructionResourceBufferW[buildingId];
            }
                
            constructionResourceBuffer[buildingId] += inc;
        }

        public static void DataInit()
        {
            constructionResourcesTotal = 0;
            for (int i = 0; i < MainDataStore.foodBuffer.Length; i++)
            {
                foodBuffer[i] = 0;
                lumberBuffer[i] = 0;
                coalBuffer[i] = 0;
                petrolBuffer[i] = 0;
                constructionResourceBuffer[i] = 0;
                constructionResourceBufferW[i] = 0;
                reservedResourceBuffer[i] = 0;
                operationResourceBuffer[i] = 0;
                resourceCategory[i] = 0;
            }
        }

        public static void Save(ref byte[] saveData)
        {
            //638976
            int i = 0;
            SaveAndRestore.SaveData(ref i, foodBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, lumberBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, coalBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, petrolBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, constructionResourceBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, constructionResourceBufferW, ref saveData);
            SaveAndRestore.SaveData(ref i, reservedResourceBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, operationResourceBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, resourceCategory, ref saveData);
        }

        public static void Load(byte[] saveData)
        {
            //638976
            int i = 0;
            SaveAndRestore.LoadData(ref i, saveData, ref foodBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref lumberBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref coalBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref petrolBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref constructionResourceBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref constructionResourceBufferW);
            SaveAndRestore.LoadData(ref i, saveData, ref reservedResourceBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref operationResourceBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref resourceCategory);
        }
    }
}

﻿using System;
using ICities;
using System.IO;

namespace RealConstruction
{
    public class SaveAndRestore : SerializableDataExtensionBase
    { 
        private static ISerializableData _serializableData;


        public static void save_ushorts(ref int idex, ushort[] item, ref byte[] container)
        {
            int i; int j;
            byte[] temp_data;
            for (j = 0; j < item.Length; j++)
            {
                temp_data = BitConverter.GetBytes(item[j]);
                for (i = 0; i < temp_data.Length; i++)
                {
                    container[idex + i] = temp_data[i];
                }
                idex = idex + temp_data.Length;
            }
        }

        public static ushort[] load_ushorts(ref int idex, byte[] container, int length)
        {
            ushort[] tmp = new ushort[length];
            int i;
            if (idex < container.Length)
            {
                for (i = 0; i < length; i++)
                {
                    tmp[i] = BitConverter.ToUInt16(container, idex);
                    idex = idex + 2;
                }
            }
            else
            {
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
                for (i = 0; i < length; i++)
                {
                    idex = idex + 2;
                }
            }
            return tmp;
        }

        public static void gather_saveData()
        {
            MainDataStore.save();
        }


        public override void OnCreated(ISerializableData serializableData)
        {
            SaveAndRestore._serializableData = serializableData;
        }

        public override void OnReleased()
        {
        }

        public override void OnSaveData()
        {
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                DebugLog.LogToFileOnly("startsave");
                MainDataStore.saveData = new byte[786432];
                gather_saveData();
                SaveAndRestore._serializableData.SaveData("RealConstruction MainDataStore", MainDataStore.saveData);
                RealConstruction.SaveSetting();
            }
        }

        public override void OnLoadData()
        {
            MainDataStore.DataInit();
            MainDataStore.saveData = new byte[786432];
            DebugLog.LogToFileOnly("OnLoadData");
            DebugLog.LogToFileOnly("startload");

            MainDataStore.saveData = SaveAndRestore._serializableData.LoadData("RealConstruction MainDataStore");
            if (MainDataStore.saveData == null)
            {
                DebugLog.LogToFileOnly("no RealConstruction MainDataStore save data, please check");
            }
            else
            {
                MainDataStore.load();
            }
            RealConstruction.LoadSetting();
        }
    }
}

using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using HarmonyLib;
using RealConstruction.NewAI;
using RealConstruction.Util;
using System;
using System.Reflection;
using UnityEngine;

namespace RealConstruction.CustomAI
{
    [HarmonyPatch]
    class BuildingAIRenderInstancePath
    {
        public static Material m_;
        public static MethodBase TargetMethod()
        {
            return typeof(BuildingAI).GetMethod("RenderInstance", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {
                typeof(RenderManager.CameraInfo),
                typeof(ushort), 
                typeof(Building).MakeByRefType(),
                typeof(int),
                typeof(RenderManager.Instance).MakeByRefType()
            }, null);
        }
                
        public static InstanceID GetPropRenderID(BuildingAI __instance, ushort buildingID, int propIndex)
        {
            InstanceID eid = new InstanceID();
            if (__instance.m_info.m_randomEffectTiming)
            {
                eid.SetBuildingProp(buildingID, propIndex);
            }
            else
            {
                eid.Building = buildingID;
            }
            return eid;
        }

        public static void Postfix(BuildingAI __instance, RenderManager.CameraInfo cameraInfo, ushort buildingID, ref Building data, int layerMask, ref RenderManager.Instance instance)
        {
            if ((cameraInfo.m_layerMask & (3 << 24)) == 0)
                return;

            if (MainDataStore.construction_site_info != null)
            {
                Locale.Get("sddssdsd", 12);
                Matrix4x4 m = new Matrix4x4();
                m.SetTRS(data.m_position, new Quaternion(), new Vector3(1.0f, 1.0f));
                if (m_ == null)
                {
                    m_ = new Material(Shader.Find("Custom/Props/Decal/Blend"))
                    {
                        name = "NodeMarkup",
                        color = new Color(1f, 1f, 1f, 1f),
                        doubleSidedGI = false,
                        enableInstancing = false,
                        globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack,
                    };
                    m_.EnableKeyword("MULTI_INSTANCE");
                }

                if (MainDataStore.construction_site_info.m_subMeshes.Length > 0)
                    Graphics.DrawMesh(MainDataStore.construction_site_info.m_subMeshes[0].m_subInfo.m_mesh, m, m_, 10);
                else
                    Locale.Get("xerty", 12);

                if (MainDataStore.construction_site_info.m_mesh != null)
                    Graphics.DrawMesh(MainDataStore.construction_site_info.m_mesh, m, m_, 10);

                if (MainDataStore.construction_site_info.m_generatedMesh != null)
                    Graphics.DrawMesh(MainDataStore.construction_site_info.m_generatedMesh, m, m_, 10);

                int length;
                Texture texture;
                Vector4 zero;
                Vector4 vector2;
                Matrix4x4 matrixx;
                bool flag;
                DistrictManager manager;
                byte district;
                byte park;
                
                if (MainDataStore.construction_site_info.m_props.Length > 0)
                {
                    for (int i = 0; i < MainDataStore.construction_site_info.m_props.Length; ++i)
                    {
                        BuildingInfo.Prop prop = MainDataStore.construction_site_info.m_props[i];
                        Randomizer r = new Randomizer((buildingID << 6) | prop.m_index);
                        if ((MainDataStore.construction_site_info.m_props == null) || (((layerMask & MainDataStore.construction_site_info.m_treeLayers) == 0) && !cameraInfo.CheckRenderDistance(instance.m_position, MainDataStore.construction_site_info.m_maxPropDistance + 72f)))
                        {
                            return;
                        }
                        else
                        {
                            length = data.Length;
                            texture = null;
                            zero = Vector4.zero;
                            vector2 = Vector4.zero;
                            matrixx = Matrix4x4.zero;
                            flag = false;
                            manager = Singleton<DistrictManager>.instance;
                            district = manager.GetDistrict(data.m_position);
                            Vector3 position = data.m_position;
                            ushort index = Building.FindParentBuilding(buildingID);
                            if (index != 0)
                            {
                                position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[index].m_position;
                            }
                            park = manager.GetPark(position);
                        }

                        if ((r.Int32(100) < prop.m_probability) && (length >= prop.m_requiredLength))
                        {
                            PropInfo finalProp = prop.m_finalProp;
                            TreeInfo finalTree = prop.m_finalTree;
                            if (finalProp == null)
                            {
                                if (finalTree != null)
                                {
                                    finalTree = finalTree.GetVariation(ref r);
                                    float scale = finalTree.m_minScale + ((r.Int32(0x2710) * (finalTree.m_maxScale - finalTree.m_minScale)) * 0.0001f);
                                    float brightness = finalTree.m_minBrightness + ((r.Int32(0x2710) * (finalTree.m_maxBrightness - finalTree.m_minBrightness)) * 0.0001f);
                                    if (((layerMask & (1 << (finalTree.m_prefabDataLayer & 0x1f))) != 0) && (!prop.m_fixedHeight ? true : true))
                                    {
                                        Vector3 position = instance.m_dataMatrix1.MultiplyPoint(prop.m_position);
                                        if (!prop.m_fixedHeight || MainDataStore.construction_site_info.m_requireHeightMap)
                                            position.y = instance.m_extraData.GetUShort(i) * 0.015625f;

                                        Vector4 objectIndex = instance.m_dataVector3;
                                        if (!__instance.m_info.m_colorizeEverything)
                                            objectIndex.z = 0f;
                                        TreeInstance.RenderInstance(cameraInfo, finalTree, position, scale, brightness, objectIndex);
                                    }
                                }
                            }
                            else
                            {
                                finalProp = finalProp.GetVariation(ref r, ref manager.m_districts.m_buffer[district], park);
                                float x = finalProp.m_minScale + ((r.Int32(0x2710) * (finalProp.m_maxScale - finalProp.m_minScale)) * 0.0001f);
                                Color color = finalProp.GetColor(ref r);
                                if (((layerMask & (1 << (finalProp.m_prefabDataLayer & 0x1f))) != 0) || finalProp.m_hasEffects)
                                {
                                    Vector3 vector4;
                                    Vector4 objectIndex = instance.m_dataVector3;
                                    if (!prop.m_fixedHeight)
                                    {
                                        vector4 = instance.m_dataMatrix1.MultiplyPoint(prop.m_position);
                                        if (!MainDataStore.construction_site_info.m_isFloating)
                                            vector4.y = instance.m_extraData.GetUShort(i) * 0.015625f;

                                        if (!__instance.m_info.m_colorizeEverything || finalProp.m_isDecal)
                                        {
                                            objectIndex.z = 0f;
                                        }
                                    }
                                    else
                                    {
                                        if (!__instance.m_info.m_isFloating)
                                        {
                                            vector4 = instance.m_dataMatrix1.MultiplyPoint(prop.m_position);
                                            if (__instance.m_info.m_requireHeightMap)
                                            {
                                                vector4.y = instance.m_extraData.GetUShort(i) * 0.015625f;
                                            }
                                        }
                                        else
                                        {
                                            if (!flag)
                                            {
                                                float num7;
                                                Vector3 vector6;
                                                Singleton<TerrainManager>.instance.HeightMap_sampleWaterHeightAndNormal(instance.m_position, 0.15f, out num7, out vector6);
                                                Vector3 position = instance.m_position;
                                                position.y = num7;
                                                matrixx = Matrix4x4.TRS(position, Quaternion.FromToRotation(Vector3.up, vector6) * instance.m_rotation, Vector3.one);
                                                flag = true;
                                            }
                                            Matrix4x4 matrix = new Matrix4x4();
                                            matrix.SetTRS(prop.m_position, Quaternion.AngleAxis(prop.m_radAngle * 57.29578f, Vector3.down), new Vector3(x, x, x));
                                            matrix = matrixx * matrix;
                                            vector4 = matrix.MultiplyPoint(Vector3.zero);
                                            if (cameraInfo.CheckRenderDistance(vector4, finalProp.m_maxRenderDistance))
                                            {
                                                PropInstance.RenderInstance(cameraInfo, finalProp,GetPropRenderID(__instance, buildingID, i), matrix, vector4, x, data.m_angle + prop.m_radAngle, color, objectIndex, true);
                                                break;
                                            }
                                        }
                                    }
                                    if (cameraInfo.CheckRenderDistance(vector4, finalProp.m_maxRenderDistance))
                                    {
                                        InstanceID id = GetPropRenderID(__instance, buildingID, i);
                                        if (finalProp.m_requireWaterMap)
                                        {
                                            if (texture == null)
                                            {
                                                Singleton<TerrainManager>.instance.GetWaterMapping(data.m_position, out texture, out zero, out vector2);
                                            }
                                            PropInstance.RenderInstance(cameraInfo, finalProp, id, vector4, x, data.m_angle + prop.m_radAngle, color, objectIndex, true, instance.m_dataTexture0, instance.m_dataVector1, instance.m_dataVector2, texture, zero, vector2);
                                        }
                                        else if (finalProp.m_requireHeightMap)
                                        {
                                            PropInstance.RenderInstance(cameraInfo, finalProp, id, vector4, x, data.m_angle + prop.m_radAngle, color, objectIndex, true, instance.m_dataTexture0, instance.m_dataVector1, instance.m_dataVector2);
                                        }
                                        else
                                        {
                                            PropInstance.RenderInstance(cameraInfo, finalProp, id, vector4, x, data.m_angle + prop.m_radAngle, color, objectIndex, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public class CustomPlayerBuildingAI
    {
        public static bool CanOperation(ushort buildingID, ref Building buildingData, bool userReject = true)
        {
            if ((MainDataStore.resourceCategory[buildingID] == 4) && userReject)
            {
                return false;
            }

            if (ResourceBuildingAI.IsSpecialBuilding(buildingID))
            {
                return false;
            }

            if (buildingData.Info.m_buildingAI is CampusBuildingAI)
            {
                return false;
            }

            if (buildingData.Info.m_class.m_service == ItemClass.Service.Beautification)
            {
                return false;
            }

            PlayerBuildingAI AI = buildingData.Info.m_buildingAI as PlayerBuildingAI;
            return AI.RequireRoadAccess();
        }

        public static bool CanConstruction(ushort buildingID, ref Building buildingData, bool userReject = true)
        {
            if ((MainDataStore.resourceCategory[buildingID] == 4) && userReject)
            {
                return false;
            }

            PlayerBuildingAI AI = buildingData.Info.m_buildingAI as PlayerBuildingAI;
            return AI.RequireRoadAccess();
        }

        public static bool CanRemoveNoResource(ushort buildingID, ref Building buildingData)
        {
            if (buildingData.Info.m_buildingAI is ProcessingFacilityAI || buildingData.Info.m_buildingAI is UniqueFactoryAI)
            {
                return false;
            }
            return true;
        }
    }
}

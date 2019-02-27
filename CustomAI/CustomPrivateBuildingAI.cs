﻿using ColossalFramework;
using ColossalFramework.Math;
using RealConstruction.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealConstruction.CustomAI
{
    public class CustomPrivateBuildingAI: CommonBuildingAI
    {
        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            // NON-STOCK CODE START
            // Update problems
            if (!buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                if (MainDataStore.constructionResourceBuffer[buildingID] >= 8000)
                {
                    Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoResources);
                    buildingData.m_problems = problem;
                }
                else
                {
                    if (buildingData.m_problems == Notification.Problem.None)
                    {
                        Notification.Problem problem = Notification.AddProblems(buildingData.m_problems, Notification.Problem.NoResources);
                        buildingData.m_problems = problem;
                    }
                }
            }
            /// NON-STOCK CODE END
            if ((buildingData.m_levelUpProgress == 255 || (buildingData.m_flags & Building.Flags.Collapsed) == Building.Flags.None) && buildingData.m_fireIntensity == 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(10u) == 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                ushort style = instance.m_districts.m_buffer[(int)district].m_Style;
                if (style > 0 && (int)(style - 1) < instance.m_Styles.Length)
                {
                    DistrictStyle districtStyle = instance.m_Styles[(int)(style - 1)];
                    if (districtStyle != null && this.m_info.m_class != null && districtStyle.AffectsService(this.m_info.GetService(), this.m_info.GetSubService(), this.m_info.m_class.m_level) && !districtStyle.Contains(this.m_info) && Singleton<ZoneManager>.instance.m_lastBuildIndex == Singleton<SimulationManager>.instance.m_currentBuildIndex)
                    {
                        buildingData.m_flags |= Building.Flags.Demolishing;
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
                    }
                }
            }
            if ((buildingData.m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None && (buildingData.m_levelUpProgress == 255 || (buildingData.m_flags & Building.Flags.Collapsed) == Building.Flags.None))
            {
                SimulationManager instance2 = Singleton<SimulationManager>.instance;
                if (buildingData.m_fireIntensity == 0 && instance2.m_randomizer.Int32(10u) == 0 && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance2.m_currentBuildIndex)
                {
                    buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
                    if (!buildingData.CheckZoning(this.m_info.m_class.GetZone(), this.m_info.m_class.GetSecondaryZone(), true))
                    {
                        buildingData.m_flags |= Building.Flags.Demolishing;
                        CheckNearbyBuildingZones(buildingData.m_position);
                        instance2.m_currentBuildIndex += 1u;
                        return;
                    }
                }
            }
            if ((buildingData.m_flags & (Building.Flags.Abandoned | Building.Flags.Downgrading | Building.Flags.Collapsed)) != Building.Flags.None)
            {
                if (buildingData.m_fireIntensity != 0)
                {
                    DistrictManager instance3 = Singleton<DistrictManager>.instance;
                    byte district2 = instance3.GetDistrict(buildingData.m_position);
                    DistrictPolicies.Services servicePolicies = instance3.m_districts.m_buffer[(int)district2].m_servicePolicies;
                    base.HandleFire(buildingID, ref buildingData, ref frameData, servicePolicies);
                }
                else if ((buildingData.m_majorProblemTimer == 255 || (buildingData.m_flags & Building.Flags.Abandoned) == Building.Flags.None) && (buildingData.m_levelUpProgress == 255 || (buildingData.m_flags & Building.Flags.Collapsed) == Building.Flags.None) && (buildingData.m_flags & Building.Flags.Historical) == Building.Flags.None)
                {
                    SimulationManager instance4 = Singleton<SimulationManager>.instance;
                    ZoneManager instance5 = Singleton<ZoneManager>.instance;
                    int num;
                    switch (this.m_info.m_class.m_service)
                    {
                        case ItemClass.Service.Residential:
                            num = instance5.m_actualResidentialDemand;
                            goto IL_347;
                        case ItemClass.Service.Commercial:
                            num = instance5.m_actualCommercialDemand;
                            goto IL_347;
                        case ItemClass.Service.Industrial:
                            num = instance5.m_actualWorkplaceDemand;
                            goto IL_347;
                        case ItemClass.Service.Office:
                            num = instance5.m_actualWorkplaceDemand;
                            goto IL_347;
                    }
                    num = 0;
                    IL_347:
                    if ((buildingData.m_flags & Building.Flags.Collapsed) != Building.Flags.None)
                    {
                        DistrictManager instance6 = Singleton<DistrictManager>.instance;
                        byte district3 = instance6.GetDistrict(buildingData.m_position);
                        DistrictPolicies.CityPlanning cityPlanningPolicies = instance6.m_districts.m_buffer[(int)district3].m_cityPlanningPolicies;
                        if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoRebuild) != DistrictPolicies.CityPlanning.None)
                        {
                            District[] expr_3A8_cp_0 = instance6.m_districts.m_buffer;
                            byte expr_3A8_cp_1 = district3;
                            expr_3A8_cp_0[(int)expr_3A8_cp_1].m_cityPlanningPoliciesEffect = (expr_3A8_cp_0[(int)expr_3A8_cp_1].m_cityPlanningPoliciesEffect | DistrictPolicies.CityPlanning.NoRebuild);
                            num = 0;
                        }
                    }
                    if (instance4.m_randomizer.Int32(100u) < num && instance5.m_lastBuildIndex == instance4.m_currentBuildIndex)
                    {
                        float num2 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(buildingData.m_position));
                        if (num2 <= buildingData.m_position.y && buildingData.CheckZoning(this.m_info.m_class.GetZone(), this.m_info.m_class.GetSecondaryZone(), false))
                        {
                            ItemClass.SubService subService = this.m_info.m_class.m_subService;
                            ItemClass.Level level = ItemClass.Level.Level1;
                            int width = buildingData.Width;
                            int num3 = buildingData.Length;
                            if (this.m_info.m_class.m_service == ItemClass.Service.Industrial)
                            {
                                ZoneBlock.GetIndustryType(buildingData.m_position, out subService, out level);
                            }
                            else if (this.m_info.m_class.m_service == ItemClass.Service.Commercial)
                            {
                                ZoneBlock.GetCommercialType(buildingData.m_position, this.m_info.m_class.GetZone(), width, num3, out subService, out level);
                            }
                            else if (this.m_info.m_class.m_service == ItemClass.Service.Residential)
                            {
                                ZoneBlock.GetResidentialType(buildingData.m_position, this.m_info.m_class.GetZone(), width, num3, out subService, out level);
                            }
                            else if (this.m_info.m_class.m_service == ItemClass.Service.Office)
                            {
                                ZoneBlock.GetOfficeType(buildingData.m_position, this.m_info.m_class.GetZone(), width, num3, out subService, out level);
                            }
                            DistrictManager instance7 = Singleton<DistrictManager>.instance;
                            byte district4 = instance7.GetDistrict(buildingData.m_position);
                            ushort style2 = instance7.m_districts.m_buffer[(int)district4].m_Style;
                            BuildingInfo randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, this.m_info.m_class.m_service, subService, level, width, num3, this.m_info.m_zoningMode, (int)style2);
                            if (randomBuildingInfo != null)
                            {
                                buildingData.m_flags |= Building.Flags.Demolishing;
                                float num4 = buildingData.m_angle + 1.57079637f;
                                if (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
                                {
                                    num4 -= 1.57079637f;
                                    num3 = width;
                                }
                                else if (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
                                {
                                    num4 += 1.57079637f;
                                    num3 = width;
                                }
                                ushort num5;
                                if (Singleton<BuildingManager>.instance.CreateBuilding(out num5, ref Singleton<SimulationManager>.instance.m_randomizer, randomBuildingInfo, buildingData.m_position, buildingData.m_angle, num3, Singleton<SimulationManager>.instance.m_currentBuildIndex))
                                {
                                    Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
                                    switch (this.m_info.m_class.m_service)
                                    {
                                        case ItemClass.Service.Residential:
                                            instance5.m_actualResidentialDemand = Mathf.Max(0, instance5.m_actualResidentialDemand - 5);
                                            break;
                                        case ItemClass.Service.Commercial:
                                            instance5.m_actualCommercialDemand = Mathf.Max(0, instance5.m_actualCommercialDemand - 5);
                                            break;
                                        case ItemClass.Service.Industrial:
                                            instance5.m_actualWorkplaceDemand = Mathf.Max(0, instance5.m_actualWorkplaceDemand - 5);
                                            break;
                                        case ItemClass.Service.Office:
                                            instance5.m_actualWorkplaceDemand = Mathf.Max(0, instance5.m_actualWorkplaceDemand - 5);
                                            break;
                                    }
                                }
                                instance4.m_currentBuildIndex += 1u;
                            }
                        }
                    }
                }
            }
        }

        // PrivateBuildingAI
        private static void CheckNearbyBuildingZones(Vector3 position)
        {
            int num = Mathf.Max((int)((position.x - 35f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((position.z - 35f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((position.x + 35f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((position.z + 35f) / 64f + 135f), 269);
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = buildingGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        ushort nextGridBuilding = buildings.m_buffer[(int)num5].m_nextGridBuilding;
                        Building.Flags flags = buildings.m_buffer[(int)num5].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Demolishing | Building.Flags.ZonesUpdated | Building.Flags.Collapsed)) == (Building.Flags.Created | Building.Flags.ZonesUpdated))
                        {
                            BuildingInfo info = buildings.m_buffer[(int)num5].Info;
                            if (info != null && info.m_placementStyle == ItemClass.Placement.Automatic)
                            {
                                ItemClass.Zone zone = info.m_class.GetZone();
                                ItemClass.Zone secondaryZone = info.m_class.GetSecondaryZone();
                                if (zone != ItemClass.Zone.None && VectorUtils.LengthSqrXZ(buildings.m_buffer[(int)num5].m_position - position) <= 1225f)
                                {
                                    Building[] expr_18C_cp_0 = buildings.m_buffer;
                                    ushort expr_18C_cp_1 = num5;
                                    expr_18C_cp_0[(int)expr_18C_cp_1].m_flags = (expr_18C_cp_0[(int)expr_18C_cp_1].m_flags & ~Building.Flags.ZonesUpdated);
                                    if (!buildings.m_buffer[(int)num5].CheckZoning(zone, secondaryZone, true))
                                    {
                                        Singleton<BuildingManager>.instance.ReleaseBuilding(num5);
                                    }
                                }
                            }
                        }
                        num5 = nextGridBuilding;
                        if (++num6 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }


    }
}

/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Element ToRevitElement(this BuildingElement buildingElement, Document document, PushSettings pushSettings = null)
        {
            throw new System.NotImplementedException("The method to convert a BHoM Building Element into a Revit Element has not been fixed yet. Check Issue #247 for more info");
            /*if (buildingElement == null || buildingElement.BuildingElementProperties == null || document == null)
                return null;

            Element aElement = pushSettings.FindRefObject<Element>(document, buildingElement.BHoM_Guid);
            if (aElement != null)
                return aElement;

            pushSettings.DefaultIfNull();

            ElementType aElementType = null;

            if (buildingElement.BuildingElementProperties != null)
                aElementType = buildingElement.BuildingElementProperties.ToRevitElementType(document, pushSettings);

            if (aElementType == null)
            {
                string aFamilyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(buildingElement);
                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList().FindAll(x => x.Name == aFamilyTypeName && x.Category != null);
                    if (aElementTypeList != null || aElementTypeList.Count() != 0)
                        aElementType = aElementTypeList.First();
                }
            }

            if (aElementType == null)
            {
                string aFamilyTypeName = buildingElement.Name;

                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList().FindAll(x => x.Name == aFamilyTypeName && x.Category != null);
                    if (aElementTypeList != null || aElementTypeList.Count() != 0)
                        aElementType = aElementTypeList.First();
                }
            }

            if (aElementType == null)
                return null;

            BuildingElementType? aBuildingElementType = null;
            if (buildingElement.BuildingElementProperties != null)
                aBuildingElementType = buildingElement.BuildingElementProperties.BuildingElementType;

            if (aBuildingElementType == null || !aBuildingElementType.HasValue || aBuildingElementType == BuildingElementType.Undefined)
                aBuildingElementType = Query.BuildingElementType(aElementType.Category);

            if (aBuildingElementType == null || !aBuildingElementType.HasValue)
                return null;

            Level aLevel = null;
            double aElevation_BuildingElement = 0;

            BuiltInParameter[] aBuiltInParameters = null;
            switch (aBuildingElementType.Value)
            {
                case BuildingElementType.Ceiling:
                    //TODO: Create Ceiling from BuildingElement
                    break;
                case BuildingElementType.Floor:
                    aElevation_BuildingElement = buildingElement.MinimumLevel();

                    aLevel = document.HighLevel(aElevation_BuildingElement, true);

                    double aElevation = aLevel.Elevation;
                    if (pushSettings.ConvertUnits)
                        aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

                    oM.Geometry.Plane aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aElevation_BuildingElement), BH.Engine.Geometry.Create.Vector(0, 0, 1));
                    ICurve aCurve = BH.Engine.Geometry.Modify.Project(buildingElement.PanelCurve as dynamic, aPlane) as ICurve;

                    CurveArray aCurveArray = null;
                    if (aCurve is PolyCurve)
                        aCurveArray = ((PolyCurve)aCurve).ToRevitCurveArray(pushSettings);
                    else if (aCurve is Polyline)
                        aCurveArray = ((Polyline)aCurve).ToRevitCurveArray(pushSettings);

                    if (aCurveArray == null)
                        break;

                    aElement = document.Create.NewFloor(aCurveArray, aElementType as FloorType, aLevel, false);

                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM };
                    break;
                case BuildingElementType.Roof:
                    ModelCurveArray aModelCurveArray = new ModelCurveArray();
                    aElevation_BuildingElement = buildingElement.MinimumLevel();

                    aLevel = document.HighLevel(aElevation_BuildingElement, true);
                    if (aLevel == null)
                        break;

                    //double 
                    aElevation = aLevel.Elevation;
                    if (pushSettings.ConvertUnits)
                        aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

                    //oM.Geometry.Plane
                    aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aElevation_BuildingElement), BH.Engine.Geometry.Create.Vector(0, 0, 1));
                    
                    //ICurve 
                    aCurve = BH.Engine.Geometry.Modify.Project(buildingElement.PanelCurve as dynamic, aPlane) as ICurve;
                    
                    //CurveArray 
                    aCurveArray = null;
                    if (aCurve is PolyCurve)
                        aCurveArray = ((PolyCurve)aCurve).ToRevitCurveArray(pushSettings);
                    else if(aCurve is Polyline)
                        aCurveArray = ((Polyline)aCurve).ToRevitCurveArray(pushSettings);

                    if (aCurveArray == null)
                        break;

                    FootPrintRoof aFootPrintRoof = document.Create.NewFootPrintRoof(aCurveArray, aLevel, aElementType as RoofType, out aModelCurveArray);
                    if (aFootPrintRoof != null)
                    {
                        List<ICurve> aCurveList = null;

                        if (buildingElement.PanelCurve is PolyCurve)
                            aCurveList = ((PolyCurve)buildingElement.PanelCurve).Curves;
                        else if (buildingElement.PanelCurve is Polyline)
                            aCurveList = Query.Curves((Polyline)buildingElement.PanelCurve);

                        if (aCurveList != null && aCurveList.Count > 2)
                        {
                            SlabShapeEditor aSlabShapeEditor = aFootPrintRoof.SlabShapeEditor;
                            aSlabShapeEditor.ResetSlabShape();

                            foreach (ICurve aCurve_Temp in aCurveList)
                            {
                                oM.Geometry.Point aPoint = BH.Engine.Geometry.Query.IStartPoint(aCurve_Temp);
                                aSlabShapeEditor.DrawPoint(aPoint.ToRevit(pushSettings));
                            }
                        }

                        aElement = aFootPrintRoof;
                    }
                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM };
                    break;
                case BuildingElementType.Wall:

                    double aElevation_Minimum = buildingElement.MinimumLevel();

                    aLevel = document.LowLevel(aElevation_Minimum, true);
                    aElement = Wall.Create(document, Convert.ToRevitCurveList(buildingElement.Curve(), pushSettings), aElementType.Id, aLevel.Id, false);

                    Parameter aParameter = null;

                    aParameter = aElement.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                    if(aParameter != null)
                        aParameter.Set(ElementId.InvalidElementId);
                    aParameter = aElement.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                    if (aParameter != null)
                    {
                        double aHeight = UnitUtils.ConvertToInternalUnits(buildingElement.MaximumLevel() - buildingElement.MinimumLevel(), DisplayUnitType.DUT_METERS);
                        aParameter.Set(aHeight);
                    }

                    double aElevation_Level = aLevel.Elevation;
                    if (pushSettings.ConvertUnits)
                        aElevation_Level = UnitUtils.ConvertFromInternalUnits(aElevation_Level, DisplayUnitType.DUT_METERS);

                    if(System.Math.Abs(aElevation_Minimum - aElevation_Level) > Tolerance.MacroDistance)
                    {
                        aParameter = aElement.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                        if (aParameter != null)
                        {
                            double aOffset = aElevation_Minimum - aElevation_Level;
                            if(pushSettings.ConvertUnits)
                                aOffset = UnitUtils.ConvertToInternalUnits(aOffset, DisplayUnitType.DUT_METERS);

                            aParameter.Set(aOffset);
                        }
                    }

                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.WALL_BASE_OFFSET };
                    break;
                case BuildingElementType.Door:
                    //TODO: Create Door from BuildingElement
                    break;
                case BuildingElementType.Window:
                    //TODO: Create Window from BuildingElement
                    break;
            }

            aElement.CheckIfNullPush(buildingElement);
            if (aElement == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, buildingElement, aBuiltInParameters, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(buildingElement, aElement);

            return aElement;*/
        }

        /***************************************************/
    }
}
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

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static BuildingElement AddSpaceId(this BuildingElement buildingElement, EnergyAnalysisSurface energyAnalysisSurface)
        {
            if (buildingElement == null)
                return null;

            BuildingElement aBuildingElement = buildingElement.GetShallowClone() as BuildingElement;
            aBuildingElement.CustomData.Add(BH.Engine.Adapters.Revit.Convert.SpaceId, -1);

            if (energyAnalysisSurface == null)
                return aBuildingElement;

            EnergyAnalysisSpace aEnergyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
            if (aEnergyAnalysisSpace == null)
                return aBuildingElement;

            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (aSpatialElement == null)
                return aBuildingElement;

            aBuildingElement.CustomData[BH.Engine.Adapters.Revit.Convert.SpaceId] = aSpatialElement.Id.IntegerValue;

            return aBuildingElement;
        }

        /***************************************************/
    }
}
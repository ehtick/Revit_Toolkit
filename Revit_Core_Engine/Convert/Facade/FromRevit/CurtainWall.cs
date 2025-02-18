/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.Engine.Facade;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Dimensional;
using BH.oM.Physical.Constructions;
using BH.oM.Facade.Elements;
using BH.oM.Facade.SectionProperties;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Facade.Elements.CurtainWall.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("curtainWall", "BH.oM.Facade.Elements.CurtainWall resulting from converting the input Revit Wall.")]
        public static CurtainWall CurtainWallFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (wall == null)
                return null;

            settings = settings.DefaultIfNull();

            CurtainWall bHoMCurtainWall = refObjects.GetValue<CurtainWall>(wall.Id);
            if (bHoMCurtainWall != null)
                return bHoMCurtainWall;

            if (wall.StackedWallOwnerId != null && wall.StackedWallOwnerId != ElementId.InvalidElementId)
                return null;

            IEnumerable<oM.Facade.Elements.Opening> curtainPanels = wall.CurtainGrid.FacadeCurtainPanels(wall.Document, settings, refObjects);

            if (curtainPanels == null || !curtainPanels.Any())
                BH.Engine.Base.Compute.RecordError(String.Format("Processing of panels of Revit curtain wall failed. BHoM curtain wall without location has been returned. Revit ElementId: {0}", wall.Id.IntegerValue));


            // Get external edges of whole curtain wall
            List<FrameEdge> extEdges = new List<FrameEdge>();
            List<IElement1D> cwEdgeCrvs = curtainPanels.ExternalEdges();
            foreach (ICurve crv in cwEdgeCrvs)
            {
                FrameEdge frameEdge = new FrameEdge { Curve = crv };
                extEdges.Add(frameEdge);
            }

            bHoMCurtainWall = new CurtainWall { ExternalEdges = extEdges, Openings = curtainPanels.ToList(), Name = wall.WallType.Name };

            bHoMCurtainWall.Name = wall.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMCurtainWall.SetIdentifiers(wall);
            bHoMCurtainWall.CopyParameters(wall, settings.MappingSettings);
            bHoMCurtainWall.SetProperties(wall, settings.MappingSettings);

            refObjects.AddOrReplace(wall.Id, bHoMCurtainWall);

            return bHoMCurtainWall;
        }

        /***************************************************/
    }
}



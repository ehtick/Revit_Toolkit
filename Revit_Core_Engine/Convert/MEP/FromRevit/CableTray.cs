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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.MEP.System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit Cable Tray to BHoM cable trays.")]
        [Input("revitCableTray", "Revit Cable Tray to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("cableTrays", "BHoM cable tray objects converted from a Revit cable tray elements.")]
        public static List<BH.oM.MEP.System.CableTray> CableTrayFromRevit(this Autodesk.Revit.DB.Electrical.CableTray revitCableTray, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM cable tray from refObjects if it has been converted before
            List<BH.oM.MEP.System.CableTray> bhomCableTrays = refObjects.GetValues<BH.oM.MEP.System.CableTray>(revitCableTray.Id);
            if (bhomCableTrays != null)
            {
                return bhomCableTrays;
            }
            else
            {
                bhomCableTrays = new List<BH.oM.MEP.System.CableTray>();
            }

            // Section properties
            BH.oM.MEP.System.SectionProperties.CableTraySectionProperty sectionProperty = BH.Revit.Engine.Core.Query.CableTraySectionProperty(revitCableTray, settings);

            // Orientation angle
            double orientationAngle = revitCableTray.OrientationAngle(settings);

            // Revit element type proxy
            RevitTypeFragment typeFragment = null;
            ElementType type = revitCableTray.Document.GetElement(revitCableTray.GetTypeId()) as ElementType;
            if (type != null)
                typeFragment = type.TypeFragmentFromRevit(settings, refObjects);

            List<BH.oM.Geometry.Line> queried = Query.LocationCurveMEP(revitCableTray, settings);

            for (int i = 0; i < queried.Count; i++)
            {
                BH.oM.Geometry.Line segment = queried[i];
                BH.oM.MEP.System.CableTray thisSegment = new CableTray
                {
                    StartPoint = segment.StartPoint(),
                    EndPoint = segment.EndPoint(),
                    SectionProperty = sectionProperty,
                    OrientationAngle = orientationAngle
                };

                // Set the type fragment
                if (typeFragment != null)
                    thisSegment.Fragments.Add(typeFragment);

                //Set identifiers, parameters & custom data
                thisSegment.SetIdentifiers(revitCableTray);
                thisSegment.CopyParameters(revitCableTray, settings.MappingSettings);
                thisSegment.SetProperties(revitCableTray, settings.MappingSettings);
                bhomCableTrays.Add(thisSegment);
            }

            refObjects.AddOrReplace(revitCableTray.Id, bhomCableTrays);
            return bhomCableTrays;
        }
        
        /***************************************************/
    }
}



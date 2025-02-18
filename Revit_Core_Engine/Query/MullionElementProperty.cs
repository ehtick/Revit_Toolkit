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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Facade.SectionProperties;
using BH.oM.Physical.FramingProperties;
using BH.oM.Base.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the mullion element property from a Revit FamilyInstance.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("property", "BHoM mullion element property extracted from the input Revit FamilyInstance.")]
        public static FrameEdgeProperty MullionElementProperty(this FamilyInstance familyInstance, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null || familyInstance.Symbol == null)
                return null;
            
            FrameEdgeProperty frameEdgeProperty = refObjects.GetValue<FrameEdgeProperty>(familyInstance.Id);
            if (frameEdgeProperty != null)
                return frameEdgeProperty;

            // Profile and material extraction not yet implemented
            IProfile profile = null;
            BH.oM.Physical.Materials.Material material = null;

            List<ConstantFramingProperty> sectionProperties = new List<ConstantFramingProperty> { BH.Engine.Physical.Create.ConstantFramingProperty(profile, material, 0, familyInstance.Symbol.Name) };
            frameEdgeProperty = new FrameEdgeProperty { Name = familyInstance.Symbol.Name, SectionProperties = sectionProperties };

            //Set identifiers, parameters & custom data
            frameEdgeProperty.SetIdentifiers(familyInstance.Symbol);
            frameEdgeProperty.CopyParameters(familyInstance.Symbol, settings.MappingSettings);
            frameEdgeProperty.SetProperties(familyInstance.Symbol, settings.MappingSettings);

            refObjects.AddOrReplace(familyInstance.Id, frameEdgeProperty);
            return frameEdgeProperty;
        }

        /***************************************************/
    }
}



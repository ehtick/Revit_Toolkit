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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FilledRegion to BH.oM.Adapters.Revit.Elements.DraftingInstance.")]
        [Input("filledRegion", "Revit FilledRegion to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "BH.oM.Adapters.Revit.Elements.DraftingInstance resulting from converting the input Revit FilledRegion.")]
        public static DraftingInstance DraftingInstanceFromRevit(this FilledRegion filledRegion, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            DraftingInstance draftingInstance = refObjects.GetValue<DraftingInstance>(filledRegion.Id);
            if (draftingInstance != null)
                return draftingInstance;

            View view = filledRegion.Document.GetElement(filledRegion.OwnerViewId) as View;
            if (view == null)
                return null;

            InstanceProperties instanceProperties = (filledRegion.Document.GetElement(filledRegion.GetTypeId()) as ElementType).InstancePropertiesFromRevit(settings, refObjects) as InstanceProperties;

            List<ICurve> curves = new List<oM.Geometry.ICurve>();
            foreach (CurveLoop loop in filledRegion.GetBoundaries())
            {
                curves.Add(loop.FromRevit());
            }

            List<PlanarSurface> surfaces = BH.Engine.Geometry.Create.PlanarSurface(curves, settings.DistanceTolerance);
            if (surfaces.Count == 0)
            {
                BH.Engine.Base.Compute.RecordWarning("No location was returned for filled region with Element ID = " + filledRegion.Id.ToString() + ". Location has been set to null.");
                draftingInstance = new DraftingInstance { Properties = instanceProperties, ViewName = view.Name, Location = null };
            }
            else if (surfaces.Count == 1)
                draftingInstance = new DraftingInstance { Properties = instanceProperties, ViewName = view.Name, Location = surfaces[0] };
            else
                draftingInstance = new DraftingInstance { Properties = instanceProperties, ViewName = view.Name, Location = new PolySurface { Surfaces = surfaces.Cast<ISurface>().ToList() } };

            draftingInstance.Name = filledRegion.Name;

            //Set identifiers, parameters & custom data
            draftingInstance.SetIdentifiers(filledRegion);
            draftingInstance.CopyParameters(filledRegion, settings.MappingSettings);
            draftingInstance.SetProperties(filledRegion, settings.MappingSettings);

            refObjects.AddOrReplace(filledRegion.Id, draftingInstance);
            return draftingInstance;
        }

        /***************************************************/
    }
}




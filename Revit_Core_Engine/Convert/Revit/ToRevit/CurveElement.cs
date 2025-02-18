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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System;
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

        [Description("Converts BH.oM.Adapters.Revit.Elements.ModelInstance to a Revit CurveElement.")]
        [Input("modelInstance", "BH.oM.Adapters.Revit.Elements.ModelInstance to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Revit CurveElement resulting from converting the input BH.oM.Adapters.Revit.Elements.ModelInstance.")]
        public static CurveElement ToCurveElement(this ModelInstance modelInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            CurveElement curveElement = refObjects.GetValue<CurveElement>(document, modelInstance.BHoM_Guid);
            if (curveElement != null)
                return curveElement;

            settings = settings.DefaultIfNull();

            ICurve curve = modelInstance.Location as ICurve;
            if (curve == null)
                return null;

            if (!curve.IIsPlanar(settings.DistanceTolerance))
            {
                modelInstance.NonPlanarCurveError();
                return null;
            }

            Curve revitCurve = curve.IToRevit();
            if (revitCurve == null)
                return null;

            if ((revitCurve is NurbSpline || revitCurve is HermiteSpline) && curve.IIsClosed(settings.DistanceTolerance))
            {
                modelInstance.ClosedNurbsCurveError();
                return null;
            }

            Autodesk.Revit.DB.Plane revitPlane;
            BH.oM.Geometry.Plane plane = curve.IFitPlane();
            if (plane == null)
                revitPlane = revitCurve.ArbitraryPlane();
            else
                revitPlane = plane.ToRevit();

            SketchPlane sketchPlane = SketchPlane.Create(document, revitPlane);
            curveElement = document.Create.NewModelCurve(revitCurve, sketchPlane);

            if (modelInstance.Properties != null)
            {
                string name = modelInstance.Properties.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    Element element = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToList().Find(x => x.Name == name);
                    if (element != null)
                        curveElement.LineStyle = element;
                }
            }

            refObjects.AddOrReplace(modelInstance, curveElement);
            return curveElement;
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.DraftingInstance to a Revit CurveElement.")]
        [Input("draftingInstance", "BH.oM.Adapters.Revit.Elements.DraftingInstance to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Revit CurveElement resulting from converting the input BH.oM.Adapters.Revit.Elements.DraftingInstance.")]
        public static CurveElement ToCurveElement(this DraftingInstance draftingInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            CurveElement curveElement = refObjects.GetValue<CurveElement>(document, draftingInstance.BHoM_Guid);
            if (curveElement != null)
                return curveElement;

            settings = settings.DefaultIfNull();

            if (!(draftingInstance.Location is ICurve))
                return null;

            ICurve curve = (ICurve)draftingInstance.Location;

            Curve revitCurve = curve.IToRevit();
            if (revitCurve == null)
                return null;

            if (!BH.Engine.Geometry.Query.IsPlanar(curve as dynamic))
                return null;

            View view = Query.View(draftingInstance, document);
            if (view == null)
                return null;

            curveElement = document.Create.NewDetailCurve(view, revitCurve);
            if (curveElement == null)
                return null;

            if (draftingInstance.Properties != null)
            {
                string name = draftingInstance.Properties.Name;
                if(!string.IsNullOrEmpty(name))
                {
                    Element element = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToList().Find(x => x.Name == name);
                    if(element != null)
                        curveElement.LineStyle = element;
                } 
            }

            refObjects.AddOrReplace(draftingInstance, curveElement);
            return curveElement;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Autodesk.Revit.DB.Plane ArbitraryPlane(this Curve curve)
        {
            XYZ origin = curve.GetEndPoint(0);
            XYZ x = (curve.GetEndPoint(1) - origin).Normalize();
            XYZ helper = 1 - Math.Abs(x.DotProduct(XYZ.BasisZ)) > BH.oM.Geometry.Tolerance.Angle ? XYZ.BasisZ : XYZ.BasisX;
            XYZ y = x.CrossProduct(helper).Normalize();
            return Autodesk.Revit.DB.Plane.CreateByOriginAndBasis(origin, x, y);
        }

        /***************************************************/
    }
}



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
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Facade.Elements;
using BH.Engine.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the panels from a Revit curtain grid and returns them in a form of BHoM facade Openings.")]
        [Input("curtainGrid", "Revit curtain grid to extract the panels from.")]
        [Input("document", "Revit document, to which the curtain grid belongs.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panels", "Panels extracted from the input Revit curtain grid and converted to BHoM facade Openings.")]
        public static List<oM.Facade.Elements.Opening> FacadeCurtainPanels(this CurtainGrid curtainGrid, Document document, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (curtainGrid == null)
                return null;

            List<Element> panels = curtainGrid.GetPanelIds().Select(x => document.GetElement(x)).ToList();
            List<CurtainCell> cells = curtainGrid.GetCurtainCells().ToList();

            if (panels.Count != cells.Count)
                return null;

            List<IElement1D> cwMullions = curtainGrid.CurtainWallMullions(document, settings, refObjects).Select(x => x as IElement1D).ToList();

            List<oM.Facade.Elements.Opening> result = new List<oM.Facade.Elements.Opening>();

            for (int i = 0; i < panels.Count; i++)
            {
                FamilyInstance panel = panels[i] as FamilyInstance;
                List<PolyCurve> pcs = new List<PolyCurve>();
                if (panel == null)
                    continue;
                try
                { 
                    pcs = cells[i].CurveLoops.FromRevit(); 
                }
                catch  // This catches when CurveLoops throws an exception due to the cell having no loops, meaning in Revit it exists in the database but is no longer on the CurtainWall with any corresponding Curves
                { 
                    continue; 
                }

                foreach (PolyCurve pc in pcs)
                {
                    BH.oM.Facade.Elements.Opening bHoMOpening = null;
                    // If panel is a basic wall, the panel is not the actual element, it is an empty panel that hosts the
                    // actual element, so we assign it a null construction and separately return the wall
                    if (panel is Autodesk.Revit.DB.Panel && document.GetElement((panel as Autodesk.Revit.DB.Panel).FindHostPanel()) is Wall)
                    {
                        Wall hostElement = document.GetElement((panel as Autodesk.Revit.DB.Panel).FindHostPanel()) as Wall;
                        BH.oM.Facade.Elements.Panel bHoMCWPanel = hostElement.FacadePanelFromRevit(settings, refObjects);
                        bHoMOpening = bHoMCWPanel.FacadePanelAsOpening(hostElement.Id.ToString(), refObjects);
                    }
                    else
                        bHoMOpening = panel.FacadeOpeningFromRevit(settings, refObjects);

                    if (bHoMOpening == null)
                        continue;

                    List<FrameEdge> edges = bHoMOpening.Edges;
                    foreach  (FrameEdge edge in edges)
                    {
                        List<FrameEdge> adjEdges = edge.AdjacentElements(cwMullions).OfType<FrameEdge>().ToList() ;
                        if (adjEdges.Count > 0)
                            edge.FrameEdgeProperty = adjEdges[0].FrameEdgeProperty;
                        else
                            edge.FrameEdgeProperty = null;
                    }
                    bHoMOpening.Edges = edges;

                    result.Add(bHoMOpening);
                }
            }
            
            return result;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static oM.Facade.Elements.Opening FacadePanelAsOpening(this oM.Facade.Elements.Panel panel, string refId = "", Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (panel == null)
                return null;
            ;
            oM.Facade.Elements.Opening opening = refObjects.GetValue<oM.Facade.Elements.Opening>(refId);
            if (opening != null)
                return opening;

            opening = new oM.Facade.Elements.Opening { Name = panel.Name, Edges = panel.ExternalEdges, Fragments = panel.Fragments, OpeningConstruction = panel.Construction, Tags = panel.Tags, CustomData = panel.CustomData, Type = oM.Facade.Elements.OpeningType.Undefined };

            return opening;
        }

        /***************************************************/
    }
}


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

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this List<Curve> curves, PullSettings pullSettings = null)
        {
            if (curves == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            return curves.Select(c => c.ToBHoM(pullSettings)).ToList();
        }

        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this CurveArray curveArray, PullSettings pullSettings = null)
        {
            if (curveArray == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            for (int i = 0; i < curveArray.Size; i++)
            {
                result.Add(curveArray.get_Item(i).ToBHoM(pullSettings));
            }
            return result;
        }

        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this EdgeArray edgeArray, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            foreach (Edge aEdge in edgeArray)
            {
                result.Add(aEdge.ToBHoM(pullSettings));
            }

            return result;
        }
    }
}
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
 
using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Settings
{
    [Description("Revit family load settings for Revit Adapter. Prototype, currently with limited functionality.")]
    public class FamilyLoadSettings : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Library of Revit families that can be loaded to the model.")]
        public virtual FamilyLibrary FamilyLibrary { get; set; } = new FamilyLibrary();

        [Description("If true, Revit family will be overwritten on load, if false it will not be changed.")]
        public virtual bool OverwriteFamily { get; set; } = true;

        [Description("If true, Revit family parameters will be overwritten on load, if false they will not be changed.")]
        public virtual bool OverwriteParameterValues { get; set; } = true;

        /***************************************************/
    }
}




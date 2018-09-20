﻿using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.DataManipulation.Queries;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this BuildingElementType buildingElementType)
        {
            switch (buildingElementType)
            {
                case (oM.Environment.Elements.BuildingElementType.Ceiling):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings;
                case (oM.Environment.Elements.BuildingElementType.Door):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Doors;
                case (oM.Environment.Elements.BuildingElementType.Floor):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Floors;
                case (oM.Environment.Elements.BuildingElementType.Roof):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Roofs;
                case (oM.Environment.Elements.BuildingElementType.Wall):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Walls;
                case (oM.Environment.Elements.BuildingElementType.Window):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Windows;
                case (oM.Environment.Elements.BuildingElementType.Undefined):
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
                default:
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
            }
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document)
        {
            if (bHoMObject == null || document == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;


            BuiltInCategory aBuiltInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID;
            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(bHoMObject);
            if (!string.IsNullOrEmpty(aCategoryName))
                aBuiltInCategory = BuiltInCategory(document, aCategoryName);

            if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
            {
                string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
                if (string.IsNullOrEmpty(aFamilyName))
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;

                string aTypeName = BH.Engine.Adapters.Revit.Query.TypeName(bHoMObject);

                List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();

                ElementType aElementType = null;
                if (string.IsNullOrEmpty(aTypeName))
                    aElementType = aElementTypeList.Find(x => x.FamilyName == aFamilyName);
                else
                {
                    aElementType = aElementTypeList.Find(x => x.FamilyName == aFamilyName && x.Name == aTypeName);
                    if(aElementType == null)
                        aElementType = aElementTypeList.Find(x => x.FamilyName == aFamilyName);
                }

                if (aElementType != null && aElementType.Category != null)
                    aBuiltInCategory = (BuiltInCategory)aElementType.Category.Id.IntegerValue;

            }

            return aBuiltInCategory;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this Document document, string categoryName)
        {
            if (document == null || string.IsNullOrEmpty(categoryName)|| document.Settings == null || document.Settings.Categories == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;


            foreach (Category aCategory in document.Settings.Categories)
                if (aCategory.Name == categoryName)
                    return (BuiltInCategory)aCategory.Id.IntegerValue;

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document, FamilyLibrary familyLibrary)
        {
            BuiltInCategory aBuiltInCategory = bHoMObject.BuiltInCategory(document);
            if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
            {
                string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
                if (!string.IsNullOrEmpty(aFamilyName))
                {
                    string aCategoryName = document.CategoryName(aFamilyName);
                    if (!string.IsNullOrEmpty(aCategoryName))
                        aBuiltInCategory = document.BuiltInCategory(aCategoryName);
                }

                if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID && familyLibrary != null)
                {
                    List<string> aCategoryNameList = BH.Engine.Adapters.Revit.Query.CategoryNames(familyLibrary, aFamilyName, BH.Engine.Adapters.Revit.Query.TypeName(bHoMObject));
                    if (aCategoryNameList != null && aCategoryNameList.Count > 0)
                        aBuiltInCategory = document.BuiltInCategory(aCategoryNameList.First());

                    if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    {
                        aCategoryNameList = BH.Engine.Adapters.Revit.Query.CategoryNames(familyLibrary, aFamilyName);
                        if (aCategoryNameList != null && aCategoryNameList.Count > 0)
                            aBuiltInCategory = document.BuiltInCategory(aCategoryNameList.First());
                    }
                }

            }

            return aBuiltInCategory;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this FilterQuery filterQuery, Document document)
        {
            if (document == null || document.Settings == null || document.Settings.Categories == null || filterQuery == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            if(!filterQuery.Equalities.ContainsKey(BH.Engine.Adapters.Revit.Convert.FilterQuery.CategoryName))
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            string aCategoryName = filterQuery.Equalities[BH.Engine.Adapters.Revit.Convert.FilterQuery.CategoryName] as string;

            return BuiltInCategory(document, aCategoryName);
        }

        /***************************************************/
    }
}

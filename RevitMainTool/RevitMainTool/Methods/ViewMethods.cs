﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMainTool
{
    public static class ViewMethods
    {

        public static void SetLocationOnSheet(this Viewport viewport, XYZ location)
        {
            Document doc = viewport.Document;
            ViewSheet viewSheet = doc.GetElement(viewport.SheetId) as ViewSheet;
            View view = doc.GetElement(viewport.ViewId) as View;

            bool cropBoxActive = view.CropBoxActive;
            bool cropBoxVisible = view.CropBoxVisible;

            if (!cropBoxActive)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("SetLocation");

                    view.CropBoxActive = true;

                    tx.Commit();
                }
            }

            CurveLoop savedBox = view.GetCropRegionShapeManager().GetCropShape().First();
            var isNotRectangular = view.GetCropRegionShapeManager().ShapeSet;

            if (isNotRectangular)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("SetLocation");

                    view.GetCropRegionShapeManager().RemoveCropRegionShape();

                    tx.Commit();
                }
            }

            XYZ start = new XYZ(-9999999, -9999999, 0);
            XYZ End = new XYZ(9999999, 9999999, 0);
            BoundingBoxXYZ bigBoundingBox = new BoundingBoxXYZ();
            bigBoundingBox.Min = start;
            bigBoundingBox.Max = End;

            using (var tx = new Transaction(doc))
            {
                tx.Start("Make Crop Big");

                view.CropBoxActive = true;
                view.CropBoxVisible = true;
                view.CropBox = bigBoundingBox;

                tx.Commit();
            }

            using (var tx = new Transaction(doc))
            {
                tx.Start("SetLocation");

                viewport.SetBoxCenter(location);
                view.CropBoxActive = cropBoxActive;
                view.CropBoxVisible = cropBoxVisible;
                view.GetCropRegionShapeManager().SetCropShape(savedBox);

                tx.Commit();
            }
        }

        public static XYZ GetLocationOnSheet(this Viewport viewport)
        {
            Document doc = viewport.Document;
            ViewSheet viewSheet = doc.GetElement(viewport.SheetId) as ViewSheet;
            View view = doc.GetElement(viewport.ViewId) as View;

            bool cropBoxActive = view.CropBoxActive;
            bool cropBoxVisible = view.CropBoxVisible;

            if (!cropBoxActive)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("SetLocation");

                    view.CropBoxActive = true;

                    tx.Commit();
                }
            }

            CurveLoop savedBox = view.GetCropRegionShapeManager().GetCropShape().First();
            bool isNotRectangular = view.GetCropRegionShapeManager().ShapeSet;

            if (isNotRectangular)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("SetLocation");

                    view.GetCropRegionShapeManager().RemoveCropRegionShape();

                    tx.Commit();
                }
            }

            XYZ start = new XYZ(-999999, -999999, 0);
            XYZ End = new XYZ(999999, 999999, 0);
            BoundingBoxXYZ bigBoundingBox = new BoundingBoxXYZ();
            bigBoundingBox.Min = start;
            bigBoundingBox.Max = End;

            using (var tx = new Transaction(doc))
            {
                tx.Start("Make Crop Big");

                view.CropBoxActive = true;
                view.CropBoxVisible = true;
                view.CropBox = bigBoundingBox;

                tx.Commit();
            }
            
            XYZ brro = viewport.GetBoxCenter();

            using (var tx = new Transaction(doc))
            {
                tx.Start("Make Crop Big");

                view.CropBoxActive = cropBoxActive;
                view.CropBoxVisible = cropBoxVisible;
                view.GetCropRegionShapeManager().SetCropShape(savedBox);

                tx.Commit();
            }

            return brro;
        }

        public static Viewport GetMainPlanViewInSheet(ViewSheet viewSheet)
        {
            Document doc = viewSheet.Document;
            var viewIds = viewSheet.GetAllViewports();

            List<Viewport> viewports = new List<Viewport>();

            foreach (ElementId viewId in viewIds)
            {
                Element eleViewport = doc.GetElement(viewId);
                if (eleViewport is Viewport viewPort)
                {
                    Element eleView = doc.GetElement(viewPort.ViewId);

                    if (eleView is ViewPlan)
                    {
                        viewports.Add(viewPort);
                    }
                }
            }

            Viewport result = null;

            if (viewports.Count > 0)
            {
                Element test = GeneralMethods.GetElementWithBiggestBoundingBox(viewports.Cast<Element>().ToList(), viewSheet);

                result = (Viewport)test;
            }

            return result;
        }

        public static void AdjustCropToElements(View view, IEnumerable<Element> elements, XYZ border)
        {

            BoundingBoxXYZ bounding = GeneralMethods.GetBoundingBox(elements);
            //XYZ origin = new XYZ(52.24, 105.19, 19.61);
            XYZ origin = view.CropBox.Transform.Origin;

            XYZ pointMax = bounding.Max.Add(border).Subtract(origin);
            XYZ pointMin = bounding.Min.Subtract(border).Subtract(origin);

            //XYZ pointMax = new XYZ(-15.86, 10.578, 0);
            //XYZ pointMin = new XYZ(-16.19, 10.367, -4.41);

            view.CropBox = new BoundingBoxXYZ() { Min = pointMin, Max = pointMax };
            //currentView.CropBox = new BoundingBoxXYZ() { Min = pointMin, Max = pointMax };

        }


        public static void CreateSheetAndFilters(Element element)
        {
            if (element is Pipe)
            {
                Document doc = element.Document;
                string abbreviationString = element.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString();
                string uniqueName = abbreviationString;

                List<string> strings = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewPlan))
                    .Cast<ViewPlan>()
                    .Select(view => view.Name)
                    .ToList();

                bool checker = true;
                int indx = 1;

                while (checker)
                {
                    if (strings.Contains(uniqueName))
                    {
                        uniqueName = abbreviationString + " - " + indx;
                        indx++;
                    }
                    else
                    {
                        break;
                    }
                }

                View currentView = doc.ActiveView;
                ElementId newViewId = currentView.Duplicate(ViewDuplicateOption.Duplicate);
                View newView = doc.GetElement(newViewId) as View;

                newView.Name = uniqueName;
                List<ViewPlan> allTemplates = new FilteredElementCollector(doc).OfClass(typeof(ViewPlan)).Cast<ViewPlan>().ToList();
                ViewPlan theTemplate = allTemplates.First(i => i.Name == "SMJ - (IT) H1 - Flatmynd");
                newView.ViewTemplateId = theTemplate.Id;

                newView.CropBoxActive = false;

                var allSimilarElements = new FilteredElementCollector(doc, newViewId)
                    .OfCategory(element.Category.BuiltInCategory)
                    .Where(i => i.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString() == abbreviationString)
                    .Cast<Element>()
                    .ToList();
                BoundingBoxXYZ bounding = GeneralMethods.GetBoundingBox(allSimilarElements);
                newView.CropBox = bounding;
                newView.CropBoxActive = true;

                string prefixText = "ZAutoGenerated - ";
                string originalFilterName = prefixText + uniqueName;



                //Create or find filter for Everything not current abbreviation
                string filterNameNotCurrentAbbreviation = "Not " + originalFilterName;

                checker = true;
                indx = 1;


                while (checker)
                {
                    if (!ParameterFilterElement.IsNameUnique(doc, filterNameNotCurrentAbbreviation))
                    {
                        filterNameNotCurrentAbbreviation = originalFilterName + " - " + indx;
                        indx++;
                    }
                    else
                    {
                        break;
                    }
                }



                ICollection<ElementId> builtInCategories = new List<ElementId>
                {
                    new ElementId(BuiltInCategory.OST_DuctAccessory),
                    new ElementId(BuiltInCategory.OST_DuctFitting),
                    new ElementId(BuiltInCategory.OST_DuctInsulations),
                    new ElementId(BuiltInCategory.OST_DuctLinings),
                    new ElementId(BuiltInCategory.OST_DuctCurves),
                    new ElementId(BuiltInCategory.OST_PlaceHolderDucts),
                    new ElementId(BuiltInCategory.OST_FlexDuctCurves),
                    new ElementId(BuiltInCategory.OST_FlexPipeCurves),
                    new ElementId(BuiltInCategory.OST_PipeAccessory),
                    new ElementId(BuiltInCategory.OST_PipeFitting),
                    new ElementId(BuiltInCategory.OST_PipeInsulations),
                    new ElementId(BuiltInCategory.OST_PlaceHolderPipes),
                    new ElementId(BuiltInCategory.OST_PipeCurves),
                    new ElementId(BuiltInCategory.OST_PlumbingFixtures),
                    new ElementId(BuiltInCategory.OST_MechanicalEquipment)
                };

                FilterRule filterRule = ParameterFilterRuleFactory.CreateNotBeginsWithRule(new ElementId(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM), uniqueName);

                ElementParameterFilter to = new ElementParameterFilter(filterRule);

                ParameterFilterElement filter = ParameterFilterElement.Create(doc, filterNameNotCurrentAbbreviation, builtInCategories, to);
                ElementId filterId = filter.Id;
                newView.AddFilter(filterId);
                OverrideGraphicSettings graphicSettings = new OverrideGraphicSettings();
                graphicSettings.SetHalftone(true);
                graphicSettings.SetProjectionLineColor(new Color(0, 0, 0));
                graphicSettings.SetSurfaceTransparency(80);

                newView.SetFilterOverrides(filterId, graphicSettings);

                CreateSheetForView(newView, new string[] { "HSKV", abbreviationString });

            }
        }

        public static ViewSheet CreateSheetForView(View view, string[] numberAndName)
        {
            Document doc = view.Document;
            string name = numberAndName[1];
            var test = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).Where(x => (x as ViewSheet).IsPlaceholder).Cast<ViewSheet>();
            FamilySymbol titleBlock = GetTitleblockBasedOnView(view);
            ViewSheet sheet = null;

            foreach (ViewSheet viewSheet in test)
            {
                if (CalcLevenshteinDistance(viewSheet.Name.ToLower(), name.Remove(0, 4).ToLower()) <= 1)
                {
                    sheet = viewSheet;
                    sheet.ConvertToRealSheet(titleBlock.Id);
                    break;
                }
            }

            if (sheet == null)
            {
                sheet = ViewSheet.Create(doc, titleBlock.Id);
                sheet.SheetNumber = numberAndName[0];
                sheet.Name = name;
            }

            BoundingBoxXYZ bound = titleBlock.get_BoundingBox(sheet);
            double lengthX = (bound.Max.X - bound.Min.X) / 2;
            double lengthY = (bound.Max.Y - bound.Min.Y) / 2;
            XYZ middlePoint = new XYZ(bound.Min.X + lengthX, bound.Min.Y + lengthY, bound.Min.Z);

            Viewport.Create(doc, sheet.Id, view.Id, middlePoint);

            return sheet;
        }

        public static FamilySymbol GetTitleblockBasedOnView(View view)
        {
            Document doc = view.Document;
            FilteredElementCollector allTitleBlocks = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsElementType();
            BoundingBoxXYZ viewBoundBox = view.CropBox;
            int scale = view.Scale;
            double viewHeight = (viewBoundBox.Max.Y - viewBoundBox.Min.Y) / scale;
            double viewWidth = (viewBoundBox.Max.X - viewBoundBox.Min.X) / scale;

            FamilySymbol currentTitleBlock = null;
            BoundingBoxXYZ currentBound = null;

            foreach (FamilySymbol titleBlock in allTitleBlocks)
            {
                BoundingBoxXYZ titleBlockBox = titleBlock.get_BoundingBox(view);
                double titleHeight = titleBlockBox.Max.Y - titleBlockBox.Min.Y;
                double titleWidth = titleBlockBox.Max.X - titleBlockBox.Min.X;

                if (currentTitleBlock == null)
                {
                    if (titleHeight > viewHeight && titleWidth > viewWidth)
                    {
                        currentTitleBlock = titleBlock;
                        currentBound = titleBlock.get_BoundingBox(view);
                    }
                }
                else
                {
                    if (titleHeight > viewHeight && titleWidth > viewWidth)
                    {
                        double currentHeight = currentBound.Max.Y - currentBound.Min.Y;
                        double currentWidth = currentBound.Max.X - currentBound.Min.X;
                        if (currentHeight > titleHeight && currentWidth > titleWidth)
                        {
                            currentTitleBlock = titleBlock;
                            currentBound = titleBlock.get_BoundingBox(view);
                        }
                    }
                }
            }

            if (currentTitleBlock == null)
            {
                return null;
            }

            return currentTitleBlock;

        }




        public static View3D CreateViewForRay(Document doc)
        {
            string name = "Auto Generated View for Raytracing";
            var maybe3DView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).Where(i => i is View3D).Cast<View3D>();
            View3D newView = null;
            if (maybe3DView.Any(i => i.Name == name))
            {
                newView = maybe3DView.First(i => i.Name == name);
            }

            if (newView == null)
            {
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

                if (viewFamilyType == null)
                {
                    return null;
                }
                newView = View3D.CreateIsometric(doc, viewFamilyType.Id);
            }

            List<BuiltInCategory> demCategoriesToShow = new List<BuiltInCategory>() {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Ceilings,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_RoomSeparationLines,
                BuiltInCategory.OST_Windows,
                BuiltInCategory.OST_CurtainGridsWall,
                BuiltInCategory.OST_StructuralFoundation,
                BuiltInCategory.OST_RvtLinks,
                BuiltInCategory.OST_FoundationSlabAnalytical
            };

            List<BuiltInCategory> demCategoriesToHide = new List<BuiltInCategory>() {
                BuiltInCategory.OST_Furniture,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_ElectricalEquipment,
                BuiltInCategory.OST_Stairs,
                BuiltInCategory.OST_Planting,
                BuiltInCategory.OST_Casework,
                BuiltInCategory.OST_GenericModel,
                BuiltInCategory.OST_PlumbingEquipment,
                BuiltInCategory.OST_Entourage,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_FurnitureSystems,
                BuiltInCategory.OST_Site,
                BuiltInCategory.OST_LightingFixtures,
                BuiltInCategory.OST_LightingDevices,
                BuiltInCategory.OST_SpecialityEquipment,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_Columns,
                BuiltInCategory.OST_Railings,
                BuiltInCategory.OST_DuctFitting
            };

            newView.IsSectionBoxActive = false;
            newView.Name = name;
            var categories = doc.Settings.Categories;

            foreach (Category cat in categories)
            {
                BuiltInCategory builtIn = cat.BuiltInCategory;
                if (demCategoriesToShow.Contains(builtIn))
                {
                    cat.set_Visible(newView, true);
                }
                else if (demCategoriesToHide.Contains(builtIn))
                {
                    cat.set_Visible(newView, false);
                }
            }

            return newView;
        }

        public static View GetCurrentViewFromElement(Element ele)
        {
            Document doc = ele.Document;
            if (doc == null)
            {
                return null;
            }
            else
            {
                return doc.ActiveView;
            }
        }

        internal static int CalcLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(a))
            {
                return b.Length;
            }

            if (string.IsNullOrEmpty(b))
            {
                return a.Length;
            }

            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];

            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
            {
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;

                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                    );
                }
            }

            return distances[lengthA, lengthB];
        }


    }
}

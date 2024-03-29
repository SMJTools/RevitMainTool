#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace RevitMainTool
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            var tabName = "SMJ Main Tool";
            application.CreateRibbonTab(tabName);


            //RibbonPanel panelTest = application.CreateRibbonPanel(tabName, "Testing");

            //new ButtonBuilder("AlignTagsRight", typeof(Tester))
            //    .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Settings.png")
            //    .Text("Settings")
            //    .Build(panelTest);


            // Add a new ribbon panel
            RibbonPanel panelTags = application.CreateRibbonPanel(tabName, "Tags");

            SplitButtonData splitButtonData = new SplitButtonData("Tags", "Tag Functions");
            SplitButton splitButton = panelTags.AddItem(splitButtonData) as SplitButton;

            new ButtonBuilder("TagAllSimilar", typeof(TagAllSimilar))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\TagSimilar.png")
                .Text("Tag All\nSimilar")
                .Build(splitButton);

            new ButtonBuilder("SpaceTagsFixedDistance", typeof(SpaceTagsFixedDistanceUI))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignFixedDistance.png")
                .Text("Space Tags\nFixed Distance")
                .Build(splitButton);

            new ButtonBuilder("SpaceTagsEvenly", typeof(SpaceTagsEvenly))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\SpaceEvenly.png")
                .Text("Space Tags\nEvenly")
                .Build(splitButton);

            new ButtonBuilder("AlignTagsLeft", typeof(AlignTagsLeft))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignLeft.png")
                .Text("Align Tags\nLeft")
                .Build(splitButton);

            new ButtonBuilder("AlignTagsCenter", typeof(AlignTagsCenter))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignCenter.png")
                .Text("Align Tags\nCenter")
                .Build(splitButton);

            new ButtonBuilder("AlignTagsRight", typeof(AlignTagsRight))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignRight.png")
                .Text("Align Tags\nRight")
                .Build(splitButton);

            //RibbonPanel panelPipes = application.CreateRibbonPanel(tabName, "Pipes");

            //new ButtonBuilder("DimensionPipes", typeof(DimensionPipesThatCutView))
            //    .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\DimensionPipes.png")
            //    .Text("Dimension\nPipes")
            //    .Build(panelPipes);

            //RibbonPanel panelSheet = application.CreateRibbonPanel(tabName, "Sheet");

            //new ButtonBuilder("UpdatePaperSizesAndSMJScale", typeof(UpdatePaperSizesAndSMJScale))
            //    .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Settings.png")
            //    .Text("Update Sheet\nParameters")
            //    .Build(panelSheet);

            RibbonPanel panelSpaces = application.CreateRibbonPanel(tabName, "Spaces");

            new ButtonBuilder("CreateOrUpdateSpaceFromLinkedFile", typeof(CreateOrUpdateSpaceFromLinkedFile))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\IconRoomImport.png")
                .Text("Create Spaces\nFrom Rooms")
                .Build(panelSpaces);

            RibbonPanel panelHSKV = application.CreateRibbonPanel(tabName, "HSKV");

            new ButtonBuilder("CreateDrawingFromPipe", typeof(CreateDrawingFromPipe))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Sheet.png")
                .Text("Create Drawing\nFrom Pipe")
                .Build(panelHSKV);

            new ButtonBuilder("CropViewFromPipe", typeof(CropViewFromPipe))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Cropping.png")
                .Text("Crop View\nFrom Pipe")
                .Build(panelHSKV);

            new ButtonBuilder("CreateSheetFromPipe", typeof(CreateSheetFromPipe))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Sheet.png")
                .Text("Create Sheet\nFrom Pipe")
                .Build(panelHSKV);

            new ButtonBuilder("CreateFiltersFromPipes", typeof(CreateFiltersFromPipes))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Tester.png")
                .Text("Create Filters\nFrom Pipe")
                .Build(panelHSKV);

            new ButtonBuilder("UpdatePaperAndScaleParameterOnCurrent", typeof(UpdatePaperAndScaleParameterOnCurrent))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Settings.png")
                .Text("Update Current\nSheet Parameter")
                .Build(panelHSKV);

            new ButtonBuilder("UpdatePaperSizesAndSMJScale", typeof(UpdatePaperSizesAndSMJScale))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Settings.png")
                .Text("Update Sheet\nParameters")
                .Build(panelHSKV);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}

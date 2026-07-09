#region Namespaces
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Application = Autodesk.Revit.ApplicationServices.Application;
#endregion

namespace DVTools
{
    [Transaction(TransactionMode.Manual)]
    public class AdjustBeamCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // code

            //try
            //{
                using (TransactionGroup transG = new TransactionGroup(doc))
                {
                    transG.Start("");

                    AdjustBeamViewModel viewModel = new AdjustBeamViewModel(uidoc);
                    AdjustBeamWindow window = new AdjustBeamWindow(viewModel);

                    bool? showDialog = window.ShowDialog();

                    if (showDialog == null || showDialog == false)
                    {
                        transG.RollBack();
                        return Result.Cancelled;
                    }
                    transG.Assimilate();
                    return Result.Succeeded;
                }
            //}
            //catch
            //{
            //    return Result.Cancelled;
            //}
        }

    }
}
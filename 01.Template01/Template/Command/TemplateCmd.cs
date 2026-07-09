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
    public class TemplateCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            try
            {
                using (TransactionGroup transG = new TransactionGroup(doc))
                {
                    transG.Start("");

                    // Viết code của bạn ở đây

                    MessageBox.Show("Hello, Revit!");


                    transG.Assimilate();
                    return Result.Succeeded;
                }
            }
            catch
            {
                return Result.Cancelled;
            }
        }

    }
}
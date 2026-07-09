using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DVTools.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;


namespace DVTools
{
    public class SelectedElement
    {
        public UIDocument UiDoc;
        public Document Doc;
        public List<Element> elements = new List<Element>();
        public List<Element> beams = new List<Element>();
        public List<Element> supports = new List<Element>();
        public SelectedElement(UIDocument uiDoc)
        {
            UiDoc = uiDoc;
            Doc = UiDoc.Document;

            BeamAndColumnSelectionFilter filter = new BeamAndColumnSelectionFilter();
            while (true)
            {
                try
                {

                    IList<Reference> refs = UiDoc.Selection.PickObjects(ObjectType.Element, filter, "Please Select Elements");

                    // ❌ Trường hợp không chọn gì
                    if (refs == null || refs.Count == 0)
                    {
                        TaskDialogResult result = ReportUtils.ShowDialogWaning("Error Select Elements", "Haven't Selected A Beam Yet?.", "Do You Want To Continue ? ");
                        if (result == TaskDialogResult.No || result == TaskDialogResult.Cancel)
                            throw new InvalidOperationException("Error Select Elements");
                        continue;
                    }

                    //// ❌ Trường hợp chọn < 2 tường
                    //if (refs.Count < 3)
                    //{
                    //    TaskDialogResult result = ReportUtils.ShowDialogWaning("Error Select Elements", "Please Select At Least 3 Elements.", "Do You Want To Continue ? ");
                    //    if (result == TaskDialogResult.No || result == TaskDialogResult.Cancel)
                    //        throw new InvalidOperationException("Error Select Elements");
                    //    continue;
                    //}

                    // ✅ Trường hợp chọn đủ
                    foreach (Reference re in refs)
                    {
                        elements.Add(Doc.GetElement(re));
                    }

                    break; // THOÁT khỏi while vì đã chọn đủ
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    throw new InvalidOperationException();
                }

            }

            //if (elements.Count > 3)
            //{


            for (int i = 0; i < elements.Count; i++)
                {
                    if (IsStructuralBeam(elements[i]))
                    {
                        beams.Add(elements[i]);
                    }
                    else if (IsStructuralSupport(elements[i]))
                    {
                        supports.Add(elements[i]);
                    }
                }
            //}
        }

        public bool IsStructuralSupport(Element element)
        {
            if (element?.Category == null) return false;

            BuiltInCategory category = (BuiltInCategory)element.Category.Id.IntegerValue;

            return category == BuiltInCategory.OST_StructuralColumns
                || category == BuiltInCategory.OST_Walls;
        }

        public bool IsStructuralBeam(Element element)
        {
            if (element?.Category == null) return false;

            BuiltInCategory category = (BuiltInCategory)element.Category.Id.IntegerValue;

            return category == BuiltInCategory.OST_StructuralFraming;
        }
    }
}

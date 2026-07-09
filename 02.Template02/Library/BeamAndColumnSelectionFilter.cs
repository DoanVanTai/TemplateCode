#region Namespaces

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
#endregion

namespace DVTools
{
    public class BeamAndColumnSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            string name = elem.Category.Name;
            return name.Equals("Structural Framing") || name.Equals("Structural Columns") || name.Equals("Walls");
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
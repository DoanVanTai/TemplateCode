#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Autodesk.Revit.DB;
#endregion

namespace DVTools
{
    public static class FamilyUtils
    {
        public static FamilySymbol GetFamilySymbol(Document doc, string familyName, string typeName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(f => f.Family.Name.Equals(familyName)
                    && f.Name.Equals(typeName)) 
                .FirstOrDefault();
        }
    }
}
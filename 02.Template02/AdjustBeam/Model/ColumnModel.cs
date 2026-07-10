using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DVTools;
using DVTools.Library;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Linq;

namespace QHTools
{
    public class ColumnModel
    {
        public Element column { get; set; }
        public XYZ locationPoint{ get; set; }
        public XYZ direction1 { get; set; }
        public XYZ direction2 { get; set; }
      
        public ColumnModel(Document doc, Element e)
        {
            column = e;

            locationPoint = (column.Location as LocationPoint).Point;
            direction1 = (column as FamilyInstance).HandOrientation;
            direction2 = (column as FamilyInstance).FacingOrientation;
           
        }
    }
}


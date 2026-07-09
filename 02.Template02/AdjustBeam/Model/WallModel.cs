using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DVTools;
using DVTools.Library;

namespace QHTools
{
    public class WallModel
    {
        public Wall wall { get; set; }
        public double width { get; set; }
        public XYZ startPoint { get; set; }
        public XYZ endPoint { get; set; }
        public XYZ direction1 { get; set; }
        public XYZ direction2 { get; set; }
        public Plane plane { get; set; }
        public WallModel(Document doc, Element e)
        {
            wall = e as Wall;
           
            width = wall.Width;
            LocationCurve locationCurve = wall.Location as LocationCurve;
            Curve curve = (wall.Location as LocationCurve).Curve;
            startPoint = curve.GetEndPoint(0);
            endPoint = curve.GetEndPoint(1);

            direction1 = (startPoint - endPoint).Normalize();
            direction2 = direction1.CrossProduct(XYZ.BasisZ).Normalize();
            // Plane normal

           

            XYZ normal = direction2.Normalize();

            //MessageBox.Show(normal.ToString());

            plane = Plane.CreateByNormalAndOrigin(normal, startPoint);

        }

    }
}


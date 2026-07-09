using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
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
using DVTools.Library;

namespace DVTools
{
    public class BeamModel
    {
        public Element beam { get; set; }
        public string typeName { get; set; }
        public double B_Height { get; set; }
        public double B_HeightLedge1 { get; set; }
        public double B_HeightLedge2 { get; set; }
        public double B_Width { get; set; }
        public double B_WidthLedge1 { get; set; }
        public double B_WidthLedge2 { get; set; }
        public XYZ startPoint { get; set; }
        public XYZ endPoint { get; set; }
        public XYZ direction { get; set; }
        public Plane plane { get; set; }
        public Line line { get; set; }
        public BeamModel(Document doc, Element e)
        {
            beam = e;
            typeName = e.Name;

            Element eType = doc.GetElement(e.GetTypeId());
            var para = eType.LookupParameter("Family Name");
            if (para != null)
            {
                var familyName = para.AsString();
                if (!GetFamilyName.BeamT.Contains(familyName))
                {
                    ReportUtils.ShowDialogError("ERROR", "KHÔNG ĐÚNG FAMILY BEAM ", "CHỌN ĐÚNG FAMILY BEAM");
                    throw new InvalidOperationException("KHÔNG ĐÚNG FAMILY BEAM");
                }
            }

            //B_Height = eType.LookupParameter(GetParameterBeamT.B_Height).AsDouble();
            //B_HeightLedge1 = eType.LookupParameter(GetParameterBeamT.B_HeightLedge1).AsDouble();
            //B_HeightLedge2 = eType.LookupParameter(GetParameterBeamT.B_HeightLedge2).AsDouble();
            //B_Width = eType.LookupParameter(GetParameterBeamT.B_Width).AsDouble();
            //B_WidthLedge1 = eType.LookupParameter(GetParameterBeamT.B_WidthLedge1).AsDouble();
            //B_WidthLedge2 = eType.LookupParameter(GetParameterBeamT.B_WidthLedge2).AsDouble();


            B_Width = eType.LookupParameter(GetParameterBeamT.B_Width).AsDouble();
            B_WidthLedge1 = 200 / 304.8;
            B_WidthLedge2 = 200 / 304.8;

            LocationCurve locCurve = beam.Location as LocationCurve;
            if (locCurve == null) return;

            Curve curve = locCurve.Curve;

            // Start & End
             startPoint = curve.GetEndPoint(0);
             endPoint = curve.GetEndPoint(1);
  
            // Direction
             direction = (endPoint - startPoint).Normalize();

            // Plane normal
            XYZ normal = direction.CrossProduct(XYZ.BasisZ).Normalize();

            // Create plane
            plane = Plane.CreateByNormalAndOrigin(normal, startPoint);

            // Line
             line = curve as Line;

        }
      
    }
}


using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DVTools.Library;
using QHTools;
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
using Transform = Autodesk.Revit.DB.Transform;

namespace DVTools
{
    public class WallBeamModel
    {
        public Element beam { get; set; }
        public bool isIntersection { get; set; }
        public XYZ wallDirection { get; set; }
        public XYZ intersectionPoint { get; set; }
        public XYZ vec { get; set; }
        public double anpha { get; set; }
        public bool isStartPoint { get; set; }
        public bool IsWallBeamPer { get; set; }
        public WallModel wallModel { get; set; }
        public BeamModel beamModel { get; set; }
        public WallBeamModel(Document doc, Element wall , Element Beam)
        {
            beam = Beam;

            wallModel = new WallModel(doc, wall);
            
            beamModel = new BeamModel(doc, beam);

            wallDirection = null;
            isIntersection = Utils.IsBeamIntersectSupport(wall, Beam);
         
            if (isIntersection)
            {
                wallDirection = wallModel.direction1;
              
                intersectionPoint = Utils.IntersectLinePlane(beamModel.line, wallModel.plane);
                isStartPoint = IsStartPoint(doc, intersectionPoint, beamModel);
                IsWallBeamPer = Utils.IsPerpendicular(wallDirection, beamModel.line.Direction);

                if (isStartPoint)
                {
                    vec = (beamModel.endPoint - intersectionPoint).Normalize();
                }
                else
                {
                    vec = (beamModel.startPoint - intersectionPoint).Normalize();
                }
            }

        }

        public  bool IsStartPoint(Document doc, XYZ intersectionPoint, BeamModel beamModel)
        {
            if (intersectionPoint.DistanceTo(beamModel.startPoint) < intersectionPoint.DistanceTo(beamModel.endPoint))
            {
               
                return isStartPoint = true;
            }
            else
            {
                return isStartPoint = false;
            }
              
        }
       
        public  bool IsParallelBeam(FamilyInstance beam1, FamilyInstance beam2, double tolerance = 1e-6)
        {
            LocationCurve lc1 = beam1.Location as LocationCurve;
            LocationCurve lc2 = beam2.Location as LocationCurve;

            if (lc1 == null || lc2 == null)
                return false;

            XYZ dir1 = (lc1.Curve.GetEndPoint(1) - lc1.Curve.GetEndPoint(0)).Normalize();
            XYZ dir2 = (lc2.Curve.GetEndPoint(1) - lc2.Curve.GetEndPoint(0)).Normalize();

            double dot = Math.Abs(dir1.DotProduct(dir2));

            return Math.Abs(dot - 1.0) < tolerance;
        }
    }
}


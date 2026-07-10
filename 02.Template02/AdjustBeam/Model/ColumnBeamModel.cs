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
    public class ColumnBeamModel
    {
        public Element beam { get; set; }
        public bool isIntersection { get; set; }
        public XYZ columnDirection { get; set; }
        public XYZ intersectionPoint { get; set; }
        public XYZ vec { get; set; }
        public double anpha { get; set; }
        public double width { get; set; }
        public bool isStartPoint { get; set; }
        public bool IsColumnBeamPer { get; set; }
        public ColumnModel columnModel { get; set; }
        public BeamModel beamModel { get; set; }
        public ColumnBeamModel(Document doc, Element column , Element Beam)
        {
            beam = Beam;

            columnModel = new ColumnModel(doc, column);
            beamModel = new BeamModel(doc, beam);

            //XYZ pointColumn = new XYZ(columnModel.locationPointBot.X, columnModel.locationPointBot.Y, beamModel.startPoint.Z);
           
            columnDirection = null;
            isIntersection = Utils.IsBeamIntersectSupport(column, beam);
         
            if (isIntersection)
            {
                GetColumnDirection(columnModel.direction1, columnModel.direction2, beamModel.direction);
               
                intersectionPoint = GetIntersectionPoint(doc, columnModel, beamModel);
                isStartPoint = IsStartPoint(doc, intersectionPoint, beamModel);


                IsColumnBeamPer = Utils.IsPerpendicular(columnDirection, beamModel.line.Direction);

                if(isStartPoint)
                {
                    vec = (beamModel.endPoint - intersectionPoint).Normalize();
                }
                else                 {
                     vec = (beamModel.startPoint - intersectionPoint).Normalize();
                }

                GetWidthColumn(doc, column);
            }

        }

       
        public  void GetColumnDirection(XYZ direction1, XYZ direction2, XYZ beamDirection)
        {
            const double cos45 = 0.70710678;

            double dot1 = Math.Abs(direction1.DotProduct(beamDirection));
            double dot2 = Math.Abs(direction2.DotProduct(beamDirection));

            if (dot1 >= cos45)
            {
                columnDirection = direction2;
                anpha = Math.Acos(dot1) * (180 / Math.PI);
            }
            else
            {
                columnDirection = direction1;
                anpha = Math.Acos(dot2) * (180 / Math.PI);
            }   

        }

        public  bool IsStartPoint(Document doc, XYZ intersectionPoint, BeamModel beamModel)
        {
            if (intersectionPoint.DistanceTo(beamModel.startPoint) < intersectionPoint.DistanceTo(beamModel.endPoint))
            {
               
                return  true;
            }
            else
            {
                return false;
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

        public XYZ GetIntersectionPoint(Document doc, ColumnModel columnModel, BeamModel beamModel)
        {
            XYZ normal = columnDirection.CrossProduct(XYZ.BasisZ).Normalize();
            Plane plane = Plane.CreateByNormalAndOrigin(normal, columnModel.locationPoint);

            XYZ intersectionPoint = Utils.IntersectLinePlane(beamModel.line, plane);

            return intersectionPoint;
        }

        public double GetWidthColumn(Document doc, Element column)
        {
            XYZ normal = columnDirection.CrossProduct(XYZ.BasisZ).Normalize();
            Solid s = Utils.GetSolidElement(column);
            List<PlanarFace> faces = Utils.GetPlanrFacePerpendicular(s, normal);
            width = 0;
            if (faces.Count == 2)
            {
                XYZ p1 = faces[0].Origin;
                XYZ p2 = faces[1].Origin;
                width = Math.Abs((p2 - p1).DotProduct(normal.Normalize()));
            }
            return width;
        }
    }
}


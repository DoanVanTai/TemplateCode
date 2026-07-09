#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace DVTools
{
    public static class Utils
    {
        public static bool IsPerpendicular(XYZ v1, XYZ v2, double tolerance = 1e-6)
        {
            v1 = v1.Normalize();
            v2 = v2.Normalize();

            return Math.Abs(v1.DotProduct(v2)) < tolerance;
        }
        public static bool IsVectorParallel(XYZ v1, XYZ v2, double tol = 1e-9)
        {
            if (v1 == null || v2 == null) return false;
            if (v1.GetLength() < tol || v2.GetLength() < tol) return false;

            double dot = v1.DotProduct(v2);
            double lenProduct = v1.GetLength() * v2.GetLength();

            double cosTheta = dot / lenProduct;

            double cosTol = Math.Cos(1 * Math.PI / 180.0);

            // gần 0° hoặc gần 180°
            return Math.Abs(cosTheta) >= cosTol;

        }
        public static bool IsVectorParallel1(XYZ v1, XYZ v2, double tol = 1e-9)
        {
            if (v1 == null || v2 == null) return false;
            if (v1.GetLength() < tol || v2.GetLength() < tol) return false;
            double crossLen = v1.CrossProduct(v2).GetLength();
            return crossLen <= tol;
        }
        public static XYZ GetIntersectionPointOfTwoBeams(Document doc, FamilyInstance beam1, FamilyInstance beam2)
        {
            // =========================
            // 1️⃣ Lấy curve của 2 beam
            // =========================
            LocationCurve lc1 = beam1.Location as LocationCurve;
            LocationCurve lc2 = beam2.Location as LocationCurve;

            if (lc1 == null || lc2 == null)
                return null;

            Curve c1 = lc1.Curve;
            Curve c2 = lc2.Curve;

            // Beam axis luôn là Line
            if (!(c1 is Line) || !(c2 is Line))
                return null;

            Line l1 = c1 as Line;
            Line l2 = c2 as Line;

            // =========================
            // 2️⃣ Tạo line vô hạn
            // =========================
            Line line1 = Line.CreateUnbound(
                l1.GetEndPoint(0),
                (l1.GetEndPoint(1) - l1.GetEndPoint(0)).Normalize());

            Line line2 = Line.CreateUnbound(
                l2.GetEndPoint(0),
                (l2.GetEndPoint(1) - l2.GetEndPoint(0)).Normalize());

            // =========================
            // 3️⃣ Tạo plane chứa line1
            // =========================
            XYZ dir1 = line1.Direction;
            XYZ origin = line1.Origin;

            XYZ dir2 = dir1.CrossProduct(XYZ.BasisZ).Normalize();
            XYZ normal = dir1.CrossProduct(dir2).Normalize();
            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
            
            // =========================
            // 4️⃣ Giao điểm plane & line2
            // =========================
            return IntersectLinePlane(line2, plane);
        }
        public static XYZ IntersectLinePlane( Line line,  Plane plane)
        {
            XYZ intersectionPoint = null;

            XYZ p0 = line.Origin;
            XYZ v = line.Direction;
            XYZ n = plane.Normal;
            XYZ pp = plane.Origin;

            double denom = n.DotProduct(v);

            // line song song plane
            if (Math.Abs(denom) < 1e-9)
                return null;

            double t = n.DotProduct(pp - p0) / denom;
            intersectionPoint = p0 + t * v;

            return intersectionPoint;
        }

        public static XYZ GetIntersectionBeamAndColumn(Document doc, FamilyInstance beam)
        {
            View3D view3D = Get3DView(doc);
            var curveBeam = (beam.Location as LocationCurve).Curve;
            var staPoint = curveBeam.GetEndPoint(0);
            var endPoint = curveBeam.GetEndPoint(1);

            XYZ direction = null;
            XYZ topPoint = null;

            if (staPoint.Z > endPoint.Z)
            {
                direction = (staPoint - endPoint).Normalize();
                topPoint = endPoint;
            }
            else
            {
                direction = (endPoint - staPoint).Normalize();
                topPoint = staPoint;
            }

            List<ElementId> excluse = new List<ElementId>();
            excluse.Add(beam.Id);

            ExclusionFilter BeamFilter = new ExclusionFilter(excluse);
            ElementCategoryFilter Col_filter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns);

            List<ElementFilter> filters = new List<ElementFilter>();
            filters.Add(Col_filter);
            filters.Add(BeamFilter);

            LogicalOrFilter filter = new LogicalOrFilter(filters);
            ReferenceIntersector Col_Face_Ref_IntSector = new ReferenceIntersector(filter, FindReferenceTarget.Face, view3D);

            IList<ReferenceWithContext> Col_Face_RWC_St = Col_Face_Ref_IntSector.Find(topPoint, direction);
            List<ReferenceWithContext> filteredRefs_St = Col_Face_RWC_St.OrderBy(a => a.Proximity).ToList();

            XYZ intersecPoint = filteredRefs_St[0].GetReference().GlobalPoint;

            return intersecPoint;
        }

        public static View3D Get3DView(Document doc)
        {
            View3D view3D = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .Where(v => v.IsTemplate == false)
                .First();

            return view3D;
        }

        public static Line GetLocationCurveColumn(XYZ orgin)
        {
            return Line.CreateUnbound(orgin, new XYZ(0, 0, 1));
        }
        public static Plane GetPlaneBeam(Element beam)
        {
            FamilyInstance beam1 = beam as FamilyInstance;

            LocationCurve lc1 = beam1.Location as LocationCurve;

            if (lc1 == null)
                return null;

            Curve c1 = lc1.Curve;

            // Beam axis luôn là Line
            if (!(c1 is Line))
                return null;

            Line l1 = c1 as Line;

            //MessageBox.Show((z*304.8).ToString());
            // =========================
            // 2️⃣ Tạo line vô hạn
            // =========================

            Line line1 = Line.CreateUnbound(
                l1.GetEndPoint(0) ,
                (l1.GetEndPoint(1) - l1.GetEndPoint(0)).Normalize());

            // =========================
            // 3️⃣ Tạo plane chứa line1
            // =========================
            XYZ dir1 = line1.Direction;
            XYZ origin = line1.Origin;

            XYZ dir2 = dir1.CrossProduct(XYZ.BasisZ).Normalize();
            XYZ normal = dir1.CrossProduct(dir2).Normalize();
            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);

            return plane;
        }
        public static Solid GetSolidElement(Element element)
        {
            Options options = new Options
            {
                ComputeReferences = true,
               
            };

            List<Solid> solids = new List<Solid>();

            GeometryElement geometry = element.get_Geometry(options);

            foreach (GeometryObject obj in geometry)
            {
                if (obj is Solid solid && solid.Volume > 1e-6)
                {
                    solids.Add(solid);
                }
                else if (obj is GeometryInstance instance)
                {
                    GeometryElement symbolGeo = instance.GetInstanceGeometry();

                    foreach (GeometryObject obj2 in symbolGeo)
                    {
                        if (obj2 is Solid solid2 && solid2.Volume > 1e-6)
                        {
                            solids.Add(solid2);
                        }
                    }
                }
            }

            return solids
                .OrderByDescending(x => x.Volume)
                .FirstOrDefault();
        }
        public static List<PlanarFace> GetPlanrFacePerpendicular(Solid solid, XYZ vector)
        {
            XYZ dir = vector.Normalize();
            //MessageBox.Show(dir.ToString());
            List<PlanarFace> faces = new List<PlanarFace>();

            foreach (Face face in solid.Faces)
            {
                if (!(face is PlanarFace pf))
                    continue;
                //MessageBox.Show(pf.FaceNormal.Normalize().ToString());
                double dot = Math.Abs(dir.DotProduct(pf.FaceNormal.Normalize()));
                //MessageBox.Show(dot.ToString());
                if (dot > 0.999999)
                {
                    faces.Add(pf);
                }
            }

            return faces
                .OrderBy(f => f.Origin.DotProduct(dir))
                .ToList();
        }
    }
}
#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Transform = Autodesk.Revit.DB.Transform;
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

        public static bool IsBeamIntersectSupport(Element support, Element beam)
        {
            // 2. Lấy Solid gốc của Dầm
            Options geoOptions = new Options { DetailLevel = ViewDetailLevel.Fine };
            Solid beamSolid = GetElementSolid(beam, geoOptions);

            if (beamSolid == null)
            {
                TaskDialog.Show("Lỗi", "Không lấy được hình học của Dầm.");
            }

            // 3. Tạo Solid mở rộng CHÍNH XÁC theo hình dạng cột (Ví dụ mở rộng lên 300mm)
            double extensionInFeet = 300.0 / 304.8;
            Solid extendedColumnSolid = CreateExactExtendedSolidFromColumn(support, extensionInFeet, geoOptions);

            if (extendedColumnSolid == null)
            {
                TaskDialog.Show("Lỗi", "Không thể tạo khối Solid mở rộng cho Cột.");
            }

            // 4. Tính toán phần giao bằng Boolean Operation
            bool isIntersect = false;
            try
            {
                Solid intersectionSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                    extendedColumnSolid, beamSolid, BooleanOperationsType.Intersect);

                if (intersectionSolid != null && intersectionSolid.Volume > 0.00001)
                {
                    isIntersect = true;
                }
            }
            catch
            {
                // Dự phòng nếu lỗi hình học phức tạp
                isIntersect = false;
            }

            // 5. Kết quả
            if (isIntersect)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        // Hàm tạo Solid mở rộng CHÍNH XÁC từ hình dáng thực tế của Cột
        private static Solid CreateExactExtendedSolidFromColumn(Element column, double heightExtension, Options opts)
        {
            // 1. Lấy Solid gốc, thực tế của cột (Giữ nguyên hình tròn, chữ L, góc xoay...)
            Solid originalSolid = GetElementSolid(column, opts);
            if (originalSolid == null) return null;

            try
            {
                // 2. Tạo một phép dịch chuyển (Transform) theo trục Z thẳng đứng lên trên
                Transform transform = Transform.CreateTranslation(new XYZ(0, 0, heightExtension));

                // 3. Biến đổi khối Solid gốc dịch lên phía trên theo khoảng cách chỉ định
                Solid movedSolid = SolidUtils.CreateTransformed(originalSolid, transform);

                // 4. Hợp nhất (Union) Khối gốc và Khối đã dịch chuyển lại với nhau
                // Kết quả sẽ là một khối Solid kéo dài từ đáy cột cũ lên đến đỉnh cột mới một cách chính xác tuyệt đối
                Solid exactExtendedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                    originalSolid, movedSolid, BooleanOperationsType.Union);

                return exactExtendedSolid;
            }
            catch
            {
                // Dự phòng nếu lỗi hình học quá phức tạp, trả về solid gốc
                return originalSolid;
            }
        }

        // Hàm phụ tách lấy Solid từ Element (xử lý cả trường hợp GeometryInstance)
        private static Solid GetElementSolid(Element elem, Options opts)
        {
            GeometryElement geoElem = elem.get_Geometry(opts);
            if (geoElem == null) return null;

            foreach (GeometryObject geoObj in geoElem)
            {
                if (geoObj is Solid solid && solid.Volume > 5) return solid;

                if (geoObj is GeometryInstance geoInst)
                {
                    foreach (GeometryObject instObj in geoInst.GetInstanceGeometry())
                    {
                        if (instObj is Solid instSolid && instSolid.Volume > 0) return instSolid;
                    }
                }
            }
            return null;
        }

    }
}
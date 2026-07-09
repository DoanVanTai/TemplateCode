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
    public class BeamColumnModel
    {
        public Element beam { get; set; }
        public bool isIntersection { get; set; }
        public XYZ columnDirection { get; set; }
        public double anpha { get; set; }
        public bool isStartPoint { get; set; }
        public ColumnModel columnModel { get; set; }
        public BeamModel beamModel { get; set; }
        public BeamColumnModel(Document doc, Element column , Element Beam)
        {
            beam = Beam;

            columnModel = new ColumnModel(doc, column);
            
            beamModel = new BeamModel(doc, beam);

            XYZ pointColumn = new XYZ(columnModel.locationPointBot.X, columnModel.locationPointBot.Y, beamModel.startPoint.Z);
           
            columnDirection = null;
            isIntersection = IsBeamIntersectColumn(column, Beam);
           // MessageBox.Show(isIntersection.ToString());
            if (isIntersection)
            {
                GetColumnDirection(columnModel.direction1, columnModel.direction2, beamModel.direction);
                NearestPoint(doc, columnModel, beamModel);
            }

        }

        public bool IsBeamIntersectColumn(Element column, Element Beam)
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
            Solid extendedColumnSolid = CreateExactExtendedSolidFromColumn(column, extensionInFeet, geoOptions);

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
        private Solid CreateExactExtendedSolidFromColumn(Element column, double heightExtension, Options opts)
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
        private Solid GetElementSolid(Element elem, Options opts)
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

        public  bool NearestPoint(Document doc, ColumnModel columnModel, BeamModel beamModel)
        {
            XYZ pointColumn = new XYZ(columnModel.locationPointBot.X, columnModel.locationPointBot.Y, beamModel.startPoint.Z);
          
            if (pointColumn.DistanceTo(beamModel.startPoint) < pointColumn.DistanceTo(beamModel.endPoint))
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


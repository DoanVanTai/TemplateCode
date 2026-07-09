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
    public class ColumnModel
    {
        public Element column { get; set; }
        public string typeName { get; set; }

        public double width { get; set; }
        public double depth { get; set; }

        public XYZ locationPointTop { get; set; }
        public XYZ locationPointBot { get; set; }
        public XYZ direction1 { get; set; }
        public XYZ direction2 { get; set; }
        public double rotation { get; set; }

        public double topColumn { get; set; }
        public Plane plane { get; set; }
        public ColumnModel(Document doc, Element e)
        {
            column = e;
            typeName = e.Name;

            Element eType = doc.GetElement(e.GetTypeId());

            var para = eType.LookupParameter("Family Name");
            if (para != null)
            {
                var familyName = para.AsString();

                if (!GetFamilyName.ColumnRec.Contains(familyName))
                {
                    ReportUtils.ShowDialogError("ERROR", "KHÔNG ĐÚNG FAMILY CỘT", "CHỌN ĐÚNG FAMILY CỘT");
                    throw new InvalidOperationException("KHÔNG ĐÚNG FAMILY CỘT");
                }
            }

            width = eType.LookupParameter(GetParameterColumnRec.C_Width).AsDouble();
            depth = eType.LookupParameter(GetParameterColumnRec.C_Depth).AsDouble();
           
            GetPointCenter(doc, e);

            direction1 = (column as FamilyInstance).HandOrientation;
            direction2 = (column as FamilyInstance).FacingOrientation;
            // Plane normal
            XYZ normal = direction1.CrossProduct(XYZ.BasisZ).Normalize();
            plane = Plane.CreateByNormalAndOrigin(normal, locationPointBot);

        }

        public void GetPointCenter(Document doc, Element column)
        {
            // Lấy điểm gốc từ LocationPoint:
            LocationPoint locPoint = column.Location as LocationPoint;
            XYZ basePoint = locPoint.Point;
            rotation = locPoint.Rotation;
            // Lấy thông tin Level của cột:
            // Lấy Base Level & Offset
            Level baseLevel = doc.GetElement(column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM)?.AsElementId() ?? ElementId.InvalidElementId) as Level;
            Level topLevel = doc.GetElement(column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM)?.AsElementId() ?? ElementId.InvalidElementId) as Level;

            double baseElevation = baseLevel.Elevation;
            double topElevation = topLevel.Elevation;

            double baseOffset = column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();
            double topOffset = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();

            // Tính lại tọa độ Z thực sự của đáy cột:
            topColumn = topElevation + topOffset;
            locationPointTop = new XYZ(basePoint.X, basePoint.Y, topColumn);

            locationPointBot = basePoint;
        }
    }
}


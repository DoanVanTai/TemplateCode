using Autodesk.Revit.Creation;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using Document = Autodesk.Revit.DB.Document;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DVTools
{
    public class NodeModel
    {
        public Document doc { get; set; }
        public List<ColumnBeamModel> columnBeams { get; set; } = new List<ColumnBeamModel>();
        public List<WallBeamModel> wallBeams { get; set; } = new List<WallBeamModel>();
        public bool isBeamWall { get; set; }
        public bool isBeamPillar { get; set; }
        public bool isBeamBeamInline { get; set; }
        public bool isBeamBeamPerpendicular { get; set; }
        public DataView dataView { get; set; }

        public NodeModel(Document Doc, Element support, List<Element> beams, DataView DataView)
        {
            doc = Doc;

            dataView = DataView;

            foreach (Element beam in beams)
            {
                if (IsColumn(support))
                {
                    //column = Support;
                    var columnBeam = new ColumnBeamModel(doc, support, beam);
                    if (columnBeam.isIntersection)
                    {
                        columnBeams.Add(columnBeam);
                    }
                }
                else
                {
                    //wall = Support;
                    var wallBeam = new WallBeamModel(doc, support, beam);
                    if (wallBeam.isIntersection)
                    {
                        wallBeams.Add(wallBeam);
                    }
                }
            }

            if (wallBeams.Count > 0)
            {
                for (int i = 0; i < wallBeams.Count; i++)
                {
                    XYZ vec = wallBeams[i].vec;
                    XYZ intersectionPoint = wallBeams[i].intersectionPoint;

                    if (wallBeams[i].IsWallBeamPer)
                    {
                        using (Transaction trans = new Transaction(Doc, "Offset FamilyInstance"))
                        {
                            trans.Start();
                            FailureHandlingOptions option = trans.GetFailureHandlingOptions();
                            option.SetFailuresPreprocessor(new DeleteWarningSuper());
                            trans.SetFailureHandlingOptions(option);

                            XYZ startPoint = wallBeams[i].beamModel.startPoint;
                            XYZ endPoint = wallBeams[i].beamModel.endPoint;
                            XYZ newPoint = intersectionPoint + vec * (dataView.beamWall / 304.8 - wallBeams[i].wallModel.width / 2);
                            if (wallBeams[i].beamModel.beamT)
                            {
                                if (wallBeams[i].isStartPoint)
                                {
                                    CreateNewCurve(wallBeams[i].beam, endPoint, newPoint);
                                }
                                else
                                {
                                    CreateNewCurve(wallBeams[i].beam, newPoint, startPoint);
                                }
                            }
                            else                         
                            {
                                if (wallBeams[i].isStartPoint)
                                {
                                    CreateNewCurve(wallBeams[i].beam, newPoint, endPoint);
                                }
                                else
                                {
                                    CreateNewCurve(wallBeams[i].beam, startPoint, newPoint);
                                }
                            }

                            doc.Regenerate();
                            trans.Commit();
                        }
                    }

                }
            }
            if (columnBeams.Count > 0)
            {
                TypeBeams(doc, support, columnBeams);

                for (int i = 0; i < columnBeams.Count; i++)
                {
                    XYZ vec = columnBeams[i].vec;
                    XYZ intersectionPoint = columnBeams[i].intersectionPoint;

                    if (columnBeams[i].IsColumnBeamPer)
                    {
                        using (Transaction trans = new Transaction(Doc, "Create New Curve"))
                        {
                            trans.Start();
                            FailureHandlingOptions option = trans.GetFailureHandlingOptions();
                            option.SetFailuresPreprocessor(new DeleteWarningSuper());
                            trans.SetFailureHandlingOptions(option);

                            XYZ startPoint = columnBeams[i].beamModel.startPoint;
                            XYZ endPoint = columnBeams[i].beamModel.endPoint;

                            if (columnBeams[i].beamModel.beamT)
                            {
                                if (isBeamPillar)
                                {
                                    XYZ newPoint = intersectionPoint + vec * (dataView.beamPillar / 304.8 - columnBeams[i].width / 2);

                                    if (columnBeams[i].isStartPoint)
                                    {
                                        CreateNewCurve(columnBeams[i].beam, endPoint, newPoint);
                                    }
                                    else
                                    {
                                        CreateNewCurve(columnBeams[i].beam, newPoint, startPoint);
                                    }

                                }
                                else if (isBeamBeamInline)
                                {
                                    XYZ newPoint = intersectionPoint + vec * 0.5 * dataView.beamBeamInline / 304.8;
                                    if (columnBeams[i].isStartPoint)
                                    {
                                        CreateNewCurve(columnBeams[i].beam, endPoint, newPoint);
                                    }
                                    else
                                    {
                                        CreateNewCurve(columnBeams[i].beam, newPoint, startPoint);
                                    }
                                }
                            }
                            else
                            {
                                if (isBeamPillar)
                                {
                                    XYZ newPoint = intersectionPoint + vec * (dataView.beamPillar / 304.8 - columnBeams[i].width / 2);

                                    if (columnBeams[i].isStartPoint)
                                    {
                                        CreateNewCurve(columnBeams[i].beam, newPoint, endPoint);
                                    }
                                    else
                                    {
                                        CreateNewCurve(columnBeams[i].beam, startPoint, newPoint);
                                    }

                                }
                                else if (isBeamBeamInline)
                                {
                                    XYZ newPoint = intersectionPoint + vec * 0.5 * dataView.beamBeamInline / 304.8;
                                    if (columnBeams[i].isStartPoint)
                                    {
                                        CreateNewCurve(columnBeams[i].beam, newPoint, endPoint);
                                    }
                                    else
                                    {
                                        CreateNewCurve(columnBeams[i].beam, startPoint, newPoint);
                                    }
                                }
                            }

                            

                            doc.Regenerate();
                            trans.Commit();
                        }
                    }
                    else
                    {
                        double angle = columnBeams[i].columnDirection.Normalize().AngleTo(vec);

                        double w = columnBeams[i].beamModel.B_Width + 200 / 304.8;

                        XYZ p1 = intersectionPoint + vec * (0.5 * dataView.beamBeamInline / Math.Sin(angle)) / 304.8 + columnBeams[i].columnDirection.Normalize() * w / 2;
                        XYZ p2 = intersectionPoint + vec * (0.5 * dataView.beamBeamInline / Math.Sin(angle)) / 304.8 - columnBeams[i].columnDirection.Normalize() * w / 2;
                        XYZ p3 = p1 - vec * 500 / 304.8;
                        XYZ p4 = p2 - vec * 500 / 304.8;

                        CurveArray profile = new CurveArray();

                        profile.Append(Line.CreateBound(p1, p3));
                        profile.Append(Line.CreateBound(p3, p4));
                        profile.Append(Line.CreateBound(p4, p2));
                        profile.Append(Line.CreateBound(p2, p1));

                        using (Transaction trans = new Transaction(doc, "Opening"))
                        {
                            trans.Start();
                            FailureHandlingOptions option = trans.GetFailureHandlingOptions();
                            option.SetFailuresPreprocessor(new DeleteWarningSuper());
                            trans.SetFailureHandlingOptions(option);

                            Opening opening = doc.Create.NewOpening(columnBeams[i].beam, profile, eRefFace.CenterZ);

                            doc.Regenerate();
                            trans.Commit();
                        }
                    }

                }
            }
        }
        public void CreateNewCurve(Element beam, XYZ point1, XYZ point2)
        {
            LocationCurve locationCurve = beam.Location as LocationCurve;
            Line newLine = Line.CreateBound(point1, point2);
            locationCurve.Curve = newLine;
        }

        public bool TypeBeams(Document doc, Element column, List<ColumnBeamModel> beams)
        {
            if (beams.Count == 1)
            {
                return isBeamPillar = true;
            }
            else if (beams.Count == 2)
            {
                return isBeamBeamInline = true;
            }
            else if (beams.Count == 3)
            {
                return isBeamBeamInline = true;
            }
            else
            {

                return false;
            }
        }
        public static bool IsColumn(Element element)
        {
            return element?.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns;
        }
    }
}


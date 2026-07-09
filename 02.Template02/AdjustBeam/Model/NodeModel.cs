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
        public Element column { get; set; }
        public List<BeamColumnModel> beams { get; set; } = new List<BeamColumnModel>();
        public List<BeamWallModel> beamWalls { get; set; } = new List<BeamWallModel>();
        public Plane plane { get; set; }
        public bool isBeamWall { get; set; }
        public bool isBeamPillar { get; set; }
        public bool isBeamBeamInline { get; set; }
        public bool isBeamBeamPerpendicular { get; set; }
        public XYZ columnDirection { get; set; }
        public DataView dataView { get; set; }
        public int isbeam { get; set; }
        public double distance { get; set; }
        public NodeModel(Document Doc, Element Support, List<Element> Beams, DataView DataView)
        {
            doc = Doc;
            column = Support;
            dataView = DataView;

            foreach (Element beam in Beams)
            {
                if (IsColumn(Support))
                {
                    //MessageBox.Show(1.ToString());
                    column = Support;

                    var beamColumn = new BeamColumnModel(doc, column, beam);

                    if (beamColumn.isIntersection)
                    {
                        beams.Add(beamColumn);
                    }
                }
                else
                {
                    //MessageBox.Show(2.ToString());
                    var beamWall = new BeamWallModel(doc, column, beam);
                    if (beamWall.isIntersection)
                    {
                        beamWalls.Add(beamWall);
                    }
                }

            }
           
            if (beamWalls.Count > 0)
            {

                for (int i = 0; i < beamWalls.Count; i++)
                {

                    XYZ intersectionPoint = beamWalls[i].intersectionPoint;

                    var IsPer = Utils.IsPerpendicular(beamWalls[i].columnDirection, beamWalls[i].beamModel.line.Direction);

                    //MessageBox.Show(beamWalls.Count.ToString());

                    if (IsPer)
                    {
                        using (Transaction trans = new Transaction(Doc, "Offset FamilyInstance"))
                        {
                            trans.Start();
                            FailureHandlingOptions option = trans.GetFailureHandlingOptions();
                            option.SetFailuresPreprocessor(new DeleteWarningSuper());
                            trans.SetFailureHandlingOptions(option);

                            if (beamWalls[i].isStartPoint)
                            {
                                XYZ vec = (beamWalls[i].beamModel.endPoint - intersectionPoint).Normalize();
                                CreateNewCurve(beamWalls[i].beam, intersectionPoint + vec * (dataView.beamWall / 304.8 - beamWalls[i].wallModel.width/2), beamWalls[i].beamModel.endPoint);
                            }
                            else
                            {
                                XYZ vec = (beamWalls[i].beamModel.startPoint - intersectionPoint).Normalize();
                                CreateNewCurve(beamWalls[i].beam, beamWalls[i].beamModel.startPoint, intersectionPoint + vec * (dataView.beamWall / 304.8 - beamWalls[i].wallModel.width / 2));
                            }

                            doc.Regenerate();
                            trans.Commit();
                        }
                    }


                }
            }
            if (beams.Count > 0)
            {
                CheckBeam(doc, column, beams);

                for (int i = 0; i < beams.Count; i++)
                {
                    XYZ normal = beams[i].columnDirection.CrossProduct(XYZ.BasisZ).Normalize();
                    plane = Plane.CreateByNormalAndOrigin(normal, beams[0].columnModel.locationPointBot);

                    XYZ intersectionPoint = Utils.IntersectLinePlane(beams[i].beamModel.line, plane);
                  
                    var IsPer = Utils.IsPerpendicular(beams[i].columnDirection, beams[i].beamModel.line.Direction);
                   
                    //MessageBox.Show(IsPer.ToString());


                    if (IsPer)
                    {
                        using (Transaction trans = new Transaction(Doc, "Offset FamilyInstance"))
                        {
                            trans.Start();
                            FailureHandlingOptions option = trans.GetFailureHandlingOptions();
                            option.SetFailuresPreprocessor(new DeleteWarningSuper());
                            trans.SetFailureHandlingOptions(option);

                            if (isBeamBeamInline)
                            {
                                if (beams[i].isStartPoint)
                                {
                                    XYZ vec = (beams[i].beamModel.endPoint - intersectionPoint).Normalize();

                                    CreateNewCurve(beams[i].beam, beams[i].beamModel.endPoint, intersectionPoint + vec * 0.5 * dataView.beamBeamInline / 304.8);
                                }
                                else
                                {
                                    XYZ vec = (beams[i].beamModel.startPoint - intersectionPoint).Normalize();

                                    CreateNewCurve(beams[i].beam, intersectionPoint + vec * 0.5 * dataView.beamBeamInline / 304.8, beams[i].beamModel.startPoint);
                                }
                            }
                            else if (isBeamPillar)
                            {
                                if (beams[i].isStartPoint)
                                {

                                    XYZ vec = (beams[i].beamModel.endPoint - intersectionPoint).Normalize();

                                    CreateNewCurve(beams[i].beam, beams[i].beamModel.endPoint, intersectionPoint + vec * (dataView.beamPillar / 304.8 - distance / 2));
                                }
                                else
                                {
                                    XYZ vec = (beams[i].beamModel.startPoint - intersectionPoint).Normalize();

                                    CreateNewCurve(beams[i].beam, intersectionPoint + vec * (dataView.beamPillar / 304.8 - distance / 2), beams[i].beamModel.startPoint);
                                }
                            }

                            doc.Regenerate();
                            trans.Commit();
                        }
                    }
                    else
                    {
                        XYZ vec;
                        if (beams[i].isStartPoint)
                        {
                            vec = (beams[i].beamModel.endPoint - intersectionPoint).Normalize();
                        }
                        else
                        {
                            vec = (beams[i].beamModel.startPoint - intersectionPoint).Normalize();
                        }

                        double angle = beams[i].columnDirection.Normalize().AngleTo(vec);


                        double w = beams[i].beamModel.B_Width + beams[i].beamModel.B_WidthLedge1 + beams[i].beamModel.B_WidthLedge2;

                        XYZ p1 = intersectionPoint + vec * (0.5 * dataView.beamBeamInline / Math.Sin(angle)) / 304.8 + beams[i].columnDirection.Normalize() * w / 2;
                        XYZ p2 = intersectionPoint + vec * (0.5 * dataView.beamBeamInline / Math.Sin(angle)) / 304.8 - beams[i].columnDirection.Normalize() * w / 2;
                        XYZ p3 = p1 - vec * 400 / 304.8;
                        XYZ p4 = p2 - vec * 400 / 304.8;

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

                            Opening opening = doc.Create.NewOpening(beams[i].beam, profile, eRefFace.CenterZ);

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

        public bool CheckBeam(Document doc, Element column, List<BeamColumnModel> beams)
        {
            if (beams.Count == 1)
            {

                columnDirection = beams[0].columnDirection.Normalize();

                XYZ normal = columnDirection.CrossProduct(XYZ.BasisZ).Normalize();

                Solid s = Utils.GetSolidElement(column);
                List<PlanarFace> faces = Utils.GetPlanrFacePerpendicular(s, normal);

                // MessageBox.Show(faces.Count.ToString());
                if (faces.Count == 2)
                {
                    XYZ p1 = faces[0].Origin;
                    XYZ p2 = faces[1].Origin;

                    distance = Math.Abs((p2 - p1).DotProduct(normal.Normalize()));

                    //TaskDialog.Show("Distance", (distance*304.8).ToString());
                }

                return isBeamPillar = true;
            }
            else if (beams.Count == 2)
            {
                if (beams[0].anpha < beams[1].anpha)
                {
                    columnDirection = beams[0].columnDirection;
                }
                else
                {
                    columnDirection = beams[1].columnDirection;
                }
                return isBeamBeamInline = true;
            }
            else if (beams.Count == 3)
            {
                //if (beams[0].anpha < beams[1].anpha)
                //{
                //    columnDirection = beams[0].columnDirection;
                //}
                //else
                //{
                //    columnDirection = beams[1].columnDirection;
                //}
                return isBeamBeamInline = true;
            }
            else
            {
                // MessageBox.Show(2.ToString());
                return false;
            }
        }
        public static bool IsColumn(Element element)
        {
            return element?.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns;
        }
    }
}


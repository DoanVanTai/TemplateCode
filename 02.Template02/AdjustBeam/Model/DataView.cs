using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DVTools.Library;


namespace DVTools
{
    public class DataView : ViewModelBase
    {
        private double _beamWall { get; set; }
        public double beamWall {  get => _beamWall;  set { _beamWall = value;  OnPropertyChanged(); }  }
        private double _beamPillar { get; set; }
        public double beamPillar { get => _beamPillar; set { _beamPillar = value; OnPropertyChanged(); } }
        private double _beamBeamInline { get; set; }
        public double beamBeamInline { get => _beamBeamInline; set { _beamBeamInline = value; OnPropertyChanged(); } }
        private double _beamBeamPerpendicular { get; set; }
        public double beamBeamPerpendicular { get => _beamBeamPerpendicular; set { _beamBeamPerpendicular = value; OnPropertyChanged(); } }
        private bool _isCornerPillar { get; set; }
        public bool isCornerPillar { get => _isCornerPillar; set { _isCornerPillar = value; OnPropertyChanged(); } }
        private bool _isCornerWall { get; set; }
        public bool isCornerWall { get => _isCornerWall; set { _isCornerWall = value; OnPropertyChanged(); } }
        public DataView(Document Doc)
        {
            beamWall = 20;
            beamPillar = 20;
            beamBeamInline = 20;
            beamBeamPerpendicular = 20;
            isCornerPillar = true;
            isCornerWall = true;
        }
        
    }
}
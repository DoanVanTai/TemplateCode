#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DVTools.Library;
using Newtonsoft.Json.Linq;
using QHTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ProgressBar = System.Windows.Controls.ProgressBar;

#endregion

namespace DVTools
{
    public class AdjustBeamViewModel : ViewModelBase
    {
        public UIDocument UiDoc;
        public Document Doc;

        private DataView _dataView;
        public DataView dataView { get => _dataView; set { _dataView = value; OnPropertyChanged(); } }
        public double value { get; set; }
        private double _percent;
        public double Percent { get => _percent; set { _percent = value; OnPropertyChanged(); } }

        public List<Element> node = new List<Element>();


        #region Command

        public ICommand OKCommand { get; set; }
      

        #endregion
        public AdjustBeamViewModel(UIDocument uiDoc)
        {
            UiDoc = uiDoc;
            Doc = UiDoc.Document;

            dataView = new DataView(Doc);
            SelectedElements selectedElements = new SelectedElements(UiDoc);
            OKCommand = new RelayCommand<AdjustBeamWindow>((p) => { return true; }, (p) =>
            {
                List<NodeModel> nodeModels = new List<NodeModel>();

                p.ProgressWindow.Maximum = selectedElements.supports.Count;

                foreach (Element support in selectedElements.supports)
                {
                    NodeModel nodeModel = new NodeModel(Doc, support, selectedElements.beams, dataView);
                    SetValue(p.ProgressWindow, 1);
                }
                p.DialogResult = true;
                p.Close();

            });

        }
        private void SetValue(ProgressBar ProgressWindow, int n)
        {
            value += n;
            Percent = value / ProgressWindow.Maximum * 100;
            ProgressWindow.Dispatcher.Invoke(() => ProgressWindow.Value = value, DispatcherPriority.Background);
        }
    }
}
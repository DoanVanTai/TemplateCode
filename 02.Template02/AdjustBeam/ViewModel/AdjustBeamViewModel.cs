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
using System.Xml.Linq;


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

            OKCommand = new RelayCommand<AdjustBeamWindow>((p) => { return true; }, (p) =>
            {
                p.DialogResult = true;
                p.Close();

                SelectedElement selectedElement = new SelectedElement(UiDoc);

                List<NodeModel> nodeModels = new List<NodeModel>();

                //MessageBox.Show(selectedElement.columns.Count.ToString());
              
                foreach (Element support in selectedElement.supports)
                {
                    NodeModel nodeModel = new NodeModel(Doc, support, selectedElement.beams, dataView);

                    if (nodeModel.beams.Count > 0)
                    {
                        nodeModels.Add(nodeModel);
                    }
                }

             
                //foreach (NodeModel node in nodeModels)
                //{
                    
                //}

            });

        }
        
    }
}
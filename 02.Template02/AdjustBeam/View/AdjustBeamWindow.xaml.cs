
using System.Windows;
using System.Windows.Controls;


namespace DVTools
{
    public partial class AdjustBeamWindow
    {
        private AdjustBeamViewModel _viewModel;
        public AdjustBeamWindow(AdjustBeamViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
           
        }
    }
}
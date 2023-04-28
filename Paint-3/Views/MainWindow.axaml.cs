using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Figurator.ViewModels;

namespace Figurator.Views {
    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
        }

        /* private void CanvasTap(object sender, RoutedEventArgs e) {
            var mwvm = (MainWindowViewModel?) DataContext;
            if (mwvm == null) return;

            var src = e.Source;
            if (src == null || src is not Shape @shape || @shape.Name == "marker") return;

            mwvm.ShapeTap(@shape.Name ?? "");
        }*/
    }
}
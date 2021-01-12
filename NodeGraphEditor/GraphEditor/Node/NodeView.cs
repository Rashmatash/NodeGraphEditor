// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraphEditor.Editors
{
    public class NodeView : ContentControl
    {
        public Brush TitleBrush
        {
            get { return (Brush)GetValue(TitleBrushProperty); }
            set { SetValue(TitleBrushProperty, value); }
        }
        public static readonly DependencyProperty TitleBrushProperty =
            DependencyProperty.Register("TitleBrush", typeof(Brush), typeof(NodeView),
            new PropertyMetadata(new SolidColorBrush(Colors.White)));

        static NodeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeView),
                    new FrameworkPropertyMetadata(typeof(NodeView)));
        }

        public NodeView()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataContextChanged -= OnDataContextChanged;
            var vm = DataContext as Node;
            SizeChanged += (s, _) => vm?.OnPropertyChanged(nameof(vm.TopLeft));
        }
    }
}

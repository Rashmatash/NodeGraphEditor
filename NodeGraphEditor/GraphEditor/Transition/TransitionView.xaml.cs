// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraphEditor.Editors
{
    public class TransitionThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return double.Parse((string)parameter) / (double)value;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class TransitionView : UserControl
    {
        public TransitionView()
        {
            InitializeComponent();
        }
        private void RemoveTransition(object sender)
        {
            if ((sender is Path) && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                var vm = (sender as Path).DataContext as Transition;
                vm?.Input.RemoveTransitionTo(vm.Output);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RemoveTransition(sender);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemoveTransition(sender);
        }
    }
}

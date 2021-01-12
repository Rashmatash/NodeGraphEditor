// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using NodeGraphEditor.Helpers;
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
    public class FlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return (value is InputConnector) ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class NodeConnectorView : UserControl
    {
        public string ConnectorName
        {
            get { return (string)GetValue(ConnectorNameProperty); }
            set { SetValue(ConnectorNameProperty, value); }
        }
        public static readonly DependencyProperty ConnectorNameProperty =
            DependencyProperty.Register("ConnectorName", typeof(string), typeof(NodeConnectorView),
                new PropertyMetadata(""));

        public NodeConnectorView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            var node = this.FindVisualParent<NodeView>();
            if (node?.DataContext is Node nodeVM)
            {
                nodeVM.PropertyChanged += (s, _) =>
                {
                    if (_.PropertyName == nameof(nodeVM.TopLeft))
                    {
                        AdjustAnchorpoint(node, nodeVM);
                    }
                };

                node.SizeChanged += (s, _) => AdjustAnchorpoint(node, nodeVM);

                AdjustAnchorpoint(node, nodeVM);
            }
        }

        private void AdjustAnchorpoint(NodeView node, Node nodeVM)
        {
            center.X = socket.RenderSize.Width * 0.5;
            center.Y = socket.RenderSize.Height * 0.5;
            center = socket.TranslatePoint(center, this);

            var p = TranslatePoint(center, node);
            p.X += nodeVM.TopLeft.X;
            p.Y += nodeVM.TopLeft.Y;

            (DataContext as NodeConnector).AnchorPoint = p;

            // TODO: this is actually wrong!
            center.X = socket.RenderSize.Width * 0.5;
            center.Y = RenderSize.Height * 0.5;
        }

        private bool IsInput
        {
            get { return (DataContext is InputConnector); }
        }

        private bool IsOutput
        {
            get { return (DataContext is OutputConnector); }
        }

        private bool captured = false;
        private Point mousePos;
        private UIElement selectedElement = null;

        private readonly SolidColorBrush White = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush Red = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush Green =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));

        private Path toMouse = null;
        private Point center = new Point(0, 0);
        private double dpiX, dpiY;

        private void CreatePath()
        {
            toMouse = new Path()
            {
                Opacity = 0.5,
                Stroke = White,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            BezierSegment bezierSeg = new BezierSegment(
                new Point(center.X, center.Y),
                Mouse.GetPosition(this),
                Mouse.GetPosition(this), true);
            PathSegmentCollection segs = new PathSegmentCollection { bezierSeg };
            PathFigure fig = new PathFigure(center, segs, false);
            PathFigureCollection figures = new PathFigureCollection { fig };
            toMouse.Data = new PathGeometry(figures);
        }

        private void ConnectionMsg(bool on, string msg = "")
        {
            if (msg != "" || !on)
            {
                connectionLabel.Content = msg;
                connectionMsg.Visibility = (on) ? Visibility.Visible : Visibility.Collapsed;

                Canvas.SetTop(connectionMsg, -24);
                Panel.SetZIndex(connectionMsg, 10000);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                Mouse.Capture(element);
                captured = true;

                var source = PresentationSource.FromVisual(this);
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;

                mousePos = PointToScreen(e.GetPosition(this));
                var scale = 1.0 / (element.FindVisualParent<GraphEditorView>().
                    DataContext as GraphEditor).ScaleFactor;
                mousePos.X *= scale / dpiX;
                mousePos.Y *= scale / dpiY;
                selectedElement = element;
                e.Handled = true;

                CreatePath();
                canvas.Children.Add(toMouse);

                DragDrop.DoDragDrop(element, new DataObject(this), DragDropEffects.Move);
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                IDataObject data = e.Data;
                if (data.GetDataPresent(typeof(NodeConnectorView)))
                {
                    if (data.GetData(typeof(NodeConnectorView)) is NodeConnectorView connector &&
                        CanConnect(connector, out _))
                    {
                        if (IsInput && connector.IsOutput)
                        {
                            (DataContext as InputConnector).AddTransitionTo(
                                connector.DataContext as OutputConnector);
                        }
                        else if (IsOutput && connector.IsInput)
                        {
                            (connector.DataContext as InputConnector).AddTransitionTo(
                                DataContext as OutputConnector);
                        }
                    }
                    canConnectIndicator.Visibility = Visibility.Hidden;
                    connectorName.Visibility = Visibility.Collapsed;
                    ConnectionMsg(false);
                    e.Handled = true;
                }
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                IDataObject data = e.Data;
                if (data.GetDataPresent(typeof(NodeConnectorView)))
                {
                    if ((data.GetData(typeof(NodeConnectorView)) is NodeConnectorView connector) &&
                        (connector != this))
                    {
                        if (CanConnect(connector, out string reason))
                        {
                            canConnectIndicator.Background = Green;
                        }
                        else
                        {
                            canConnectIndicator.Background = Red;
                        }
                        canConnectIndicator.Visibility = Visibility.Visible;
                        connectorName.Visibility = Visibility.Visible;
                        ConnectionMsg(true, reason);
                    }
                }
            }
        }

        private bool CanConnect(NodeConnectorView connector, out string reason)
        {
            if ((IsInput && connector.IsInput) ||
                (IsOutput && connector.IsOutput))
            {
                reason = "Cannot connect two input connectors or two output connectors";
                return false;
            }

            var me = DataContext as NodeConnector;
            var her = connector.DataContext as NodeConnector;

            if (me.ParentNode == her.ParentNode)
            {
                // Eeewh! No incest!
                reason = "Cannot connect connectors on the same node";
                return false;
            }

            var myType = me.DataType;
            var herType = her.DataType;

            if (((myType == null) || (herType == null)) ||
                !(TypeConversion.CanConvert(myType, herType) &&
                TypeConversion.CanConvert(herType, myType)))
            {
                string output = (connector.IsOutput) ? herType.Name : myType.Name;
                string input = (this.IsInput) ? myType.Name : herType.Name;
                if (myType != typeof(object) && herType != typeof(object))
                {
                    reason = $"Cannot connect {output} to {input}";
                    return false;
                }
            }

            InputConnector vm = null;
            if (this.IsInput)
            {
                vm = DataContext as InputConnector;
            }
            else if (connector.IsInput)
            {
                vm = connector.DataContext as InputConnector;
            }

            if ((vm != null) && (vm.MaxConnections > 1) &&
                (vm.Transitions.Count >= vm.MaxConnections))
            {
                reason = "Reached maximum number of connections";
                return false;
            }

            reason = "";
            return true;
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                IDataObject data = e.Data;
                if (data.GetDataPresent(typeof(NodeConnectorView)))
                {
                    if ((data.GetData(typeof(NodeConnectorView)) is NodeConnectorView connector) &&
                        (connector != this))
                    {
                        canConnectIndicator.Visibility = Visibility.Hidden;
                        connectorName.Visibility = Visibility.Collapsed;
                        ConnectionMsg(false);
                    }
                }
            }
        }

        private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (captured && (selectedElement == element) && (toMouse != null))
            {
                BezierSegment seg =
                    ((PathGeometry)toMouse.Data).Figures[0].Segments[0] as BezierSegment;
                var scale = 1.0 / (element.FindVisualParent<GraphEditorView>().
                    DataContext as GraphEditor).ScaleFactor;
                var mouse = MouseHelper.GetCursor();
                mouse.X *= scale / dpiX;
                mouse.Y *= scale / dpiY;
                mouse.X -= mousePos.X - center.X;
                mouse.Y -= mousePos.Y - center.Y;
                var x = (center.X - mouse.X) * 0.5;
                seg.Point1 = new Point(center.X - x, center.Y);
                seg.Point2 = new Point(mouse.X + x, mouse.Y);
                seg.Point3 = mouse;
            }
        }

        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (captured && (e.KeyStates != DragDropKeyStates.LeftMouseButton))
            {
                canvas.Children.Clear();
                canConnectIndicator.Visibility = Visibility.Hidden;
                connectorName.Visibility = Visibility.Collapsed;
                Mouse.Capture(null);
                captured = false;
            }
        }
    }
}

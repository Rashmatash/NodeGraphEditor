// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using EditorDemo.MathEditor.Nodes;
using NodeGraphEditor.Editors;
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
using static System.Math;

namespace EditorDemo.MathEditor.Nodes
{
    public partial class VectorNodeView : UserControl
    {
        public VectorNodeView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                (DataContext as ConstVector2Node).Vector.PropertyChanged += SetRectangleColor;
                SetRectangleColor(null, new PropertyChangedEventArgs(nameof(OutputConnector.Value)));
            };
        }

        private void SetRectangleColor(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(OutputConnector.Value)) return;

            Color c;
            Vector4 v = new Vector4();
            if (DataContext is ConstVector4Node v4node)
            {
                v = v4node.Vector.GetValue<Vector4>();
            }
            else if (DataContext is ConstVector3Node v3node)
            {
                var v3 = v3node.Vector.GetValue<Vector3>();
                v = new Vector4(v3, 1.0f);
            }
            else if (DataContext is ConstVector2Node v2node)
            {
                var v2 = v2node.Vector.GetValue<Vector2>();
                v = new Vector4(v2, 0.0f, 1.0f);
            }

            v = NormalizeColor(v);

            c = new System.Windows.Media.Color()
            {
                R = (byte)(v.X * 255f),
                G = (byte)(v.Y * 255f),
                B = (byte)(v.Z * 255f),
                A = (byte)(v.W * 255f)
            };

            colorRect.Fill = new SolidColorBrush(c);
        }

        private Vector4 NormalizeColor(Vector4 v)
        {
            float x = Max(0.0f, v.X);
            float y = Max(0.0f, v.Y);
            float z = Max(0.0f, v.Z);
            float w = Min(Max(0.0f, v.W), 1.0f);

            float maxVal = Max(x, Max(y, z));
            float normalizer = 1.0f;

            if ((maxVal - 1.0) > 1e-5f)
            {
                normalizer /= maxVal;
            }

            return new Vector4(x * normalizer, y * normalizer, z * normalizer, w);
        }
    }
}

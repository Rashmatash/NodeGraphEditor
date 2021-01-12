// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Input;
using System.IO;
using NodeGraphEditor.Editors;

namespace EditorDemo.MathEditor.Nodes
{
    abstract class MathNode : Node { }

    [Description("Constant Float")]
    class ConstFloatNode : MathNode
    {
        protected OutputConnector _Constant;
        public OutputConnector Constant
        {
            get { return _Constant; }
            set
            {
                if (_Constant != value)
                {
                    _Constant = value;
                    OnPropertyChanged(nameof(Constant));
                }
            }
        }

        private float _X;
        public float X
        {
            get { return _X; }
            set
            {
                if (Math.Abs(_X - value) > 1e-5)
                {
                    _X = value;
                    SetVectorValue();
                }
            }
        }



        protected virtual void SetVectorValue()
        {
            Constant.Value = X;
            OnPropertyChanged(nameof(X));
        }

        public override List<InputConnector> GetInputs()
        {
            return new List<InputConnector>();
        }

        public override List<OutputConnector> GetOutputs()
        {
            return new List<OutputConnector>() { _Constant };
        }

        public override XmlNode ToXML(XmlDocument xmlDoc)
        {
            var element = base.ToXML(xmlDoc);

            var attribute = xmlDoc.CreateAttribute(nameof(X));
            attribute.Value = X.ToString();
            element.Attributes.Append(attribute);

            return element;
        }

        public override void FromXML(XmlElement node, GraphEditor graphEditor)
        {
            base.FromXML(node, graphEditor);

            try
            {
                X = float.Parse(node.GetAttribute(nameof(X)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: failed to deserialize node " + Name);
            }
        }

        public ConstFloatNode()
        {
            Name = "Constant";
            _Constant = new OutputConnector(this, typeof(float));
        }
    }

    [Description("Constant 2D-Vector")]
    class ConstVector2Node : MathNode
    {
        protected OutputConnector _Vector;
        public OutputConnector Vector
        {
            get { return _Vector; }
            set
            {
                if (_Vector != value)
                {
                    _Vector = value;
                    OnPropertyChanged(nameof(Vector));
                }
            }
        }

        private float _X;
        public float X
        {
            get { return _X; }
            set
            {
                if (Math.Abs(_X - value) > 1e-5f)
                {
                    _X = value;
                    SetVectorValue();
                }
            }
        }

        private float _Y;
        public float Y
        {
            get { return _Y; }
            set
            {
                if (Math.Abs(_Y - value) > 1e-5f)
                {
                    _Y = value;
                    SetVectorValue();
                }
            }
        }

        protected virtual void SetVectorValue()
        {
            Vector.Value = new Vector2(X, Y);
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        public override List<InputConnector> GetInputs()
        {
            return new List<InputConnector>();
        }

        public override List<OutputConnector> GetOutputs()
        {
            return new List<OutputConnector>() { _Vector };
        }

        public override XmlNode ToXML(XmlDocument xmlDoc)
        {
            var element = base.ToXML(xmlDoc);

            var attribute = xmlDoc.CreateAttribute(nameof(X));
            attribute.Value = X.ToString();
            element.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute(nameof(Y));
            attribute.Value = Y.ToString();
            element.Attributes.Append(attribute);
            return element;
        }

        public override void FromXML(XmlElement node, GraphEditor graphEditor)
        {
            base.FromXML(node, graphEditor);

            try
            {
                X = float.Parse(node.GetAttribute(nameof(X)));
                Y = float.Parse(node.GetAttribute(nameof(Y)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: failed to deserialize node " + Name);
            }
        }

        public ConstVector2Node()
        {
            Name = "2D Vector";
            _Vector = new OutputConnector(this, typeof(Vector2));
        }
    }

    [Description("Constant 3D-Vector")]
    class ConstVector3Node : ConstVector2Node
    {
        private float _Z;
        public float Z
        {
            get { return _Z; }
            set
            {
                if (Math.Abs(_Z - value) > 1e-5f)
                {
                    _Z = value;
                    SetVectorValue();
                }
            }
        }

        protected override void SetVectorValue()
        {
            Vector.Value = new Vector3(X, Y, Z);
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Z));
        }

        public override XmlNode ToXML(XmlDocument xmlDoc)
        {
            var element = base.ToXML(xmlDoc);

            var attribute = xmlDoc.CreateAttribute(nameof(Z));
            attribute.Value = Z.ToString();
            element.Attributes.Append(attribute);

            return element;
        }

        public override void FromXML(XmlElement node, GraphEditor graphEditor)
        {
            base.FromXML(node, graphEditor);

            try
            {
                Z = float.Parse(node.GetAttribute(nameof(Z)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: failed to deserialize node " + Name);
            }
        }

        public ConstVector3Node()
        {
            Name = "3D Vector";
            _Vector = new OutputConnector(this, typeof(Vector3));
        }
    }

    [Description("Constant 4D-Vector")]
    class ConstVector4Node : ConstVector3Node
    {
        private float _W;
        public float W
        {
            get { return _W; }
            set
            {
                if (Math.Abs(_W - value) > 1e-5f)
                {
                    _W = value;
                    SetVectorValue();
                }
            }
        }

        protected override void SetVectorValue()
        {
            Vector.Value = new Vector4(X, Y, Z, W);
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Z));
            OnPropertyChanged(nameof(W));
        }

        public override XmlNode ToXML(XmlDocument xmlDoc)
        {
            var element = base.ToXML(xmlDoc);

            var attribute = xmlDoc.CreateAttribute(nameof(W));
            attribute.Value = W.ToString();
            element.Attributes.Append(attribute);

            return element;
        }

        public override void FromXML(XmlElement node, GraphEditor graphEditor)
        {
            base.FromXML(node, graphEditor);

            try
            {
                W = float.Parse(node.GetAttribute(nameof(W)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: failed to deserialize node " + Name);
            }
        }

        public ConstVector4Node()
        {
            Name = "4D Vector";
            _Vector = new OutputConnector(this, typeof(Vector4));
        }
    }

    abstract class BasicMathOperationNode : MathNode
    {
        private InputConnector _Operand1;
        public InputConnector Operand1
        {
            get { return _Operand1; }
            set
            {
                if (_Operand1 != value)
                {
                    _Operand1 = value;
                    OnPropertyChanged(nameof(Operand1));
                }
            }
        }

        private InputConnector _Operand2;
        public InputConnector Operand2
        {
            get { return _Operand2; }
            set
            {
                if (_Operand2 != value)
                {
                    _Operand2 = value;
                    OnPropertyChanged(nameof(Operand2));
                }
            }
        }

        private OutputConnector _Result;
        public OutputConnector Result
        {
            get { return _Result; }
            set
            {
                if (_Result != value)
                {
                    _Result = value;
                    OnPropertyChanged(nameof(Result));
                }
            }
        }

        protected string Operator { get; set; } = "+";

        public override List<InputConnector> GetInputs()
        {
            return new List<InputConnector>() { _Operand1, _Operand2 };
        }

        public override List<OutputConnector> GetOutputs()
        {
            return new List<OutputConnector>() { _Result };
        }

        public BasicMathOperationNode()
        {
            _Operand1 = new InputConnector(this, typeof(object));
            _Operand2 = new InputConnector(this, typeof(object));

            _Result = new OutputConnector(this, typeof(object));

            _Operand1.ValueChanged += OnOperandValueChanged;
            _Operand2.ValueChanged += OnOperandValueChanged;
        }

        private void OnOperandValueChanged(object sender, EventArgs e)
        {
            if (Operand1.IsConnected && Operand2.IsConnected)
            {
                Type[] types = new Type[] { typeof(float), typeof(Vector2), typeof(Vector3), typeof(Vector4) };
                var index1 = -1;
                var index2 = -1;
                for (int i = 0; i < types.Length; ++i)
                {
                    if (Operand1.Transitions[0].Output.DataType == types[i])
                    {
                        index1 = i;
                        break;
                    }
                }

                for (int i = 0; i < types.Length; ++i)
                {
                    if (Operand2.Transitions[0].Output.DataType == types[i])
                    {
                        index2 = i;
                        break;
                    }
                }

                if (index1 != -1 || index2 != -1)
                {
                    var index = Math.Max(index1, index2);
                    Result.DataType = types[index];
                }
            }
            //Result.OnPropertyChanged(nameof(Result.Value));
        }
    }

    [Description("Add")]
    class AddNode : BasicMathOperationNode
    {
        public AddNode()
        {
            Name = "Add";
            Operator = "+";
        }
    }

    [Description("Subtract")]
    class SubtractNode : BasicMathOperationNode
    {
        public SubtractNode()
        {
            Name = "Subtract";
            Operator = "-";
        }
    }

    [Description("Multiply")]
    class MultiplyNode : BasicMathOperationNode
    {
        public MultiplyNode()
        {
            Name = "Multiply";
            Operator = "*";
        }
    }

    [Description("Divide")]
    class DivideNode : BasicMathOperationNode
    {
        public DivideNode()
        {
            Name = "Divide";
            Operator = "/";

            Operand1.ValueChanged += ValidateOperandType;
            Operand2.ValueChanged += ValidateOperandType;
        }

        private void ValidateOperandType(object sender, EventArgs e)
        {
            if (Operand1.IsConnected && Operand2.IsConnected &&
                Operand1.Transitions[0].Output.DataType != Operand2.Transitions[0].Output.DataType &&
                Operand2.Transitions[0].Output.DataType != typeof(float))
            {
                IsValid = false;
            }
            else
            {
                IsValid = true;
            }
        }
    }

    [Description("Linear Interpolation (Lerp)")]
    class LerpNode : MathNode
    {
        private InputConnector _A;
        public InputConnector A
        {
            get { return _A; }
            set
            {
                if (_A != value)
                {
                    _A = value;
                    OnPropertyChanged(nameof(A));
                }
            }
        }

        private InputConnector _B;
        public InputConnector B
        {
            get { return _B; }
            set
            {
                if (_B != value)
                {
                    _B = value;
                    OnPropertyChanged(nameof(B));
                }
            }
        }

        private InputConnector _Alpha;
        public InputConnector Alpha
        {
            get { return _Alpha; }
            set
            {
                if (_Alpha != value)
                {
                    _Alpha = value;
                    OnPropertyChanged(nameof(Alpha));
                }
            }
        }

        private OutputConnector _Result;
        public OutputConnector Result
        {
            get { return _Result; }
            set
            {
                if (_Result != value)
                {
                    _Result = value;
                    OnPropertyChanged(nameof(Result));
                }
            }
        }

        public override List<InputConnector> GetInputs()
        {
            return new List<InputConnector>() { _A, _B, _Alpha };
        }

        public override List<OutputConnector> GetOutputs()
        {
            return new List<OutputConnector>() { _Result };
        }

        public LerpNode()
        {
            Name = "Linear Interpolation";

            _A = new InputConnector(this, typeof(object));
            _B = new InputConnector(this, typeof(object));
            _Alpha = new InputConnector(this, typeof(float));

            _Result = new OutputConnector(this, typeof(object));

            _A.ValueChanged += ValdiateDataTypes;
            _B.ValueChanged += ValdiateDataTypes;
            _Alpha.ValueChanged += ValdiateDataTypes;
        }

        private void ValdiateDataTypes(object sender, EventArgs e)
        {
            if (A.IsConnected && B.IsConnected)
            {
                IsValid = (A.Transitions[0].Output.DataType == B.Transitions[0].Output.DataType);
                Result.DataType = A.Transitions[0].Output.DataType;
            }
            //Result.OnPropertyChanged(nameof(Result.Value));
        }
    }
}

// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using NodeGraphEditor;
using NodeGraphEditor.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorDemo.MathEditor
{
    class MathEditorViewModel : BaseViewModel
    {
        public GraphEditor GraphEditor { get; } = new GraphEditor();
    }
}

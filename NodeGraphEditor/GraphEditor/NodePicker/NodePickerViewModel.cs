// Copyright (c) Arash Khatami
// Distributed under the MIT license. See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NodeGraphEditor.Editors
{
    public class NodePickerViewModel : BaseViewModel
    {
        public ObservableCollection<Tuple<string, Type>> MaterialNodeTypes
        { get; } = new ObservableCollection<Tuple<string, Type>>();

        public CollectionViewSource FilteredNodes
        { get; } = new CollectionViewSource();

        private string _SearchPhrase = "";
        public string SearchPhrase
        {
            get { return _SearchPhrase; }
            set
            {
                if (_SearchPhrase != value)
                {
                    _SearchPhrase = value;
                    FilteredNodes.View.Refresh();
                    OnPropertyChanged(nameof(SearchPhrase));
                }
            }
        }

        public NodePickerViewModel(Type nodeType)
        {
            var types = nodeType.Assembly.GetTypes().Where(x => x.IsSubclassOf(nodeType));
            Debug.Assert(types != null && types.Count() > 0);

            foreach (var type in types)
            {
                var descriptions = (DescriptionAttribute[])type
                    .GetCustomAttributes(typeof(DescriptionAttribute), false);

                foreach (var desc in descriptions)
                {
                    MaterialNodeTypes.Add(new Tuple<string, Type>(desc.Description, type));
                }
            }

            FilteredNodes.Source = MaterialNodeTypes;
            FilteredNodes.Filter += NodeFilter;
        }

        private void NodeFilter(object sender, FilterEventArgs e)
        {
            var tuple = e.Item as Tuple<string, Type>;
            if (string.IsNullOrEmpty(SearchPhrase.Trim()))
            {
                e.Accepted = true;
            }
            else
            {
                var filter = SearchPhrase.ToLower().Trim();
                var nodeDescription = tuple.Item1.ToLower().Trim();
                bool accepted = false;
                char[] separators = { ';' };
                var subFilters = filter.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in subFilters)
                {
                    accepted |= nodeDescription.Contains(item.Trim());
                }

                e.Accepted = accepted;
            }
        }
    }
}

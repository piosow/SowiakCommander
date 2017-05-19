using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SowiakCommander
{
    public class DummyTreeViewItem : TreeViewItem
    {
        public DummyTreeViewItem()
            : base()
        {
            base.Header = "Dummy";
            base.Tag = "Dummy";
        }
    }
}

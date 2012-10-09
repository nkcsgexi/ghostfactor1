using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NLog;
using warnings.util;

namespace warnings.ui
{
    public partial class RefactoringWariningsForm : Form
    {
        private readonly Logger logger = NLoggerUtil.GetNLogger(typeof(RefactoringWariningsForm));

        public RefactoringWariningsForm()
        {
            InitializeComponent();
        }
     
        private void button1_Click_1(object sender, EventArgs e)
        {
            for (int  i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var removedItem = listView1.SelectedItems[i];
                listView1.Items.Remove(removedItem);   
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
       
        private ListViewItem CreateListViewItem(IEnumerable<string> messages, int imageIndex)
        {
            // Get the enumerator.
            var enumerator = messages.GetEnumerator();

            // If messages have the first item
            if (enumerator.MoveNext())
            {
                // Create an item leaded by the first element.
                var item = new ListViewItem(enumerator.Current);

                // Add subitems by reading the rest element. 
                for (; enumerator.MoveNext(); )
                {
                    item.SubItems.Add(enumerator.Current);
                }
                item.ImageIndex = imageIndex;
                return item;
            }
            return null;
        }


        public bool AddRefactoringWarning(IEnumerable<string> messages)
        {
            // Create a list view item by the given messages.
            var item = CreateListViewItem(messages, 0);
           
            // If the item is created, add to the list view.
            if (item != null)
            {
                listView1.Items.Add(item);
                listView1.Invalidate();
                return true;
            }
            return false;
        }

        /* If two list view item contains exactly same information. */
        private bool AreListViewItemsSame(ListViewItem item1, ListViewItem item2)
        {
            if (item1.Text.Equals(item2.Text))
            {
                if (item1.SubItems.Count == item2.SubItems.Count)
                {
                    for (int i = 0; i < item1.SubItems.Count; i++)
                    {
                        var s1 = item1.SubItems[i].Text;
                        var s2 = item2.SubItems[i].Text;
                        if (s1.Equals(s2))
                        {
                            if (i == item1.SubItems.Count - 1)
                            {
                                return true;
                            }
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return false;
        }


        public bool RemoveRefactoringWarning(IEnumerable<string> messages)
        {
            // Set the item to remove as null;
            ListViewItem removedItem = null;

            // Create an item by the given messages.
            var item = CreateListViewItem(messages, 0);

            // Search for the item that is equal to the created item.
            foreach (ListViewItem current in listView1.Items)
            {
                if (AreListViewItemsSame(current, item))
                {
                    removedItem = current;
                }
            }

            // If the removed item is found, remove it from the list and
            // return true, otherwise return false.
            if (removedItem != null)
            {
                listView1.Items.Remove(removedItem);
                logger.Info("removed");
                return true;
            }
            return false;
        }

        /* Invoked when double clicking a warning, shall redirect to where the problem is. */
        private void listView1_DoubleClicked(object sender, EventArgs e)
        {
            var selectedItems = listView1.SelectedItems;
            if(selectedItems.Count > 0)
            {
                
            }
        }
    }
}

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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Drawing;
using ImageViewer.Model;

namespace ImageViewer.View
{
    /// <summary>
    /// Interaction logic for FileExplorerView.xaml
    /// </summary>
    public partial class FileExplorerView : UserControl
    {

        public FileExplorerView()
        {
            InitializeComponent();
        }

        private void FileExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var drive in Directory.GetLogicalDrives())
            {
                var item = new TreeViewItemImage()
                {
                    Header = drive,
                    Tag = drive,
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/FolderIcon.png", UriKind.Absolute))
                };

                item.Items.Add(null);
                item.Expanded += Folder_Expanded;

                FolderTreeView.Items.Add(item);
            }
            
        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItemImage treeViewItem = (TreeViewItemImage)sender;
            if(treeViewItem.Items.Count != 1 || treeViewItem.Items[0] != null)
            {
                return;
            }

            treeViewItem.Items.Clear();
            string folderName = (string)treeViewItem.Tag;

            var directories = new List<string>();

            try
            {
                var dirs = Directory.GetDirectories(folderName);
                if(dirs.Length > 0)
                {
                    directories.AddRange(dirs);
                }
            }
            catch (Exception)
            {

                throw;
            }

            directories.ForEach(directoryPath =>
            {
                TreeViewItemImage subItem = new TreeViewItemImage()
                {
                    Header = GetPath(directoryPath),
                    Tag = directoryPath
                };

                subItem.Items.Add(null);
                subItem.Expanded += Folder_Expanded;

                treeViewItem.AddItem(subItem);

            });

            GetFiles(treeViewItem);

        }

        public static string GetPath(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            int index = path.LastIndexOf('\\');
            if (index <= 0)
            {
                return path;
            }

            return path.Substring(index + 1);
        }

        private static void GetFiles(TreeViewItemImage item)
        {
            List<string> files = new List<string>();
            string folderName = (string)item.Tag;
            try
            {
                var dirs = Directory.GetFiles(folderName);
                if (dirs.Length > 0)
                {
                    files.AddRange(dirs);
                }
            }
            catch (Exception)
            {

                throw;
            }

            files.ForEach(directoryPath =>
            {
                TreeViewItemImage subItem = new TreeViewItemImage()
                {
                    Header = GetPath(directoryPath),
                    Tag = directoryPath,
                };

                item.AddItem(subItem);
            });
        }
    }
}

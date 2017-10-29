﻿using System;
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
using ImageViewer.Model.Event;
using Prism.Events;

namespace ImageViewer.View
{
    /// <summary>
    /// Interaction logic for FileExplorerView.xaml
    /// </summary>
    public partial class FileExplorerView : UserControl
    {
        private IEventAggregator _aggregator = GlobalEvent.GetEventAggregator();

        public FileExplorerView()
        {
            InitializeComponent();
        }

        private void FileExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            DriveInfo[] logicalDrives = DriveInfo.GetDrives().Where(x => x.DriveType != DriveType.CDRom).ToArray();
            foreach (var drive in Directory.GetLogicalDrives().Where(x => logicalDrives.Any(d => d.Name == x)))
            {
                var item = new TreeViewItemImage()
                {
                    Header = drive,
                    Tag = drive,
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/DriveIcon.png", UriKind.Absolute))
                };

                if (!CheckIfEmpty(item.Tag.ToString()))
                {
                    item.Items.Add(null);
                }
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

                if (Path.GetExtension(subItem.Tag.ToString()) == "")
                {
                    if (!CheckIfEmpty(subItem.Tag.ToString()))
                    {
                        subItem.Items.Add(null);
                    }
                }

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

        private static bool CheckIfEmpty(string folderPath)
        {
            try
            {
                var files = Directory.EnumerateFileSystemEntries(folderPath).ToArray();
                if (files.Where(x => Path.GetExtension(x) == "").Any())
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            var dirs = Directory.GetFiles(folderPath);
            var images = GetImages(folderPath);

            return images.Count == 0 ? true : false;
        }

        private static List<string> GetImages(string path)
        {
            return Directory.GetFiles(path).Where(x => Path.GetExtension(x) == ".jpg" || Path.GetExtension(x) == ".JPG" || Path.GetExtension(x) == ".BMP"
                   || Path.GetExtension(x) == ".bmp" || Path.GetExtension(x) == ".png" || Path.GetExtension(x) == ".PNG"
                    || Path.GetExtension(x) == ".tiff" || Path.GetExtension(x) == ".TIFF").ToList();
        }

        private static void GetFiles(TreeViewItemImage item)
        {
            List<string> files = new List<string>();
            string folderName = (string)item.Tag;
            try
            {
                var dirs = Directory.GetFiles(folderName);
                var images = GetImages(folderName);

                if (images.Count > 0)
                {
                    files.AddRange(images);
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

        public void TreeView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var clickedItem = TryGetClickedItem(e);
            if (clickedItem == null)
                return;

            e.Handled = true; // to cancel expanded/collapsed toggle
            Model.Image image = new Model.Image();
            try
            {
                image.FileName = clickedItem.Header.ToString();
                image.FilePath = clickedItem.Tag.ToString();
                image.Extension = Path.GetExtension(image.FilePath);
                if(image.Extension!="")
                    _aggregator.GetEvent<SendImage>().Publish(image);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        TreeViewItem TryGetClickedItem(MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null && !(hit is TreeViewItem))
                hit = VisualTreeHelper.GetParent(hit);

            return hit as TreeViewItem;
        }
    }
}

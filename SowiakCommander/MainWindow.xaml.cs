using System;
using System.Collections.Generic;
using System.IO;
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
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace SowiakCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            this.LoadDirectories();
        }

        public void LoadDirectories()
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                this.treeView.Items.Add(this.GetItem(drive));
            }
        }

        private TreeViewItem GetItem(DriveInfo drive)
        {
            var item = new TreeViewItem
            {
                Header = drive.Name,
                DataContext = drive,
                Tag = drive,
                Foreground = Brushes.LimeGreen
                
            };
            this.AddDummy(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            item.MouseDoubleClick += new MouseButtonEventHandler(item_DoubleClicked);
            return item;
        }

        private TreeViewItem GetItem(DirectoryInfo directory)
        {
            var item = new TreeViewItem
            {
                Header = directory.Name,
                DataContext = directory,
                Tag = directory,
                Foreground = Brushes.LimeGreen
            };
            this.AddDummy(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            item.MouseDoubleClick += new MouseButtonEventHandler(item_DoubleClicked);
            return item;
        }

        private TreeViewItem GetItem(FileInfo file)
        {
            var item = new TreeViewItem
            {
                Header = file.Name,
                DataContext = file,
                Tag = file,
                Foreground = Brushes.LimeGreen
            };
            item.MouseDoubleClick += new MouseButtonEventHandler(item_DoubleClicked);
            return item;
        }

        private void AddDummy(TreeViewItem item)
        {
            item.Items.Add(new DummyTreeViewItem());
        }

        private bool HasDummy(TreeViewItem item)
        {
            return item.HasItems && (item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is DummyTreeViewItem).Count > 0);
        }

        private void RemoveDummy(TreeViewItem item)
        {
            var dummies = item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is DummyTreeViewItem);
            foreach (var dummy in dummies)
            {
                item.Items.Remove(dummy);
            }
        }

        private void ExploreDirectories(TreeViewItem item)
        {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo)
            {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
            {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo)
            {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var directory in directoryInfo.GetDirectories())
            {
                var isHidden = (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (directory.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem)
                {
                    item.Items.Add(this.GetItem(directory));
                }
            }
        }

        private void ExploreFiles(TreeViewItem item)
        {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo)
            {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
            {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo)
            {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var file in directoryInfo.GetFiles())
            {
                var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (file.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem)
                {
                    item.Items.Add(this.GetItem(file));
                }
            }
        }

        void item_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (this.HasDummy(item))
            {
                this.Cursor = Cursors.Wait;
                this.RemoveDummy(item);
                this.ExploreDirectories(item);
                this.ExploreFiles(item);
                this.Cursor = Cursors.Arrow;
            }
        }

        void item_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (this.HasDummy(item))
            {
                this.Cursor = Cursors.Wait;
                this.RemoveDummy(item);
                this.ExploreDirectories(item);
                this.ExploreFiles(item);
                this.Cursor = Cursors.Arrow;
            }
            else
            {
                if (item.Tag is FileInfo)
                {
                    try
                    {
                        FileInfo fi = (FileInfo)item.Tag;
                        Process.Start(fi.FullName);
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        MessageBox.Show("Nieobsługiwany typ pliku:\n\n"+ex.Message);
                    }
                }
                
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            richTextBox.Document.Blocks.Clear();
            TreeViewItem x = (TreeViewItem)e.NewValue;
            if (x.Tag is DriveInfo)
            {
                DriveInformation(richTextBox, (DriveInfo)x.Tag);
            }
            if (x.Tag is DirectoryInfo)
            {
                DirectoryInformation(richTextBox, (DirectoryInfo)x.Tag);
            }
            if (x.Tag is FileInfo)
            {
                FileInformation(richTextBox, (FileInfo)x.Tag);
            }
        }

        private void DriveInformation(RichTextBox txt, DriveInfo di)
        {
            txt.AppendText($"Litera dysku: {di.Name}");
            txt.AppendText($"\nNazwa dysku: {di.VolumeLabel}\n");
            txt.AppendText($"System plików: {di.DriveFormat}\n");
            txt.AppendText($"Rodzaj napędu: {di.DriveType}\n");
            double total, free;
            free = di.TotalFreeSpace / 1024 / 1024 / 1024; //GB
            total = di.TotalSize / 1024 / 1024 / 1024; //GB
            txt.AppendText($"Rozmiar: {total} GB\n");
            txt.AppendText($"Wolne: {free} GB\n");
        }

        private void DirectoryInformation(RichTextBox txt, DirectoryInfo di)
        {
            txt.AppendText($"Pełna ścieżka: {di.FullName}");
            txt.AppendText($"\nRozmiar katalogu (MB): {DirectorySize(di)/1024/1024}");
            txt.AppendText($"\nData utworzenia: {di.CreationTime}");
            txt.AppendText($"\nOstatnia modyfikacja: {di.LastWriteTime}");
        }

        private long DirectorySize(DirectoryInfo di)
        {
            long size = 0;
            FileInfo[] filesArray = di.GetFiles();
            foreach (FileInfo fi in filesArray)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo dinf in dis)
            {
                size += DirectorySize(dinf);
            }
            return size;
        }

        private void FileInformation(RichTextBox txt, FileInfo fi)
        {
            
            if (new[] { ".txt", ".html", ".css", ".log" }.Contains(fi.Extension))
            {
                ReadFileLines(fi.FullName, richTextBox);
            }
            if (new[] { ".jpg", ".jpeg", ".png", ".bmp" }.Contains(fi.Extension))
            {
                ShowImage(fi.FullName, richTextBox);
            }
        }

        private void ShowImage(string filePath, RichTextBox txt)
        {
            byte[] image = FileToByteArray(filePath);
            LoadImage(image);
            richTextBox.Paste();
        }

        private void ReadFileLines(string filePath, RichTextBox txt, int linesCount = 5)
        {
            try
            {
                string textLine;
                int counter = 0;
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader fileText = new StreamReader(filePath);
                while ((textLine = fileText.ReadLine()) != null && counter <= linesCount)
                {
                    txt.AppendText(textLine + "\n");
                    counter++;
                }
                fileText.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private byte[] FileToByteArray(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        private static void LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.DecodePixelWidth = 240;
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            Clipboard.SetImage(image);
        }

        private void treeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
                //var x = (TreeViewItem)sender;
                //if (this.HasDummy(x))
                //{
                //    this.Cursor = Cursors.Wait;
                //    this.RemoveDummy(x);
                //    this.ExploreDirectories(x);
                //    this.ExploreFiles(x);
                //    this.Cursor = Cursors.Arrow;
                //}


           
        }
    }
}

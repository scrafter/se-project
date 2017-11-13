using ImageViewer.Model;
using ImageViewer.Model.Event;
using ImageViewer.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

public static class ImageSaver
{
    public static void SafeToText(ObservableCollection<Image> list)
    {
        string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer";
        string filename = @"\ImagesKeeper.txt";
        string path = directory + filename;
        try
        {
            System.IO.File.WriteAllBytes(path, new byte[0]);
        }
        catch(DirectoryNotFoundException e)
        {
            Directory.CreateDirectory(directory);
            if (!File.Exists(path))
                File.Create(path).Close();
        }
        finally
        {
            StreamWriter sw = File.AppendText(path);
            foreach (Image image in list)
            {
                sw.WriteLine(image.Extension);
                sw.WriteLine(image.FilePath);
                sw.WriteLine(image.FileName);
            }
            sw.Close();
        }
    }
    public static void SendTheLoadedImages(ObservableCollection<Image> list)
    {

        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\ImagesKeeper.txt";
        try
        {
            var lineCount = File.ReadLines(path).Count();
            var f = System.IO.File.ReadAllLines(path);

            for (int i = 0; i < lineCount; i += 3)
            {
                Image image = new Image
                {
                    FileName = f[i + 2],
                    FilePath = f[i + 1],
                    Extension = (f[i])
                };
                App.Current.Dispatcher.Invoke(() => list.Add(image));

            }
        }
        catch(FileNotFoundException e)
        {
            return;
        }
        catch(DirectoryNotFoundException e)
        {
            return;
        }
    }
}

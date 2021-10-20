using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocalDatabase_Client
{
    public class DirectoryElement
    {
        public string path { get; set; }
        public List<string> pathArray { get; set; }
        public string name { get; set; }
        public long size { get; set; }
        public string sizeText { get; set; }
        public string lwr { get; set; }
        public bool isFolder { get; set; }
        public string buttonName { get; set; }
        public string image { get; set; }


        public DirectoryElement(string path, long size, string lwr, bool isFolder)
        {
            //full path
            this.path = path;
            //path is splitted here to separated strings by subfolder names. That means  C:\Directory_test\Folder2\Folder6 -> C:,Directory_test,Folder2,Folder6,(empty)
            pathArray = path.Split('\\').ToList<String>();
            //name of file or folder put into variable name and delete it from path array
            this.name = pathArray[pathArray.Count - 1];
            pathArray.RemoveAt(pathArray.Count - 1);
            this.path = this.path.Substring(0, path.LastIndexOf(name));
            this.size = size;
            this.lwr = lwr;
            this.isFolder = isFolder;
            if (isFolder)
            {
                buttonName = "Open";
                image = @"Images\folder_icon.png";
            }
            else
            {
                buttonName = "Download";
                image = @"Images\file_icon.png";
            }
            sizeText = convertNumber(size);
        }

        public DirectoryElement(string pathWithoutName, string name, long size, string lwr, string isFolder)
        {
            //sometimes the last character of path string is a whitespace and then it has to be deleted but i cant use split (the title of folder could contain whitespaces)
            if (pathWithoutName[pathWithoutName.Length - 1].Equals(' '))
                this.path = pathWithoutName.Remove(pathWithoutName.Length - 1);
            else
                this.path = pathWithoutName;
            //path is splitted here to separated strings by subfolder names. That means  C:\Directory_test\Folder2\Folder6 -> C:,Directory_test,Folder2,Folder6,(empty)
            pathArray = pathWithoutName.Split('\\').ToList<String>();
            pathArray.RemoveAt(pathArray.Count - 1);
            this.name = name;
            this.size = size;
            this.lwr = lwr;
            if (isFolder.Equals("True"))
            {
                this.isFolder = true;
                buttonName = "Open";
                image = @"Images\folder_icon.png";
            }
            else
            {
                this.isFolder = false;
                buttonName = "Download";
                image = @"Images\file_icon.png";
            }
            sizeText = convertNumber(size);
        }

        private string convertNumber(long number)
        {
            if (number > 1000 & number < 1000000) //kilobytes
            {
                return (Math.Round(number / 1024.0, 2) + " kB");
            }
            else if (number > 1000000 & number < 1000000000) //megabytes
            {
                return (Math.Round(number / 1048576.0, 2) + " MB");
            }
            else if (number > 1000000000 & number < 1000000000000) //gigabytes
            {
                return (Math.Round(number / 1073741824.0, 2) + " GB");
            }
            return (number + " B"); //bytes
        }

        public override string ToString()
        {
            return "Path: " + path + ", name: " + name + ", size: " + size + ", lwr: " + lwr + ", isFolder: " + isFolder.ToString();
        }
    }
}


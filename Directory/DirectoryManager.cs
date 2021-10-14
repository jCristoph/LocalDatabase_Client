using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace LocalDatabase_Client
{
    public class DirectoryManager
    {
        public ObservableCollection<DirectoryElement> directoryElements { get; set; }

        //Consturctor
        public DirectoryManager()
        {
            directoryElements = new ObservableCollection<DirectoryElement>();
        }
        //Consturctor
        public DirectoryManager(ObservableCollection<DirectoryElement> directoryElements)
        {
            this.directoryElements = directoryElements;
        }

        //method that finds every element in folder. It look for in every subfolder so this method is recurrent.
        public void ProcessDirectory(string targetDirectory)
        {

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string path in fileEntries)
            {
                DirectoryElement de = new DirectoryElement(path.Replace(@"C:\Directory_test", "Main_Folder"), new FileInfo(path).Length, File.GetLastWriteTime(path).ToString(), false);
                directoryElements.Add(de);
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryElement de = new DirectoryElement(subdirectory.Replace(@"C:\Directory_test", "Main_Folder"), 0, "None", true);
                directoryElements.Add(de);
                ProcessDirectory(subdirectory);
            }
        }

        /*If server sends message with path, it has to be translate. The message is in kind of xml message.
         * This method is used to translate it. 
         */
        public void ProcessPath(string s)
        {
            try
            {
                int isFolderIndexHome = s.IndexOf("<Folder>") + "<Folder>".Length;
                int isFolderIndexEnd = s.LastIndexOf("</Folder>");
                string isFolder = s.Substring(isFolderIndexHome, isFolderIndexEnd - isFolderIndexHome);

                int pathIndexHome = s.IndexOf("<Path>") + "<Path>".Length;
                int pathIndexEnd = s.LastIndexOf("</Path>");
                string path = s.Substring(pathIndexHome, pathIndexEnd - pathIndexHome);

                int nameIndexHome = s.IndexOf("<Name>") + "<Name>".Length;
                int nameIndexEnd = s.LastIndexOf("</Name>");
                string name = s.Substring(nameIndexHome, nameIndexEnd - nameIndexHome);

                int sizeIndexHome = s.IndexOf("<Size>") + "<Size>".Length;
                int sizeIndexEnd = s.LastIndexOf("</Size>");
                string size = s.Substring(sizeIndexHome, sizeIndexEnd - sizeIndexHome);

                int lwrIndexHome = s.IndexOf("<Last Write>") + "<Last Write>".Length;
                int lwrIndexEnd = s.LastIndexOf("</Last Write>");
                string lwr = s.Substring(lwrIndexHome, lwrIndexEnd - lwrIndexHome);

                DirectoryElement de = new DirectoryElement(path, name, long.Parse(size), lwr, isFolder);
                directoryElements.Add(de);
                    
            }
            catch (Exception e)
            {

            }

        }

        //method where every file in directory is summed and returns a size of data space in GigaBytes
        public double usedSpace()
        {
            long usedSpaceCounter = 0;
            for(int i = 0; i < directoryElements.Count; i++)
            {
                usedSpaceCounter += directoryElements[i].size;
            }
            return (double)(usedSpaceCounter / 1000000000.0);
        }
    }
}

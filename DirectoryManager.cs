using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace LocalDatabase_Client
{
    public class DirectoryManager
    {
        public List<DirectoryElement> directoryElements { get; set; }

        public DirectoryManager()
        {
            directoryElements = new List<DirectoryElement>();
        }
        public DirectoryManager(List<DirectoryElement> directoryElements)
        {
            this.directoryElements = directoryElements;
        }

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
        public List<string> PrintFolderContent()
        {
            List<string> folderContent = new List<string>();
            foreach(var dirEl in directoryElements)
            {
                if (dirEl.isFolder)
                {
                    folderContent.Add("Inside " + dirEl.name + " are:");
                    foreach (var de in directoryElements)
                    {
                        if (de.pathArray.Count > 1)
                        {
                            if (de.pathArray[de.pathArray.Count - 1].Equals(dirEl.name) && de.pathArray[de.pathArray.Count - 2].Equals(dirEl.pathArray[dirEl.pathArray.Count - 1]))
                                folderContent.Add("\t" + de.name + " path of subfolder: " + dirEl.pathArray[dirEl.pathArray.Count - 1]);
                        }
                        else
                            if (de.pathArray[de.pathArray.Count - 1].Equals(dirEl.name))
                            folderContent.Add("\t" + de.name);
                    }
                }
            }
            return folderContent;
        }
    }
}

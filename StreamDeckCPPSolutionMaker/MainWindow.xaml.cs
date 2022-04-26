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
using System.Windows.Shapes;

using System.IO;
using Path = System.IO.Path;

public class Tools
{
    //https://docs.microsoft.com/fr-fr/dotnet/standard/io/how-to-copy-directories
    public static List<String> CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        List<String> lFileList = new List<string>();

        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = System.IO.Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);

            lFileList.Add(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = System.IO.Path.Combine(destinationDir, subDir.Name);


                List<String> lSubFileList = CopyDirectory(subDir.FullName, newDestinationDir, true);
                
                //PostAdd in order to keep integrity when renaming
                lFileList.Add(newDestinationDir);

                lFileList.AddRange(lSubFileList);
            }
        }

        return lFileList;
    }
}


namespace StreamDeckCPPSolutionMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Task mBackgroundTask = null;

        Dictionary<string, string> mListOfReplacements = new Dictionary<string, string>();

        List<string> mExtensionsToProcess = new List<string>();
        List<string> mDirectoriesToIgnore = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CopyAndReplace()
        {
            if (mBackgroundTask != null)
            {
                if (mBackgroundTask.IsCompleted)
                {
                    mBackgroundTask.Dispose();
                    mBackgroundTask = null;
                }
            }

            if (mBackgroundTask == null)
            {
                GenerateButton.IsEnabled = false;
                mBackgroundTask = new Task(
                () =>
                {
                    List<String> lFileList = Tools.CopyDirectory("Base", mListOfReplacements["%PL_NM%"], true);
                    
                    foreach (string lFilter in mDirectoriesToIgnore)
                    {
                        string lConstructedFilter = Path.DirectorySeparatorChar + lFilter + Path.DirectorySeparatorChar;
                        lFileList = lFileList.Where(lFileName => !lFileName.Contains(lConstructedFilter)).ToList();
                    }

                    lFileList.Reverse();

                    foreach (string lPath in lFileList)
                    {
                        FileInfo lFileInfo = new FileInfo(lPath);
                        if ( !lFileInfo.Exists )
                        {
                            DirectoryInfo lDirInfo = new DirectoryInfo(lPath);

                            string lDestinationDirName = lDirInfo.Name;
                            
                            if ( ReplaceHolders(ref lDestinationDirName, true) )
                            {
                                lDirInfo.MoveTo(lDirInfo.Parent.FullName + Path.DirectorySeparatorChar + lDestinationDirName);
                            }
                           
                        }
                        else
                        {
                            {//Replace content
                                if (mExtensionsToProcess.Contains(lFileInfo.Extension.ToLower()))
                                {
                                    string lFileContent = File.ReadAllText(lFileInfo.FullName);
                                    foreach(KeyValuePair<string,string> lReplaceKeyValue in mListOfReplacements)
                                    {
                                        lFileContent = lFileContent.Replace(lReplaceKeyValue.Key, lReplaceKeyValue.Value);
                                    }
                                    File.WriteAllText(lFileInfo.FullName, lFileContent);
                                }
                            }


                            string lDestinationFileName = lFileInfo.Name;

                            if (ReplaceHolders(ref lDestinationFileName, false))
                            {
                                lFileInfo.MoveTo(lFileInfo.DirectoryName + Path.DirectorySeparatorChar + lDestinationFileName);
                            }
                        }
                    }


                    Dispatcher.Invoke(() =>
                    {
                        GenerateButton.IsEnabled = true;
                    });
                });

             
                mBackgroundTask.Start();
            }
        }

        bool ReplaceHolders(ref string pBase, bool pLowerTheCase = false)
        {
            string lOldValue = pBase;

            foreach (KeyValuePair<string, string> lReplaceItem in mListOfReplacements)
            {
                pBase = pBase.Replace(lReplaceItem.Key, pLowerTheCase ? lReplaceItem.Value.ToLower(): lReplaceItem.Value);
            }

            return pBase != lOldValue;
        }

        private void GenerateClick(object sender, RoutedEventArgs e)
        {
            //Generate Keys
            mListOfReplacements["%BC_NM%"] = mBaseClassNameTB.Text;
            mListOfReplacements["%CPNY%"] = mCompanyNameTB.Text;
            mListOfReplacements["%PL_NM%"] = mPluginNameTB.Text;
            mListOfReplacements["%UWRL%"] = mUrlTB.Text;
            mListOfReplacements["%CTGR%"] = mCategoryTB.Text;
            mListOfReplacements["%AU_NM%"] = mAuthorTB.Text;
            mListOfReplacements["%CP_RGT%"] = mCopyrightContentTB.Text;

            mListOfReplacements["%bc_nm%"] = mListOfReplacements["%BC_NM%"].ToLower();
            mListOfReplacements["%cpny%"] = mListOfReplacements["%CPNY%"].ToLower();
            mListOfReplacements["%pl_nm%"] = mListOfReplacements["%PL_NM%"].ToLower();
            mListOfReplacements["%uwrl%"] = mListOfReplacements["%UWRL%"].ToLower();
            mListOfReplacements["%ctgr%"] = mListOfReplacements["%CTGR%"].ToLower();
            mListOfReplacements["%au_nm%"] = mListOfReplacements["%AU_NM%"].ToLower();
            mListOfReplacements["%cp_rgt%"] = mListOfReplacements["%CP_RGT%"].ToLower();

            mExtensionsToProcess = mExtensionsToProcessTB.Text.ToLower().Split(';').ToList();
            mDirectoriesToIgnore = mDirectoriesToIgnoreTB.Text.Split(';').ToList();

            CopyAndReplace();
        }
    }

}

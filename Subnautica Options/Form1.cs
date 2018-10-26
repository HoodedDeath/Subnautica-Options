using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Subnautica_Options
{
    public partial class Form1 : Form
    {
        //Used when the application is launched through the Death Game Launcher
        private bool nolaunch = false;
        //Called when there are no arguments given to the program
        public Form1()
        {
            //Calls Initialize for general initialization
            Initialize();
        }
        //Called when there were arguments given to the program
        public Form1(string arg)
        {
            //Calls Initialize for general initialization
            Initialize();
            //Checks that the argument given matches the expected argument from the Death Game Launcher
            if (arg.ToLower().Trim() == "nolaunch")
            {
                //Sets the nolaunch variable to true to allow the Form1_FormClosing method to know that the application was launched through the Death Game Launcher, so it will not ask if the user is sure they want to exit
                this.nolaunch = true;
                //Hides and disables the the Launch button so the game can be launched through the Death Game Launcher
                launchButton.Enabled = false;
                launchButton.Visible = false;
            }
        }
        //Common steps to initialize the Form
        private void Initialize()
        {
            //The standard method for initializing the Form control
            InitializeComponent();
            //Creates a new instance of the Config (Settings class) for editing
            Config c = new Config();
            //Uses Scan to scan for Subnautica's install folder and saves the saved games folder to the Path field in the Config class
            c.Path = Scan();
            //Saves the changes to the Config class
            c.Save();
            //Called to fill in the drop-down box (Combo Box) with the available save files
            PopulateComboBox(c.Path);
            //Called to make sure the files in the HoodedDeath folder in the user's Roaming folder exist
            WriteRoamingFiles();
        }
        //Makes sure the files in the HoodedDeath folder in the user's Roaming folder exist
        private void WriteRoamingFiles()
        {
            try
            {
                //The HoodedDeath folder in the user's Roaming folder
                string hdFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath");
                //The folder specific to this application, inside the HoodedDeath folder
                string folder = Path.Combine(hdFolder, "Subnautica Options");
                //The file for telling the Death Game Launcher where the most recently opened executable of this application (incase there are multiple copies)
                string roamingFile = Path.Combine(folder, "Subnautica Options.txt");
                //The file for letting any user who finds this folder why the Subnautica Options.txt file is needed for the Death Game Launcher
                string readmeFile = Path.Combine(folder, "readme.txt");
                //Creates the HoodedDeath folder if it does not exist
                if (!Directory.Exists(hdFolder))
                    Directory.CreateDirectory(hdFolder);
                //Creates the Subnautica Options folder if it does not exist
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                //If the Subnautica Options.txt exists, this deletes it and rewrites it with the current executable's path
                if (File.Exists(roamingFile))
                    File.Delete(roamingFile);
                StreamWriter sw = new StreamWriter(File.OpenWrite(roamingFile));
                sw.Write(Application.ExecutablePath);
                sw.Close();
                sw.Dispose();
                //Creates the readme.txt file if it does not exist
                if (!File.Exists(readmeFile))
                {
                    sw = null; sw = new StreamWriter(File.OpenWrite(readmeFile));
                    sw.Write("Please do NOT delete these files, they are used with the corresponding Death Game Launcher.\r\n\r\nSubnautica Options.txt is used to tell the game launcher where to find the Subnautica Options application.\r\n\r\nIf Death Game Launcher is not able to find the executable for the Subnautica Options application, launch Subnautica Options and it should fix the path issue.");
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message); } //Shows a MessageBox about any exception was thrown
        }

        //Called by clicking the Exit button on the menu strip
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //Called by clicking the Settings button on the menu strip
        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //SettingsForm sf = new SettingsForm();
            //Opens the SettingsForm as a dialog box
            //If the user clicked Save in the Form, the game save drop-down box is updated
            if (new SettingsForm().ShowDialog() == DialogResult.OK)
                PopulateComboBox(new Config().Path);
        }

        //Determines if the form should continue with the closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //If the application was launched through the Death Game Launcher, it will exit without confirmation
            if (nolaunch == true)
            {
                e.Cancel = false;
                return;
            }
            //Makes a popup box to make sure the user wants to exit, and exits if the answer was yes
            else if (MessageBox.Show("Are you sure you want to exit?", "Exit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                e.Cancel = false;
            else //Lets the application exit if anything was given except the user answering yes
                e.Cancel = true;
        }

        //Scans for Subnautica's install folder
        private string Scan()
        {
            //String to hold the save game path that will be returned
            string path = "";
            //Looks into the Windows registry to first see if Steam is installed, and get the install path
            //This variable will be set to "NO VAL" if the value cannot be found in the registry
            object ret = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", "NO VAL");
            if (ret == null || (string)ret == "NO VAL")
            {
                //If Steam is not found in the registry, the application will let the user know they will have to manually enter the path to their installation of Subnautica
                MessageBox.Show("No Steam installation found. Click 'Path' to enter the path to Subnautica manually.");
            }
            else
            {
                //Temporary variable to store the default Steam library folder path, not putting this in the path variable incase Subnautica is installed in a different library folder
                string tpath = Path.Combine(ret.ToString(), "steamapps");
                //Returns the paths of all the Steam Application Manifest files in the default Steam Library folder
                string[] files = Directory.GetFiles(tpath, "*.acf", SearchOption.TopDirectoryOnly);
                //Runs through each of the found App Manifest files to find the Subnautica manifest file
                foreach (string s in files)
                {
                    try
                    {
                        //Opens a Stream Reader to read through the current manifest file
                        StreamReader sr = new StreamReader(File.OpenRead(s));
                        //Skips 4 lines in the manifest file to reach the line containing the name given
                        sr.ReadLine(); sr.ReadLine(); sr.ReadLine(); sr.ReadLine();
                        //Reads the fifth line in the file (the name line) and splits it at the quotation marks
                        string[] t = sr.ReadLine().Split('"');
                        //Stores the name of the given application by getting the second to last element in the array representing the name line. Because of the way the split happens, the last element will be the end-line characters, meaning the second to last element should always be the application's name
                        string name = t[t.Length - 2];
                        //Checks if the application's name was found to be Subnautica
                        if (name == "Subnautica")
                        {
                            //Skips a line to reach the line giving the install path
                            sr.ReadLine();
                            //A likely un-needed line to help make sure the elements read from the name line will not contaminate the install path line. This is likely 100% un-needed, it is more included because I am paranoid.
                            t = null;
                            //Reads the install path line and splits it to an array
                            t = sr.ReadLine().Split('"');
                            //Generates the saved games path by combining the default Steam library folder, the 'common' folder, the second to last element read from the install path line (same reasons given for the name variable), and the path to Subnautica's saved games folder
                            //path = tpath + "\\common\\" + t[t.Length - 2] + "\\SNAppData\\SavedGames";
                            path = Path.Combine(tpath, "common", t[t.Length - 2], Path.Combine("SNAppData", "SavedGames"));
                        }
                        //Closes the Stream Reader and drops its resources to ensure that opening the current file that was being read will not cause an error due to it being used by another process
                        sr.Close();
                        sr.Dispose();
                    }
                    catch (Exception e) { MessageBox.Show("1 " + e.Message); } //Shows a Message Box with the details of any exception that was thrown. The "1" is in there to help with debugging purposes, it will likely be removed in a finished version of this application
                }
                //Given that the Steam installation folder was found, but no path for Subnautica was found, this block will look for user-defined libraries and search them for a Subnautica installation
                if (path == null || path == "")
                {
                    //Combines the default Steam library folder path with the name of the file storing information on user-defined library folders
                    string file = Path.Combine(tpath, "libraryfolders.vdf");
                    //Checks that the library folders file exists (even though it always should if Steam is running correctly)
                    if (!File.Exists(file))
                    {
                        //If the library folders file does not exist, it asks the user to manually enter the path to Subnautica
                        MessageBox.Show("'LibraryFolders.vdf' does not exist in steam folder. Click 'Path' to enter the path to Subnautica manually.");
                    }
                    else
                    {
                        //A List to store the found library folders
                        List<string> libraryfolders = new List<string>();
                        try
                        {
                            //Opens a Stream Reader to read through the library folders file
                            StreamReader sr = new StreamReader(File.OpenRead(file));
                            //Skips 4 lines to reach the lines which will hold the paths of any existing user-defined library folders
                            sr.ReadLine(); sr.ReadLine(); sr.ReadLine(); sr.ReadLine();
                            //A loop to read each line to the end of the file
                            for (; ;)
                            {
                                //Stores the currently read line
                                string temp = sr.ReadLine();
                                //If it has read to the end of the file, a blank line, or the closing brackets that Steam puts at the end of the file, the loop will be broken
                                if (temp == null || temp == "" || temp == "}") break;
                                //Splits the current line at the quotation marks to make the path of the library folder easier to access
                                string[] t = temp.Split('"');
                                //Adds the path of library folder to the List for further searching
                                //The index of Array.Length - 2 is for the same reason as the application name and install path previously explained
                                libraryfolders.Add(t[t.Length - 2]);
                            }
                            //Closes the Stream Reader and drops its resources to ensure that opening the current file that was being read will not cause an error due to it being used by another process
                            sr.Close();
                            sr.Dispose();
                            //Runs through each path in the library folders List (if there are any) to read all the Steam Application Manifest files in search of the Subnautica manifest
                            foreach (string folder in libraryfolders.ToArray<string>())
                            {
                                //Generates the path to the manifest files for the given library folder
                                string temppath = Path.Combine(folder, "steamapps");
                                //Gets the paths to all the manifest files in the current library folder
                                string[] tfiles = Directory.GetFiles(temppath, "*.acf", SearchOption.TopDirectoryOnly);
                                //Runs through each manifest files to check if it is the Subnautica manifest
                                foreach (string f in tfiles)
                                {
                                    //Redefines the previously used Stream Reader to read the current manifest
                                    sr = new StreamReader(File.OpenRead(f));
                                    //Skips 4 lines in the file to reach the name line
                                    sr.ReadLine(); sr.ReadLine(); sr.ReadLine(); sr.ReadLine();
                                    //Reads the name line and splits it
                                    string[] t = sr.ReadLine().Split('"');
                                    //Gets the name of the application
                                    string name = t[t.Length - 2];
                                    //Checks if the name is Subnautica
                                    if (name == "Subnautica")
                                    {
                                        //Skips a line to reach the install path line
                                        sr.ReadLine();
                                        //Stores and splites the install path line
                                        t = null; t = sr.ReadLine().Split('"');
                                        //Saves the Subnautica saved games path into the path variable to be returned
                                        path = Path.Combine(temppath, "common", t[t.Length - 2], Path.Combine("SNAppData", "SavedGames"));
                                    }
                                    //Closes the Stream Reader and drops its resources to ensure that opening the current file that was being read will not cause an error due to it being used by another process
                                    sr.Close();
                                    sr.Dispose();
                                }
                            }
                        }
                        catch (Exception e) { MessageBox.Show("2 " + e.Message); } //Shows a Message Box with the details of any exception that was thrown. The "2" is in there to help with debugging purposes, it will likely be removed in a finished version of this application
                    }
                }
            }
            return path;
        }

        private void PopulateComboBox(string path)
        {
            try
            {
                //Clears the drop-down box items so there will not be any duplicates when re-scanning for game saves
                comboBox1.Items.Clear();
                //Clears the selected item from the drop-down box
                comboBox1.SelectedItem = null;
                //Gets all the folders in the saved games directory (provided in the path parameter)
                string[] folders = Directory.GetDirectories(path);
                //Runs through each of the folders found
                foreach (string folder in folders)
                {
                    //Splits the path to get just the folder name
                    string[] t = folder.Split('\\');
                    //If the folder is not the options folder, the folder name is added to the drop-down box
                    if (t[t.Length - 1].ToLower().Trim() != "options")
                        comboBox1.Items.Add(t[t.Length - 1]);
                }
                //Pre-fills the drop-down selection with the first item in the collection, helping to avoid null reference exceptions when trying to store the selected item
                comboBox1.SelectedIndex = 0;
            } catch { }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Tries to exit the application when the user presses escape on the keyboard, prompting the exit confirmation
            if (e.KeyCode == Keys.Escape) Application.Exit();
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            //Calls Steam through Windows Explorer, asking Steam to launch Subnautica
            System.Diagnostics.Process.Start("explorer.exe", "steam://rungameid/264710");
        }

        private void BackupButton_Click(object sender, EventArgs e)
        {
            //
            //LoadingForm lf = new LoadingForm();
            //lf.Show();
            Thread thread = new Thread(new ThreadStart(LoadingThreadForm));
            thread.Start();
            //
            //Gets the selected item from the drop-down (which will be either nothing or one of the save folders)
            string item = comboBox1.SelectedItem.ToString();
            //Stores the path for the game save
            string folder = Path.Combine(new Config().Path, item); //new Config().Path + "\\" + item;
            //Stores the path for what will become the backup of the save
            string file = Path.Combine(new Config().Path, item + ".zip"); //new Config().Path + "\\" + item + ".zip";
            //To store if the backup succeeded
            bool result = false;
            //To know if an old backup file was found and had to be renamed
            bool oldFound = false;

            //Checks to make sure the save game folder exists (even though it should always exist because the selections the user gets are the folders in the game save directory
            if (Directory.Exists(folder))
            {
                try
                {
                    //If the backup file does not exist already
                    if (!File.Exists(file))
                    {
                        //Creates the backup file using the System.IO.Compression.ZipFile class
                        ZipFile.CreateFromDirectory(folder, file, CompressionLevel.Fastest, false);
                        //Sets the result of the backup to true, assuming there were no exceptions thrown, otherwise this line wouldn't be reached
                        result = true;
                    }
                    //If there is a previous backup, this will temporarily rename the backup, and create the new one. After which, if the new backup was completed, the old one will be deleted
                    else
                    {
                        //Renames the existing backup file by using File.Move to give it the new name of "slotXXXX.zip.tmp"
                        File.Move(file, file + ".tmp");
                        //Stores that an old backup file had to be renamed
                        oldFound = true;
                        //Creates the new backup file using the System.IO.Compression.ZipFile class
                        ZipFile.CreateFromDirectory(folder, file, CompressionLevel.Fastest, false);
                        //If the new backup was completed
                        if (File.Exists(file))
                        {
                            //Result for backup was success
                            result = true;
                            //Delete the old backup
                            File.Delete(file + ".tmp");
                        }
                        //If the new backup failed
                        else
                        {
                            //Result for backup was failure
                            result = false;
                            //Removes the ".tmp" from the end of the old backup file
                            File.Move(file + ".tmp", file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Shows info about any exception thrown
                    MessageBox.Show(ex.Message);
                    //Result for backup was failure
                    result = false;
                    //If there was an old backup file found the had to be renamed, this will remove the ".tmp" from the end of the old backup file
                    if (oldFound)
                        File.Move(file + ".tmp", file);
                }
            }
            //If the save folder does not exist or is unreadable for some reason
            else
            {
                MessageBox.Show("Save file does exist.");
                //Result for backup was failure
                result = false;
            }
            //
            //lf.Close();
            //lf.Dispose();
            thread.Abort();
            //
            if (result)
                MessageBox.Show("Backup completed");
            else
                MessageBox.Show("Backup failed");
        }
        //Thread function for loading popup
        private void LoadingThreadForm()
        {
            new LoadingForm().ShowDialog();
        }

        private void RestoreButton_Click(object sender, EventArgs e)
        {
            //Gets the selected item from the drop-down (which will be either nothing or one of the save folders)
            string item = comboBox1.SelectedItem.ToString();
            //Stores the path for the game save
            string folder = new Config().Path + "\\" + item;
            //Stores the path for the backup file
            string file = new Config().Path + "\\" + item + ".zip";
            //Stores the result of the backup restore
            bool result = false;
            //Stores if the current 
            bool moved = false;

            //If the backup file does exist
            if (File.Exists(file))
            {
                //Makes sure the user wants to overwrite their current save with the backup
                if (MessageBox.Show("Are you sure you want to overwrite save with backup?","",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        //If the save folder doesn't exist and the backup does, simply restore the backup
                        //This should not happen without injecting a backup name into the drop-down for a folder that it did not find
                        if (!Directory.Exists(folder))
                        {
                            //Restores the backup file
                            ZipFile.ExtractToDirectory(file, folder);
                            //Backup was successful if the code reached this line without exception
                            result = true;
                        }
                        //If the save folder does exist, rename the folder temporarily, attempt to extract the folder then delete the temporary copy
                        //This should ALWAYS be the section that runs
                        else
                        {
                            //Renames the target save folder to "slotXXXX.tmp"
                            Directory.Move(folder, folder + ".tmp");
                            //Incase an exception is thrown, the catch is to rename the folder to its original "slotXXXX"
                            moved = true;
                            //Extract the backup to restore the game save
                            ZipFile.ExtractToDirectory(file, folder);
                            //Calls a method to run through the file tree in the "slotXXXX.tmp" folder and delete all files and subdirectories before deleting the base directory
                            result = RemDir(folder + ".tmp");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Shows the details of any exception thrown
                        MessageBox.Show(ex.Message);
                        //Result of restore was failure
                        result = false;
                        //If the original game save folder had to be renamed, but the restore failed, this will restore the original back to its original "slotXXXX" name
                        if (moved)
                            Directory.Move(folder + ".tmp", folder);
                    }
                }
                //Backup was cancelled by user
                else { MessageBox.Show("Backup canceled"); return; }
            }
            //If the backup file does not exist, the restore fails and alerts the user
            else
            {
                MessageBox.Show("Backup does not exist");
                //Result of backup was failure
                result = false;
            }
            if (result)
                MessageBox.Show("Restore completed");
            else
                MessageBox.Show("Restore failed");
        }
        //Simple method to delete a non-empty directory as to not throw an error
        private bool RemDir(string target)
        {
            //Gets all files and directories in the target directory
            string[] files = Directory.GetFiles(target, "*.*", SearchOption.AllDirectories);
            string[] dirs = Directory.GetDirectories(target, "*", SearchOption.AllDirectories);
            //Delete all files inside the target directory and its subdirectories
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            //Delete all subdirectories in the target directory
            foreach (string dir in dirs)
                Directory.Delete(dir);
            //The target directory should now be empty and safe to delete
            Directory.Delete(target);
            //Returns whether or not the target directory was deleted successfully by inverting if it exists
            //Should ALWAYS return true
            return !Directory.Exists(target);
        }

        private void LagFixButton_Click(object sender, EventArgs e)
        {
            //This lag fix works by deleting a certain file that was found to glitch and cause lag after extended time spent on a save.
            //This file is in the "CellsCache" folder in the save folder and the file name is "baked-batch-cells-14-18-15.bin"
            //I recommend backing up a save before using this fix.

            //Selected save to fix
            string item = comboBox1.SelectedItem.ToString();
            //The path to the file to delete
            string file = Path.Combine(new string[] { new Config().Path, item, "CellsCache", "baked-batch-cells-14-18-15.bin" });
            //Warns the user of how this fix works and asks if they want to continue
            if (MessageBox.Show("This lag fix works by removing a certain file in your save. This file has been found as a cause of lag after extended periods on a given save. The removal of this file should not damage the save at all, but it is still tampering with save files. Continue with that in mind or feel free to backup your save before continuing.\n\nDo you want to continue?","Warning",MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //If the file exists, attempt to delete it
                if (File.Exists(file))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        //Attempt to delete the file
                        File.Delete(file);
                        //Check if it still exists to let the user know if it worked
                        if (File.Exists(file))
                            MessageBox.Show("Lag-causing file was not deleted.");
                        else
                            MessageBox.Show("Lag-causing file deleted.");
                    }
                    catch (Exception ex)
                    {
                        //Shows details about any exceptions thrown
                        MessageBox.Show("Fix failed.\n" + ex.Message);
                    }
                }
                //If the file does not exist
                else
                {
                    MessageBox.Show("Lag-causing file not found.");
                }
            }
        }
    }
}

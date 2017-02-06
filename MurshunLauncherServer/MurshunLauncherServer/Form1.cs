﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;

namespace MurshunLauncherServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            try
            {
                if (Process.GetProcessesByName("MurshunLauncherServer").Length > 1)
                {
                    MessageBox.Show("Launcher is already running.");
                    System.Environment.Exit(1);
                }

                string iniDirectoryPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MurshunLauncher";

                xmlPath_textBox.Text = iniDirectoryPath + "\\MurshunLauncherServer.xml";

                if (!Directory.Exists(iniDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(iniDirectoryPath);
                    }
                    catch
                    {
                        MessageBox.Show("Couldn't create a folder at " + iniDirectoryPath);
                    }
                }

                if (File.Exists(xmlPath_textBox.Text))
                {
                    ReadXmlFile();
                }
                else
                {
                    try
                    {
                        LauncherSettings = new MurshunLauncherXmlSettings();

                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(MurshunLauncherXmlSettings));

                        System.IO.FileStream writer = System.IO.File.Create(xmlPath_textBox.Text);
                        serializer.Serialize(writer, LauncherSettings);
                        writer.Close();

                        ReadXmlFile();
                    }
                    catch
                    {
                        MessageBox.Show("Saving xml settings failed.");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Launcher crashed while initializing. Try running it as administrator.\n\n" + e.Message);
                System.Environment.Exit(1);
            }
        }

        public class MurshunLauncherXmlSettings
        {
            public string pathToArma3Server_textBox = Directory.GetCurrentDirectory() + "\\arma3server.exe";
            public string pathToArma3ServerMods_textBox = Directory.GetCurrentDirectory();
            public List<string> serverCustomMods_listView;
            public List<string> serverCheckedModsList_listView;
            public string serverConfig_textBox;
            public string serverCfg_textBox;
            public string serverProfiles_textBox;
            public string serverProfileName_textBox;
            public bool hideWindow_checkBox;
            public string missionFolder = Directory.GetCurrentDirectory() + "\\mpmissions";
            public bool copyMissions_checkBox;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!VerifyMods(false) || !CopyMissions(true))
            {
                DialogResult dialogResult = MessageBox.Show("Launch the server anyway?", "", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    tabControl1.SelectedTab = tabPage3;
                }
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }

            string modLine;

            modLine = defaultStartLineServer_textBox.Text;

            modLine = modLine + " \"-config=" + serverConfig_textBox.Text + "\"";

            modLine = modLine + " \"-cfg=" + serverCfg_textBox.Text + "\"";

            modLine = modLine + " \"-profiles=" + serverProfiles_textBox.Text + "\"";

            modLine = modLine + " -name=" + serverProfileName_textBox.Text;

            modLine = modLine + " \"-mod=";

            foreach (ListViewItem X in serverPresetMods_listView.Items)
            {
                modLine = modLine + pathToArma3ServerMods_textBox.Text + "\\" + X.Text + ";";
            }

            modLine = modLine + "\"";

            modLine = modLine + " \"-servermod=";

            foreach (ListViewItem X in serverCustomMods_listView.CheckedItems)
            {
                modLine = modLine + X.Text + ";";
            }

            modLine = modLine + "\"";

            if (File.Exists(pathToArma3Server_textBox.Text))
            {
                Process myProcess = new Process();

                myProcess.StartInfo.FileName = pathToArma3Server_textBox.Text;
                myProcess.StartInfo.Arguments = modLine;
                if (hideWindow_checkBox.Checked)
                    myProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                myProcess.Start();

                try
                {
                    myProcess.ProcessorAffinity = (System.IntPtr)12;
                    myProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                }
                catch
                {

                }
            }
            else
            {
                MessageBox.Show("arma3server.exe not found.");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            VerifyMods(false);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Remove " + clientExcessFiles_listView.Items.Count + " excess files?", "", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                foreach (ListViewItem X in clientExcessFiles_listView.Items)
                {
                    File.Delete(pathToArma3ServerMods_textBox.Text + X.Text.Split(':')[0]);
                }

                MessageBox.Show("Done.");
            }
        }

        private void changePathToArma3Server_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog selectFile = new OpenFileDialog();

            selectFile.Title = "Select arma3server.exe";
            selectFile.Filter = "Executable File (.exe) | *.exe";
            selectFile.RestoreDirectory = true;

            if (selectFile.ShowDialog() == DialogResult.OK)
            {
                pathToArma3Server_textBox.Text = selectFile.FileName;

                refreshServer_button_Click(null, null);
            }
        }

        private void changePathToArma3ServerMods_button_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog chosenFolder = new VistaFolderBrowserDialog();
            chosenFolder.Description = "Select server mods folder.";
            chosenFolder.UseDescriptionForTitle = true;

            if (chosenFolder.ShowDialog().Value)
            {
                pathToArma3ServerMods_textBox.Text = chosenFolder.SelectedPath;

                refreshServer_button_Click(null, null);
            }
        }

        private void changeServerConfig_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog selectFile = new OpenFileDialog();

            selectFile.Title = "Select Config";
            selectFile.Filter = "Config File (.cfg) | *.cfg";
            selectFile.RestoreDirectory = true;

            if (selectFile.ShowDialog() == DialogResult.OK)
            {
                serverConfig_textBox.Text = selectFile.FileName;

                refreshServer_button_Click(null, null);
            }
        }

        private void changeServerCfg_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog selectFile = new OpenFileDialog();

            selectFile.Title = "Select Cfg";
            selectFile.Filter = "Cfg File (.cfg) | *.cfg";
            selectFile.RestoreDirectory = true;

            if (selectFile.ShowDialog() == DialogResult.OK)
            {
                serverCfg_textBox.Text = selectFile.FileName;

                refreshServer_button_Click(null, null);
            }
        }

        private void changeServerProfiles_button_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog chosenFolder = new VistaFolderBrowserDialog();
            chosenFolder.Description = "Select profiles folder.";
            chosenFolder.UseDescriptionForTitle = true;

            if (chosenFolder.ShowDialog().Value)
            {
                serverProfiles_textBox.Text = chosenFolder.SelectedPath;

                refreshServer_button_Click(null, null);
            }
        }

        private void addCustomServerMod_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog chosenFolder = new VistaFolderBrowserDialog();
            chosenFolder.Description = "Select custom mod folder.";
            chosenFolder.UseDescriptionForTitle = true;

            if (chosenFolder.ShowDialog().Value)
            {
                serverCustomMods_listView.Items.Add(chosenFolder.SelectedPath);

                ReadPresetFile();
            }
        }

        private void removeUncheckedServerMod_button_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in serverCustomMods_listView.Items)
            {
                if (!item.Checked)
                    item.Remove();
            }
        }

        private void closeServer_button_Click(object sender, EventArgs e)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("arma3server");

                foreach (Process process in processes)
                {
                    process.Kill();
                }

                if (processes.Count() > 0)
                    MessageBox.Show("Server process closed.");
                else
                    MessageBox.Show("Server process not found.");
            }
            catch
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveXmlFile();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            refreshServer_button_Click(null, null);
        }

        private void createVerifyFile_button_Click(object sender, EventArgs e)
        {
            Thread NewThread = new Thread(() => VerifyMods(true));
            NewThread.Start();
        }

        private void refreshServer_button_Click(object sender, EventArgs e)
        {
            ReadPresetFile();

            CheckSyncFolderSize();

            VerifyMods(false);
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (defaultStartLineServer_textBox.Text.Contains("-nologs"))
            {
                defaultStartLineServer_textBox.Text = defaultStartLineServer_textBox.Text.Replace(" -nologs", "");
                defaultStartLineServer_textBox.Text = defaultStartLineServer_textBox.Text.Replace("-nologs", "");
            }
            else
            {
                defaultStartLineServer_textBox.Text = defaultStartLineServer_textBox.Text + " -nologs";
            }
        }

        private void copyMissions_button_Click(object sender, EventArgs e)
        {
            CopyMissions(false);
        }

        private void changePathToMissionFolder_button_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog chosenFolder = new VistaFolderBrowserDialog();
            chosenFolder.Description = "Select mission folder.";
            chosenFolder.UseDescriptionForTitle = true;

            if (chosenFolder.ShowDialog().Value)
            {
                missionFolder_textBox.Text = chosenFolder.SelectedPath;

                refreshServer_button_Click(null, null);
            }
        }
    }
}

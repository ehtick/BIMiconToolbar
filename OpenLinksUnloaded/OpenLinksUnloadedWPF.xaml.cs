﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMiconToolbar.Helpers.UserControls.SelectFileReferences.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BIMiconToolbar.OpenLinksUnloaded
{
    /// <summary>
    /// Interaction logic for OpenLinksUnloadedWPF.xaml
    /// </summary>
    public partial class OpenLinksUnloadedWPF : Window, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public UIApplication UIApp { get; set; }
        public SelectFileReferencesViewModel selectFileReferencesViewModel { get; set; }
        public List<ExternalFileReferenceType> SelectedFileReferences { get; set; }
        public string SelectedFilePath { get; set; }

        private string _warningMessage;
        public string WarningMessage
        {
            get { return _warningMessage; }
            set
            {
                _warningMessage = value;
                OnPropertyChanged();
            }
        }

        public OpenLinksUnloadedWPF(UIApplication uiApp)
        {
            InitializeComponent();
            UIApp = uiApp;
        }

        public void Dispose()
        {
            this.Close();
        }

        public void SelectFileReferences_Loaded(object sender, RoutedEventArgs e)
        {
            ExternalFileReferenceType[] extFileRefT = { ExternalFileReferenceType.RevitLink, ExternalFileReferenceType.CADLink };

            selectFileReferencesViewModel = new SelectFileReferencesViewModel();
            selectFileReferencesViewModel.LoadFileReferences(extFileRefT);
            SelectFileReferences.DataContext = selectFileReferencesViewModel;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            // Assign selected input to properties
            SelectedFilePath = FileBrowser.FilePath;
            SelectedFileReferences = new List<ExternalFileReferenceType>();

            foreach (var item in selectFileReferencesViewModel.FileReferences)
            {
                if (item.IsSelected)
                {
                    SelectedFileReferences.Add(item.ReferenceType);
                }
            }

            // Check for user input
            if (SelectedFilePath == null || SelectedFilePath == "")
            {
                WarningMessage = "Please select Revit project.";
            }
            // Execute the program
            else
            {
                // Reset warning message
                WarningMessage = "";

                // Variable for final modelpath
                FilePath finalPath = null;

                // Create ModelPath to project file
                FilePath modelPath = null;

                // Path to create local model 
                string localPathString = "";

                // Check if model is workshared
                bool isWorkshared = BasicFileInfo.Extract(SelectedFilePath).IsWorkshared;

                if (isWorkshared)
                {
                    // New local filename
                    FileInfo modelInfo = new FileInfo(SelectedFilePath);

                    bool isCentral = BasicFileInfo.Extract(SelectedFilePath).IsCentral;

                    // File is central file
                    if (isCentral)
                    {
                        string modelName = modelInfo.Name;
                        string localName = modelName.Replace(".rvt", "") + " - LinksUnloaded.rvt";

                        // Get user documents folder path
                        string docPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        localPathString = docPath + "\\" + localName;

                        modelPath = new FilePath(SelectedFilePath);
                    }
                    // File is local file
                    else
                    {
                        localPathString = SelectedFilePath;

                        // Retrieve central model path
                        string centralPath = BasicFileInfo.Extract(SelectedFilePath).CentralPath;
                        modelPath = new FilePath(centralPath);

                        // Delete previous local file
                        File.Delete(SelectedFilePath);
                    }

                    FilePath localPath = new FilePath(localPathString);

                    // Unload links
                    Helpers.RevitDirectories.UnloadLinks(modelPath, SelectedFileReferences);

                    // Properties for saving the file as central
                    WorksharingSaveAsOptions worksharingSaveAsOptions = new WorksharingSaveAsOptions();
                    worksharingSaveAsOptions.SaveAsCentral = true;

                    SaveAsOptions saveAsOptions = new SaveAsOptions { Compact = true, OverwriteExistingFile = true };
                    saveAsOptions.SetWorksharingOptions(worksharingSaveAsOptions);

                    // Save file as central
                    Document doc = UIApp.Application.OpenDocumentFile(SelectedFilePath);
                    doc.SaveAs(SelectedFilePath, saveAsOptions);
                    doc.Close(false);

                    // Set transmission to false to allow create new local model
                    TransmissionData transData = TransmissionData.ReadTransmissionData(modelPath);
                    if (transData.IsTransmitted)
                    {
                        transData.IsTransmitted = false;
                        TransmissionData.WriteTransmissionData(modelPath, transData);
                    }

                    // Create local copy
                    WorksharingUtils.CreateNewLocal(modelPath, localPath);

                    finalPath = new FilePath(localPathString);

                }
                // File is not workshared
                else
                {
                    finalPath = modelPath;

                    // Unload links
                    Helpers.RevitDirectories.UnloadLinks(finalPath, SelectedFileReferences);
                }

                // Open document
                OpenOptions openOptions = new OpenOptions();
                WorksetConfiguration openConfig = new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);
                openOptions.SetOpenWorksetsConfiguration(openConfig);

                UIApp.OpenAndActivateDocument(finalPath, openOptions, false);
            }

            // Close window
            this.Dispose();
        }
    }
}

﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BIMiconToolbar.NumberDoors
{
    /// <summary>
    /// Interaction logic for NumberDoorsWPF.xaml
    /// </summary>
    public partial class NumberDoorsWPF : Window, IDisposable
    {
        public ObservableCollection<ComboBoxItem> CbPhases { get; set; }
        public ComboBoxItem SelectedComboItemPhase { get; set; }
        public string Separator { get; set; }

        public bool optNumeric = false;

        public NumberDoorsWPF(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            InitializeComponent();
            DataContext = this;

            // Fill properties
            PopulatePhases(doc);

            // Associate the event-handling method with the SelectedIndexChanged event
            comboDisplayPhases.SelectionChanged += new SelectionChangedEventHandler(ComboChangedPhase);
        }

        /// <summary>
        /// Implement Disposable interface
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        
        private void PopulatePhases(Document doc)
        {
            CbPhases = new ObservableCollection<ComboBoxItem>();

            // Select phases
            PhaseArray aPhase = doc.Phases;

            IOrderedEnumerable<Phase> phases = aPhase.Cast<Phase>().OrderBy(ph => ph.Name);

            int count = 0;

            foreach (var ph in phases)
            {
                ComboBoxItem comb = new ComboBoxItem();
                comb.Content = ph.Name;
                comb.Tag = ph;
                CbPhases.Add(comb);

                if (count == 0)
                {
                    SelectedComboItemPhase = comb;
                }

                count++;
            }
        }

        /// <summary>
        /// Function to change user number selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Numeric_Checked(object sender, RoutedEventArgs e)
        {
            optNumeric = !optNumeric;
        }

        /// <summary>
        /// Method to update phase according to initial phase selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboChangedPhase(object sender, SelectionChangedEventArgs e)
        {
            int selectedItemIndex = CbPhases.IndexOf(SelectedComboItemPhase);
            SelectedComboItemPhase = CbPhases[selectedItemIndex];
        }

        /// <summary>
        /// Function to output user selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Separator = SeparatorTextBox.Text;
            Dispose();
        }

        /// <summary>
        /// Function to close window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Use separator as flag to avoid execution
            Separator = null;
            Dispose();
        }
    }
}

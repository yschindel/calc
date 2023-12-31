﻿using Calc.ConnectorRevit.Helpers;
using Calc.ConnectorRevit.ViewModels;
using Calc.Core.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calc.ConnectorRevit.Views
{
    public partial class MainView : Window
    {
        private readonly MainViewModel MainVM;

        public MainView(MainViewModel mvm)
        {
            MainVM = mvm;
            this.DataContext = MainVM;
            InitializeComponent();
            ViewMediator.Register("ViewDeselectTreeView", _=>DeselectTreeView());
        }

        private void DeselectTreeView()
        {
            if (TreeView.SelectedItem != null)
            {
                if (TreeView.Tag is TreeViewItem selectedTreeViewItem)
                {
                    selectedTreeViewItem.IsSelected = false;
                }
            }
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            await MainVM.LoadingVM.HandleLoadingAsync();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainVM.HandleWindowClosing();
        }

        private async void ProjectOKClicked(object sender, RoutedEventArgs e)
        {
            var project = ProjectsComboBox.SelectedItem;
            await MainVM.LoadingVM.HandleProjectSelectedAsync(project as Project);
            MainVM.NotifyStoreChange();
        }
        
        private void ForestSelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            var forest = ForestsComboBox.SelectedItem;
            MainVM.ForestVM.HandleForestSelectionChanged(forest as Forest);
        }

        private void MappingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainVM.MappingVM.HandleMappingSelectionChanged(MappingsComboBox.SelectedItem as Mapping);
        }

        private void NewMappingClicked(object sender, RoutedEventArgs e)
        {
            MainVM.NewMappingVM.HandleNewMappingClicked();
        }
        private async void NewMappingConfirmed(object sender, RoutedEventArgs e)
        {
            string newName = this.NewNameText.Text.Trim();
            Mapping selectedMapping = this.MappingListBox.SelectedItem as Mapping;
            await MainVM.NewMappingVM.HandleNewMappingCreate(selectedMapping, newName);
            MainVM.NotifyStoreChange();
        }

        private void NewMappingCanceld(object sender, RoutedEventArgs e)
        {
            this.NewNameText.Text = "";
            MappingListBox.SelectedItem = null;
            MainVM.NewMappingVM.HandleNewMappingCanceled();
        }

        private void TreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            if (TreeView.SelectedItem is NodeViewModel selectedBranch)
            {
                MainVM.NodeTreeVM.HandleNodeItemSelectionChanged(selectedBranch);
                TreeView.Tag = e.OriginalSource;
                e.Handled = true;
            }
        }

        private void SideClickDown(object sender, MouseButtonEventArgs e)
        {
            MainVM.NodeTreeVM.DeselectNodes();
        }

        private void BuildupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainVM.BuildupVM.HandleBuildupSelectionChanged();
        }

        private void InheritClicked(object sender, RoutedEventArgs e)
        {
            MainVM.BuildupVM.HandleInherit();
        }

        private void ViewToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            MainVM.HandleViewToggleToBuildup();
        }

        private void ViewToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
            MainVM.HandleViewToggleToBranch();
        }

        private void UpdateRevitClicked(object sender, RoutedEventArgs e)
        {
            var forest = ForestsComboBox.SelectedItem;
            MainVM.ForestVM.HandleForestSelectionChanged(forest as Forest);
        }
        private void StartCalcLiveClicked(object sender, RoutedEventArgs e)
        {
            MainVM.HandleStartCalcLive();
        }


        private void SaveResultsClicked(object sender, RoutedEventArgs e)
        {
            NewResultNameTextBox.Text = "";
            MainVM.SavingVM.HandleSavingResults();
        }

        private async void SaveResultOKClicked( object sender, RoutedEventArgs e)
        {
            await MainVM.SavingVM.HandleSendingResults(NewResultNameTextBox.Text);            
        }

        private void SaveResultCancelClicked (object sender, RoutedEventArgs e)
        {
            MainVM.SavingVM.HandleSaveResultCanceled();
        }

        private void MessageOKClicked(object sender, RoutedEventArgs e)
        {
            MainVM.HandleMessageClose();
        }

        private async void UpdateMappingClicked(object sender, RoutedEventArgs e)
        {
            await MainVM.MappingVM.HandleUpdateMapping();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace Telerik_UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static MainWindow myView = new Telerik_UI.MainWindow();

        public App()
        {
            this.InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //init Datacontext
            ClubViewModel vm = new ClubViewModel();
            vm.ListClubItems = ClubItemViewModel.GetClubs();

            myView.DataContext = vm;
            vm.AddField = new RelayCommand<object>(AddField);


            myView.Show();

        }

        //CommandInvoke
        public void AddField(object parameter)
        {
            if ((parameter as ClubViewModel) != null)
            {
                ClubViewModel vm = (ClubViewModel)parameter;

                foreach (ClubItemViewModel clubItem in vm.ListClubItems)
                {
                    clubItem.ListCustomData.Add(new CustomData() { Name = "Field " + (new Random()).Next(1, 20).ToString(), DateCreate = new DateTime(2020, 12, 23, 12, 12, 12) });
                }
            }

            RefreshTable();

        }

        public void RefreshTable()
        {
            //Get radGridView
             RadGridView radGridView = FindChild<RadGridView>(myView, "radGridView1");

            if (radGridView != null)
            {
                ObservableCollection<ClubItemViewModel> listItemSource = radGridView.ItemsSource as ObservableCollection<ClubItemViewModel>;

                //Remove CustomdataColumn
                if (radGridView.Columns.Count > 2)
                {
                    for (int i = radGridView.Columns.Count-1; i > 1; i--)
                    {
                        radGridView.Columns.RemoveAt(i);
                    }
                }

                foreach(ClubItemViewModel myItemClub in listItemSource)
                {
                    //Cho moi cusData cua myCusdataList, them cot va add vao
                    for (int i = 0; i < myItemClub.ListCustomData.Count; i++)
                    {
                        GridViewDataColumn myNewGridViewCol = new GridViewDataColumn();
                        myNewGridViewCol.DataMemberBinding = new Binding(@"ListCustomData[" +i+ "].DateCreate" );
                        myNewGridViewCol.CellEditTemplate = CreateDateTimePickerDataTemplate(@"ListCustomData[" + i + "].DateCreate");
                        myNewGridViewCol.Header = myItemClub.ListCustomData[i].Name;
                        radGridView.Columns.Add(myNewGridViewCol);
                    }
                    break;
                }

            }

        }

        public DataTemplate CreateDateTimePickerDataTemplate(string BindingPath)
        {
            //create the data template
            DataTemplate myDatatemplate = new DataTemplate();
            myDatatemplate.DataType = typeof(ClubItemViewModel);

            FrameworkElementFactory datePicker = new FrameworkElementFactory(typeof(RadDateTimePicker));
            datePicker.SetBinding(RadDateTimePicker.SelectedValueProperty, new Binding(BindingPath) { Mode = BindingMode.TwoWay });

            //set the visual tree of the data template
            myDatatemplate.VisualTree = datePicker;

            myDatatemplate.Seal();
            return myDatatemplate;
        }


        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
    }
}

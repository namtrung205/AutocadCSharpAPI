using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace Telerik_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GridView myGridView = this.myListView.View as GridView;
            MessageBox.Show("NumCol: " + myGridView.Columns.Count);

            //Create a new Column
            System.Windows.Controls.GridViewColumn myNewGridViewCol = new System.Windows.Controls.GridViewColumn();

            myNewGridViewCol.CellTemplate = CreateDateTimePickerDataTemplate("Established");
            myNewGridViewCol.Header = "abc";
            myGridView.Columns.Add(myNewGridViewCol);
        }

        public DataTemplate CreateDateTimePickerDataTemplate(string PropertyName)
        {
            //create the data template
            DataTemplate myDatatemplate = new DataTemplate();
            myDatatemplate.DataType = typeof(ClubItemViewModel);

            FrameworkElementFactory datePicker = new FrameworkElementFactory(typeof(RadDateTimePicker));
            datePicker.SetBinding(RadDateTimePicker.SelectedValueProperty, new Binding(PropertyName) { Mode = BindingMode.TwoWay });

            //set the visual tree of the data template
            myDatatemplate.VisualTree = datePicker;

            myDatatemplate.Seal();
            return myDatatemplate;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            GridViewDataColumn myNewGridViewCol = new GridViewDataColumn();
            myNewGridViewCol.DataMemberBinding = new Binding("Established");
            myNewGridViewCol.CellEditTemplate = CreateDateTimePickerDataTemplate("Established");
            myNewGridViewCol.Header = "Date " + (radGridView1.Columns.Count-2);

            radGridView1.Columns.Add(myNewGridViewCol);
        }
    }
}

using MultiModalMapProject.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace MultiModalMapProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // sets the current culture to US
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            // sets the default culture of all threads to US-en
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
            InitializeComponent();
            // initialiseMapComponent
            InitialiseMapComponent();
            //initialiseSpeechComponent();
            InitialiseBingSpeechComponents();

            listResult();
        }

        // resets the application to its initial state. All the ui elements are set to their initial state.
        private void resetApplication()
        {
            // resets the map to its initial state
            this.Dispatcher.Invoke(() =>
            {
                resetMap();
            });
        }

        private void listResult()
        {
            listBox.Items.Add("result1");
            listBox.Items.Add("result2");
        }
    }
}

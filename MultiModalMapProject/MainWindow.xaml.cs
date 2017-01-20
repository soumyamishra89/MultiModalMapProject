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
            InitializeComponent();
            //initialiseSpeechComponent();
            InitialiseBingSpeechComponents();
            // getting a session key from bing maps for using non-billable call to bing rest api
            myMap.CredentialsProvider.GetCredentials(c =>
            {
                    StaticVariables.bingMapSessionKey = c.ApplicationId;
            });
        }
    }
}

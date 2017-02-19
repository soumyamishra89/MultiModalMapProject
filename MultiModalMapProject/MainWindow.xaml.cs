using Microsoft.Maps.MapControl.WPF;
using MultiModalMapProject.JsonSchemas.NavteqPoiSchema;
using MultiModalMapProject.Util;
using System;
using System.Collections.Generic;
using System.IO;
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
        static Map thisMap;
        private SolidColorBrush systemMessageBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#41BDF2");
        private SolidColorBrush speechWhiteBackgroundBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush speechWarningBackgroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFDC17");
        private SolidColorBrush speechUserMessageBrush = new SolidColorBrush(Colors.Black);
        private SolidColorBrush systemWarningMessageBrush = new SolidColorBrush(Colors.Maroon);

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

            listMenu();
            //listResult();
            Instructionpage();
            thisMap = myMap;
        }

        bool instructionsShown = true;
       
        // resets the application to its initial state. All the ui elements are set to their initial state.
        private void resetApplication()
        {
            // resets the map to its initial state
            this.Dispatcher.Invoke(() =>
            {
                resetMap();
            });
        }

        private void listMenu()
        {
            nearbyPlacesList.Items.Add(new NearbySearchMenu { TextCategory = "Restaurants", MenuTextColor = "purple" });
           
        }


        private void listResult()
        {
            BitmapImage bitmapImage1 = new BitmapImage();

                        FileStream fileStream = new FileStream("../../Images/image1.jpg", FileMode.Open, FileAccess.Read);

            bitmapImage1.BeginInit();
            bitmapImage1.StreamSource = fileStream;
            bitmapImage1.EndInit();
            BitmapImage bitmapImage2 = new BitmapImage();
            FileStream fileStream2 = new FileStream("../../Images/image2.jpg", FileMode.Open, FileAccess.Read);

            bitmapImage2.BeginInit();
            bitmapImage2.StreamSource = fileStream2;
            bitmapImage2.EndInit();
            //Add value to the listBox
            nearbyPlacesList.Items.Add(new NearbySearch { Image = bitmapImage1, TextName = "Restaurant Alpenstuck", TextAddress ="Gartenstrasse 10115 Berlin", TextContact = "030 21751646"});
            nearbyPlacesList.Items.Add(new NearbySearch { Image = bitmapImage2, TextName = "Sushi XIV", TextAddress = "Chausseestr. 14 10115 Berlin", TextContact = "030 47599699" });

        }

        private void Instructionpage()
        {
           
            Instruction_Page.Items.Add(new Information { TextName = "Hello, here are the tasks : ", Image = "Images/blank.png" });
            Instruction_Page.Items.Add(new Information { TextName = "1. Zoom in : say 'zoom in' or move your hands close", Image = "Images/zoomin.png" });
            Instruction_Page.Items.Add(new Information { TextName = "2. Zoom out : say 'zoom out' or move your hands wide", Image = "Images/zoomout.png" });
            Instruction_Page.Items.Add(new Information { TextName = "3. To search for a place, ask 'show me (name of the place)", Image = "Images/blank.png" });
            Instruction_Page.Items.Add(new Information { TextName = "4. To show a route, ask 'I want to go from (here : point or say it out loud) to (here)", Image = "Images/blank.png" });
            Instruction_Page.Items.Add(new Information { TextName = "5. Show me places nearby (here : point or say it out loud)", Image = "Images/blank.png" });
            Instruction_Page.Items.Add(new Information { TextName = "6. Move the map : ask 'move' then move your hands.", Image = "Images/move.png" });
            Instruction_Page.Items.Add(new Information { TextName = "7. To reset, ask 'reset' ", Image = "Images/blank.png" });
            Instruction_Page.Items.Add(new Information { TextName = "8. FORGOT AN INSTRUCTION ? ASK FOR 'HELP' !  ", Image = "Images/blank.png" });
            Instruction_Page.Items.Add(new Information { TextName = "If it's ok, please say 'OK'", Image = "Images/blank.png" });

        }

        // hides the instructions page if its shown and vice versa
        private void showOrHideInstructions()
        {
            this.Dispatcher.Invoke(() =>
            {
                // if the user says ok, the instruction page disappears
                if (instructionsShown)
                {
                    instructionsShown = false;
                    Instruction_Page.Visibility = Visibility.Hidden;
                }

                // if the user says "help", the instruction page appears
                else
                {
                    instructionsShown = true;
                    Instruction_Page.Visibility = Visibility.Visible;
                }
            });
        }

        // the poi informatio is added to the listview
        private void addPOIToListView(BitmapImage image, Result poi, int poiNumber)
        {
            this.Dispatcher.Invoke(() =>
            {
                nearbyPlacesList.Items.Add(new NearbySearch { Image = image, TextName = poi.DisplayName, TextAddress = poi.AddressLine, TextContact = poi.Phone });
            });
        }

        // sets the message from system (asking for more information) in cyan color
        private void setSystemMessagesToSpeechLabel(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                SpeechLabel.Foreground = systemMessageBrush;
                SpeechLabel.Background = speechWhiteBackgroundBrush;
                SpeechLabel.Content = message;
            });
        }

        // sets the message from system (asking for more information) in cyan color
        private void setSystemWarningMessagesToSpeechLabel(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                SpeechLabel.Foreground = systemWarningMessageBrush;
                SpeechLabel.Background = speechWarningBackgroundBrush;
                SpeechLabel.Content = message;
            });
        }
        // sets the detected message from users in black color
        private void setUserRecognisedSpeechText(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                SpeechLabel.Background = speechWhiteBackgroundBrush;
                SpeechLabel.Foreground = speechUserMessageBrush;
                SpeechLabel.Content = message;
            });
        }

        // hides the nearby search listbox
        private void hideNearbyPlacesList()
        {
            this.Dispatcher.Invoke(() =>
            {
                nearbyPlacesList.Visibility = Visibility.Hidden;
            });
        }
        public class NearbySearchMenu
        {
            public string TextCategory { get; set; }
            public string MenuTextColor { get; set; }
        }


        public class NearbySearch
    {
        //Place image
        public BitmapImage Image { get; set; }   
        //Place Name   
        public string TextName { get; set; }  
        //Place Address 
        public string TextAddress { get; set; }   
        //Place Contact information
        public string TextContact { get; set; }  
    }

        public class Information
        {
            //Place image
            public string Image { get; set; }
            //Place Name   
            public string TextName { get; set; }
           
        }
    }
}

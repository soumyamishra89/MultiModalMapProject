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

            listMenu();
            listResult();
            Instructionpage();
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

        private void listMenu()
        {
            listBox.Items.Add(new nearbySearchMenu { TextCategory = "Restaurants", MenuTextColor = "purple" });
        }


        private void listResult()
        {
            //Add value to the listBox
            listBox.Items.Add(new nearbySearch { Image = "Images/image1.jpg", TextName = "Restaurant Alpenstuck", TextAddress ="Gartenstrasse 10115 Berlin", TextContact = "030 21751646"});
            listBox.Items.Add(new nearbySearch { Image = "Images/image2.jpg", TextName = "Sushi XIV", TextAddress = "Chausseestr. 14 10115 Berlin", TextContact = "030 47599699" });

        }
        
        private void Instructionpage()
        {
            Instruction_Page.Items.Add(new information { TextName = "Hello, here are the tasks : "});
            Instruction_Page.Items.Add(new information { TextName = "1. Zoom in : say 'zoom in' or move your hands close", Image3="Images/zoomin.png"});
            Instruction_Page.Items.Add(new information { TextName = "1. Zoom out : say 'zoom out' or move your hands wide", Image3 = "Images/zoomout.png" });
            Instruction_Page.Items.Add(new information { TextName = "3. To search for a place, ask 'show me (name of the place)" });
            Instruction_Page.Items.Add(new information { TextName = "4. To show a route, ask 'I want to go from (here : point or say it out loud) to (here)" });
            Instruction_Page.Items.Add(new information { TextName = "5. Show me places nearby (here : point or say it out loud)"});
            Instruction_Page.Items.Add(new information { TextName = "6. Move the map : ask 'move' then move your hands.", Image3 = "Images/move.png" });
            Instruction_Page.Items.Add(new information { TextName = "7. To reset, ask 'reset' " });
            Instruction_Page.Items.Add(new information { TextName = "8. FORGOT AN INSTRUCTION ? ASK FOR 'HELP' !  " });
            Instruction_Page.Items.Add(new information { TextName = "If it's ok, please say 'OK'" });
           
        
        }

        public class nearbySearchMenu
        {
            public string TextCategory { get; set; }
            public string MenuTextColor { get; set; }
        }


        public class nearbySearch
    {
        //Place image
        public string Image { get; set; }   
        //Place Name   
        public string TextName { get; set; }  
        //Place Address 
        public string TextAddress { get; set; }   
        //Place Contact information
        public string TextContact { get; set; }  
    }

        public class information
        {
            //Place image
            public string Image { get; set; }
            //Place Name   
            public string TextName { get; set; }
            //Place Address 
            public string Image2 { get; set; }
            //Place Contact information
            public string Image3 { get; set; }
        }
    }
}

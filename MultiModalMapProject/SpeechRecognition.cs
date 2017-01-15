using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;

namespace MultiModalMapProject
{
    // authhor: @yuanchaidoris
    partial class MainWindow
    {
        SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));
        SpeechSynthesizer VAS = new SpeechSynthesizer();
        String[] zoomlevel = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
        
        private void initialiseSpeechComponent()
        {
            CultureInfo currentCulture= Thread.CurrentThread.CurrentCulture;
            foreach (RecognizerInfo ri in SpeechRecognitionEngine.InstalledRecognizers())
            {
                Console.WriteLine(" TEST: " + ri.Culture.Name);
            }
            //"Show me" activate pointing
            Choices showActivate = new Choices("Show me");
            
            //Zooming
            Choices Zooming = new Choices("Zoom-in", "Zoom-out");
            
            //"Nearby search"
            Choices nearbyActive = new Choices("Nearby");
           
            //Show current location
            Choices locationActive = new Choices("Current location");

            //Reset
            Choices resetActive = new Choices("Reset");

            //Route Search
            // string cityName = System.IO.File.ReadAllText(@"C:\Users\Doris\Documents\GitHub\MultiModalMapProject\MultiModalMapProject\allCountries.txt");
            // Console.WriteLine(cityName.Split('\n')[0].Split(' ').Length);
            Choices placeName = new Choices(new string[] { "Berlin", "Paris", "London", "Stockholm", "Munich", "Zurich" });
            Choices places = new Choices(placeName);
            Choices nearbySearch = new Choices(new string[] { "Restaurant", "Hotel", "Bar", "Attractions", "Coffee", "Transit" });
            Choices travelMode = new Choices(new string[] { "driving", "transit", "walking" });


            GrammarBuilder GB_zooming = new GrammarBuilder(Zooming); //zooming grammar
            GB_zooming.Culture = currentCulture;
            GrammarBuilder GB_route = new GrammarBuilder(); //route search grammar
            GB_route.Culture = currentCulture;
            GrammarBuilder GB_travelmode = new GrammarBuilder(travelMode); //travel mode
            GB_travelmode.Culture = currentCulture;
            GrammarBuilder GB_show = new GrammarBuilder(); //show a place grammar
            GB_show.Culture = currentCulture;
            GrammarBuilder GB_nearbySearch = new GrammarBuilder(); //search place nearby grammar
            GB_nearbySearch.Culture = currentCulture;
            GrammarBuilder GB_location = new GrammarBuilder();// show current location grammar
            GB_location.Culture = currentCulture;
            GrammarBuilder GB_reset = new GrammarBuilder(resetActive); // reset the map grammar
            GB_reset.Culture = currentCulture;
            

            GB_zooming.Append("By");
            GB_zooming.Append(new Choices(zoomlevel));

            GB_route.Append(showActivate);
            GB_route.Append("from");
            GB_route.Append(places);
            GB_route.Append("to");
            GB_route.Append(places);


            GB_show.Append(showActivate);
            GB_show.Append(places);

            GB_nearbySearch.Append(showActivate);
            GB_nearbySearch.Append(nearbySearch);
            GB_nearbySearch.Append(nearbyActive);

            GB_location.Append(showActivate);
            GB_location.Append(locationActive);

   

            Grammar SudeepGrammer = new Grammar(GB_zooming);
            Grammar routeGrammar = new Grammar(GB_route);
            routeGrammar.Name = ("Route Search");
            Grammar showGrammar = new Grammar(GB_show);
            showGrammar.Name = ("Show a place");
            Grammar nearbyGrammar = new Grammar(GB_nearbySearch);
            nearbyGrammar.Name = ("Show nearby information");
            Grammar locationGrammar = new Grammar(GB_location);
            locationGrammar.Name = ("Show current location");
            Grammar resetGrammar = new Grammar(GB_reset);
            resetGrammar.Name = ("Reset the map");
            Grammar travelmodeGrammar = new Grammar(GB_travelmode);
            travelmodeGrammar.Name = ("Choose travel mode");

            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.LoadGrammar(SudeepGrammer);
            _recognizer.LoadGrammar(routeGrammar);
            _recognizer.LoadGrammar(showGrammar);
            _recognizer.LoadGrammar(nearbyGrammar);
            _recognizer.LoadGrammar(locationGrammar);
            _recognizer.LoadGrammar(resetGrammar);
           

            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized_zooming);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized_travelmode);

            _recognizer.LoadGrammar(travelmodeGrammar);
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            
        }
        void _recognizer_SpeechRecognized_zooming(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            String[] splitSpeech = speech.Split(' ');
            Console.WriteLine(speech);
            if (speech.Contains("Zoom-in"))
            {
                zoominFactor = Array.FindIndex(zoomlevel, value => value.Equals(splitSpeech[2]));
                Console.WriteLine(" Test: " + speech + " : " + zoominFactor);
                zoominMap();
            }
            else if (speech.Contains("Zoom-out"))
            {
                zoomoutFactor = Array.FindIndex(zoomlevel, value => value.Equals(splitSpeech[2]));
                Console.WriteLine(" Test: " + speech + " : " + zoomoutFactor);
                zoomoutMap();
            }
        }
        void _recognizer_SpeechRecognized_travelmode(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            String[] splitSpeech = speech.Split(' ');
            Console.WriteLine(speech);
            if (speech.Contains("from") && speech.Contains("to"))
            {
                Console.WriteLine("How would you like to travel, by driving, transit or walking?");

            }
        }



    }
}

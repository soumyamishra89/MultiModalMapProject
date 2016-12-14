using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;

namespace MultiModalMapProject
{
    // authhor: @yuanchaidoris
    partial class MainWindow
    {
        SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();
        SpeechSynthesizer VAS = new SpeechSynthesizer();
        String[] zoomlevel = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };

        private void initialiseSpeechComponent()
        {
            foreach (RecognizerInfo ri in SpeechRecognitionEngine.InstalledRecognizers())
            {
                Console.WriteLine(" TEST: " + ri.Culture.Name);
            }
            //"Show me" activate pointing
            Choices pointActivate = new Choices("Show me");
            //Zooming
            Choices Zooming = new Choices("Hello", "Zoom-in", "Zoom-out");

            //Route Search
            Choices cities = new Choices(new string[] { "Berlin", "Barcelona", "Paris", "London", "Beijing" });
            GrammarBuilder GB_zooming = new GrammarBuilder(Zooming);
            GrammarBuilder GB_route = new GrammarBuilder();
            GrammarBuilder GB_point = new GrammarBuilder();

            GB_zooming.Append("By");
            GB_zooming.Append(new Choices(zoomlevel));

            GB_route.Append("from");
            GB_route.Append(cities);
            GB_route.Append("to");
            GB_route.Append(cities);

            GB_point.Append(pointActivate);
            GB_point.Append(cities);

            Grammar SudeepGrammer = new Grammar(GB_zooming);
            Grammar routeGrammar = new Grammar(GB_route);
            routeGrammar.Name = ("Route Search");
            Grammar pointGrammar = new Grammar(GB_point);
            pointGrammar.Name = ("Show a place");

            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.LoadGrammar(SudeepGrammer);
            _recognizer.LoadGrammar(routeGrammar);
            _recognizer.LoadGrammar(pointGrammar);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized);
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            InitializeComponent();
        }
        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            String[] splitSpeech = speech.Split(' ');

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
    }
}

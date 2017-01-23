using Microsoft.CognitiveServices.SpeechRecognition;
using MultiModalMapProject.SpeechUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using MultiModalMapProject.JsonSchemas.LUISJson;
using MultiModalMapProject.Util;
using BingMapsRESTToolkit;
using MultiModalMapProject.BingQueryFilters;

namespace MultiModalMapProject
{
    // Copyright (c) Microsoft. All rights reserved.
    // Licensed under the MIT license.
    //
    // Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
    //
    // Microsoft Cognitive Services (formerly Project Oxford) GitHub:
    // https://github.com/Microsoft/Cognitive-Speech-STT-Windows
    //
    // Copyright (c) Microsoft Corporation
    // All rights reserved.
    //
    // MIT License:
    // Permission is hereby granted, free of charge, to any person obtaining
    // a copy of this software and associated documentation files (the
    // "Software"), to deal in the Software without restriction, including
    // without limitation the rights to use, copy, modify, merge, publish,
    // distribute, sublicense, and/or sell copies of the Software, and to
    // permit persons to whom the Software is furnished to do so, subject to
    // the following conditions:

    // The above copyright notice and this permission notice shall be
    // included in all copies or substantial portions of the Software.
    //
    // THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
    // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    // </copyright>
    partial class MainWindow
    {

        /// <summary>
        /// The microphone client
        /// </summary>
        private MicrophoneRecognitionClient micClient;

       
        /// <summary>
        /// Gets or sets subscription key
        /// </summary>
        public string SubscriptionKey
        {
            get { return ConfigurationManager.AppSettings["BingSpeechSubscriptionID"]; }
        }

        /// <summary>
        /// Gets the LUIS application identifier.
        /// </summary>
        /// <value>
        /// The LUIS application identifier.
        /// </value>
        private string LuisAppId
        {
            get { return ConfigurationManager.AppSettings["luisAppID"]; }
        }

        /// <summary>
        /// Gets the LUIS subscription identifier.
        /// </summary>
        /// <value>
        /// The LUIS subscription identifier.
        /// </value>
        private string LuisSubscriptionID
        {
            get { return ConfigurationManager.AppSettings["luisSubscriptionID"]; }
        }

        /// <summary>
        /// Gets the default locale.
        /// </summary>
        /// <value>
        /// The default locale.
        /// </value>
        private string DefaultLocale
        {
            get { return "en-US"; }
        }

        private void InitialiseBingSpeechComponents()
        {
            this.LogRecognitionStart();

            if (this.micClient == null)
            {

                this.CreateMicrophoneRecoClientWithIntent();
                
                this.micClient.StartMicAndRecognition();
            }
        }

        /// <summary>
        /// Logs the recognition start.
        /// </summary>
        private void LogRecognitionStart()
        {
            string recoSource = "microphone";

            this.WriteLine("\n--- Start speech recognition using " + recoSource + "in " + this.DefaultLocale + " language ----\n\n");
        }

        /// <summary>
        /// Creates a new microphone reco client with LUIS intent support.
        /// </summary>
        private void CreateMicrophoneRecoClientWithIntent()
        {
            this.WriteLine("--- Start microphone dictation with Intent detection ----");

            this.micClient =
                SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntent(
                this.DefaultLocale,
                this.SubscriptionKey,
                this.LuisAppId,
                this.LuisSubscriptionID);
            this.micClient.OnIntent += this.OnIntentHandler;

            // Event handlers for speech recognition results
            this.micClient.OnMicrophoneStatus += this.OnMicrophoneStatus;
            //this.micClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.micClient.OnResponseReceived += this.OnMicShortPhraseResponseReceivedHandler;
            this.micClient.OnConversationError += this.OnConversationErrorHandler;
        }

        /// <summary>
        /// Called when a final response is received and its intent is parsed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechIntentEventArgs"/> instance containing the event data.</param>
        private void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            this.WriteLine("--- Intent received by OnIntentHandler() ---");
            if (!string.IsNullOrEmpty(e.Payload))
            {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(e.Payload)))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(JsonSchemas.LUISJson.LUISJsonObject));
                    JsonSchemas.LUISJson.LUISJsonObject luisJson = (JsonSchemas.LUISJson.LUISJsonObject)ser.ReadObject(stream);
                    identifyIntentAndPerformAction(luisJson);
                    Trace.WriteLine(luisJson.Intents[0].IntentValue);
                }
            }
            else
            {
                // TODO not recognised error message
            }
            this.WriteLine("{0}", e.Intent.Body);
            
            // this.WriteLine();
        }

        // identifies what is the intent of the recognised speech and delegates the intent to the corresponding action in the map
        // with the entity, if available. 
        private void identifyIntentAndPerformAction(LUISJsonObject luisJson)
        {
            if (luisJson.Intents.Length > 0)
            {
                // the json result from LUIS returns intent with highest probability as the first result.
                JsonSchemas.LUISJson.Intent probableIntent = luisJson.Intents[0];
                switch (probableIntent.IntentValue.ToUpper())
                {
                    case LUISIntents.SHOW_LOCATION:
                        ProcessShowMeIntent(luisJson);
                        Trace.WriteLine("LUISIntents.SHOW_ME");
                        break;
                    case LUISIntents.SHOW_NEARBY:
                        ProcessShowNearbyIntent(luisJson);
                        break;
                    case LUISIntents.SHOW_ROUTE:// TODO add implementation for this block
                        break;
                    case LUISIntents.ZOOM_IN:
                        HandleZoomMapIntent(luisJson, ZoomFlags.ZOOM_IN);
                        break;
                    case LUISIntents.ZOOM_OUT: HandleZoomMapIntent(luisJson, ZoomFlags.ZOOM_OUT);
                        break;
                    case LUISIntents.RESET: resetApplication();
                        break;
                    default: break;
                }
            }
        }

        // this method processes the SHOW_ME intent by resolving the location it wants to add pushpin to.
        // TODO try to extract location if entity is not identified
        private async void ProcessShowMeIntent(LUISJsonObject luisJson)
        {
            // first the map is cleared of all child elements
            clearMap();
            // the entity whose location is to be identified
            Entity showLocationEntity = null;
            // boolean for any abstract location e.g."here", "there"
            bool isAbstractLocationPresent = false;

            if (luisJson.Entities.Length > 0)
            {    
                foreach (Entity entity in luisJson.Entities)
                {
                    if (entity.Type.Contains(LuisEntityTypes.CITY) || entity.Type.Contains(LuisEntityTypes.ARCHSTRUCTURE))
                    {
                        showLocationEntity = entity;
                    }
                    else if (entity.Type.Contains(LuisEntityTypes.ABSTRACTLOCATION))
                    {
                        isAbstractLocationPresent = true;
                    }
                   
                }
            }
            
            if (null != showLocationEntity)
            {
                Coordinate coordinateOfEntity = await getLocationFromAddress(showLocationEntity.EntityValue);
                if (null != coordinateOfEntity)
                {
                    addPushpinToLocation(coordinateOfEntity.Latitude, coordinateOfEntity.Longitude);
                }
                // if showlocation entity is identified from speech but there is no such location on map
                else
                {
                    // TODO message that location of the address could not be found
                }

                Trace.WriteLine("Inside ProcessShowMeIntent");
            }
            // if abstractLocation like "here", "there" is found then pushpin is added to kinect hand position 
            else if(isAbstractLocationPresent)
            {
                ShowLocationOfKinectHandPoint();
            }
            // if no entities are found and SHOW_LOCATION intent is identified
            // then the pushpin is added to the center of the map where the kinect hand is poiting to
            else
            {
                addPushpinToLocation(myMap.Center.Latitude, myMap.Center.Longitude);
            }
        }

        // this method processes the SHOW_NEARBY intent by resolving the location and adding Places of Interest (POI) near it.
        private async void ProcessShowNearbyIntent(LUISJsonObject luisJson)
        {
            // first the map is cleared of all child elements
            clearMap();
            // the entity whose location is to be identified
            Entity showNearbyEntity = null;
            Entity bingEntity = null;
            // boolean for any abstract location e.g."here", "there"
            bool isAbstractLocationPresent = false;
            if (luisJson.Entities.Length > 0)
            {
                foreach (Entity entity in luisJson.Entities)
                {
                    if (entity.Type.Contains(LuisEntityTypes.CITY) || entity.Type.Contains(LuisEntityTypes.ARCHSTRUCTURE))
                    {
                        showNearbyEntity = entity;
                    }
                    else if (entity.Type.Contains(LuisEntityTypes.BINGENTITYTYPE))
                    {
                        bingEntity = entity;
                    }
                    else if (entity.Type.Contains(LuisEntityTypes.ABSTRACTLOCATION))
                    {
                        isAbstractLocationPresent = true;
                    }        
                }
            }
            if (null != showNearbyEntity || null != bingEntity)
            {
                int? entityId = null;
                Coordinate coordinateOfEntity = null;
                if (null != showNearbyEntity)
                {
                    coordinateOfEntity = await getLocationFromAddress(showNearbyEntity.EntityValue);
                    if (null == coordinateOfEntity)
                    {
                        // TODO message that location of the address could not be found
                        return;
                    }
                }

                if (null != bingEntity)
                {
                    if (StaticVariables.bingPOISearchEntityNameToEntityId.ContainsKey(bingEntity.EntityValue))
                    {
                        entityId = StaticVariables.bingPOISearchEntityNameToEntityId[bingEntity.EntityValue];
                    }
                    if (!entityId.HasValue)
                    {
                        // TODO entity name cannot be recognised error message
                        return;
                    }
                    Trace.WriteLine("Inside ProcessShowMeIntent");
                }

                // adds POI at the location
                if (null != coordinateOfEntity)
                {
                    if (entityId.HasValue)
                    {
                        //  add specific places like coffee, bus stop, bar, based on enity name specified by user
                        addPushPinAtPOI(await getPOIForLocation(coordinateOfEntity.Latitude, coordinateOfEntity.Longitude, new EntityTypeFilter(entityId.Value.ToString())));
                    }
                    else {
                        // finds all POI at the location
                        addPushPinAtPOI(await getPOIForLocation(coordinateOfEntity.Latitude, coordinateOfEntity.Longitude));
                    }
                }
                // in case location is not specified but specific POI is mentioned then POI is found at the place where the user kinect hand is pointed.
                else
                {
                    if (entityId.HasValue)
                    {
                        // if the user mentions a abstractlocation like "here" or "there"
                        if (isAbstractLocationPresent)
                        {
                            addPOIToMapFromKinectHandPosition(new EntityTypeFilter(entityId.Value.ToString()));
                        }
                        // if the user does not mention any location for finding POI then map center is taken as reference location
                        else
                        {
                            addPushPinAtPOI(await getPOIForLocation(myMap.Center.Latitude, myMap.Center.Longitude, new EntityTypeFilter(entityId.Value.ToString())));
                        }
                    }
                }
                
            }
            // if no entities are found and SHOW_NEARBY intent is identified
            // then the POI nearby to the location where the kinect hand is pointing to is found
            else
            {
                addPOIToMapFromKinectHandPosition();
            }
        }


        // this method handles the zoom intent from the recognised speech. 
        // helps in zoom in or out of the map. 
        // the zoom flag tells if the map is to be zoomed in or zoomed out. 
        // zoomFlag = ZOOM_IN means the map is to be zoomed in and ZOOM_OUT means map is to be zoomed out
        private async void HandleZoomMapIntent(LUISJsonObject luisJson, ZoomFlags zoomFlag)
        {
            // Zoom in intent expects a number entity for be able to zoomin bu that factor.
            Entity numberEntity = null;
            // zooming can be done on a particular location. e.g "Zoom in on Berlin by 8"
            Entity locationEntity = null;
            int? zoomFactor = null;
            Coordinate coordinateOfLocationEntity = null;
            // for abstarctLocation like "here", "there"
            bool isAbstractLocationPresent = false;
            if (luisJson.Entities.Length > 0)
            {
                foreach (Entity entity in luisJson.Entities)
                {
                    if (entity.Type.Contains(LuisEntityTypes.NUMBER))
                    {
                        numberEntity = entity;
                    }
                    else if (entity.Type.Contains(LuisEntityTypes.CITY) || entity.Type.Contains(LuisEntityTypes.ARCHSTRUCTURE))
                    {
                        locationEntity = entity;
                    }
                    else if (entity.Type.Contains(LuisEntityTypes.ABSTRACTLOCATION))
                    {
                        isAbstractLocationPresent = true;
                    }
                }
            }
            if (null != numberEntity || null != locationEntity)
            {
                if (null != numberEntity)
                {
                    try
                    {
                        zoomFactor = int.Parse(numberEntity.EntityValue);
                    }
                    catch (Exception e)
                    {
                        // TODO show some error message
                        this.WriteLine("{0}", e.Message);
                    }
                }
                if (null != locationEntity)
                {
                    coordinateOfLocationEntity = await getLocationFromAddress(locationEntity.EntityValue);
                }
            }
            // if the zoom intent is identified but there is no number entity then the zoom is done by a default value
            // sending null value tells the function to zoom by a default value defined in StaticVariables
            // if zoom is directed at a particular location then the center of the map is set to that location and then zooming is done
            if (null != coordinateOfLocationEntity)
            {
                if (ZoomFlags.ZOOM_IN == zoomFlag)
                    setCenterAndZoominLevelOfMap(coordinateOfLocationEntity.Latitude, coordinateOfLocationEntity.Longitude, zoomFactor);
                else if (ZoomFlags.ZOOM_OUT == zoomFlag)
                    setCenterAndZoomoutLevelOfMap(coordinateOfLocationEntity.Latitude, coordinateOfLocationEntity.Longitude, zoomFactor);
            }
            // if no location is specified then following part is executed
            else {
                // abstarctLocation specifies values like "here", "there". which would zoom at the location where kinect hand is pointing to
                if (isAbstractLocationPresent)
                {
                    if (ZoomFlags.ZOOM_IN == zoomFlag)
                        zoominMapAtKinectHandLocation(zoomFactor);
                    else if (ZoomFlags.ZOOM_OUT == zoomFlag)
                        zoomoutMapAtKinectHandLocation(zoomFactor);
                }
                // if only zooming is requested then the zooming is done at the default center
                else {
                    if (ZoomFlags.ZOOM_IN == zoomFlag)
                        zoominMap(zoomFactor);
                    else if (ZoomFlags.ZOOM_OUT == zoomFlag)
                        zoomoutMap(zoomFactor);
                }
            }
        }

        /// Called when an error is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechErrorEventArgs"/> instance containing the event data.</param>
        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            this.WriteLine("--- Error received by OnConversationErrorHandler() ---");
            this.WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
            this.WriteLine("Error text: {0}", e.SpeechErrorText);
            // this.WriteLine();
        }
        /// <summary>
        /// Called when the microphone status has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MicrophoneEventArgs"/> instance containing the event data.</param>
        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                WriteLine("--- Microphone status change received by OnMicrophoneStatus() ---");
                WriteLine("********* Microphone status: {0} *********", e.Recording);
                if (e.Recording)
                {
                    
                    WriteLine("Please start speaking.");
                }
                
                // WriteLine();
            });
        }

        /// <summary>
        /// Called when a final response is received;
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechResponseEventArgs"/> instance containing the event data.</param>
        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                this.WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");

                // we got the final result, so it we can end the mic reco.  No need to do this
                // for dataReco, since we already called endAudio() on it as soon as we were done
                // sending all the data.
                //this.micClient.EndMicAndRecognition();

                this.WriteResponseResult(e);

                this.micClient.StartMicAndRecognition();

    


            }));
        }
        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        private void WriteLine(string format, params object[] args)
        {
            var formattedStr = string.Format(format, args);
            Trace.WriteLine(formattedStr);

        }

        /// <summary>
        /// Writes the response result.
        /// </summary>
        /// <param name="e">The <see cref="SpeechResponseEventArgs"/> instance containing the event data.</param>
        private void WriteResponseResult(SpeechResponseEventArgs  e)
        {
            if (e.PhraseResponse.Results.Length == 0)
            {
                this.WriteLine("No phrase response is available.");
            }
            else
            {
                this.WriteLine("********* Final n-BEST Results *********");
                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    this.WriteLine(
                        "[{0}] Confidence={1}, Text=\"{2}\"",
                        i,
                        e.PhraseResponse.Results[i].Confidence,
                        e.PhraseResponse.Results[i].DisplayText);
                }

                //this.WriteLine();
            }
        }
    }
}


namespace MultiModalMapProject
{

    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Coding4Fun.Kinect.Wpf;
    using System;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Diagnostics;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Width of output drawing : window vertical
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing window horizontal
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 5;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 1;
        //before : 10

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;
        // before : 10

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = Brushes.Black;

        /// <summary>
        /// Brush used for drawing joints that are currently inferred : déduits (pas vus ?)
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Black;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Transparent, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Transparent, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        private DateTime handsclosed = default(DateTime);
        int zoomin = 0;
        int zoomout = 0;
        int time = 0;
        bool isZoomedIn = false;
        bool isZoomedOut = false;
        Joint joint1 = new Joint();
        Joint joint2 = new Joint();
        Joint hip = new Joint();
        Joint head = new Joint();
        int imp_click = 0;
        int imp_click1 = 0;
        int cursorX = 0;
        int cursorY = 0;
        bool move_trigger = true; // implementation of the moving part

        private const float SkeletonMaxX = 0.90f;
        private const float SkeletonMaxY = 0.40f;

        private int counterin = 0;
        private int counterout;

        public struct Pos
        {
            public float X;
            public float Y;
        }
        public Pos HandsClosedL;
        public Pos HandsClosedR;
        public Pos HandLeftZoomout;
        public Pos HandRightZoomout;
        public Pos HandRightMoveRight;
        public Pos HandLeftMoveRight;
        public Pos HandRightMoveLeft;
        public Pos HandLeftMoveLeft;

        bool leftClick=false;
        bool Allow_Zoom = true;


        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
 

        // Create a DrawingVisual that contains a rectangle.
        private DrawingVisual CreateDrawingVisualRectangle(DrawingContext drawingContext)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            // Create a rectangle and draw it in the DrawingContext.
            Rect rect = new Rect(new Point(160, 100), new Size(320, 80));
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.LightGreen, (System.Windows.Media.Pen)null, rect);

            // Persist the drawing content.
            drawingContext.Close();

            return drawingVisual;
        }

        internal struct MouseInput
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        internal struct Input
        {
            public int Type;
            public MouseInput MouseInput;
        }

        public static class NativeMethods
        {

            public static Microsoft.Maps.MapControl.WPF.Location mapLocBeforeClick;
            public const int InputMouse = 0;

            public const int MouseEventMove = 0x0001;
            public const int MouseEventLeftDown = 0x0002;
            public const int MouseEventLeftUp = 0x0004;
            public const int MouseEventRightDown = 0x08;
            public const int MouseEventRightUp = 0x10;
            public const int MouseEventAbsolute = 0x8000;

            public static bool lastLeftDown;

            [DllImport("user32.dll", SetLastError = true)]
            private static extern uint SendInput(uint numInputs, Input[] inputs, int size);

            [DllImport("user32.dll")]
            public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);


            //public static void SendMouseInput(int positionX, int positionY, int maxX, int maxY, bool leftDown)
            public static void SendMouseInput(int positionX, int positionY, int maxX, int maxY, bool leftDown)
            {
                if (positionX > int.MaxValue)
                    throw new ArgumentOutOfRangeException("positionX");
                if (positionY > int.MaxValue)
                    throw new ArgumentOutOfRangeException("positionY");

                Input[] i = new Input[2];

                // move the mouse to the position specified
                i[0] = new Input();
                i[0].Type = InputMouse;
                i[0].MouseInput.X = (positionX * 65535) / maxX;
                //i[0].MouseInput.X = positionX;
                i[0].MouseInput.Y = (positionY * 65535) / maxY;
                i[0].MouseInput.Flags = MouseEventAbsolute | MouseEventMove;

                // determine if we need to send a mouse down or mouse up event
                if(!lastLeftDown && leftDown)
                {
                	i[1] = new Input();
                	i[1].Type = InputMouse;
                	i[1].MouseInput.Flags = MouseEventLeftDown;
                    //i[0].MouseInput.X = 0;

                    //mouse_event(MouseEventAbsolute | MouseEventMove | MouseEventLeftDown, i[0].MouseInput.X, i[0].MouseInput.Y, 0, 0);
                    mouse_event(MouseEventAbsolute | MouseEventMove | MouseEventLeftDown,0, i[0].MouseInput.Y, 0, 0);
                    lastLeftDown = leftDown;
                    Trace.WriteLine("mouse click:"+ i[0].MouseInput.X);
                    return;
                }
                else if(lastLeftDown && !leftDown)
                {
                    i[1] = new Input();
                    i[1].Type = InputMouse;
                    i[1].MouseInput.Flags = MouseEventLeftUp;
                    mouse_event(MouseEventLeftUp, i[0].MouseInput.X, i[0].MouseInput.Y, 0, 0);
                    Trace.WriteLine("mouse not click");
                    lastLeftDown = leftDown;
                    return;
                }

                //lastLeftDown = leftDown;
                mouse_event(MouseEventAbsolute | MouseEventMove, i[0].MouseInput.X, i[0].MouseInput.Y, 0, 0);
                // send it off
                //uint result = SendInput(2, i, Marshal.SizeOf(i[0]));
                //if (result == 0)
                  //  throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }



    /// <summary>
    /// Draws indicators to show which edges are clipping skeleton data
    /// </summary>
    /// <param name="skeleton">skeleton to draw clipping information for</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Transparent,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                Brushes.Transparent,
                null,
                new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                Brushes.Transparent,
                null,
                new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                Brushes.Transparent,
                null,
                new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //PictureBox pb1 = new PictureBox();
            //pb1.ImageLocation = "C:/Users/Marion/Documents/GitHubMultiModalMapProject/android.jpeg";
            //pb1.SizeMode = PictureBoxSizeMode.AutoSize;
        }
        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            KinectCanvas.Source = this.imageSource;
            
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try  
                {
                    this.sensor.Start();
    
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                System.Windows.MessageBox.Show("Kinect Sensor is not Powered");
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();

                /** leftHandX.Sort();
                 leftHandY.Sort();
                 rightHandX.Sort();
                 rightHandX.Sort();
                 Console.WriteLine("Left Hand max X: " + leftHandX[leftHandX.Count - 1] + ", Left Hand min X: " + leftHandX[0] + ", Right Hand max X: " + rightHandX[rightHandX.Count - 1] + ", Right Hand min X: " + rightHandX[0]);
                 Console.WriteLine("Left Hand max Y: " + leftHandY[leftHandY.Count - 1] + ", Left Hand min Y " + leftHandY[0] + ", Right Hand max Y: " + rightHandY[rightHandY.Count - 1] + ", Right Hand min Y: " + rightHandY[0]);        **/
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {


            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {

                Rect rec = new Rect(50.0, 50.0, 50.0, 50.0); // rectangle that helps debugging. Depending on the color it has different meanings
                Joint HandLeft = new Joint();
                Joint HandRight = new Joint();
                Rect rec2 = new Rect(0.0, 0.0, 25.0, 25.0);


                // Draw a transparent background to set the render size
                //dc.DrawImage(imageSource, rec);
                //dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));  // only to say the background has to be dark but actually it's not our case

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);  // draw the skeleton
                            foreach (Joint joint in skel.Joints) // we go through the whole skeleton
                            {
                                if (joint.JointType.Equals(JointType.HipCenter))
                                {
                                    hip.Position = joint.Position; // we track the position of the hip to detect when the hands are down
                                }

                                if (joint.JointType.Equals(JointType.Head))
                                {
                                    head.Position = joint.Position;
                                    if (head.Position.X == 0.0)
                                    {
                                        dc.DrawRectangle(Brushes.Red, null, rec);
                                    }
                                }

                                if (joint.JointType.Equals(JointType.HandLeft) || joint.JointType.Equals(JointType.HandRight))
                                {
                                    if (joint.JointType.Equals(JointType.HandLeft))
                                    {
                                        HandLeft.Position = joint.Position; // we track the position of the left hand because that's what we are interested in

                                    }
                                    else if (joint.JointType.Equals(JointType.HandRight))
                                    {
                                        HandRight.Position = joint.Position; // we track the position of the right hand because that's what we are interested in
                                    }

                                    // TODO not sure
                                }
                            }
                            if (HandRight.Position.X < (0.1f + head.Position.X) && HandLeft.Position.X > (-0.15f + head.Position.X))  // if the hands are closed on to each other in the center
                                    {


                                        // if the hands are closed one to each other, it either means that we start zooming in, or we finish zooming out

                                        if (counterout > 7) // if the hands have decreased from wide opened to close to each other 
                                        //&& !isZoomedIn)
                                        {
                                            zoomoutMap(null); // we zoom in
                                            isZoomedOut = true;
                                            // we initialize the values used before : the wide hands are no longer saved and the compteur goes back to 0
                                            HandLeftZoomout.X = 0.0f;
                                            HandRightZoomout.X = 0.0f;
                                            counterout = 0;

                                        }

                                        // if the hands are closed and we haven't zoomed out, we want to zoom in so we save the positions of the hands. Thanks to that, we will see if they get further to each others
                                        HandsClosedR.X = HandRight.Position.X;
                                        HandsClosedR.Y = HandRight.Position.Y;
                                        HandsClosedL.X = HandLeft.Position.X;
                                        HandsClosedL.Y = HandLeft.Position.Y;
                                    }


                                    if (HandRight.Position.X < (0.35f + head.Position.X) && HandLeft.Position.X > (-0.35f + head.Position.X)) // if the hands are really far one from each other 
                                    {
                                        if (HandsClosedR.X < HandRight.Position.X && HandsClosedL.X > HandLeft.Position.X) // if the hands are further than when they were closed
                                        {
                                            counterin = counterin + 1; // we increase a counter which tells us if they more spaced
                                                                       // if (counterin>8) dc.DrawRectangle(Brushes.Purple, null, rec);
                                        }
                                        else if (HandRight.Position.X < (0.1f + head.Position.X) && HandLeft.Position.X > (-0.15f + head.Position.X)) // if they stay in the middle or return in the middle before getting very wide we initialize everything again
                                        {
                                            counterin = 0;
                                            //dc.DrawRectangle(Brushes.Green, null, rec);
                                        }

                                        if (HandLeftZoomout.X < HandLeft.Position.X && HandRightZoomout.X > HandRight.Position.X) // if the hands get closer and closer
                                        {
                                            counterout = counterout + 1;
                                        }
                                        else if (HandRight.Position.X > (0.45f + head.Position.X) && HandLeft.Position.X < (-0.45f + head.Position.X)) // if they don't and go wide again, we initialize everything
                                        {
                                            counterout = 0;
                                        }
                                    }

                                    if (HandLeft.Position.Y < hip.Position.Y || HandRight.Position.Y < hip.Position.Y)  // if the hands go below the hips, everything is initialized
                                    {
                                        counterin = 0;
                                        counterout = 0;
                                        HandLeftZoomout.X = 0.0f;
                                        HandRightZoomout.X = 0.0f;
                                        HandRightMoveRight.X = 1f;
                                        HandLeftMoveRight.X = -0.5f;
                                        HandsClosedL.X = 0.5f;
                                        HandsClosedR.X = 0.5f;
                                        imp_click = 0;
                                        imp_click1 = 0;
                                        leftClick = false;
                                        //move_trigger = false;
                                        //Allow_Zoom = true;
                                         
                                        dc.DrawRectangle(Brushes.Black, null, rec);
                                    }


                                    if (HandRight.Position.X > (0.3f + head.Position.X) && HandLeft.Position.X < (-0.3f + head.Position.X)) // if the hands are wide 
                                    {

                                        if (counterin > 2)  // if they have been closed before and got wider

                                        {
                                            zoominMap(null); // we zoom in
                                            isZoomedIn = true;
                                            HandsClosedL.X = 0.5f;
                                            HandsClosedR.X = 0.5f;
                                            counterin = 0;
                                            counterout = 0;
                                        }
                                    }

                                    if (HandRight.Position.X > (0.5f + head.Position.X) && HandLeft.Position.X < (-0.5f + head.Position.X)) // if the hands are wide, it can also mean that we want to zoom out, so we save the positions
                                    {
                                        HandLeftZoomout.X = HandLeft.Position.X;
                                        HandRightZoomout.X = HandRight.Position.X;
                                        HandLeftZoomout.Y = HandLeft.Position.Y;
                                        HandRightZoomout.Y = HandRight.Position.Y;
                                    }


                                    //if (HandLeft.Position.Y + 0.3f>HandsClosedL.Y && HandRight.Position.Y<HandsClosedR.Y)
                                    //{
                                    //    imp_click = 1;
                                    //}

                                    //if (HandLeft.Position.Y<HandsClosedL.Y && imp_click==1)
                                    //{
                                    //    imp_click1 = 1;
                                    //}

                                    //if (Math.Abs(head.Position.X+0.1)<Math.Abs(HandLeft.Position.X))
                                    //{
                                    //    imp_click = 0;
                                    //    imp_click1 = 0;
                                        //dc.DrawRectangle(Brushes.Brown, null, rec);
                                    //}

                                    //if (HandLeft.Position.Y < hip.Position.Y)
                                    //{
                                    //    imp_click = 0;
                                    //    imp_click1 = 0;
                                    //}


                                    Joint scaledRight = HandRight.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);
                                    //Joint scaledLeft = HandLeft.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);


                                    {
                                        //cursorX = (int)(HandRight.Position.X*1400);
                                        //cursorY = (int)(HandRight.Position.Y*100);
                                    }
                                    cursorX = (int)scaledRight.Position.X;
                                    cursorY = (int)scaledRight.Position.Y;
                                    //NativeMethods.SendMouseInput(cursorX, cursorY, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);
                                   
                                    //if (HandLeft.Position.Y < HandsClosedL.Y && imp_click == 1 && imp_click1 == 1)
                                    //   if (HandLeft.Position.Y < HandsClosedL.Y && imp_click == 1 && imp_click1 == 1)
                                    //{
                                        //Allow_Zoom = false;
                                        //leftClick = true;
                                        //dc.DrawRectangle(Brushes.Green, null, rec);
                                        //imp_click = 0;
                                        //
                                    //}

                                    if (move_trigger)
                                    {
                                        
                                        dc.DrawRectangle(Brushes.Green, null, rec);
                                //if (!NativeMethods.lastLeftDown && leftClick)
                                //{
                                //    NativeMethods.mapLocBeforeClick = myMap.Center;
                                //}

                                Trace.WriteLine(leftClick + "" + cursorX + " : " + myMap.Center);
                                NativeMethods.SendMouseInput(cursorX, cursorY, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);

                                //if (!NativeMethods.lastLeftDown && leftClick)
                                //{
                                //    myMap.Center = NativeMethods.mapLocBeforeClick;
                                //}
                                //myMap.SetView(StaticVariables.defaultCenter, 0);
                                 //NativeMethods.SendMouseInput(cursorX, cursorY, 1000, 1300, leftClick);
                                Trace.WriteLine(leftClick+ ""+cursorX+" : "+ myMap.Center);
                                        leftClick = true;

                                    }

                                }


                                isZoomedIn = false;
                                isZoomedOut = false;
                          //  }
                        //}
                       

                    }
                }
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// Since we only need to show the hands, all the others tracks and bones are transparent
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.HandRight, JointType.HandLeft);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
                if (joint.JointType == JointType.HandLeft || joint.JointType == JointType.HandRight)
                {
                    Brush drawBrush = null;


                    if (joint.TrackingState == JointTrackingState.Tracked)
                    {
                        drawBrush = this.trackedJointBrush;
                    }
                    else if (joint.TrackingState == JointTrackingState.Inferred)
                    {
                        drawBrush = this.inferredJointBrush;
                    }

                    if (drawBrush != null)
                    {
                        if (joint.JointType == JointType.HandRight)
                        {
                            // stores the right hand position to be used on map
                            kinectHandPositionOnScreen = this.SkeletonPointToScreen(joint.Position);
                        }
                        drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);  // draw everything : needed !
                        
                    }

                }

        }



        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
       
    }
}



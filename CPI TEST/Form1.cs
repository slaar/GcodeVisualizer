using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Web;
using System.IO;
using System.Globalization;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EXT;

namespace GcodeVisualizer
{

    public partial class MainForm : Form
    {
        IList<LineSegment> ThingsToDraw = new List<LineSegment>();
        IList<Drawable> Paths = new List<Drawable>();
        bool loaded = false;
        int index = 0;
        Dictionary<int, IList<string>> TempCodes = new Dictionary<int, IList<string>>();
        Dictionary<int, IList<string>> GCODECommands =
            new Dictionary<int, IList<string>>();
        public float currentX;
        public float currentY;
        public float currentZ;
        public int speed = 1;
        bool INVERTX = false;
        bool INVERTY = false;
        //CAMERA CONTROL STUFF
        bool adjusting_camera = false;
        Point3D RotationAxis = new Point3D(0, 0, 0);
        Point3D CameraAnchor = new Point3D(0, 0, 0);
        float RotationMagnitude = 0;
        public string LastWhich = "";
        //END CAMERA CONTROL


        //ANIMATION STUFF
        Sculpture MAIN;
        bool MAINLoaded = false;
        public static bool animating = false;
        public static double TimeElapsed = 0;
        //END ANIMATION STUFF

        double ViewportX = 0;
        double ViewportY = 0;
        float GlobalScale = 0.2F;
        Point3D HEAD_LOCATION = new Point3D(1, 1, 0);
        bool LASER_ON = true;
        bool incremental = false;
        public System.Drawing.Color DefaultColor = Color.Gray;
        Graphics g;
        IList<double> StepLengths = new List<double>();


        //EDIT STUFF
        public bool EditMode = false;

        //END EDIT STUFF
        public MainForm()
        {
            InitializeComponent();
            Arena.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Arena_MouseWheel);
        }

        Stopwatch sw = new Stopwatch();
        private void Arena_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GlobalScale += (e.Delta / 10000.0F);
        }

        private void Arena_Load(object sender, EventArgs e)
        {
            HEAD_LOCATION = new Point3D(0, 0, 0);
            GL.ClearColor(Color.Black);
            SetupViewport();
            ThingsToDraw.Add(new LineSegment(new Point3D(-0.181434f, -1.541976f, 0.0f), new Point3D(-0.161434f, 0.220898f, 0.0f)));
            ThingsToDraw.Add(new LineSegment(new Point3D(-0.181434f, -1.541976f, 0.0f), new Point3D(-0.171434f, 0.320898f, 0.0f)));
            ThingsToDraw.Add(new LineSegment(new Point3D(-0.181434f, -1.541976f, 0.0f), new Point3D(-0.181434f, 0.420898f, 0.0f)));
            loaded = true;
            sw.Start();
            Application.Idle += Application_Idle;
        }

        private void Animate(double milliseconds)
        {
            if (animating)
                TimeElapsed = TimeElapsed + milliseconds;
            Arena.Invalidate();
        }

        double accumulator = 0;
        int idleCounter = 0;

        private void Accumulate(double milliseconds)
        {
            idleCounter++;
            accumulator += milliseconds;
            if (accumulator > 1000)
            {
                //label1.Text = idleCounter.ToString();
                accumulator -= 1000;
                idleCounter = 0; // don't forget to reset the counter!
            }
        }

        private double ComputeTimeSlice()
        {
            sw.Stop();
            double timeslice = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            return timeslice;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            double milliseconds = ComputeTimeSlice();
            Accumulate(milliseconds);
            Animate(milliseconds);
        }

        private void SetupViewport()
        {
            var w = Arena.Width;
            var h = Arena.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -3, 3); // Bottom-left corner pixel has coordinate (0, 0)
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }

        private void Arena_Resize(object sender, EventArgs e)
        {
            if (!loaded)
                return;
            debugText.AppendText("HI");
        }



        private void debugText_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Declare the string to search for in the control. 
            //string searchString = "G03";
            if (!EditMode)
            {
                // Determine whether the user clicks the left mouse button and whether it is a double click. 
                if (e.Clicks == 1 && e.Button == MouseButtons.Left)
                {
                    // Obtain the character index where the user clicks on the control. 
                    int positionToSearch = debugText.GetCharIndexFromPosition(new Point(e.X, e.Y));
                    // Search for the search string text within the control from the point the user clicked. 
                    int LineNumber = debugText.GetLineFromCharIndex(positionToSearch);
                    int TempCounter = 0;
                    bool found = false;
                    foreach (Drawable item in Paths.ToList())
                    {
                        if (TempCounter == LineNumber)
                        {
                            MAIN.Highlight(item);
                            //Arena.Invalidate();
                            found = true;
                            IList<string> ColumnA = new List<string>();
                            ColumnA = MAIN.ListElements();
                            //int pos = debugText.SelectionStart;
                            Point pos = debugText.ScrollPos;
                            ExRichTextBox buffer = new ExRichTextBox();

                            //debugText.Clear();
                            buffer.Font = new Font("Consolas", debugText.Font.Size);
                            //ColumnB = MAIN2.ListElements();
                            for (int i = 0; i < MAIN.CountElements(); i++)
                            {
                                if (i == LineNumber)
                                {
                                    buffer.SelectionBackColor = Color.LightGreen;
                                }
                                buffer.AppendText(ColumnA[i]);
                            }

                            debugText.Rtf = buffer.Rtf;
                            debugText.ScrollPos = pos;
                        }
                        TempCounter++;
                    }

                    //MessageBox.Show(debugText.Lines[debugText.GetLineFromCharIndex(positionToSearch)]);

                }
            }
            else
            {
                if (e.Clicks == 1 && e.Button == MouseButtons.Left)
                {
                    // Obtain the character index where the user clicks on the control. 
                    int positionToSearch = debugText.GetCharIndexFromPosition(new Point(e.X, e.Y));
                    // Search for the search string text within the control from the point the user clicked. 
                    int LineNumber = debugText.GetLineFromCharIndex(positionToSearch);
                    if (LineNumber > 0)
                    {
                        EditForm E = new EditForm(debugText.Lines[LineNumber],Paths[LineNumber-1]);
                        DialogResult dialogResult = E.ShowDialog(this);
                        if (dialogResult == DialogResult.OK)
                        {
                            Paths[LineNumber] = HandleGCODE(E.EditTextBox.Text,Paths[LineNumber-1]);
                            ToggleEdit();
                            //get user/password values from dialog
                        }
                        //Point3D EP = EndPoint(Paths[LineNumber - 1]);
                        //Paths[LineNumber] = new LineSegment(EP, new Point3D(1, 0, 0));
                    }
                    //MessageBox.Show("You clicked " + LineNumber);
                    MainForm.animating = false;
                    MAIN = new Sculpture(Paths);
                    MAINLoaded = true;
                    //Paths.Clear();
                    //DrawSmile();
                    //Sculpture MAIN2 = new Sculpture(Paths);
                    //MAIN = new Sculpture(Paths);
                    IList<string> ColumnA = new List<string>();
                    //IList<string> ColumnB = new List<string>();
                    foreach (Drawable item in Paths)
                    {
                        StepLengths.Add(item.GetLength());
                    }
                    double TotalDisplayLength = StepLengths.Aggregate((a, b) => b + a);
                    ColumnA = MAIN.ListElements();
                    debugText.Clear();
                    debugText.Font = new Font("Consolas", debugText.Font.Size);
                    //ColumnB = MAIN2.ListElements();
                    for (int i = 0; i < MAIN.CountElements(); i++)
                    {
                        debugText.AppendText(ColumnA[i]);
                    }
                }
            }
        }

        static float NextFloat(Random random)
        {
            var mantissa = (random.NextDouble() * 2.0) - 1.0;
            var exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }
        public Point3D EndPoint(Drawable item)
        {
            if (item is HeadMotion)
            {
                HeadMotion a = (HeadMotion)item;
                return a.EndVertex;
            }
            else if (item is LineSegment)
            {
                LineSegment a = (LineSegment)item;
                return a.EndVertex;
            }
            else if (item is Arc)
            {
                Arc a = (Arc)item;
                return a.EndVertex;
            }
            else
            {
                MessageBox.Show("Somehow EndPoint() received a non-drawable.");
                return new Point3D(0, 0, 0);
            }
        }
        private void Arena_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;

            // render graphics
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            //GL.Ortho(ViewportX, 3 + ViewportX, ViewportY, 3 + ViewportY, -3, 3);
            if (MAINLoaded)
            {
                IDictionary<string, double> off = MAIN.Offsets();
                GL.Translate(GlobalScale * (-1 * (off["XMinimum"] + off["XMaximum"]) / 2), GlobalScale * (-1 * (off["YMinimum"] + off["YMaximum"]) / 2), 0);
                //GL.Translate(-1 , -1 , 0);
            }
            GL.Scale(GlobalScale, GlobalScale, GlobalScale);
            if (RotationMagnitude > 0)
                GL.Rotate(RotationMagnitude, RotationAxis.X, RotationAxis.Y, RotationAxis.Z);

            if (MAINLoaded)
            {
                if (MainForm.animating)
                {
                    var rate = 1100 - (speed * 100);
                    var distance = (TimeElapsed) / rate;
                    MAIN.Draw(distance);
                    double z = 0;
                    int index = -1;
                    foreach (var l in StepLengths)
                    {
                        z += l;
                        index++;
                        if (z >= distance)
                        {
                            break;
                        }
                    }
                    if (index > 0)
                    {
                        string OldText = debugText.Lines[index - 1];
                        debugText.Select(debugText.GetFirstCharIndexFromLine(index - 1), OldText.Length);
                        debugText.SelectionColor = Color.Black;
                    }
                    string text = debugText.Lines[index];
                    debugText.Select(debugText.GetFirstCharIndexFromLine(index), text.Length);
                    debugText.SelectionColor = Color.Red;
                    //debugText.ScrollToCaret();
                    if (distance > MAIN.GetLength())
                    {
                        MainForm.animating = false;
                    }
                    /*
                    if (Math.Floor(TimeElapsed) % 1000 == 0)
                        debugText.AppendText("tick: " + distance + "\n");
                    */
                }
                else
                {
                    MAIN.Draw();
                }

            }
            Arena.SwapBuffers();
        }

        public void MoveHead(out Point3D HEAD_LOCATION, Point3D location)
        {
            HEAD_LOCATION = location;
        }

        public void LaserOn(out bool LASER_ON)
        {
            LASER_ON = true;
        }
        public void LaserOff(out bool LASER_ON)
        {
            LASER_ON = false;
        }
        public bool LaserStatus()
        {
            return LASER_ON;
        }
        private void lineButton_Click(object sender, EventArgs e)
        {/*
            DrawSmile();
            MainForm.animating = true;
            //if (MainForm.animating) debugText.AppendText("FLIP");
            MAIN = new Sculpture(Paths);
            MAINLoaded = true;
        */}
        private void DrawCircle()
        {
            HEAD_LOCATION.X = 1.0F;
            HEAD_LOCATION.Y = -1.0F;
            HEAD_LOCATION.Z = 0;
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(1.0F, 1.0F, 0.0F), new Point3D(-1.0F, 1.0F, 0.0F), 'Z', true));

        }

        private void G01(float x, float y, float z)
        {
            System.Drawing.Color dColor = Color.Black;
            if (LASER_ON)
                dColor = DefaultColor;
            if (INVERTX)
            {
                x = -x;
            }
            if (INVERTY)
            {
                y = -y;
            }
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(x, y, z),dColor));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, z));
        }

        private bool G02(float x, float y, float i, float j)
        {
            System.Drawing.Color dColor = Color.Black;
            if (this.LASER_ON)
                dColor = DefaultColor;
            var prev = Paths.Count();
            //throw new Exception("X: " + x + " HX: " + HEAD_LOCATION.X);
            if (HEAD_LOCATION.X == x && HEAD_LOCATION.Y == y)
                return false;
            if (INVERTX)
            {
                x = -x;
                //i = -i;
            }
            if (INVERTY)
            {
                y = -y;
                //j = -j;
            }
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z), new Point3D(i, j, HEAD_LOCATION.Z), 'Z', true, incremental, dColor));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z));
            if (prev < Paths.Count())
                return true;
            else
                return false;
        }

        private bool G03(float x, float y, float i, float j)
        {
            System.Drawing.Color dColor = Color.Black;
            if (LASER_ON)
                dColor = DefaultColor;
            var prev = Paths.Count();
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z), new Point3D(i, j, HEAD_LOCATION.Z), 'Z', false, incremental, dColor));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z));
            if (INVERTX)
            {
                x = -x;
                //i = -i;
            }
            if (INVERTY)
            {
                y = -y;
                //j = -j;
            }
            if (prev < Paths.Count())
                return true;
            else
                return false;
        }

        private void G00(float x, float y, float z)
        {
            Paths.Add(new HeadMotion(HEAD_LOCATION, new Point3D(x, y, z)));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, z));
        }

        private void LoadGCODEFile_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void AddLine(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.MidnightBlue);
            GL.Vertex3(x1, y1, z1);
            GL.Vertex3(x2, y2, z2);
            GL.End();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (LoadGCODEFile.ShowDialog() == DialogResult.OK)
            {
                debugText.ReadOnly = true;
                debugText.BackColor = Color.LightGray;

                MoveHead(out HEAD_LOCATION, new Point3D(0, 0, 0));
                System.IO.StreamReader file = new System.IO.StreamReader(LoadGCODEFile.FileName);
                this.Text = "GCode Visualizer - " + LoadGCODEFile.FileName;
                GCODECommands.Clear();
                string line;
                IList<string> parsedLine;
                int counter = 0;
                debugText.ScrollBars = RichTextBoxScrollBars.Vertical;
                while ((line = file.ReadLine()) != null)
                {
                    //if (line[0] == 'G')
                    {line.Split(' ').ToList<string>();
                        parsedLine = line.Split(' ').ToList<string>();
                        var debugNum = counter.ToString();
                        //debugText.Text = debugNum;
                        GCODECommands.Add(counter, parsedLine);
                        //debugText.AppendText(debugNum + ": " + line + "\n");
                        counter++;
                    }
                }
                file.Close();
                Paths.Clear();
                foreach (KeyValuePair<int, IList<string>> cmd in GCODECommands)
                {
                    HandleGCODE(cmd.Value);
                }
                MainForm.animating = false;
                MAIN = new Sculpture(Paths);
                MAINLoaded = true;
                //Paths.Clear();
                //DrawSmile();
                //Sculpture MAIN2 = new Sculpture(Paths);
                //MAIN = new Sculpture(Paths);
                IList<string> ColumnA = new List<string>();
                //IList<string> ColumnB = new List<string>();
                foreach (Drawable item in Paths)
                {
                    StepLengths.Add(item.GetLength());
                }
                double TotalDisplayLength = StepLengths.Aggregate((a, b) => b + a);
                ColumnA = MAIN.ListElements();
                debugText.Font = new Font("Consolas", debugText.Font.Size);
                //ColumnB = MAIN2.ListElements();
                for (int i = 0; i < MAIN.CountElements(); i++)
                {
                    debugText.AppendText(ColumnA[i]);
                }

                //debugText.AppendText("Total Length: " + TotalDisplayLength);
                //debugText.Select(1, 2);
                /*
                foreach (Drawable D in Paths)
                {
                    D.Render();
                    debugText.AppendText(D.GetType() + "\n");
                }
                 */
                //string debugNum = counter.ToString();
                //debugText.Text = debugNum;
                //writeArea.ScrollBars = ScrollBars.Vertical;
                //string content = File.ReadAllText(LoadGCODEFile.FileName);
                //writeArea.Text = content;
            }
        }

        private void DrawSphere(Point3D loc)
        {
            float width = 0.08F;
            int i, j;
            int lats = 10;
            int longs = 10;

            for (i = 0; i <= lats; i++)
            {
                double lat0 = Math.PI * (-0.5 + (double)(i - 1) / lats);
                double z0 = Math.Sin(lat0);
                double zr0 = Math.Cos(lat0) * width;

                double lat1 = Math.PI * (-0.5 + (double)i / lats);
                double z1 = Math.Sin(lat1);
                double zr1 = Math.Cos(lat1) * width;

                GL.Begin(PrimitiveType.QuadStrip);
                GL.Color3(Color.Red);
                for (j = 0; j <= longs; j++)
                {
                    double lng = 2 * Math.PI * (double)(j - 1) / longs;
                    double x = Math.Cos(lng);
                    double y = Math.Sin(lng);

                    GL.Normal3((x * zr0) + loc.X, (y * zr0) + loc.Y, z0 * width);
                    GL.Vertex3((x * zr0) + loc.X, (y * zr0) + loc.Y, z0 * width);
                    GL.Normal3((x * zr1) + loc.X, (y * zr1) + loc.Y, z1 * width);
                    GL.Vertex3((x * zr1) + loc.X, (y * zr1) + loc.Y, z1 * width);
                }
                GL.End();
            }
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(loc.X, loc.Y, loc.Z);
            GL.Vertex3(loc.X, loc.Y, loc.Z + 0.5);
            GL.End();
        }

        private void debugText_TextChanged(object sender, EventArgs e)
        {

        }

        private void HandleGCODE(IList<string> cmd)
        {
            string which = "";
            float xComponent;
            float yComponent;
            float zComponent;
            float iComponent;
            float jComponent;
            float kComponent;
            string[] array = cmd.ToArray();
            string TempForIndices = string.Join(" ", array);
            var CommentMarker = TempForIndices.IndexOf(";");
            var OpenParen = TempForIndices.IndexOf("(");
            var CloseParen = TempForIndices.IndexOf(")");
            int index = this.index;
            Dictionary<string, float> args = new Dictionary<string, float>();
            foreach (var bit in cmd)
            {
                if (bit.Count() > 0 && (CommentMarker < 0 || TempForIndices.IndexOf(bit) < CommentMarker) && (OpenParen < 0 || TempForIndices.IndexOf(bit) < OpenParen))
                {
                    int Endpoint = bit.Length-1;
                    if (bit.IndexOf(';') > 0)
                    {
                        Endpoint = bit.IndexOf(';')-1;
                    }
                    if (bit[0] == 'X')
                    {
                        if (float.TryParse(bit.Substring(1, Endpoint), out xComponent))
                            args.Add("X", xComponent);
                        /*
                        xComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        args.Add("X", xComponent);
                    */
                    }
                    else if (bit[0] == 'Y')
                    {
                        if(float.TryParse(bit.Substring(1,Endpoint), out yComponent))
                            args.Add("Y", yComponent);
                    }
                    else if (bit[0] == 'Z')
                    {
                        if (float.TryParse(bit.Substring(1, Endpoint), out zComponent))
                            args.Add("Z", zComponent);
                    }
                    else if (bit[0] == 'I')
                    {
                        if (bit == "If") continue;
                        if (float.TryParse(bit.Substring(1, Endpoint), out iComponent))
                            args.Add("I", iComponent);
                    }
                    else if (bit[0] == 'J')
                    {
                        if (float.TryParse(bit.Substring(1, Endpoint), out jComponent))
                            args.Add("J", jComponent);
                    }
                    else if (bit[0] == 'K')
                    {
                        if (float.TryParse(bit.Substring(1, Endpoint), out kComponent))
                            args.Add("K", kComponent);
                    }
                    else if (bit[0] == 'G' && (bit[1] == '0' || bit[1] == '1' || bit[1] == '2' || bit[1] == '3'))
                    {
                        which = bit;
                    }
                    else if (bit == "G90")
                        incremental = false;  //this is true but you know how it is
                    else if (bit == "G91")
                        incremental = true;  //this is true but you know how it is
                    else if (bit == "M54")
                        LASER_ON = true;
                    else if (bit == "M55")
                        LASER_ON = false;
                }
            }
            if (which == "")
            {
                which = LastWhich;
            }
            if (which == "G00" || which == "G0")
            {
                LastWhich = "G00";
                float x, y, z;
                x = HEAD_LOCATION.X; y = HEAD_LOCATION.Y; z = HEAD_LOCATION.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                if (x == HEAD_LOCATION.X && y == HEAD_LOCATION.Y && z == HEAD_LOCATION.Z)
                    return;
                else
                    G00(x, y, z);  //Z-PROBLEM
                //debugText.AppendText(which + " " + x + " " + y + " " + z + "\n");

            }
            else if (which == "G01" || which == "G1")
            {
                LastWhich = "G01";
                float x, y, z;
                x = HEAD_LOCATION.X; y = HEAD_LOCATION.Y; z = HEAD_LOCATION.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                if (x == HEAD_LOCATION.X && y == HEAD_LOCATION.Y && z == HEAD_LOCATION.Z)
                    return;
                G01(x, y, z);  //Z-PROBLEM
                //debugText.AppendText(which + " " + x + " " + y + " " + z + "\n");
            }
            else if (which == "G02" || which == "G2")
            {
                LastWhich = "G02";
                float x, y, z, i, j, k;
                x = HEAD_LOCATION.X; y = HEAD_LOCATION.Y; z = HEAD_LOCATION.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                if ((x == HEAD_LOCATION.X) && (y == HEAD_LOCATION.Y) && (z == HEAD_LOCATION.Z))
                    return;
                else if ((args.ContainsKey("I")) && (args.ContainsKey("J")))
                {
                    G02(x, y, args["I"], args["J"]);
                }
            }
            else if (which == "G03" || which == "G3")
            {
                LastWhich = "G03";
                float x, y, z, i, j, k;
                x = HEAD_LOCATION.X;
                y = HEAD_LOCATION.Y;
                z = HEAD_LOCATION.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                if ((x == HEAD_LOCATION.X) && (y == HEAD_LOCATION.Y) && (z == HEAD_LOCATION.Z))
                    return;
                else if ((args.ContainsKey("I")) && (args.ContainsKey("J")))
                {
                    if (G03(x, y, args["I"], args["J"]))
                    {
                        //debugText.AppendText("G03 added\n");
                    }
                    //debugText.AppendText(which + " " + x + " " + y + " " + z + " " + args["I"] + " " + args["J"] + "\n");
                }
            }
            //debugText.Clear();
            //drawArea.Image = null;

            /* FOR LATER
            int local_index = 0;
            foreach (KeyValuePair<int, IList<string>> entry in TempCodes)
            {
                red = false;
                string entryValue = entry.Key.ToString() + ": ";
                foreach (string command in entry.Value)
                {
                    entryValue += command + " ";
                }
                if (local_index <= index)
                {
                    debugText.SelectionColor = Color.Red;
                    if (local_index == index)
                    {
                        red = true;
                    }
                    ExecuteGCODE(g, which, args, red);
                }
                else
                {
                    debugText.SelectionColor = Color.Black;
                }

                debugText.AppendText(entryValue + "\n");
                local_index++;
            }
            */

            //debugText.AppendText("Executed up to command " + index + "\n");

            //debugText.AppendText(local_index.ToString() + " " + index.ToString());
        }
        private Drawable HandleGCODE(string line, Drawable prev)
        {
            string which = "";
            float xComponent;
            float yComponent;
            float zComponent;
            float iComponent;
            float jComponent;
            float kComponent;
            var cmd = line.Split(' ').ToList<string>();
            var CommentMarker = cmd.IndexOf(";");
            var OpenParen = cmd.IndexOf("(");
            var CloseParen = cmd.IndexOf(")");
            int index = this.index;
            Drawable PreviousElement = prev;
            var HL = PreviousElement.GetEndVertex();

            Dictionary<string, float> args = new Dictionary<string, float>();
            foreach (var bit in cmd)
            {
                if (bit.Count() > 0 && (CommentMarker < 0 || cmd.IndexOf(bit) < CommentMarker) && (OpenParen < 0 || cmd.IndexOf(bit) < OpenParen))
                {
                    if (bit[0] == 'X')
                    {
                        xComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        args.Add("X", xComponent);
                    }
                    else if (bit[0] == 'Y')
                    {
                        yComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        args.Add("Y", yComponent);
                    }
                    else if (bit[0] == 'Z')
                    {
                        zComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        args.Add("Z", zComponent);
                    }
                    else if (bit[0] == 'I')
                    {
                        iComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        args.Add("I", iComponent);
                    }
                    else if (bit[0] == 'J')
                    {
                        jComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        args.Add("J", jComponent);
                    }
                    else if (bit[0] == 'K')
                    {
                        kComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        args.Add("K", kComponent);
                    }
                    else if (bit[0] == 'G' && (bit[1] == '0' || bit[1] == '1' || bit[1] == '2' || bit[1] == '3'))
                    {
                        which = bit;
                    }
                    else if (bit == "G90")
                        incremental = false;  //this is true but you know how it is
                    else if (bit == "G91")
                        incremental = true;  //this is true but you know how it is
                    else if (bit == "M54")
                        LASER_ON = true;
                    else if (bit == "M55")
                        LASER_ON = false;
                }
            }
            if (which == "")
            {
                which = LastWhich;
            }
            if (which == "G00" || which == "G0")
            {
                LastWhich = "G00";
                float x, y, z;
                x = HL.X; y = HL.Y; z = HL.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                return new HeadMotion(HL, new Point3D(x, y, z));
                //G00(x, y, z);  //Z-PROBLEM
                //debugText.AppendText(which + " " + x + " " + y + " " + z + "\n");

            }
            else if (which == "G01" || which == "G1")
            {
                LastWhich = "G01";
                float x, y, z;
                x = HL.X; y = HL.Y; z = HL.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                return new LineSegment(HL, new Point3D(x, y, z),Color.RoyalBlue);
                //G01(x, y, z);  //Z-PROBLEM
                //debugText.AppendText(which + " " + x + " " + y + " " + z + "\n");
            }
            else if (which == "G02" || which == "G2")
            {
                LastWhich = "G02";
                float x, y, z, i, j, k;
                x = HL.X; y = HL.Y; z = HL.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                if ((args.ContainsKey("I")) && (args.ContainsKey("J")))
                {
                    //Arc NI = new Arc(OI.StartVertex, OI.EndVertex, OI.OffsetVertex, OI.Axis, OI.CW, OI.incremental);

                    return new Arc(HL, new Point3D(x, y, z), new Point3D(args["I"], args["J"], z), 'Z', true, incremental,DefaultColor);
                    //G02(x, y, args["I"], args["J"]);
                }
            }
            else if (which == "G03" || which == "G3")
            {
                LastWhich = "G03";
                float x, y, z, i, j, k;
                x = HL.X;
                y = HL.Y;
                z = HL.Z;
                if (args.ContainsKey("X"))
                {
                    x = args["X"];
                }
                if (args.ContainsKey("Y"))
                {
                    y = args["Y"];
                }
                if (args.ContainsKey("Z"))
                {
                    z = args["Z"];
                }
                if ((args.ContainsKey("I")) && (args.ContainsKey("J")))
                {
                    //Arc NI = new Arc(OI.StartVertex, OI.EndVertex, OI.OffsetVertex, OI.Axis, OI.CW, OI.incremental);

                    return new Arc(HL, new Point3D(x, y, z), new Point3D(args["I"], args["J"], z), 'Z', false, incremental, DefaultColor);
                    //G02(x, y, args["I"], args["J"]);
                }
            }
            return new Drawable();
            //debugText.Clear();
            //drawArea.Image = null;

            /* FOR LATER
            int local_index = 0;
            foreach (KeyValuePair<int, IList<string>> entry in TempCodes)
            {
                red = false;
                string entryValue = entry.Key.ToString() + ": ";
                foreach (string command in entry.Value)
                {
                    entryValue += command + " ";
                }
                if (local_index <= index)
                {
                    debugText.SelectionColor = Color.Red;
                    if (local_index == index)
                    {
                        red = true;
                    }
                    ExecuteGCODE(g, which, args, red);
                }
                else
                {
                    debugText.SelectionColor = Color.Black;
                }

                debugText.AppendText(entryValue + "\n");
                local_index++;
            }
            */

            //debugText.AppendText("Executed up to command " + index + "\n");

            //debugText.AppendText(local_index.ToString() + " " + index.ToString());
        }
        private void RenderDrawable(Drawable item)
        {
            if (item is LineSegment)
            {
                LineSegment l = (LineSegment)item;
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.BlueViolet);
                GL.Vertex3(l.StartVertex.X, l.StartVertex.Y, l.StartVertex.Z);
                GL.Vertex3(l.EndVertex.X, l.EndVertex.Y, l.EndVertex.Z);

                GL.End();

            }
            else if (item is Dot)
            {
                Dot p = (Dot)item;
                GL.PointSize(1);
                GL.Begin(PrimitiveType.Points);
                GL.Color3(Color.White);
                GL.Vertex3(p.X, p.Y, p.Z);
                GL.End();
            }
            else if (item is Arc)
            {
                Arc a = (Arc)item;
                GL.Begin(PrimitiveType.LineStrip);
                GL.Color3(DefaultColor);
                double Angstart = 0;
                double Angfinish = 0;
                double Angiterate = 0;
                double Xiterate = 0;
                double Yiterate = 0;
                double Xoffset = 0;
                double Yoffset = 0;
                double radius = 0;
                double Totalangle = 0;
                double DeltaAngle = 0;

                float X = a.StartVertex.X;
                float Y = a.StartVertex.Y;
                float Xcode = a.EndVertex.X;
                float Ycode = a.EndVertex.Y;
                float Icode = a.OffsetVertex.X;
                float Jcode = a.OffsetVertex.Y;
                incremental = true;
                if (incremental)
                {
                    //Xcode += X;
                    //Ycode += Y;
                    Icode += X;
                    Jcode += Y;
                }
                Xoffset = X - Icode;
                Yoffset = Y - Jcode;

                if (Xoffset > 0 && Yoffset > 0)
                {
                    Angstart = Math.Atan(Yoffset / Xoffset);
                }
                if (Xoffset > 0 && Yoffset < 0)
                {
                    Angstart = Math.Atan(Yoffset / Xoffset) + (2 * Math.PI);
                }
                if (Xoffset < 0 && Yoffset <= 0)
                {
                    Angstart = Math.Atan(Yoffset / Xoffset) + Math.PI;
                }
                if (Xoffset < 0 && Yoffset > 0)
                {
                    Angstart = Math.Atan(Yoffset / Xoffset) + Math.PI;
                }
                if (Xoffset == 0 && Yoffset > 0)
                {
                    Angstart = Math.PI / 2;
                }
                if (Xoffset == 0 && Yoffset < 0)
                {
                    Angstart = -Math.PI / 2;
                }

                Xoffset = Xcode - Icode;
                Yoffset = Ycode - Jcode;

                if (Xoffset > 0 && Yoffset > 0)
                {
                    Angfinish = Math.Atan(Yoffset / Xoffset);
                }
                if (Xoffset > 0 && Yoffset < 0)
                {
                    Angfinish = Math.Atan(Yoffset / Xoffset) + (2 * Math.PI);
                }
                if (Xoffset < 0 && Yoffset <= 0)
                {
                    Angfinish = Math.Atan(Yoffset / Xoffset) + Math.PI;
                }
                if (Xoffset < 0 && Yoffset > 0)
                {
                    Angfinish = Math.Atan(Yoffset / Xoffset) + Math.PI;
                }
                if (Xoffset == 0 && Yoffset > 0)
                {
                    Angfinish = Math.PI / 2;
                }
                if (Xoffset == 0 && Yoffset < 0)
                {
                    Angfinish = -Math.PI / 2;
                }

                if (Angfinish < Angstart && !a.CW)
                {
                    Angfinish += (2 * Math.PI);
                }
                else if (Angfinish > Angstart && a.CW)
                {
                    Angfinish -= (2 * Math.PI);
                }
                radius = Math.Pow(Math.Pow(Yoffset, 2) + Math.Pow(Xoffset, 2), 0.5);

                Totalangle = Angstart - Angfinish;

                if (Totalangle < 0)
                    Totalangle += 2 * Math.PI;

                DeltaAngle = Totalangle / ((2 * Math.PI * radius * (Totalangle / (2 * Math.PI))) / 0.00050);

                Angiterate = Angstart;
                Xiterate = X;
                Yiterate = Y;
                if (a.CW)
                {
                    while (Angiterate > (Angfinish - DeltaAngle))
                    {
                        Xiterate = (radius * Math.Cos(Angiterate)) + Icode;
                        Yiterate = (radius * Math.Sin(Angiterate)) + Jcode;
                        GL.Vertex3(Xiterate, Yiterate, 0);
                        Angiterate -= DeltaAngle;
                    }
                }
                else
                {
                    while (Angiterate < (Angfinish - DeltaAngle))
                    {
                        Xiterate = (radius * Math.Cos(Angiterate)) + Icode;
                        Yiterate = (radius * Math.Sin(Angiterate)) + Jcode;
                        GL.Vertex3(Xiterate, Yiterate, 0);
                        Angiterate += DeltaAngle;
                    }
                }
            }
        }
        private double DistanceBetweenPoints(Point3D a, Point3D b)
        {
            return Math.Pow(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2), 0.5);
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            if (MAINLoaded)
            {
                debugText.AppendText("Total Length: " + MAIN.GetLength());
            }
            /*
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            rotate_angle += 1;
             */
            /*
            foreach (Drawable item in Paths)
            {
                RenderDrawable(item);
            }*/

            //Arena.SwapBuffers();
            /* THE REAL CODE HERE OK
            this.currentX = 0;
            this.currentY = 0;
            //this.index = 0;
            int commands_executed = 0;
            //this.g = drawArea.CreateGraphics();
            if(this.index < TempCodes.Count()){
                //g.Clear(Color.White);
                for (int i = 0; i <= this.index; i++)
                {
                    HandleGCODE(TempCodes[i]);
                }
                this.index++;
                commands_executed++;
            }
            */
        }
        private void ExecuteGCODE(Graphics g, string cmd, Dictionary<string, float> args, bool red)
        {
            float currentX = this.currentX;
            float currentY = this.currentY;
            float x_component = currentX;
            float y_component = currentY;
            float z_component = currentZ;
            float i_component = 0;
            float j_component = 0;
            float k_component = 0;
            foreach (KeyValuePair<string, float> entry in args)
            {
                if (entry.Key == "X")
                {
                    x_component = entry.Value;
                }
                else if (entry.Key == "Y")
                {
                    y_component = entry.Value;
                }
                else if (entry.Key == "Z")
                {
                    z_component = entry.Value;
                }
                else if (entry.Key == "I")
                {
                    i_component = entry.Value;
                }
                else if (entry.Key == "J")
                {
                    j_component = entry.Value;
                }
                else if (entry.Key == "K")
                {
                    k_component = entry.Value;
                }
            }

        }

        private void Arena_MouseClick(object sender, MouseEventArgs e)
        {
            //rotation_magnitude = 0;
            //ChangeScale(0.1F);
        }

        private void Arena_MouseDown(object sender, MouseEventArgs e)
        {
            adjusting_camera = true;
            CameraAnchor.X = e.X;
            CameraAnchor.Y = e.Y;
            debugText.AppendText(String.Format("{0:G3}", (e.X - 208) / 42.0) + " " + String.Format("{0:G3}", -(e.Y - 173) / 42.0) + "\n");
        }

        private void Arena_MouseMove(object sender, MouseEventArgs e)
        {
            if (adjusting_camera)
            {
                float dX = e.X - CameraAnchor.X;
                float dY = e.Y - CameraAnchor.Y;
                RotationMagnitude = (float)DistanceBetweenPoints(new Point3D(dX, dY, 0), new Point3D(0, 0, 0));
                RotationAxis.X = dY;
                RotationAxis.Y = dX;
            }
        }

        private void Arena_MouseUp(object sender, MouseEventArgs e)
        {
            adjusting_camera = false;
        }

        private void FasterButton_Click(object sender, EventArgs e)
        {
            if (speed < 10)
            {
                int temp_rate = 1100 - (speed * 100);
                double distance = (TimeElapsed) / temp_rate;
                speed += 1;
                temp_rate = 1100 - (speed * 100);
                TimeElapsed = (float)((temp_rate) * distance);
            }
        }

        private void SlowerButton_Click(object sender, EventArgs e)
        {
            if (speed > 0)
            {
                int TempRate = 1100 - (speed * 100);
                double distance = (TimeElapsed) / TempRate;
                speed -= 1;
                TempRate = 1100 - (speed * 100);
                TimeElapsed = (float)((TempRate) * distance);
            }
        }

        private void BackUp_Click(object sender, EventArgs e)
        {
            double TotalTime = MAIN.GetLength() * (1100 - (speed * 100));
            double FivePercent = TotalTime / 20;
            TimeElapsed -= FivePercent;
        }

        private void Draw_Click(object sender, EventArgs e)
        {
            if (Paths.Count() > 0)
            {
                MainForm.TimeElapsed = 0;
                MainForm.animating = true;
                if (MainForm.animating) debugText.AppendText("FLIP");
                MAIN = new Sculpture(Paths);
                MAINLoaded = true;
            }
        }

        private void ResetView_Click(object sender, EventArgs e)
        {
            RotationMagnitude = 0;
            GlobalScale = 0.2F;
            SetupViewport();
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            var TestInput = ";Part No.: ZO18-135-090X";
            var cmd = TestInput.Split(' ').ToList<string>();
            var CommentMarker = TestInput.IndexOf(";");
            var OpenParen = cmd.IndexOf("(");
            var CloseParen = cmd.IndexOf(")");
            int index = this.index;
            Dictionary<string, float> args = new Dictionary<string, float>();
            string output = "";
            foreach (var bit in cmd)
            {
                output = output + bit + " " + TestInput.IndexOf(bit) + " " + CommentMarker + "\n";
            }
            MessageBox.Show(output);
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void ToggleEdit()
        {
            if (!EditMode)
            {
                EditMode = true;
                debugText.BackColor = Color.Pink;
            }
            else
            {
                EditMode = false;
                debugText.BackColor = Color.LightGray;
            }

        }
        private void EditButton_Click(object sender, EventArgs e)
        {
            ToggleEdit();
        }

        private void InvertX_CheckedChanged(object sender, EventArgs e)
        {
            INVERTX = !INVERTX;
            ResetDrawing();
        }

        private void InvertY_CheckedChanged(object sender, EventArgs e)
        {
            INVERTY = !INVERTY;
            ResetDrawing();
        }
        private void ResetDrawing()
        {
            IList<Drawable> t= new List<Drawable>();
            foreach (var item in Paths)
            {
                t.Add(item);
            }
            Paths.Clear();
            MoveHead(out HEAD_LOCATION, new Point3D(0,0,0));
            foreach (var item in t)
            {
                if (item is HeadMotion)
                {
                    HeadMotion a = (HeadMotion)item;
                    G00(a.EndVertex.X, a.EndVertex.Y, a.EndVertex.Z);
                }
                if (item is LineSegment)
                {
                    LineSegment a = (LineSegment)item;
                    G01(a.EndVertex.X, a.EndVertex.Y, a.EndVertex.Z);
                }
                if (item is Arc)
                {
                    Arc a = (Arc)item;
                    if(a.CW)
                        G02(a.EndVertex.X, a.EndVertex.Y, a.OffsetVertex.X, a.OffsetVertex.Y);
                    else
                        G03(a.EndVertex.X, a.EndVertex.Y, a.OffsetVertex.X, a.OffsetVertex.Y);
                }
            }
            Arena.Invalidate();
            MAIN = new Sculpture(Paths);
        }
    }
    public class Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Point3D() { }
        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public bool IsSame(Point3D other)
        {
            return ((X == other.X) && (Y == other.Y) && (Z == other.Z));
        }
    }

    public class Drawable
    {
        public Drawable() { }
        public virtual void Render() { }
        public virtual Point3D Render(double distance) { return new Point3D(0, 0, 0); }
        public virtual double GetLength()
        {
            if (this is Arc)
                throw new Exception("No length method defined for this! (Arc) " + this.GetType());
            else if (this is LineSegment)
                throw new Exception("No length method defined for this! (Line) " + this.GetType());
            else
                throw new Exception("No length method defined for this! (???) " + this.GetType());
            return 0;
        }
        public bool IsEqualTo(Drawable d)
        {
            if (this is HeadMotion && d is HeadMotion)
            {
                HeadMotion a = (HeadMotion)this;
                HeadMotion b = (HeadMotion)d;
                if (a.StartVertex.IsSame(b.StartVertex) && a.EndVertex.IsSame(b.EndVertex))
                    return true;
            }
            else if (this is LineSegment && d is LineSegment)
            {
                LineSegment a = (LineSegment)this;
                LineSegment b = (LineSegment)d;
                if (a.StartVertex.IsSame(b.StartVertex) && a.EndVertex.IsSame(b.EndVertex))
                    return true;
            }
            else if (this is Arc && d is Arc)
            {
                Arc a = (Arc)this;
                Arc b = (Arc)d;
                if (a.StartVertex.IsSame(b.StartVertex) && a.EndVertex.IsSame(b.EndVertex) && a.OffsetVertex.IsSame(b.OffsetVertex))
                    return true;
            }
            return false;
        }
        public Point3D StartVertex { get; set; }
        public Point3D EndVertex { get; set; }
        public virtual Point3D GetEndVertex() { return new Point3D(0, 0, 0); }
        public System.Drawing.Color dColor { get; set; }
    }

    public class Dot : Drawable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Dot() { }
        public Dot(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class LineSegment : Drawable
    {
        public Point3D StartVertex { get; set; }
        public Point3D EndVertex { get; set; }
        new public System.Drawing.Color dColor = Color.Gray;
        public override Point3D GetEndVertex()
        {
            return EndVertex;
        }
        public LineSegment() { }
        public LineSegment(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            StartVertex.X = x1;
            StartVertex.Y = y1;
            StartVertex.Z = z1;
            EndVertex.X = x2;
            EndVertex.Y = y2;
            EndVertex.Z = z2;
        }

        public LineSegment(Point3D Start, Point3D End)
        {
            StartVertex = Start;
            EndVertex = End;
        }

        public LineSegment(Point3D Start, Point3D End, System.Drawing.Color in_Color)
        {
            dColor = in_Color;
            StartVertex = Start;
            EndVertex = End;
        }

        public override void Render()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(dColor);
            GL.Vertex3(StartVertex.X, StartVertex.Y, StartVertex.Z);
            GL.Vertex3(EndVertex.X, EndVertex.Y, EndVertex.Z);

            GL.End();

        }

        public override Point3D Render(double distance)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(dColor);
            double total_drawn = 0;
            int max_verts = (int)Math.Floor((GetLength() * 100));
            double Xstep = (EndVertex.X - StartVertex.X) / max_verts;
            double Ystep = (EndVertex.Y - StartVertex.Y) / max_verts;
            double Zstep = (EndVertex.Z - StartVertex.Z) / max_verts;
            double interval = Math.Pow(Math.Pow(Xstep, 2) + Math.Pow(Ystep, 2) + Math.Pow(Zstep, 2), 0.5);
            double endX = StartVertex.X;
            double endY = StartVertex.Y;
            double endZ = StartVertex.Z;
            GL.Vertex3(endX, endY, endZ);
            while (total_drawn <= distance)
            {
                endX += Xstep;
                endY += Ystep;
                endZ += Zstep;
                total_drawn += interval;
            }
            GL.Vertex3(endX, endY, endZ);
            //GL.Vertex3(EndVertex.X, EndVertex.Y, EndVertex.Z);

            GL.End();
            Point3D ret = new Point3D((float)endX, (float)endY, (float)endZ);
            return ret;

        }
        public double MaxX()
        {
            return Math.Max(StartVertex.X, EndVertex.X);
        }
        public double MinX()
        {
            return Math.Min(StartVertex.X, EndVertex.X);
        }
        public double MaxY()
        {
            return Math.Max(StartVertex.Y, EndVertex.Y);
        }
        public double MinY()
        {
            return Math.Min(StartVertex.Y, EndVertex.Y);
        }
        public override double GetLength()
        {
            return Math.Pow(Math.Pow(StartVertex.X - EndVertex.X, 2) + Math.Pow(StartVertex.Y - EndVertex.Y, 2) + Math.Pow(StartVertex.Z - EndVertex.Z, 2), 0.5);
        }
    }

    public class HeadMotion : Drawable
    {
        public Point3D StartVertex { get; set; }
        public Point3D EndVertex { get; set; }
        new public System.Drawing.Color dColor = Color.Gray;
        public bool HighLight;
        public override Point3D GetEndVertex()
        {
            return EndVertex;
        }
        public HeadMotion() { }
        public HeadMotion(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            StartVertex.X = x1;
            StartVertex.Y = y1;
            StartVertex.Z = z1;
            EndVertex.X = x2;
            EndVertex.Y = y2;
            EndVertex.Z = z2;
        }

        public HeadMotion(Point3D Start, Point3D End)
        {
            StartVertex = Start;
            EndVertex = End;
        }

        public HeadMotion(Point3D Start, Point3D End, System.Drawing.Color in_Color)
        {
            dColor = in_Color;
            StartVertex = Start;
            EndVertex = End;
            HighLight = true;
        }
        public override void Render()
        {
            if (HighLight)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.Red);
                GL.Vertex3(StartVertex.X, StartVertex.Y, StartVertex.Z);
                GL.Vertex3(EndVertex.X, EndVertex.Y, EndVertex.Z);
                GL.End();
            }
        }


        public override Point3D Render(double distance)
        {
            double total_drawn = 0;
            int max_verts = (int)Math.Floor((GetLength() * 100));
            double Xstep = (EndVertex.X - StartVertex.X) / max_verts;
            double Ystep = (EndVertex.Y - StartVertex.Y) / max_verts;
            double Zstep = (EndVertex.Z - StartVertex.Z) / max_verts;
            double interval = Math.Pow(Math.Pow(Xstep, 2) + Math.Pow(Ystep, 2) + Math.Pow(Zstep, 2), 0.5);
            double endX = StartVertex.X;
            double endY = StartVertex.Y;
            double endZ = StartVertex.Z;
            while (total_drawn <= distance)
            {
                endX += Xstep;
                endY += Ystep;
                endZ += Zstep;
                total_drawn += interval;
            }

            Point3D ret = new Point3D((float)endX, (float)endY, (float)endZ);
            return ret;

        }
        public double MaxX()
        {
            return Math.Max(StartVertex.X, EndVertex.X);
        }
        public double MinX()
        {
            return Math.Min(StartVertex.X, EndVertex.X);
        }
        public double MaxY()
        {
            return Math.Max(StartVertex.Y, EndVertex.Y);
        }
        public double MinY()
        {
            return Math.Min(StartVertex.Y, EndVertex.Y);
        }
        public override double GetLength()
        {
            return Math.Pow(Math.Pow(StartVertex.X - EndVertex.X, 2) + Math.Pow(StartVertex.Y - EndVertex.Y, 2) + Math.Pow(StartVertex.Z - EndVertex.Z, 2), 0.5);
        }
    }

    public class Arc : Drawable
    {
        public Point3D StartVertex { get; set; }
        public Point3D EndVertex { get; set; }
        public Point3D OffsetVertex { get; set; }
        public Point3D CenterVertex { get; set; }
        new public System.Drawing.Color dColor = Color.Gray;
        public bool HighLight = false;
        public char Axis { get; set; }
        public bool CW { get; set; }
        public bool incremental { get; set; }
        public override Point3D GetEndVertex()
        {
            return EndVertex;
        }
        public Arc() { }

        public Arc(Point3D Start, Point3D End, Point3D Offset, char axis, bool clockwise) //Start = HEAD_LOCATION basically always
        {
            EndVertex = End;
            StartVertex = Start;
            OffsetVertex = Offset;
            Axis = axis;
            CW = clockwise;
            incremental = true;
        }

        public Arc(Point3D Start, Point3D End, Point3D Offset, char axis, bool clockwise, bool increm) //Start = HEAD_LOCATION basically always
        {
            EndVertex = End;
            StartVertex = Start;
            OffsetVertex = Offset;
            Axis = axis;
            incremental = increm;
            CW = clockwise;
        }
        public Arc(Point3D Start, Point3D End, Point3D Offset, char axis, bool clockwise, bool increm, System.Drawing.Color in_Color) //Start = HEAD_LOCATION basically always
        {
            EndVertex = End;
            StartVertex = Start;
            OffsetVertex = Offset;
            Axis = axis;
            incremental = increm;
            CW = clockwise;
            dColor = in_Color;
            if(dColor.Equals(Color.Red))
                HighLight = true;
        }

        private static void CheckError()
        {
            ErrorCode ec = GL.GetError();
            if (ec != 0)
            {
                throw new System.Exception(ec.ToString());
            }
        }

        public IDictionary<string, double> GetAngleInfo()
        {
            IDictionary<string, double> ret = new Dictionary<string, double>();
            double Angstart = 0;
            double Angfinish = 0;
            double Xoffset = 0;
            double Yoffset = 0;
            double radius = 0;
            double XMinimum = 9999;
            double XMaximum = -9999;
            double YMinimum = 9999;
            double YMaximum = -9999;

            float X = StartVertex.X;        //3.8755
            float Y = StartVertex.Y;        //0.9929
            float Xcode = EndVertex.X;      //3.9006
            float Ycode = EndVertex.Y;      //1.1047
            float Icode = OffsetVertex.X;   //-0.2619
            float Jcode = OffsetVertex.Y;   //0
            //G01(3.8755F, 0.9929F, 0.0F);
            //G03(3.9006F, 1.1047F, -0.2619F, 0.0F);

            if (incremental)
            {
                //Xcode += X;
                //Ycode += Y;
                Icode += X;                 //icode = -0.2619 + 3.8755 = 3.6136
                Jcode += Y;                 //jcode = 0 + 0.9929 = 0.9929
            }
            Xoffset = X - Icode;            //XOFFSET = 3.8755 - (3.6136) 0.2619
            Yoffset = Y - Jcode;            //YOFFSET = 0.9929 - 0.9929 = 0.0
            //X3.8948 Y1.1597 I-.2619 J0.

            //throw new Exception("X: " + X + " Y: " + Y + " IC: " + Icode + " JC: " + Jcode + " XO: " + Xoffset + " YO: " + Yoffset + " BA: " + Math.Atan(Yoffset/Xoffset));
            if (Xoffset > 0 && Yoffset > 0)
            {
                Angstart = Math.Atan(Yoffset / Xoffset);
            }
            else if (Xoffset > 0 && Yoffset < 0)
            {
                Angstart = Math.Atan(Yoffset / Xoffset) + (2 * Math.PI);
            }
            else if (Xoffset < 0 && Yoffset <= 0)
            {
                Angstart = Math.Atan(Yoffset / Xoffset) + Math.PI;
            }
            else if (Xoffset < 0 && Yoffset > 0)
            {
                Angstart = Math.Atan(Yoffset / Xoffset) + Math.PI;
            }
            else if (Xoffset == 0 && Yoffset > 0)
            {
                Angstart = Math.PI / 2;
            }
            else if (Xoffset == 0 && Yoffset < 0)
            {
                Angstart = 3 * (Math.PI / 2);
            }

            Xoffset = Xcode - Icode;    //3.9006 - 3.6136 = 0.287
            Yoffset = Ycode - Jcode;    //1.1047 - 0.9929 = 0.1118

            if (Xoffset > 0 && Yoffset > 0)
            {
                Angfinish = Math.Atan(Yoffset / Xoffset);
            }
            else if (Xoffset > 0 && Yoffset < 0)
            {
                Angfinish = Math.Atan(Yoffset / Xoffset) + (2 * Math.PI);
            }
            else if (Xoffset < 0 && Yoffset <= 0)
            {
                Angfinish = Math.Atan(Yoffset / Xoffset) + Math.PI;
            }
            else if (Xoffset < 0 && Yoffset > 0)
            {
                Angfinish = Math.Atan(Yoffset / Xoffset) + Math.PI;
            }
            else if (Xoffset == 0 && Yoffset > 0)
            {
                Angfinish = Math.PI / 2;
            }
            else if (Xoffset == 0 && Yoffset < 0)
            {
                Angfinish = 3 * (Math.PI / 2);
            }
            //throw new Exception("X: " + X + " Y: " + Y + " IC: " + Icode + " JC: " + Jcode + " XO: " + Xoffset + " YO: " + Yoffset + " BA: " + Angfinish);

            //throw new Exception("KC: " + Xcode + " IC : " + Icode + " XO: " + Xoffset + " inc: " + incremental);
            //G02 If Theta2 > Theta1 Then Theta2 = Theta2 - 2 * 3.14159
            //G03 If Theta2 < Theta1 Then Theta1 = Theta1 - 2 * 3.14159

            if (CW)
            {
                while (Angfinish > Angstart)
                {
                    Angfinish -= (2 * Math.PI);
                }
            }
            else
            {
                while (Angfinish < Angstart)
                {
                    Angstart -= 2 * (Math.PI);
                }
            }
            if (CW && (Angstart - Angfinish > (2 * Math.PI)))
            {
                Angstart -= (2 * Math.PI);
            }
            if (!CW && (Angfinish - Angstart > (2 * Math.PI)))
            {
                Angfinish -= (2 * Math.PI);
            }
            //throw new Exception("S: " + Angstart + " F: " + Angfinish);
            double TotalAngle = 0;

            if (CW)
                TotalAngle = Angstart - Angfinish;
            else
                TotalAngle = Angfinish - Angstart;

            radius = Math.Pow(Math.Pow(Yoffset, 2) + Math.Pow(Xoffset, 2), 0.5);


            var DeltaAngle = TotalAngle / ((2 * Math.PI * radius * (TotalAngle / (2 * Math.PI))) / 0.0050);

            var AngIterate = Angstart;

            var Xiterate = (double)X;
            var Yiterate = (double)Y;

            if (CW)
            {
                while (AngIterate > (Angfinish - DeltaAngle))
                {
                    Xiterate = (radius * Math.Cos(AngIterate)) + Icode;
                    Yiterate = (radius * Math.Sin(AngIterate)) + Jcode;
                    if (Yiterate > YMaximum)
                        YMaximum = Yiterate;
                    if (Yiterate < YMinimum)
                        YMinimum = Yiterate;
                    if (Xiterate > XMaximum)
                        XMaximum = Xiterate;
                    if (Xiterate < XMinimum)
                        XMinimum = Xiterate;
                    AngIterate -= DeltaAngle;
                }
            }
            else
            {
                while (AngIterate < (Angfinish - DeltaAngle))
                {
                    Xiterate = (radius * Math.Cos(AngIterate)) + Icode;
                    Yiterate = (radius * Math.Sin(AngIterate)) + Jcode;
                    if (Yiterate > YMaximum)
                        YMaximum = Yiterate;
                    if (Yiterate < YMinimum)
                        YMinimum = Yiterate;
                    if (Xiterate > XMaximum)
                        XMaximum = Xiterate;
                    if (Xiterate < XMinimum)
                        XMinimum = Xiterate;
                    AngIterate += DeltaAngle;
                }
            }


            ret.Add("AngStart", Angstart);
            ret.Add("AngFinish", Angfinish);
            ret.Add("X", X);
            ret.Add("Y", Y);
            ret.Add("ICode", Icode);
            ret.Add("JCode", Jcode);
            ret.Add("Radius", radius);
            ret.Add("TotalAngle", TotalAngle);
            ret.Add("XMinimum", XMinimum);
            ret.Add("XMaximum", XMaximum);
            ret.Add("YMinimum", YMinimum);
            ret.Add("YMaximum", YMaximum);
            return ret;
        }

        public double GetAngle()
        {
            IDictionary<string, double> AI = GetAngleInfo();
            return AI["TotalAngle"];
        }

        public override void Render()
        {
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(dColor);
            /*
            if (HighLight)
                GL.Color3(Color.Red);
            else
                GL.Color3(DefaultColor);
             */
            IDictionary<string, double> AngleInfo = GetAngleInfo();  //{AngStart, AngFinish, X, Y, ICode, JCode, Radius, TotalAngle};
            var AngStart = AngleInfo["AngStart"];
            var AngFinish = AngleInfo["AngFinish"];
            var Radius = AngleInfo["Radius"];
            var XIterate = AngleInfo["X"];
            var YIterate = AngleInfo["Y"];
            var ICode = AngleInfo["ICode"];
            var JCode = AngleInfo["JCode"];
            var TotalAngle = AngleInfo["TotalAngle"];

            var DeltaAngle = TotalAngle / ((2 * Math.PI * Radius * (TotalAngle / (2 * Math.PI))) / 0.0050);

            var AngIterate = AngStart;
            if (CW)
            {
                while (AngIterate > (AngFinish - DeltaAngle))
                {
                    XIterate = (Radius * Math.Cos(AngIterate)) + ICode;
                    YIterate = (Radius * Math.Sin(AngIterate)) + JCode;
                    GL.Vertex3(XIterate, YIterate, StartVertex.Z);
                    AngIterate -= DeltaAngle;
                }
            }
            else
            {
                while (AngIterate < (AngFinish - DeltaAngle))
                {
                    XIterate = (Radius * Math.Cos(AngIterate)) + ICode;
                    YIterate = (Radius * Math.Sin(AngIterate)) + JCode;
                    GL.Vertex3(XIterate, YIterate, StartVertex.Z);
                    AngIterate += DeltaAngle;
                }
            }
            GL.End();
        }

        public override Point3D Render(double distance)
        {
            IDictionary<string, double> AngleInfo = GetAngleInfo();  //{AngStart, AngFinish, X, Y, ICode, JCode, Radius, TotalAngle};
            var AngStart = AngleInfo["AngStart"];
            var AngFinish = AngleInfo["AngFinish"];
            var Radius = AngleInfo["Radius"];
            var XIterate = AngleInfo["X"];
            var YIterate = AngleInfo["Y"];
            var ICode = AngleInfo["ICode"];
            var JCode = AngleInfo["JCode"];
            var TotalAngle = AngleInfo["TotalAngle"];
            if (TotalAngle > (2 * Math.PI))
                throw new Exception("Angle TOO BIG! " + TotalAngle + " " + AngStart + " " + AngFinish);
            //if (TotalAngle < 0)
            //throw new Exception("Angle TOO SMALL!" + TotalAngle);
            //if(!CW)
            //throw new Exception("Angle Info: " + TotalAngle + " " + AngStart + " " + AngFinish);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(dColor);

            var DeltaAngle = TotalAngle / ((2 * Math.PI * Radius * (TotalAngle / (2 * Math.PI))) / 0.0050);
            var AngIterate = AngStart;
            double completed = 0;
            float RetX = 0;
            float RetY = 0;
            float RetZ = 0;
            if (CW)
            {
                while (AngIterate > (AngFinish - DeltaAngle))
                {
                    var XTemp = XIterate;
                    var YTemp = YIterate;
                    XIterate = (Radius * Math.Cos(AngIterate)) + ICode;
                    YIterate = (Radius * Math.Sin(AngIterate)) + JCode;
                    completed += Math.Pow(Math.Pow(XTemp - XIterate, 2) + Math.Pow(YTemp - YIterate, 2), 0.5);
                    if (completed <= distance)
                    {
                        GL.Vertex3(XIterate, YIterate, StartVertex.Z);
                        RetX = (float)XIterate;
                        RetY = (float)YIterate;
                        RetZ = StartVertex.Z;
                    }
                    AngIterate -= DeltaAngle;
                }
            }
            else
            {
                while (AngIterate < (AngFinish - DeltaAngle))
                {
                    var XTemp = XIterate;
                    var YTemp = YIterate;
                    XIterate = (Radius * Math.Cos(AngIterate)) + ICode;
                    YIterate = (Radius * Math.Sin(AngIterate)) + JCode;
                    completed += Math.Pow(Math.Pow(XTemp - XIterate, 2) + Math.Pow(YTemp - YIterate, 2), 0.5);
                    if (completed <= distance)
                    {
                        GL.Vertex3(XIterate, YIterate, StartVertex.Z);
                        RetX = (float)XIterate;
                        RetY = (float)YIterate;
                        RetZ = StartVertex.Z;
                    }
                    AngIterate += DeltaAngle;
                }
            }
            GL.End();
            Point3D Ret = new Point3D(RetX, RetY, RetZ);
            return Ret;

        }

        public override double GetLength()
        {
            IDictionary<string, double> AngleInfo = GetAngleInfo();  //{AngStart, AngFinish, X, Y, ICode, JCode, Radius, TotalAngle};
            var AngStart = AngleInfo["AngStart"];
            var AngFinish = AngleInfo["AngFinish"];
            var Radius = AngleInfo["Radius"];
            var XIterate = AngleInfo["X"];
            var YIterate = AngleInfo["Y"];
            var ICode = AngleInfo["ICode"];
            var JCode = AngleInfo["JCode"];
            var TotalAngle = AngleInfo["TotalAngle"];

            var DeltaAngle = TotalAngle / ((2 * Math.PI * Radius * (TotalAngle / (2 * Math.PI))) / 0.0050);

            var AngIterate = AngStart;

            double Ret = 0;

            if (CW)
            {
                while (AngIterate > (AngFinish - DeltaAngle))
                {
                    var XTemp = XIterate;
                    var YTemp = YIterate;
                    XIterate = (Radius * Math.Cos(AngIterate)) + ICode;
                    YIterate = (Radius * Math.Sin(AngIterate)) + JCode;
                    Ret += Math.Pow(Math.Pow(XTemp - XIterate, 2) + Math.Pow(YTemp - YIterate, 2), 0.5);
                    AngIterate -= DeltaAngle;
                }
            }
            else
            {
                while (AngIterate < (AngFinish - DeltaAngle))
                {
                    var XTemp = XIterate;
                    var YTemp = YIterate;
                    XIterate = (Radius * Math.Cos(AngIterate)) + ICode;
                    YIterate = (Radius * Math.Sin(AngIterate)) + JCode;
                    Ret += Math.Pow(Math.Pow(XTemp - XIterate, 2) + Math.Pow(YTemp - YIterate, 2), 0.5);
                    AngIterate += DeltaAngle;
                }
            }
            return Ret;
        }
    }

    public class Sculpture
    {
        public IList<Drawable> Elements;
        public double TotalDrawn { get; set; }
        public double ItemLength { get; set; }
        public int PinkElement = -1;
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        { // mimic InterleavedArrayFormat.T2fN3fV3f
            public Vector2 TexCoord;
            public Vector3 Normal;
            public Vector3 Position;
        }

        Vertex[] Vertices;
        public Sculpture() { }
        public Sculpture(IList<Drawable> inElements)
        {
            Elements = inElements;
        }

        private void DrawTool(Point3D loc)
        {
            float width = 0.08F;
            int i, j;
            int lats = 10;
            int longs = 10;

            for (i = 0; i <= lats; i++)
            {
                double lat0 = Math.PI * (-0.5 + (double)(i - 1) / lats);
                double z0 = Math.Sin(lat0);
                double zr0 = Math.Cos(lat0) * width;

                double lat1 = Math.PI * (-0.5 + (double)i / lats);
                double z1 = Math.Sin(lat1);
                double zr1 = Math.Cos(lat1) * width;

                GL.Begin(PrimitiveType.QuadStrip);
                GL.Color3(Color.Red);
                for (j = 0; j <= longs; j++)
                {
                    double lng = 2 * Math.PI * (double)(j - 1) / longs;
                    double x = Math.Cos(lng);
                    double y = Math.Sin(lng);

                    GL.Normal3((x * zr0) + loc.X, (y * zr0) + loc.Y, (z0 * width) + loc.Z);
                    GL.Vertex3((x * zr0) + loc.X, (y * zr0) + loc.Y, (z0 * width) + loc.Z);
                    GL.Normal3((x * zr1) + loc.X, (y * zr1) + loc.Y, (z1 * width) + loc.Z);
                    GL.Vertex3((x * zr1) + loc.X, (y * zr1) + loc.Y, (z1 * width) + loc.Z);
                }
                GL.End();
            }
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(loc.X, loc.Y, loc.Z);
            GL.Vertex3(loc.X, loc.Y, loc.Z + 0.5);
            GL.End();
        }
        public IDictionary<string, double> Offsets()
        {
            IDictionary<string, double> ret = new Dictionary<string, double>();
            double XMinimum = 9999;
            double XMaximum = -9999;
            double YMinimum = 9999;
            double YMaximum = -9999;
            foreach (var item in Elements)
            {
                if (item is HeadMotion)
                {
                    HeadMotion a = (HeadMotion)item;
                    if (a.MaxX() > XMaximum)
                    {
                        XMaximum = a.MaxX();
                    }
                    if (a.MaxY() > YMaximum)
                    {
                        YMaximum = a.MaxY();
                    }
                    if (a.MinX() < XMinimum)
                    {
                        XMinimum = a.MinX();
                    }
                    if (a.MinY() < YMinimum)
                    {
                        YMinimum = a.MinY();
                    }
                }
                else if (item is LineSegment)
                {
                    LineSegment a = (LineSegment)item;
                    if (a.MaxX() > XMaximum)
                    {
                        XMaximum = a.MaxX();
                    }
                    if (a.MaxY() > YMaximum)
                    {
                        YMaximum = a.MaxY();
                    }
                    if (a.MinX() < XMinimum)
                    {
                        XMinimum = a.MinX();
                    }
                    if (a.MinY() < YMinimum)
                    {
                        YMinimum = a.MinY();
                    }
                }
                else if (item is Arc)
                {
                    Arc a = (Arc)item;
                    IDictionary<string, double> AI = a.GetAngleInfo();
                    if (AI["XMaximum"] > XMaximum)
                    {
                        XMaximum = AI["XMaximum"];
                    }
                    if (AI["YMaximum"] > YMaximum)
                    {
                        YMaximum = AI["YMaximum"];
                    }
                    if (AI["XMinimum"] < XMinimum)
                    {
                        XMinimum = AI["XMinimum"];
                    }
                    if (AI["YMinimum"] < YMinimum)
                    {
                        YMinimum = AI["YMinimum"];
                    }
                }
            }
            ret.Add("XMinimum", XMinimum);
            ret.Add("XMaximum", XMaximum);
            ret.Add("YMinimum", YMinimum);
            ret.Add("YMaximum", YMaximum);
            return ret;
        }
        public IList<string> ListElements()
        {
            IList<string> ret = new List<string>();

            string t = "";
            foreach (var item in Elements)
            {
                t = "";
                Arc TEMPARC;
                LineSegment TEMPLINE;
                HeadMotion HM;
                //t += item.GetType() + ": ";
                if (item is Arc)
                {
                    TEMPARC = (Arc)item;
                    if (TEMPARC.CW) { t += "G02" + " "; }
                    else { t += "G03" + " "; }
                    t += "X" + TEMPARC.EndVertex.X + " Y" + TEMPARC.EndVertex.Y + " I" + TEMPARC.OffsetVertex.X + " J" + TEMPARC.OffsetVertex.Y;//+ " Angle: " + String.Format("{0:G3}", TEMPARC.GetAngle()) + " Length: " + String.Format("{0:G3}", TEMPARC.GetLength() + " " + TEMPARC.incremental*/);

                }
                else if (item is LineSegment)
                {
                    TEMPLINE = (LineSegment)item;
                    t += "G01" + " " + "X" + TEMPLINE.EndVertex.X + " Y" + TEMPLINE.EndVertex.Y + " Z" + TEMPLINE.EndVertex.Z /*+ " length: " + TEMPLINE.GetLength()*/;
                }
                else if (item is HeadMotion)
                {
                    HM = (HeadMotion)item;
                    t += "G00" + " " + "X" + HM.EndVertex.X + " Y" + HM.EndVertex.Y + " Z" + HM.EndVertex.Z/* + " length: " + HM.GetLength()*/;
                }
                ret.Add(t += "\n");
            }
            return ret;

        }
        public void EditEntry(Drawable item)
        {
            /*animals.RemoveAt(2);
              animals.Insert(2, "snail");*/
            if (PinkElement >= 0)
            {
                Drawable OldItem = Elements[PinkElement];
                if (OldItem is HeadMotion)
                {
                    HeadMotion OI = (HeadMotion)OldItem;
                    HeadMotion NI = new HeadMotion(OI.StartVertex, OI.EndVertex);
                    Elements[PinkElement] = NI;
                    //.RemoveAt(PinkElement);
                    //Elements.Insert(PinkElement, NI);
                }
                if (OldItem is LineSegment)
                {
                    LineSegment OI = (LineSegment)OldItem;
                    LineSegment NI = new LineSegment(OI.StartVertex, OI.EndVertex);
                    Elements[PinkElement] = NI;
                }
                if (OldItem is Arc)
                {
                    Arc OI = (Arc)OldItem;
                    Arc NI = new Arc(OI.StartVertex, OI.EndVertex, OI.OffsetVertex, OI.Axis, OI.CW, OI.incremental, Color.Gray);
                    Elements[PinkElement] = NI;
                }
            }
            int index = -1;
            foreach (Drawable thing in Elements)
            {
                if (thing.IsEqualTo(item))
                {
                    index = Elements.IndexOf(thing);
                }
            }
            if (index >= 0)
            {
                System.Drawing.Color HColor = Color.Red;
                Drawable ToBeHighlighted = Elements[index];
                if (ToBeHighlighted is HeadMotion)
                {
                    HeadMotion HM = (HeadMotion)ToBeHighlighted;
                    HeadMotion Highlighted = new HeadMotion(HM.StartVertex, HM.EndVertex, HColor);
                    Elements[index] = Highlighted;
                    PinkElement = index;
                }
                else if (ToBeHighlighted is LineSegment)
                {
                    LineSegment HM = (LineSegment)ToBeHighlighted;
                    LineSegment Highlighted = new LineSegment(HM.StartVertex, HM.EndVertex, HColor);
                    Elements[index] = Highlighted;
                    PinkElement = index;
                }
                else if (ToBeHighlighted is Arc)
                {
                    Arc HM = (Arc)ToBeHighlighted;
                    Arc Highlighted = new Arc(HM.StartVertex, HM.EndVertex, HM.OffsetVertex, HM.Axis, HM.CW, HM.incremental, HColor);
                    Elements[index] = Highlighted;
                    PinkElement = index;
                }
            }
        }
        public void Highlight(Drawable item)
        {
            /*animals.RemoveAt(2);
              animals.Insert(2, "snail");*/
            if (PinkElement >= 0)
            {
                Drawable OldItem = Elements[PinkElement];
                if (OldItem is HeadMotion)
                {
                    HeadMotion OI = (HeadMotion)OldItem;
                    HeadMotion NI = new HeadMotion(OI.StartVertex, OI.EndVertex);
                    Elements[PinkElement] = NI;
                    //.RemoveAt(PinkElement);
                    //Elements.Insert(PinkElement, NI);
                }
                if (OldItem is LineSegment)
                {
                    LineSegment OI = (LineSegment)OldItem;
                    LineSegment NI = new LineSegment(OI.StartVertex, OI.EndVertex, Color.Gray);
                    Elements[PinkElement] = NI;
                }
                if (OldItem is Arc)
                {
                    Arc OI = (Arc)OldItem;
                    Arc NI = new Arc(OI.StartVertex, OI.EndVertex, OI.OffsetVertex, OI.Axis, OI.CW, OI.incremental, Color.Gray);
                    Elements[PinkElement] = NI;
                }
            }
            int index = -1;
            foreach (Drawable thing in Elements)
            {
                if (thing.IsEqualTo(item))
                {
                    index = Elements.IndexOf(thing);
                }
            }
            if (index >= 0)
            {
                System.Drawing.Color HColor = Color.Red;
                Drawable ToBeHighlighted = Elements[index];
                if (ToBeHighlighted is HeadMotion)
                {
                    HeadMotion HM = (HeadMotion)ToBeHighlighted;
                    HeadMotion Highlighted = new HeadMotion(HM.StartVertex, HM.EndVertex, HColor);
                    Elements[index] = Highlighted;
                    PinkElement = index;
                }
                else if (ToBeHighlighted is LineSegment)
                {
                    LineSegment HM = (LineSegment)ToBeHighlighted;
                    LineSegment Highlighted = new LineSegment(HM.StartVertex, HM.EndVertex, HColor);
                    Elements[index] = Highlighted;
                    PinkElement = index;
                }
                else if (ToBeHighlighted is Arc)
                {
                    Arc HM = (Arc)ToBeHighlighted;
                    Arc Highlighted = new Arc(HM.StartVertex, HM.EndVertex, HM.OffsetVertex, HM.Axis, HM.CW, HM.incremental, HColor);
                    Elements[index] = Highlighted;
                    PinkElement = index;
                }
            }
        }
        public void Draw()
        {
            foreach (Drawable item in Elements)
            {
                item.Render();
                /*
                if (item is LineSegment)
                    item.Render();
                else if (item is HeadMotion)
                    item.Render();
                else if (item is Arc)
                {
                    Arc a = (Arc)item;
                    if (a.GetAngle() < Math.PI * 2 && a.GetAngle() > Math.PI * -2)
                    {
                        item.Render();
                    }
                }*/
            }
        }

        public void Draw(double distance)
        {
            TotalDrawn = 0;
            ItemLength = 0;
            foreach (Drawable item in Elements)
            {
                bool drawn = false;
                ItemLength = item.GetLength();
                if (TotalDrawn + ItemLength < distance)
                {
                    drawn = true;
                    item.Render();
                }
                else if (distance - TotalDrawn > 0)
                {
                    Point3D hl = item.Render(distance - TotalDrawn);
                    TotalDrawn = distance;
                    if (!hl.IsSame(new Point3D(0, 0, 0)))
                        DrawTool(hl);
                }
                if (drawn)
                    TotalDrawn += ItemLength;
            }
        }

        public int CountElements()
        {
            return Elements.Count();
        }
        public double GetLength()
        {
            double ret = 0;
            foreach (Drawable item in Elements)
            {
                ret += item.GetLength();
            }
            return ret;
        }

    }
}
namespace EXT
{
    public static class Extensions
    {
        public static void Toggle(this bool b)
        {
            b = !b;
        }
    }
}

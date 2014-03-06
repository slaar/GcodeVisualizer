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

namespace CPI_TEST
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

        float GlobalScale = 0.2F;
        Point3D HEAD_LOCATION = new Point3D(1, 1, 0);
        bool incremental = false;
        Graphics g;

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

        static float NextFloat(Random random)
        {
            var mantissa = (random.NextDouble() * 2.0) - 1.0;
            var exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

        private void Arena_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;

            // render graphics
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Scale(GlobalScale, GlobalScale, GlobalScale);
            if (RotationMagnitude > 0)
                GL.Rotate(RotationMagnitude, RotationAxis.X, RotationAxis.Y, RotationAxis.Z);

            if (MAINLoaded)
            {
                if (MainForm.animating)
                {
                    var rate = 1100 - (speed * 100);
                    var distance = (TimeElapsed) / rate;
                    MAIN.Draw((TimeElapsed) / rate);
                    if (Math.Floor(TimeElapsed) % 1000 == 0)
                        debugText.AppendText("tick: " + distance + "\n");
                }
                else
                {
                    MAIN.Draw();
                }

            }
            Arena.SwapBuffers();
        }

        private void showButton_Click(object sender, EventArgs e)
        {
            DrawAlien();
            MainForm.animating = true;
            if (MainForm.animating) debugText.AppendText("FLIP");
            MAIN = new Sculpture(Paths);
            MAINLoaded = true;
        }

        public void MoveHead(out Point3D HEAD_LOCATION, Point3D location)
        {
            HEAD_LOCATION = location;
        }

        private void lineButton_Click(object sender, EventArgs e)
        {
            DrawSmile();
            MainForm.animating = true;
            if (MainForm.animating) debugText.AppendText("FLIP");
            MAIN = new Sculpture(Paths);
            MAINLoaded = true;
        }

        private void DrawCircle()
        {
            HEAD_LOCATION.X = 1.0F;
            HEAD_LOCATION.Y = -1.0F;
            HEAD_LOCATION.Z = 0;
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(1.0F, 1.0F, 0.0F), new Point3D(-1.0F, 1.0F, 0.0F), 'Z', true));

        }

        private void G01(float x, float y, float z)
        {
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(x, y, z), Color.White));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, z));
        }

        private bool G02(float x, float y, float i, float j)
        {
            var prev = Paths.Count();
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z), new Point3D(i, j, HEAD_LOCATION.Z), 'Z', true, incremental));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z));
            if (prev < Paths.Count())
                return true;
            else
                return false;
        }

        private bool G03(float x, float y, float i, float j)
        {
            var prev = Paths.Count();
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z), new Point3D(i, j, HEAD_LOCATION.Z), 'Z', false, incremental));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z));
            if (prev < Paths.Count())
                return true;
            else
                return false;
        }

        private void G00(float x, float y, float z)
        {
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(x, y, z), Color.BlueViolet));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, z));
        }
        private void DrawAlien()
        {
            Paths.Clear();
            G00(-0.299635F, -2.672109F, 0);
            G01(0.242283F, HEAD_LOCATION.Y, HEAD_LOCATION.Z);
            G00(0.281184F, -0.761984F, 0);
            G03(0.090095F, -0.953059F, -0.022534F, -0.168553F);
            G00(-0.324529F, -0.753181F, 0);
            G02(-0.132317F, -0.945379F, 0.022666F, -0.169545F);
            G00(-0.475024F, 0.265282F, 0);
            G02(-0.600539F, 0.198916F, -0.145025F, 0.122407F);
            G02(-0.83879F, 0.218446F, -0.069565F, 0.614393F);
            G01(-0.8388F, 0.218448F, 0);
            G02(-1.17323F, 0.343422F, 0.452748F, 1.721514F);
            G02(-1.479371F, 0.527113F, 0.95037F, 1.93085F);
            G02(-1.768868F, 0.851146F, 0.606555F, 0.833245F);
            G02(-1.839332F, 1.06599F, 0.491952F, 0.280324F);
            G03(-1.839333F, 1.066F, -0.000497F, -0.000058F);
            G02(-1.78474F, 1.285138F, 0.301344F, 0.041296F);
            G02(-1.587205F, 1.397775F, 0.243525F, -0.197547F);
            G03(-1.587193F, 1.397777F, -0.000073F, 0.000495F);
            G02(-1.419527F, 1.40102F, 0.094434F, -0.54653F);
            G02(-1.17848F, 1.332849F, -0.115339F, -0.868079F);
            G02(-0.962004F, 1.206631F, -0.44674F, -1.014955F);
            G02(-0.760908F, 1.019047F, -0.766042F, -1.022801F);
            G02(-0.525644F, 0.676937F, -1.265297F, -1.122075F);
            G02(-0.450756F, 0.477071F, -0.79209F, -0.410751F);
            G02(-0.440417F, 0.368565F, -0.342239F, -0.087354F);
            G03(-0.440418F, 0.368546F, 0.000499F, -0.00003F);
            G02(-0.475024F, 0.265282F, -0.183129F, 0.00394F);
            G00(0.457507F, 0.266706F, 0);
            G02(0.422901F, 0.36997F, 0.148522F, 0.107204F);
            G03(0.4229F, 0.369989F, -0.0005F, -0.000011F);
            G02(0.433238F, 0.478489F, 0.352559F, 0.021153F);
            G02(0.508127F, 0.678362F, 0.867003F, -0.210888F);
            G02(0.743386F, 1.020466F, 1.500537F, -0.77995F);
            G02(0.944487F, 1.208056F, 0.967165F, -0.835237F);
            G02(1.160961F, 1.334272F, 0.663209F, -0.888726F);
            G02(1.40201F, 1.402444F, 0.356389F, -0.799915F);
            G02(1.569676F, 1.399201F, 0.073232F, -0.549773F);
            G03(1.569688F, 1.399199F, 0.000085F, 0.000493F);
            G02(1.767223F, 1.286562F, -0.04599F, -0.310184F);
            G02(1.821816F, 1.067424F, -0.246751F, -0.177841F);
            G03(1.821814F, 1.067414F, 0.000495F, -0.000068F);
            G02(1.751352F, 0.852571F, -0.562414F, 0.06548F);
            G02(1.461851F, 0.528535F, -0.896059F, 0.509216F);
            G02(1.155713F, 0.344846F, -1.256498F, 1.747145F);
            G02(0.821283F, 0.219872F, -0.78718F, 1.596542F);
            G01(0.821273F, 0.21987F, 0);
            G02(0.583027F, 0.20034F, -0.168685F, 0.594851F);
            G02(0.457507F, 0.266706F, 0.019508F, 0.188779F);
            G00(2.460439F, 0.585937F, 0);
            G02(2.446297F, 0.275659F, -3.294169F, -0.005312F);
            G02(2.353243F, -0.298841F, -4.012489F, 0.35513F);
            G02(2.027545F, -1.264386F, -5.219711F, 1.223007F);
            G02(1.226249F, -2.57332F, -5.185175F, 2.2745F);
            G02(0.961742F, -2.861901F, -2.578692F, 2.098069F);
            G01(0.961736F, -2.861906F, 0);
            G02(0.742197F, -3.058791F, -1.898965F, 1.896629F);
            G01(0.74219F, -3.058796F, 0);
            G02(0.371109F, -3.30128F, -1.243094F, 1.497168F);
            G02(0.16307F, -3.38167F, -0.54818F, 1.109229F);
            G02(-0.057563F, -3.413565F, -0.231363F, 0.821398F);
            G02(-0.369146F, -3.351629F, 0.011233F, 0.871233F);
            G02(-0.629013F, -3.218586F, 0.486391F, 1.270358F);
            G02(-1.047155F, -2.872594F, 1.371483F, 2.083143F);
            G02(-1.626308F, -2.125388F, 3.153785F, 3.042526F);
            G02(-2.222722F, -0.841132F, 4.913442F, 3.062441F);
            G02(-2.395533F, -0.152417F, 4.918815F, 1.600253F);
            G02(-2.460446F, 0.554692F, 4.223296F, 0.744236F);
            G02(-2.297429F, 1.565427F, 3.14925F, 0.010585F);
            G02(-1.438033F, 2.831134F, 2.723838F, -0.924834F);
            G02(-0.78046F, 3.243225F, 1.730883F, -2.031278F);
            G02(-0.410567F, 3.366616F, 0.959272F, -2.259514F);
            G02(-0.023439F, 3.413565F, 0.383777F, -1.54498F);
            G02(0.490695F, 3.335659F, -0.020852F, -1.873067F);
            G02(1.297896F, 2.930895F, -0.75152F, -2.505977F);
            G02(2.163515F, 1.899368F, -1.681057F, -2.289642F);
            G02(2.460439F, 0.585937F, -2.662859F, -1.292264F);
        }
        private void DrawSmile()
        {
            Paths.Clear();
            G00(2.012176F, -0.000028F, 0);
            G02(1.870461F, -0.740932F, -1.999488F, -0.001556F);
            G00(1.870461F, -0.740932F, 0);
            G02(1.481845F, -1.356568F, -1.865996F, 0.747422F);
            G00(1.481845F, -1.356568F, 0);
            G02(0.901111F, -1.792156F, -1.47533F, 1.362021F);
            G00(0.901111F, -1.792156F, 0);
            G02(0.183056F, -1.992898F, -0.887602F, 1.790346F);
            G00(0.183056F, -1.992898F, 0);
            G02(-0.574636F, -1.91265F, -0.171449F, 1.998346F);
            G00(-0.574636F, -1.91265F, 0);
            G02(-1.222538F, -1.57365F, 0.588588F, 1.913563F);
            G00(-1.222538F, -1.57365F, 0);
            G02(-1.70244F, -1.030477F, 1.242599F, 1.581443F);
            G00(-1.70244F, -1.030477F, 0);
            G02(-1.959544F, -0.337942F, 1.708798F, 1.028384F);
            G00(-1.959544F, -0.337942F, 0);
            G02(-1.942268F, 0.427086F, 1.987973F, 0.337818F);
            G00(-1.942268F, 0.427086F, 0);
            G02(-1.655536F, 1.104596F, 1.954611F, -0.427791F);
            G00(-1.655536F, 1.104596F, 0);
            G02(-1.152582F, 1.626155F, 1.680277F, -1.117055F);
            G00(-1.152582F, 1.626155F, 0);
            G02(-0.488204F, 1.936976F, 1.161573F, -1.617388F);
            G00(-0.488204F, 1.936976F, 0);
            G02(0.274668F, 1.982985F, 0.500894F, -1.957725F);
            G00(0.274668F, 1.982985F, 0);
            G02(0.979122F, 1.751164F, -0.259957F, -1.976203F);
            G00(0.979122F, 1.751164F, 0);
            G02(1.539692F, 1.291073F, -0.977669F, -1.762725F);
            G00(1.539692F, 1.291073F, 0);
            G02(1.901581F, 0.65751F, -1.525463F, -1.291475F);
            G00(1.901581F, 0.65751F, 0);
            G02(2.012176F, -0.000028F, -1.903004F, -0.658147F);
            G00(2.012176F, -0.000028F, 0);
            G00(1.317526F, -0.4665F, 0);
            G03(1.353012F, -0.613573F, 0.166635F, -0.037611F);
            G00(1.353012F, -0.613573F, 0);
            G03(1.420049F, -0.651998F, 0.102419F, 0.100994F);
            G00(1.420049F, -0.651998F, 0);
            G03(1.496966F, -0.660349F, 0.067781F, 0.265906F);
            G00(1.496966F, -0.660349F, 0);
            G00(1.184537F, -0.830645F, 0);
            G02(0.870269F, -1.26847F, -2.636548F, 1.560795F);
            G00(0.870269F, -1.26847F, 0);
            G02(0.677143F, -1.460749F, -1.716911F, 1.531343F);
            G00(0.677143F, -1.460749F, 0);
            G02(0.45307F, -1.61552F, -0.915852F, 1.086361F);
            G00(0.45307F, -1.61552F, 0);
            G03(0.297418F, -1.561688F, -0.435935F, -1.008543F);
            G00(0.297418F, -1.561688F, 0);
            G03(0.133657F, -1.54072F, -0.148131F, -0.506932F);
            G00(0.133657F, -1.54072F, 0);
            G03(0.024497F, -1.570109F, -0.008006F, -0.187679F);
            G00(0.024497F, -1.570109F, 0);
            G03(0.024337F, -1.570276F, 0.000269F, -0.000421F);
            G00(0.024337F, -1.570276F, 0);
            G02(0.020573F, -1.573074F, -0.005224F, 0.003096F);
            G00(0.020573F, -1.573074F, 0);
            G02(0.019207F, -1.572192F, 0.000342F, 0.002028F);
            G00(0.019207F, -1.572192F, 0);
            G03(0.019122F, -1.572094F, -0.000415F, -0.000279F);
            G00(0.019122F, -1.572094F, 0);
            G03(-0.035521F, -1.542826F, -0.090436F, -0.103199F);
            G00(-0.035521F, -1.542826F, 0);
            G03(-0.035544F, -1.54282F, -0.00013F, -0.000483F);
            G00(-0.035544F, -1.54282F, 0);
            G03(-0.181435F, -1.541977F, -0.074923F, -0.341516F);
            G00(-0.181435F, -1.541977F, 0);
            G03(-0.380202F, -1.599832F, 0.199018F, -1.054117F);
            G00(-0.380202F, -1.599832F, 0);
            G02(-0.618567F, -1.431453F, 0.624644F, 1.137186F);
            G00(-0.618567F, -1.431453F, 0);
            G02(-0.917357F, -1.112263F, 1.453132F, 1.659704F);
            G00(-0.917357F, -1.112263F, 0);
            G02(-1.154914F, -0.751063F, 2.064573F, 1.616561F);
            G00(-1.154914F, -0.751063F, 0);
            G03(-0.648702F, -1.188029F, 1.612585F, 1.35643F);
            G00(-0.648702F, -1.188029F, 0);
            G03(-0.172699F, -1.372308F, 0.736533F, 1.195597F);
            G00(-0.172699F, -1.372308F, 0);
            G03(0.575651F, -1.270556F, 0.215078F, 1.220982F);
            G00(0.575651F, -1.270556F, 0);
            G03(0.901233F, -1.076468F, -0.849046F, 1.794397F);
            G00(0.901233F, -1.076468F, 0);
            G03(1.184537F, -0.830645F, -1.575111F, 2.101437F);
            G00(1.184537F, -0.830645F, 0);
            G00(0.133654F, -1.54172F, 0);
            G02(0.297152F, -1.562652F, -0.000003F, -0.649033F);
            G00(0.297152F, -1.562652F, 0);
            G02(0.451946F, -1.616146F, -0.411784F, -1.442275F);
            G00(0.451946F, -1.616146F, 0);
            G02(0.02096F, -1.719761F, -0.401255F, 0.720871F);
            G00(0.02096F, -1.719761F, 0);
            G01(HEAD_LOCATION.X, -1.574199F, 0);
            G00(HEAD_LOCATION.X, -1.574199F, 0);
            G01(0.021039F, -1.573878F, 0);
            G00(0.021039F, -1.573878F, 0);
            G01(0.025062F, -1.570934F, 0);
            G00(0.025062F, -1.570934F, 0);
            G01(0.025069F, -1.570929F, 0);
            G00(0.025069F, -1.570929F, 0);
            G02(0.077066F, -1.548214F, 0.073084F, -0.096428F);
            G00(0.077066F, -1.548214F, 0);
            G03(0.077093F, -1.548209F, -0.000087F, 0.000492F);
            G00(0.077093F, -1.548209F, 0);
            G02(0.133654F, -1.54172F, 0.055971F, -0.238125F);
            G00(0.133654F, -1.54172F, 0);
            G00(0.02F, -1.573914F, 0);
            G01(0.019975F, -1.57402F, 0);
            G00(0.019975F, -1.57402F, 0);
            G03(0.01996F, -1.574138F, 0.000486F, -0.000118F);
            G00(0.01996F, -1.574138F, 0);
            G01(HEAD_LOCATION.X, -1.719714F, 0);
            G00(HEAD_LOCATION.X, -1.719714F, 0);
            G02(-0.37913F, -1.600445F, 0.052055F, 0.901523F);
            G00(-0.37913F, -1.600445F, 0);
            G02(-0.181266F, -1.542962F, 0.319591F, -0.730796F);
            G00(-0.181266F, -1.542962F, 0);
            G03(-0.181236F, -1.542957F, -0.000076F, 0.000494F);
            G00(-0.181236F, -1.542957F, 0);
            G02(-0.035763F, -1.543796F, 0.070856F, -0.326151F);
            G00(-0.035763F, -1.543796F, 0);
            G02(0.018432F, -1.572818F, -0.027245F, -0.11599F);
            G00(0.018432F, -1.572818F, 0);
            G03(0.02F, -1.573914F, 0.003055F, 0.0027F);
            G00(0.02F, -1.573914F, 0);
            G00(-0.380437F, -1.599926F, 0);
            G02(-0.181434F, -1.541976F, 0.320898F, -0.731315F);
            G00(-0.181434F, -1.541976F, 0);
            G02(-0.03554F, -1.542822F, 0.071054F, -0.327132F);
            G00(-0.03554F, -1.542822F, 0);
            G02(0.019134F, -1.572105F, -0.027473F, -0.116973F);
            G00(0.019134F, -1.572105F, 0);
            G02(0.019169F, -1.572141F, -0.000342F, -0.000365F);
            G00(0.019169F, -1.572141F, 0);
            G03(0.0205F, -1.573033F, 0.002318F, 0.002023F);
            G00(0.0205F, -1.573033F, 0);
            G01(0.024465F, -1.570132F, 0);
            G00(0.024465F, -1.570132F, 0);
            G02(0.076878F, -1.547232F, 0.073688F, -0.097225F);
            G00(0.076878F, -1.547232F, 0);
            G02(0.133655F, -1.54072F, 0.056185F, -0.239097F);
            G00(0.133655F, -1.54072F, 0);
            G02(0.29741F, -1.561686F, -0.000004F, -0.650029F);
            G00(0.29741F, -1.561686F, 0);
            G02(0.297421F, -1.561689F, -0.000126F, -0.000484F);
            G00(0.297421F, -1.561689F, 0);
            G02(0.453287F, -1.615614F, -0.412053F, -1.443239F);
            G00(0.453287F, -1.615614F, 0);
            G00(-1.157502F, -0.746436F, 0);
            G01(-1.157366F, -0.746585F, 0);
            G00(-1.157366F, -0.746585F, 0);
            G02(-1.157297F, -0.746682F, -0.00037F, -0.000336F);
            G00(-1.157297F, -0.746682F, 0);
            G03(-0.824995F, -1.224645F, 2.361019F, 1.286996F);
            G00(-0.824995F, -1.224645F, 0);
            G03(-0.618573F, -1.431448F, 1.491865F, 1.282694F);
            G00(-0.618573F, -1.431448F, 0);
            G03(-0.379986F, -1.599951F, 1.041638F, 1.221718F);
            G00(-0.379986F, -1.599951F, 0);
            G03(0.020483F, -1.719744F, 0.454564F, 0.790329F);
            G00(0.020483F, -1.719744F, 0);
            G03(0.452857F, -1.615638F, 0.025623F, 0.843511F);
            G00(0.452857F, -1.615638F, 0);
            G01(0.452865F, -1.615634F, 0);
            G00(0.452865F, -1.615634F, 0);
            G03(0.677139F, -1.460753F, -0.538412F, 1.019463F);
            G00(0.677139F, -1.460753F, 0);
            G03(0.956651F, -1.163987F, -1.315277F, 1.518819F);
            G00(0.956651F, -1.163987F, 0);
            G03(1.186533F, -0.827268F, -2.543872F, 1.983565F);
            G00(1.186533F, -0.827268F, 0);
            G02(1.187052F, -0.826429F, 0.002039F, -0.000681F);
            G00(1.187052F, -0.826429F, 0);
            G00(0.761877F, 0.468794F, 0);
            G02(0.751313F, 0.403023F, -0.185749F, -0.0039F);
            G00(0.751313F, 0.403023F, 0);
            G03(0.751302F, 0.402987F, 0.000471F, -0.000167F);
            G00(0.751302F, 0.402987F, 0);
            G02(0.734624F, 0.372283F, -0.073146F, 0.019852F);
            G00(0.734624F, 0.372283F, 0);
            G03(0.734606F, 0.372263F, 0.000373F, -0.000334F);
            G00(0.734606F, 0.372263F, 0);
            G02(0.703309F, 0.357285F, -0.031232F, 0.025073F);
            G00(0.703309F, 0.357285F, 0);
            G02(0.672022F, 0.37225F, 0.00042F, 0.041067F);
            G00(0.672022F, 0.37225F, 0);
            G02(0.64474F, 0.468781F, 0.131009F, 0.089147F);
            G00(0.64474F, 0.468781F, 0);
            G03(HEAD_LOCATION.X, 0.468815F, -0.000499F, 0.000023F);
            G00(HEAD_LOCATION.X, 0.468815F, 0);
            G02(0.655304F, 0.53458F, 0.185135F, 0.003992F);
            G00(0.655304F, 0.53458F, 0);
            G03(0.655315F, 0.534616F, -0.000471F, 0.000167F);
            G00(0.655315F, 0.534616F, 0);
            G02(0.671994F, 0.565315F, 0.073173F, -0.019874F);
            G00(0.671994F, 0.565315F, 0);
            G03(0.672011F, 0.565335F, -0.000373F, 0.000333F);
            G00(0.672011F, 0.565335F, 0);
            G02(0.703308F, 0.580311F, 0.031241F, -0.025097F);
            G00(0.703308F, 0.580311F, 0);
            G02(0.734596F, 0.565348F, -0.000409F, -0.041048F);
            G00(0.734596F, 0.565348F, 0);
            G02(0.761878F, 0.468828F, -0.131079F, -0.089166F);
            G00(0.761878F, 0.468828F, 0);
            G03(0.761877F, 0.468794F, 0.000499F, -0.000023F);
            G00(0.761877F, 0.468794F, 0);
            G00(0.921559F, 0.583582F, 0);
            G02(0.885821F, 0.363964F, -0.621296F, -0.011615F);
            G00(0.885821F, 0.363964F, 0);
            G03(0.885815F, 0.363947F, 0.000471F, -0.000167F);
            G00(0.885815F, 0.363947F, 0);
            G02(0.79398F, 0.22309F, -0.298299F, 0.094118F);
            G00(0.79398F, 0.22309F, 0);
            G02(0.671694F, 0.203696F, -0.077349F, 0.092483F);
            G00(0.671694F, 0.203696F, 0);
            G02(0.610443F, 0.250149F, 0.062073F, 0.145454F);
            G00(0.610443F, 0.250149F, 0);
            G02(0.568352F, 0.314705F, 0.26387F, 0.218046F);
            G00(0.568352F, 0.314705F, 0);
            G02(0.512162F, 0.52795F, 0.503829F, 0.246786F);
            G00(0.512162F, 0.52795F, 0);
            G02(0.530632F, 0.74777F, 0.747421F, 0.047886F);
            G00(0.530632F, 0.74777F, 0);
            G02(0.591136F, 0.893077F, 0.451134F, -0.102597F);
            G00(0.591136F, 0.893077F, 0);
            G02(0.647697F, 0.952145F, 0.18162F, -0.117296F);
            G00(0.647697F, 0.952145F, 0);
            G02(0.726762F, 0.972349F, 0.067766F, -0.100386F);
            G00(0.726762F, 0.972349F, 0);
            G02(0.810659F, 0.929753F, -0.015439F, -0.134327F);
            G00(0.810659F, 0.929753F, 0);
            G03(0.810675F, 0.929736F, 0.000367F, 0.000339F);
            G00(0.810675F, 0.929736F, 0);
            G02(0.86476F, 0.852481F, -0.182951F, -0.185639F);
            G00(0.86476F, 0.852481F, 0);
            G02(0.921559F, 0.583602F, -0.545402F, -0.255651F);
            G00(0.921559F, 0.583602F, 0);
            G03(HEAD_LOCATION.X, 0.583582F, 0.0005F, -0.000011F);
            G00(HEAD_LOCATION.X, 0.583582F, 0);
            G00(-0.458352F, 0.58898F, 0);
            G02(-0.49409F, 0.369362F, -0.621296F, -0.011615F);
            G00(-0.49409F, 0.369362F, 0);
            G03(-0.494096F, 0.369346F, 0.000471F, -0.000167F);
            G00(-0.494096F, 0.369346F, 0);
            G02(-0.585931F, 0.228489F, -0.298299F, 0.094118F);
            G00(-0.585931F, 0.228489F, 0);
            G02(-0.708217F, 0.209094F, -0.077349F, 0.092483F);
            G00(-0.708217F, 0.209094F, 0);
            G02(-0.769463F, 0.255547F, 0.062283F, 0.145717F);
            G00(-0.769463F, 0.255547F, 0);
            G02(-0.811559F, 0.320103F, 0.265371F, 0.219048F);
            G00(-0.811559F, 0.320103F, 0);
            G02(-0.867743F, 0.533348F, 0.503807F, 0.246764F);
            G00(-0.867743F, 0.533348F, 0);
            G02(-0.849272F, 0.753168F, 0.747445F, 0.047881F);
            G00(-0.849272F, 0.753168F, 0);
            G02(-0.788769F, 0.898475F, 0.451048F, -0.102559F);
            G00(-0.788769F, 0.898475F, 0);
            G02(-0.73221F, 0.957544F, 0.18153F, -0.117205F);
            G00(-0.73221F, 0.957544F, 0);
            G02(-0.653149F, 0.977748F, 0.067755F, -0.100343F);
            G00(-0.653149F, 0.977748F, 0);
            G02(-0.569252F, 0.935151F, -0.015439F, -0.134327F);
            G00(-0.569252F, 0.935151F, 0);
            G03(-0.569235F, 0.935134F, 0.000367F, 0.000339F);
            G00(-0.569235F, 0.935134F, 0);
            G02(-0.515151F, 0.857879F, -0.182951F, -0.185639F);
            G00(-0.515151F, 0.857879F, 0);
            G02(-0.458352F, 0.589001F, -0.545402F, -0.255651F);
            G00(-0.458352F, 0.589001F, 0);
            G03(HEAD_LOCATION.X, 0.58898F, 0.0005F, -0.000011F);
            G00(HEAD_LOCATION.X, 0.58898F, 0);
            G00(-0.590893F, 0.477868F, 0);
            G02(-0.601452F, 0.412097F, -0.185597F, -0.003937F);
            G00(-0.601452F, 0.412097F, 0);
            G03(-0.601464F, 0.412061F, 0.000471F, -0.000167F);
            G00(-0.601464F, 0.412061F, 0);
            G02(-0.618141F, 0.381357F, -0.073185F, 0.019871F);
            G00(-0.618141F, 0.381357F, 0);
            G03(-0.618158F, 0.381337F, 0.000373F, -0.000333F);
            G00(-0.618158F, 0.381337F, 0);
            G02(-0.649461F, 0.366359F, -0.031262F, 0.025134F);
            G00(-0.649461F, 0.366359F, 0);
            G02(-0.680746F, 0.381324F, 0.000411F, 0.041043F);
            G00(-0.680746F, 0.381324F, 0);
            G02(-0.708017F, 0.477855F, 0.131165F, 0.089173F);
            G00(-0.708017F, 0.477855F, 0);
            G03(HEAD_LOCATION.X, 0.477889F, -0.000499F, 0.000023F);
            G00(HEAD_LOCATION.X, 0.477889F, 0);
            G02(-0.697459F, 0.54366F, 0.185528F, 0.003952F);
            G00(-0.697459F, 0.54366F, 0);
            G03(-0.697448F, 0.543696F, -0.000471F, 0.000167F);
            G00(-0.697448F, 0.543696F, 0);
            G02(-0.680775F, 0.5744F, 0.073091F, -0.019814F);
            G00(-0.680775F, 0.5744F, 0);
            G03(-0.680757F, 0.57442F, -0.000372F, 0.000334F);
            G00(-0.680757F, 0.57442F, 0);
            G02(-0.649463F, 0.589398F, 0.031236F, -0.025081F);
            G00(-0.649463F, 0.589398F, 0);
            G02(-0.618175F, 0.574433F, -0.00042F, -0.041067F);
            G00(-0.618175F, 0.574433F, 0);
            G02(-0.590893F, 0.477902F, -0.131009F, -0.089147F);
            G00(-0.590893F, 0.477902F, 0);
            G03(HEAD_LOCATION.X, 0.477868F, 0.000499F, -0.000023F);
            G00(HEAD_LOCATION.X, 0.477868F, 0);
            G00(-1.248858F, -0.628706F, 0);
            G03(-1.15735F, -0.746604F, 2.194333F, 1.608708F);
            G00(-1.15735F, -0.746604F, 0);
            G03(-0.648177F, -1.187178F, 1.615011F, 1.35196F);
            G00(-0.648177F, -1.187178F, 0);
            G03(-0.172514F, -1.371325F, 0.736007F, 1.194744F);
            G00(-0.172514F, -1.371325F, 0);
            G03(0.575221F, -1.269653F, 0.214895F, 1.21998F);
            G00(0.575221F, -1.269653F, 0);
            G03(0.900641F, -1.075662F, -0.848624F, 1.793506F);
            G00(0.900641F, -1.075662F, 0);
            G03(1.186638F, -0.827122F, -1.574474F, 2.100578F);
            G00(1.186638F, -0.827122F, 0);
            G03(1.369388F, -0.628673F, -3.398123F, 3.312692F);
            G00(1.369388F, -0.628673F, 0);
            G00(-1.411634F, -0.681153F, 0);
            G03(-1.264561F, -0.645667F, 0.037611F, 0.166635F);
            G00(-1.264561F, -0.645667F, 0);
            G03(-1.226137F, -0.57863F, -0.100994F, 0.102419F);
            G00(-1.226137F, -0.57863F, 0);
            G03(-1.217786F, -0.501712F, -0.265906F, 0.067781F);
            G00(-1.217786F, -0.501712F, 0);
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
                MoveHead(out HEAD_LOCATION, new Point3D(0, 0, 0));
                System.IO.StreamReader file = new System.IO.StreamReader(LoadGCODEFile.FileName);
                GCODECommands.Clear();
                string line;
                IList<string> parsedLine;
                int counter = 0;
                debugText.ScrollBars = RichTextBoxScrollBars.Vertical;
                while ((line = file.ReadLine()) != null)
                {
                    //if (line[0] == 'G')
                    {
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
                ColumnA = MAIN.ListElements();
                //ColumnB = MAIN2.ListElements();
                for (int i = 0; i < MAIN.CountElements(); i++)
                {
                    debugText.AppendText(ColumnA[i]);
                }
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
            int index = this.index;
            Dictionary<string, float> args = new Dictionary<string, float>();
            foreach (var bit in cmd)
            {
                if (bit[0] == 'X')
                {
                    xComponent = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    args.Add("X", xComponent);
                }
                else if (bit[0] == 'Y')
                {
                    yComponent = Convert.ToSingle(bit.Substring(1));
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
                    incremental = true;  //this is true but you know how it is
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
                    G00(x, y, 0.01F);  //Z-PROBLEM
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
                G01(x, y, 0.01F);  //Z-PROBLEM
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
                if ((args.ContainsKey("I")) && (args.ContainsKey("J")))
                {
                    if (G02(x, y, args["I"], args["J"]))
                    {
                        // debugText.AppendText("G02 added\n");
                    }
                    //debugText.AppendText(which + " " + x + " " + y + " " + z + " " + args["I"] + " " + args["J"] + "\n");
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
                if ((args.ContainsKey("I")) && (args.ContainsKey("J")))
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
                GL.Color3(Color.Wheat);
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
            debugText.AppendText(String.Format("{0:G3}",(e.X-208)/42.0) + " " + String.Format("{0:G3}",-(e.Y-173)/42.0) + "\n");
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
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            G00(0F, 0F, 0F);
            G00(0.1027F, 2.064F, 0F);
            G00(0F, 0F, 0F);
            G01(0.3367F, 2.028F, 0F);
            G01(0F, 0F, 0F);
            G00(0.3511F, 2.0281F, 0F);
            /*debugText.Clear();
            foreach (var item in Paths)
            {
                if (item is Arc)
                {
                    Arc a = (Arc)item;
                    if (a.GetLength() < 0.001)
                        debugText.AppendText("Arc " + a.CW + " " + a.GetLength() + " | " + a.StartVertex.X + " " + a.StartVertex.Y + " " + a.OffsetVertex.X + " " + a.OffsetVertex.Y + " " + a.EndVertex.X + " " + a.EndVertex.Y + "\n");
                }
            }
             */
            /*
            Debug.WriteLine("HI");
            Paths.Clear();
            incremental = true;
             */
            /*
            G00(5.3654F, 4.0408F, 0F);
            G02(5.2087F, 3.6829F, -0.5713F, 0.0369F);
            G02(5.0347F, 3.5813F, -0.2712F, 0.2647F);
            */
            /*
            G00(5.0347F, 3.5813F, 0F);
            G00((float)(5.0347 - 0.2379), (float)(3.5813 + 1.0092), 0F);
            G00(4.7968F, 3.5536F, 0F);
            G00(5.0347F, 3.5813F, 0F);
            G02(4.7968F, 3.5536F, -0.2379F, 1.0092F);
            */
            //G00(0F, 0F, 0F);
            //G00(0F, -1F, 0F);
            //G02(-1F, 0F, 0F, 1F);
            /*            G02(5.2087F, 3.6829F, -0.2712F, -0.03F);
                        G02(5.0347F, 3.5813F, -0.2379F, -0.03F);
                        G02(4.7968F, 3.5536F, 0F, -0.03F);
                        G01(4.5344F, 3.585F, -0.03F);
                        G03(4.5344F, 3.585F, -0.0719F, -0.03F);
                        G03(4.4625F, 3.5896F, 0F, -0.03F);
                        G02(4.2523F, 3.5494F, -0.6237F, -0.03F);
                        G02(4.0807F, 3.4849F, -0.0898F, -0.03F);
                        G02(3.9909F, 3.4731F, 0F, -0.03F);
                        G02(3.8571F, 3.5F, 0.1014F, -0.03F);
                        G01(3.7568F, 3.5852F, 0.03F);
                        G01(3.7568F, 3.5852F, 0.06F);
                        G01(4.1123F, 3.6129F, 0.06F);
                        G01(4.1123F, 3.6129F, -0.03F);
                        G03(4.1123F, 3.6129F, -0.1198F, -0.03F);
                        G03(3.9925F, 3.6259F, 0F, -0.03F);
                        G03(3.923F, 3.6216F, 0.1592F, -0.03F);
                        G02(3.6575F, 3.5532F, -0.9777F, -0.03F);
                        G02(3.4316F, 3.4856F, -0.13F, -0.03F);
                        G02(3.3016F, 3.4709F, 0F, -0.03F);
                        G01(3.134F, 3.4956F, 0.03F);
                        G01(3.134F, 3.4956F, 0.06F);
                        G01(4.3029F, 3.7209F, 0.06F);
                        G01(4.3029F, 3.7209F, -0.03F);
                        G03(4.3029F, 3.7209F, -0.2818F, -0.03F);
                        G03(4.3033F, 3.7356F, -0.2822F, -0.03F);
                        G03(4.2926F, 3.8124F, -0.033F, -0.03F);
                        G03(4.2725F, 3.8312F, 0.005F, -0.03F);
                        G03(4.2502F, 3.8083F, 0.1874F, -0.03F);
                        G03(4.2442F, 3.7604F, 0.1934F, -0.03F);
                        G01(4.2509F, 3.7013F, -0.03F);
                        G01(4.2509F, 3.7013F, 0.03F);
                        G01(4.2509F, 3.7013F, 0.06F);
                        G01(4.0531F, 3.6952F, 0.06F);
                        G01(4.0531F, 3.6952F, -0.03F);
                        G02(4.0531F, 3.6952F, 0.7868F, -0.03F);
                        G02(3.9186F, 3.8451F, 0.2191F, -0.03F);
                        G02(3.8606F, 4.0147F, 0.2771F, -0.03F);
                        G01(3.8632F, 4.0464F, -0.03F);
                        G01(3.8632F, 4.0464F, 0.03F);
                        G01(3.8632F, 4.0464F, 0.06F);
                        G01(3.9007F, 3.7329F, 0.06F);
                        G01(3.9007F, 3.7329F, -0.03F);
                        G02(3.9007F, 3.7329F, 0.2516F, -0.03F);
                        G02(3.821F, 3.9148F, 0.4138F, -0.03F);
                        G02(3.8177F, 3.9671F, 0.4171F, -0.03F);
                        G01(3.8308F, 4.0707F, 0.03F);
                        G01(3.8308F, 4.0707F, 0.06F);
                        G01(3.7568F, 4.1416F, 0.06F);
                        G01(3.7568F, 4.1416F, -0.03F);
                        G03(3.7568F, 4.1416F, 0.4553F, -0.03F);
                        G03(3.8514F, 4.0551F, 1.8554F, -0.03F);
                        G03(4.0986F, 3.8862F, 0.3365F, -0.03F);
                        G03(4.2256F, 3.8335F, 0.0269F, -0.03F);
                        G03(4.2525F, 3.8305F, 0F, -0.03F);
                        G03(4.311F, 3.8452F, -0.0266F, -0.03F);
                        G03(4.3349F, 3.8834F, -0.0107F, -0.03F);
                        G03(4.3242F, 3.8929F, 0F, -0.03F);
                        G02(4.322F, 3.8927F, -1.4711F, -0.03F);
                        G02(4.2526F, 3.8823F, 0.0089F, -0.03F);
                        G02(4.2076F, 3.9002F, 0.6532F, -0.03F);
                        G03(3.9405F, 4.1497F, -2.2068F, -0.03F);
                        G03(3.7751F, 4.353F, -0.1293F, -0.03F);
                        G03(3.7257F, 4.3896F, -0.0577F, -0.03F);
                        G03(3.668F, 4.4017F, 0F, -0.03F);
                        G01(3.658F, 4.3995F, -0.03F);
                        G02(3.658F, 4.3995F, -0.1107F, -0.03F);
                        G02(3.6979F, 4.3619F, -0.0058F, -0.03F);
                        G02(3.6921F, 4.3585F, 0F, -0.03F);
                        G03(3.6893F, 4.359F, -0.0506F, -0.03F);
                        G03(3.6387F, 4.3677F, 0F, -0.03F);
                        G03(3.5897F, 4.3596F, 0.0317F, -0.03F);
                        G01(3.4932F, 4.2873F, -0.03F);
                        G03(3.4932F, 4.2873F, 0.0048F, -0.03F);
                        G03(3.498F, 4.2859F, 0F, -0.03F);
                        G02(3.502F, 4.2869F, 0.0473F, -0.03F);
                        G01(3.5362F, 4.2913F, -0.03F);
                        G03(3.5362F, 4.2913F, 0.0723F, -0.03F);
                        G03(3.5339F, 4.2731F, 0.0746F, -0.03F);
                        G02(3.5349F, 4.2608F, -0.2378F, -0.03F);
                        G02(3.5426F, 4.1999F, -0.2455F, -0.03F);
                        G03(3.5421F, 4.1841F, 0.4304F, -0.03F);
                        G01(3.5628F, 4.1264F, -0.03F);
                        G01(3.564F, 4.132F, -0.03F);
                        G02(3.564F, 4.132F, -0.0465F, -0.03F);
                        G03(3.5846F, 4.1104F, 1.1848F, -0.03F);
                        G03(3.6953F, 3.9636F, 0.1532F, -0.03F);
                        G01(3.823F, 3.901F, 0.03F);
                        G01(3.823F, 3.901F, 0.06F);
                        G01(3.556F, 4.1278F, 0.06F);
                        G01(3.556F, 4.1278F, -0.03F);
                        G03(3.556F, 4.1278F, 0.0367F, -0.03F);
                        G02(3.4618F, 4.1058F, -0.3078F, -0.03F);
                        G02(3.3482F, 4.0685F, -0.062F, -0.03F);
                        G02(3.2862F, 4.0621F, 0F, -0.03F);
                        G02(3.2085F, 4.0722F, 0.2628F, -0.03F);
                        G02(3.0277F, 4.1224F, 2.4631F, -0.03F);
                        G03(2.3842F, 4.3853F, -0.1931F, -0.03F);
                        G03(2.2825F, 4.4149F, -0.4276F, -0.03F);
                        G03(2.0828F, 4.4445F, -0.0071F, -0.03F);
                        G03(2.0757F, 4.4449F, 0F, -0.03F);
                        G03(2.0444F, 4.4366F, 0.0127F, -0.03F);
                        G03(2.0395F, 4.4244F, 0.0176F, -0.03F);
                        G01(2.0558F, 4.4041F, -0.03F);
                        G01(2.1873F, 4.3759F, -0.03F);
                        G01(2.0476F, 4.3714F, -0.03F);
                        G03(2.0476F, 4.3714F, 0.0912F, -0.03F);
                        G03(1.6926F, 4.3177F, 1.5206F, -0.03F);
                        G03(1.3326F, 4.1727F, 0.3372F, -0.03F);
                        G01(1.1819F, 4.0681F, 0.03F);
                        G01(1.1819F, 4.0681F, 0.06F);
                        G01(1.252F, 4.1233F, 0.06F);
                        G01(1.252F, 4.1233F, -0.03F);
                        G02(1.252F, 4.1233F, 0.6074F, -0.03F);
                        G01(1.9264F, 4.232F, -0.03F);
                        G01(1.9391F, 4.2374F, -0.03F);
                        G01(1.8822F, 4.2613F, -0.03F);
                        G02(1.8822F, 4.2613F, 0.0306F, -0.03F);
                        G02(1.8527F, 4.2917F, 0.0176F, -0.03F);
                        G02(1.8703F, 4.3054F, 0F, -0.03F);
                        G02(1.872F, 4.3053F, 0.033F, -0.03F);
                        G02(1.905F, 4.3078F, 0F, -0.03F);
                        G02(1.9583F, 4.3012F, -0.1981F, -0.03F);
                        G01(2.1568F, 4.2494F, -0.03F);
                        G01(2.1568F, 4.2497F, -0.03F);
                        G03(2.1568F, 4.2497F, -0.0076F, -0.03F);
                        G02(2.1535F, 4.256F, 0.1087F, -0.03F);
                        G02(2.1046F, 4.31F, 0.0118F, -0.03F);
                        G02(2.1044F, 4.312F, 0.012F, -0.03F);
                        G02(2.1164F, 4.324F, 0F, -0.03F);
                        G01(2.1232F, 4.3232F, -0.03F);
                        G02(2.1232F, 4.3232F, 0F, -0.03F);
                        G02(2.1848F, 4.3136F, -0.2396F, -0.03F);
                        G03(2.3507F, 4.2567F, 1.4059F, -0.03F);
                        G03(2.8949F, 4.0324F, 0.2754F, -0.03F);
                        G01(3.2072F, 4F, -0.03F);
                        G03(3.2072F, 4F, -0.0266F, -0.03F);
                        G02(3.1927F, 4.0123F, 0.0136F, -0.03F);
                        G02(3.1783F, 4.0248F, 0.0159F, -0.03F);
                        G02(3.1776F, 4.0297F, 0.0166F, -0.03F);
                        G02(3.1854F, 4.0439F, 0.0712F, -0.03F);
                        G02(3.2566F, 4.0642F, 0F, -0.03F);
                        G01(3.2628F, 4.0641F, 0.03F);
                        G01(3.2628F, 4.0641F, 0.06F);
                        G01(3.122F, 3.8444F, 0.06F);
                        G01(3.122F, 3.8444F, -0.03F);
                        G03(3.122F, 3.8444F, -0.6607F, -0.03F);
                        G03(2.8594F, 3.9721F, -0.6509F, -0.03F);
                        G03(2.5022F, 4.0612F, -0.5612F, -0.03F);
                        G03(2.0127F, 4.1043F, -0.2459F, -0.03F);
                        G03(1.7668F, 4.1076F, 0F, -0.03F);
                        G03(1.4018F, 4.1004F, 0.09F, -0.03F);
                        G03(1.0027F, 4.0153F, 0.2165F, -0.03F);
                        G03(0.8424F, 3.9448F, 0.0253F, -0.03F);
                        G03(0.8147F, 3.9095F, 0.0485F, -0.03F);
                        G03(0.8375F, 3.8699F, 0.084F, -0.03F);
                        G03(0.9215F, 3.8437F, 0F, -0.03F);
                        G01(1.3712F, 3.8674F, -0.03F);
                        G02(1.3712F, 3.8674F, 0.0629F, -0.03F);
                        G02(1.4341F, 3.8683F, 0F, -0.03F);
                        G01(1.5538F, 3.8651F, 0.03F);
                        G01(1.5538F, 3.8651F, 0.06F);
                        G01(2.1218F, 3.796F, 0.06F);
                        G01(2.1218F, 3.796F, -0.03F);
                        G02(2.1218F, 3.796F, -0.368F, -0.03F);
                        G02(1.8101F, 3.7513F, -0.1669F, -0.03F);
                        G02(1.6432F, 3.745F, 0F, -0.03F);
                        G02(1.541F, 3.7474F, 0.0097F, -0.03F);
                        G02(1.4074F, 3.7786F, 0.0251F, -0.03F);
                        G02(1.3863F, 3.796F, 0.0123F, -0.03F);
                        G02(1.3846F, 3.8026F, 0.014F, -0.03F);
                        G02(1.3896F, 3.8133F, 0.0795F, -0.03F);
                        G02(1.4363F, 3.8393F, 0.1721F, -0.03F);
                        G01(1.5538F, 3.8651F, -0.03F);
                        G01(1.5538F, 3.8651F, 0.03F);
                        G01(1.5538F, 3.8651F, 0.06F);
                        G01(2.9601F, 3.7747F, 0.06F);
                        G01(2.9601F, 3.7747F, -0.03F);
                        G03(2.9601F, 3.7747F, -0.1722F, -0.03F);
                        G03(2.7879F, 3.7911F, 0F, -0.03F);
                        G03(2.624F, 3.7762F, 1.4255F, -0.03F);
                        G02(2.3416F, 3.714F, -0.3982F, -0.03F);
                        G02(2.1338F, 3.6828F, -0.0144F, -0.03F);
                        G02(2.1194F, 3.6819F, 0F, -0.03F);
                        G02(2.0423F, 3.7097F, 0.0169F, -0.03F);
                        G02(2.0378F, 3.7228F, 0.0214F, -0.03F);
                        G02(2.0402F, 3.7327F, 0.0838F, -0.03F);
                        G01(2.1218F, 3.796F, -0.03F);
                        G01(2.1218F, 3.796F, 0.03F);
                        G01(2.1218F, 3.796F, 0.06F);
                        G01(4.7116F, 3.8076F, 0.06F);
                        G01(4.7116F, 3.8076F, -0.03F);
                        G03(4.7116F, 3.8076F, -0.0241F, -0.03F);
                        G03(4.6875F, 3.8199F, 0F, -0.03F);
                        G03(4.6844F, 3.8198F, 0.0162F, -0.03F);
                        G02(4.6423F, 3.7992F, -2.1995F, -0.03F);
                        G02(4.5603F, 3.73F, -0.0059F, -0.03F);
                        G02(4.5544F, 3.7278F, 0F, -0.03F);
                        G03(4.5453F, 3.7368F, -0.0825F, -0.03F);
                        G03(4.5392F, 3.7674F, -0.0166F, -0.03F);
                        G03(4.5226F, 3.7741F, 0F, -0.03F);
                        G03(4.5167F, 3.7734F, 0.0231F, -0.03F);
                        G01(4.4262F, 3.7113F, -0.03F);
                        G02(4.4262F, 3.7113F, 0.023F, -0.03F);
                        G02(4.4245F, 3.7203F, 0.0247F, -0.03F);
                        G03(4.4255F, 3.7272F, -0.2009F, -0.03F);
                        G03(4.4339F, 3.768F, -0.0102F, -0.03F);
                        G03(4.4237F, 3.7697F, 0F, -0.03F);
                        G02(4.4141F, 3.7681F, -1.0062F, -0.03F);
                        G02(4.2326F, 3.6959F, -0.0797F, -0.03F);
                        G02(4.1529F, 3.6854F, 0F, -0.03F);
                        G02(4.1157F, 3.6876F, 0.0779F, -0.03F);
                        G03(3.9007F, 3.7329F, -1.0544F, -0.03F);
                        G03(3.5508F, 3.8076F, -0.5739F, -0.03F);
                        G03(3.2715F, 3.8546F, -0.0505F, -0.03F);
                        G03(3.221F, 3.8573F, 0F, -0.03F);
                        G03(3.1516F, 3.8522F, 0.0718F, -0.03F);
                        G03(3.0883F, 3.8315F, 0.4878F, -0.03F);
                        G03(2.9252F, 3.7563F, 0.0162F, -0.03F);
                        G03(2.911F, 3.7447F, 0.0082F, -0.03F);
                        G03(2.9192F, 3.7433F, 0F, -0.03F);
                        G02(2.9297F, 3.7456F, 0.0753F, -0.03F);
                        G01(2.9497F, 3.7342F, -0.03F);
                        G03(2.9497F, 3.7342F, 4.393F, -0.03F);
                        G03(2.814F, 3.6751F, 0.0975F, -0.03F);
                        G03(2.7039F, 3.5637F, 0.0076F, -0.03F);
                        G03(2.7027F, 3.5592F, 0.0088F, -0.03F);
                        G03(2.708F, 3.551F, 0.0085F, -0.03F);
                        G03(2.7165F, 3.5503F, 0F, -0.03F);
                        G01(2.7608F, 3.5723F, -0.03F);
                        G01(2.7267F, 3.484F, -0.03F);
                        G03(2.7267F, 3.484F, 0.2708F, -0.03F);
                        G03(2.7076F, 3.3806F, 0.2899F, -0.03F);
                        G03(2.7096F, 3.3468F, 0.863F, -0.03F);
                        G01(2.7465F, 3.1795F, -0.03F);
                        G01(2.7446F, 3.2643F, -0.03F);
                        G01(2.7474F, 3.2687F, -0.03F);
                        G02(2.7474F, 3.2687F, -0.0282F, -0.03F);
                        G03(2.7649F, 3.2521F, 0.2206F, -0.03F);
                        G03(2.975F, 3.1284F, 0.0584F, -0.03F);
                        G03(3.0334F, 3.1247F, 0F, -0.03F);
                        G03(3.0604F, 3.1255F, -0.0433F, -0.03F);
                        G02(3.0451F, 3.1415F, 0.132F, -0.03F);
                        G01(2.9933F, 3.201F, -0.03F);
                        G01(3.0527F, 3.1699F, -0.03F);
                        G03(3.0527F, 3.1699F, 0.0864F, -0.03F);
                        G03(3.1169F, 3.15F, 0.1289F, -0.03F);
                        G03(3.2458F, 3.1426F, 0F, -0.03F);
                        G03(3.3557F, 3.148F, -0.1068F, -0.03F);
                        G01(3.7288F, 3.2144F, 0.03F);
                        G01(3.7288F, 3.2144F, 0.06F);
                        G01(4.1526F, 3.3468F, 0.06F);
                        G01(4.1526F, 3.3468F, -0.03F);
                        G03(4.1526F, 3.3468F, 0.5745F, -0.03F);
                        G03(4.3249F, 3.1774F, 0.3547F, -0.03F);
                        G03(4.4226F, 3.1286F, 0.1048F, -0.03F);
                        G03(4.5058F, 3.1111F, 0.1019F, -0.03F);
                        G03(4.6077F, 3.1053F, 0F, -0.03F);
                        G03(4.7737F, 3.1208F, -0.0167F, -0.03F);
                        G01(4.8009F, 3.1201F, -0.03F);
                        G02(4.8009F, 3.1201F, -0.1109F, -0.03F);
                        G02(4.749F, 3.0722F, -0.2676F, -0.03F);
                        G01(4.7343F, 3.0511F, -0.03F);
                        G02(4.7343F, 3.0511F, 0.0262F, -0.03F);
                        G02(4.7605F, 3.0516F, 0F, -0.03F);
                        G02(4.8369F, 3.0476F, -0.0278F, -0.03F);
                        G02(4.9729F, 2.9796F, -0.2194F, -0.03F);
                        G02(5.0247F, 2.919F, -0.0039F, -0.03F);
                        G02(5.0278F, 2.9132F, -0.007F, -0.03F);
                        G01(4.9719F, 2.9318F, -0.03F);
                        G01(4.9261F, 2.9508F, -0.03F);
                        G01(4.9962F, 2.8779F, -0.03F);
                        G02(4.9962F, 2.8779F, -0.1414F, -0.03F);
                        G01(5.0315F, 2.7571F, -0.03F);
                        G01(5.0315F, 2.7571F, 0.03F);
                        G01(5.0315F, 2.7571F, 0.06F);
                        G01(4.1123F, 3.3139F, 0.06F);
                        G01(4.1123F, 3.3139F, -0.03F);
                        G02(4.1123F, 3.3139F, 0.3581F, -0.03F);
                        G03(4.3985F, 3.4475F, -2.2476F, -0.03F);
                        G03(4.579F, 3.4737F, -0.0389F, -0.03F);
                        G03(4.6888F, 3.5186F, -0.114F, -0.03F);
                        G01(4.7221F, 3.5563F, 0.03F);
                        G01(4.7221F, 3.5563F, 0.06F);
                        G01(4.68F, 3.7456F, 0.06F);
                        G01(4.68F, 3.7456F, -0.03F);
                        G02(4.68F, 3.7456F, 0.3951F, -0.03F);
                        G02(4.7407F, 3.849F, 0.1964F, -0.03F);
                        G02(4.9371F, 3.9447F, 0F, -0.03F);
                        G01(4.9687F, 3.9428F, 0.03F);
                        G01(4.9687F, 3.9428F, 0.06F);
                        G01(4.9729F, 3.8651F, 0.06F);
                        G01(4.9729F, 3.8651F, -0.03F);
                        G02(4.9729F, 3.8651F, 0.5285F, -0.03F);
                        G02(4.9686F, 3.9323F, 0.5328F, -0.03F);
                        G02(4.9998F, 4.112F, 0.239F, -0.03F);
                        G02(5.0491F, 4.1937F, 0.1036F, -0.03F);
                        G01(5.1761F, 4.2481F, -0.03F);
                        G03(5.1761F, 4.2481F, 1.0409F, -0.03F);
                        G03(5.1344F, 4.1155F, 0.2564F, -0.03F);
                        G03(5.1293F, 4.0642F, 0.2615F, -0.03F);
                        G03(5.1345F, 4.0125F, 0.0502F, -0.03F);
                        G03(5.1742F, 3.9788F, 0.0362F, -0.03F);
                        G03(5.2104F, 3.9758F, 0F, -0.03F);
                        G03(5.3654F, 4.0408F, -0.3943F, -0.03F);
                        G03(5.5497F, 4.4642F, -0.5786F, -0.03F);
                        G03(5.5473F, 4.5161F, -0.0942F, -0.03F);
                        G03(5.5043F, 4.5804F, -0.0416F, -0.03F);
                        G03(5.4627F, 4.5888F, 0F, -0.03F);
                        G03(5.4343F, 4.5849F, 0.0777F, -0.03F);
                        G03(5.341F, 4.5205F, 0.5516F, -0.03F);
                        G01(5.1761F, 4.2481F, 0.03F);
                        G01(5.1761F, 4.2481F, 0.06F);
                        G01(3.7288F, 3.2144F, 0.06F);
                        G01(3.7288F, 3.2144F, -0.03F);
                        G02(3.7288F, 3.2144F, 0.2816F, -0.03F);
                        G02(3.8612F, 3.2446F, 0.0328F, -0.03F);
                        G02(3.894F, 3.2468F, 0F, -0.03F);
                        G02(3.9706F, 3.2345F, -0.1346F, -0.03F);
                        G02(4.1308F, 3.1694F, -0.1437F, -0.03F);
                        G03(4.2151F, 3.0731F, 0.0482F, -0.03F);
                        G02(4.255F, 3.046F, 0.0511F, -0.03F);
                        G02(4.3061F, 3.0474F, 0F, -0.03F);
                        G02(4.3481F, 3.0464F, 0.0032F, -0.03F);
                        G02(4.3513F, 3.0467F, 0F, -0.03F);
                        G02(4.3695F, 3.035F, -0.022F, -0.03F);
                        G01(4.342F, 2.9985F, -0.03F);
                        G03(4.342F, 2.9985F, 0.0036F, -0.03F);
                        G03(4.3396F, 2.9937F, 0.006F, -0.03F);
                        G01(4.4409F, 2.9822F, -0.03F);
                        G02(4.4409F, 2.9822F, -0.0532F, -0.03F);
                        G02(4.5106F, 2.9605F, -0.0057F, -0.03F);
                        G01(4.5053F, 2.9261F, -0.03F);
                        G03(4.5053F, 2.9261F, 0.0222F, -0.03F);
                        G03(4.5223F, 2.9131F, 1.808F, -0.03F);
                        G03(4.8503F, 2.7924F, 0.3251F, -0.03F);
                        G03(5.1245F, 2.749F, 0.0942F, -0.03F);
                        G03(5.2187F, 2.7455F, 0F, -0.03F);
                        G03(5.3247F, 2.7499F, -0.0547F, -0.03F);
                        G03(5.5494F, 2.8379F, -0.5287F, -0.03F);
                        G03(5.6965F, 2.9675F, -0.2905F, -0.03F);
                        G03(5.7544F, 3.0299F, -0.0793F, -0.03F);
                        G01(5.7749F, 3.0763F, -0.03F);
                        G03(5.7749F, 3.0763F, -0.0798F, -0.03F);
                        G03(5.7563F, 3.1274F, -0.1494F, -0.03F);
                        G03(5.6666F, 3.192F, -0.3858F, -0.03F);
                        G02(5.4697F, 3.2499F, 0.0494F, -0.03F);
                        G02(5.3992F, 3.2764F, 0.0513F, -0.03F);
                        G02(5.3469F, 3.3664F, 0.1036F, -0.03F);
                        G02(5.3521F, 3.3988F, 0.1882F, -0.03F);
                        G02(5.3888F, 3.4628F, 0.4165F, -0.03F);
                        G02(5.5361F, 3.5847F, 0.1216F, -0.03F);
                        G01(5.6438F, 3.6129F, 0.03F);
                        G01(5.6438F, 3.6129F, 0.06F);
                        G01(5.5877F, 3.502F, 0.06F);
                        G01(5.5877F, 3.502F, -0.03F);
                        G03(5.5877F, 3.502F, -0.7068F, -0.03F);
                        G02(5.6885F, 3.7587F, 0.529F, -0.03F);
                        G02(5.7138F, 3.869F, 0.1163F, -0.03F);
                        G02(5.8203F, 3.9532F, 0.0439F, -0.03F);
                        G02(5.8642F, 3.9564F, 0F, -0.03F);
                        G02(6.0105F, 3.9188F, -0.0729F, -0.03F);
                        G02(6.0869F, 3.7947F, -0.6969F, -0.03F);
                        G02(6.0892F, 3.7382F, -0.6992F, -0.03F);
                        G02(6.0833F, 3.6475F, -3.612F, -0.03F);
                        G01(6.0513F, 3.4331F, 0.03F);
                        G01(6.0513F, 3.4331F, 0.06F);
                        G01(6.0793F, 3.6129F, 0.06F);
                        G01(6.0793F, 3.6129F, -0.03F);
                        G02(6.0793F, 3.6129F, -0.2371F, -0.03F);
                        G02(6.249F, 3.5827F, -0.0519F, -0.03F);
                        G02(6.3488F, 3.5326F, -0.0602F, -0.03F);
                        G02(6.3723F, 3.4745F, -0.0837F, -0.03F);
                        G02(6.3713F, 3.4617F, -0.3464F, -0.03F);
                        G02(6.3258F, 3.3433F, -0.6157F, -0.03F);
                        G01(5.8572F, 2.8598F, -0.03F);
                        G02(5.8572F, 2.8598F, -2.9976F, -0.03F);
                        G02(5.5985F, 2.6345F, -0.5025F, -0.03F);
                        G03(5.5134F, 2.5666F, 0.018F, -0.03F);
                        G01(5.584F, 2.5353F, -0.03F);
                        G02(5.584F, 2.5353F, -0.0034F, -0.03F);
                        G02(5.5873F, 2.5295F, -0.0067F, -0.03F);
                        G02(5.5825F, 2.5231F, -0.1144F, -0.03F);
                        G02(5.4681F, 2.49F, 0F, -0.03F);
                        G01(5.3917F, 2.5018F, -0.03F);
                        G01(5.2849F, 2.5386F, -0.03F);
                        G03(5.2849F, 2.5386F, 0.0303F, -0.03F);
                        G02(5.2947F, 2.5247F, -0.2648F, -0.03F);
                        G02(5.3724F, 2.4213F, -0.0706F, -0.03F);
                        G01(5.3839F, 2.3842F, -0.03F);
                        G02(5.3839F, 2.3842F, -0.0056F, -0.03F);
                        G02(5.3783F, 2.3786F, 0F, -0.03F);
                        G02(5.3744F, 2.3803F, 0.3724F, -0.03F);
                        G03(5.2934F, 2.4332F, -0.6783F, -0.03F);
                        G03(5.0177F, 2.5671F, -0.2747F, -0.03F);
                        G01(4.6256F, 2.619F, -0.03F);
                        G03(4.6256F, 2.619F, -0.0183F, -0.03F);
                        G03(4.6073F, 2.6199F, 0F, -0.03F);
                        G03(4.5662F, 2.6153F, 0.0136F, -0.03F);
                        G02(4.5344F, 2.5801F, -0.9868F, -0.03F);
                        G02(4.2119F, 2.1577F, -0.8149F, -0.03F);
                        G02(3.9227F, 1.9846F, -0.8914F, -0.03F);
                        G03(3.6101F, 1.8737F, 2.7455F, -0.03F);
                        G03(3.1674F, 1.7465F, 0.5504F, -0.03F);
                        G03(2.7794F, 1.5608F, 0.613F, -0.03F);
                        G03(2.5094F, 1.3196F, 1.145F, -0.03F);
                        G01(2.4113F, 1.1696F, 0.03F);
                        G01(2.4113F, 1.1696F, 0.06F);
                        G01(2.9976F, 1.2463F, 0.06F);
                        G01(2.9976F, 1.2463F, -0.03F);
                        G03(2.9976F, 1.2463F, -0.37F, -0.03F);
                        G02(3.1517F, 1.3804F, 2.195F, -0.03F);
                        G02(3.2765F, 1.5176F, 0.4635F, -0.03F);
                        G02(3.4502F, 1.6435F, 0.622F, -0.03F);
                        G02(3.6346F, 1.7199F, 2.3865F, -0.03F);
                        G03(3.9963F, 1.8316F, -0.8127F, -0.03F);
                        G03(4.4118F, 1.9817F, -0.6404F, -0.03F);
                        G03(4.7077F, 2.158F, -0.6527F, -0.03F);
                        G01(4.891F, 2.337F, 0.03F);
                        G01(4.891F, 2.337F, 0.06F);
                        G01(4.8031F, 2.2406F, 0.06F);
                        G01(4.8031F, 2.2406F, -0.03F);
                        G03(4.8031F, 2.2406F, 0.3149F, -0.03F);
                        G03(4.7783F, 2.1586F, 0.0653F, -0.03F);
                        G03(4.7769F, 2.1451F, 0.0667F, -0.03F);
                        G03(4.7804F, 2.1236F, 0.0094F, -0.03F);
                        G03(4.7898F, 2.1195F, 0F, -0.03F);
                        G03(4.7949F, 2.1206F, -0.1709F, -0.03F);
                        G03(4.9008F, 2.2133F, -0.7912F, -0.03F);
                        G01(4.9876F, 2.3744F, -0.03F);
                        G02(4.9876F, 2.3744F, 0.0052F, -0.03F);
                        G02(4.9928F, 2.3796F, 0F, -0.03F);
                        G03(4.9965F, 2.3779F, 0.112F, -0.03F);
                        G03(5.0255F, 2.3437F, -0.0016F, -0.03F);
                        G03(5.0399F, 2.3548F, -0.2404F, -0.03F);
                        G03(5.0541F, 2.4213F, -0.8371F, -0.03F);
                        G03(5.0621F, 2.5371F, -0.8451F, -0.03F);
                        G01(5.062F, 2.5515F, 0.03F);
                        G01(5.062F, 2.5515F, 0.06F);
                        G01(3.9542F, 1.1743F, 0.06F);
                        G01(3.9542F, 1.1743F, -0.03F);
                        G03(3.9542F, 1.1743F, -0.5613F, -0.03F);
                        G03(3.4776F, 1.2552F, -0.173F, -0.03F);
                        G03(3.3046F, 1.2593F, 0F, -0.03F);
                        G03(2.9976F, 1.2463F, 0.2756F, -0.03F);
                        G03(2.326F, 1.1509F, 0.7082F, -0.03F);
                        G01(1.3814F, 0.7845F, -0.03F);
                        G02(1.3814F, 0.7845F, -0.5948F, -0.03F);
                        G01(1.001F, 0.6686F, -0.03F);
                        G02(1.001F, 0.6686F, 0.0856F, -0.03F);
                        G01(1.0296F, 0.7321F, -0.03F);
                        G02(1.0296F, 0.7321F, -0.064F, -0.03F);
                        G02(0.9656F, 0.7227F, 0F, -0.03F);
                        G02(0.8474F, 0.7568F, 0.0059F, -0.03F);
                        G02(0.8472F, 0.7582F, 0.0061F, -0.03F);
                        G01(0.9468F, 0.7723F, -0.03F);
                        G03(0.9468F, 0.7723F, -0.0092F, -0.03F);
                        G03(0.9668F, 0.8F, -0.0292F, -0.03F);
                        G02(0.9667F, 0.8022F, 0.2852F, -0.03F);
                        G02(0.9779F, 0.8737F, 0.1112F, -0.03F);
                        G02(1.0041F, 0.9077F, 0.6821F, -0.03F);
                        G03(1.1992F, 1.0569F, -1.2981F, -0.03F);
                        G03(1.4121F, 1.1929F, -0.1831F, -0.03F);
                        G03(1.4995F, 1.2735F, -0.0401F, -0.03F);
                        G01(1.5111F, 1.307F, -0.03F);
                        G01(1.5017F, 1.3122F, -0.03F);
                        G03(1.5017F, 1.3122F, -0.0087F, -0.03F);
                        G03(1.493F, 1.3123F, 0F, -0.03F);
                        G03(1.4199F, 1.3023F, 0.1313F, -0.03F);
                        G01(1.1396F, 1.1756F, -0.03F);
                        G01(0.9997F, 1.0691F, -0.03F);
                        G02(0.9997F, 1.0691F, -0.3782F, -0.03F);
                        G02(0.753F, 0.9603F, -0.0066F, -0.03F);
                        G02(0.7464F, 0.9594F, 0F, -0.03F);
                        G01(0.7423F, 0.9949F, -0.03F);
                        G03(0.7423F, 0.9949F, -0.0136F, -0.03F);
                        G03(0.7287F, 0.9993F, 0F, -0.03F);
                        G02(0.722F, 0.9983F, -0.24F, -0.03F);
                        G02(0.5903F, 0.9702F, -0.0168F, -0.03F);
                        G02(0.5735F, 0.9694F, 0F, -0.03F);
                        G02(0.4938F, 0.9885F, 0.0066F, -0.03F);
                        G02(0.4837F, 1.0038F, 0.0167F, -0.03F);
                        G03(0.4881F, 1.0151F, -0.0337F, -0.03F);
                        G01(0.4329F, 1.1028F, -0.03F);
                        G02(0.4329F, 1.1028F, 0.1626F, -0.03F);
                        G02(0.373F, 1.1879F, 0.0967F, -0.03F);
                        G02(0.3669F, 1.2227F, 0.1028F, -0.03F);
                        G01(0.373F, 1.2574F, 0.03F);
                        G01(0.373F, 1.2574F, 0.06F);
                        G01(0.7918F, 1.1743F, 0.06F);
                        G01(0.7918F, 1.1743F, -0.03F);
                        G02(0.7918F, 1.1743F, -0.1352F, -0.03F);
                        G02(0.6566F, 1.1542F, 0F, -0.03F);
                        G02(0.4941F, 1.1836F, 0.1494F, -0.03F);
                        G02(0.2596F, 1.4276F, 0.0782F, -0.03F);
                        G02(0.2539F, 1.4581F, 0.0839F, -0.03F);
                        G02(0.2953F, 1.5305F, 0.0968F, -0.03F);
                        G01(0.4082F, 1.5669F, -0.03F);
                        G01(0.4082F, 1.5669F, 0.03F);
                        G01(0.4082F, 1.5669F, 0.06F);
                        G01(1.0054F, 1.3793F, 0.06F);
                        G01(1.0054F, 1.3793F, -0.03F);
                        G02(1.0054F, 1.3793F, -0.3487F, -0.03F);
                        G02(0.9414F, 1.3362F, -0.0485F, -0.03F);
                        G02(0.8929F, 1.3275F, 0F, -0.03F);
                        G02(0.8488F, 1.3346F, 0.0069F, -0.03F);
                        G02(0.8466F, 1.3406F, 0.0091F, -0.03F);
                        G01(0.8839F, 1.3804F, -0.03F);
                        G01(0.7698F, 1.3363F, -0.03F);
                        G02(0.7698F, 1.3363F, -0.0391F, -0.03F);
                        G02(0.7307F, 1.3326F, 0F, -0.03F);
                        G01(0.6255F, 1.3656F, -0.03F);
                        G01(0.6589F, 1.3884F, -0.03F);
                        G03(0.6589F, 1.3884F, -0.0197F, -0.03F);
                        G01(0.5023F, 1.4737F, -0.03F);
                        G02(0.5023F, 1.4737F, 0.1009F, -0.03F);
                        G02(0.4292F, 1.5278F, 0.0992F, -0.03F);
                        G02(0.4018F, 1.6064F, 0.1266F, -0.03F);
                        G02(0.4022F, 1.6152F, 0.1889F, -0.03F);
                        G02(0.5257F, 1.7871F, 0.3666F, -0.03F);
                        G02(0.7493F, 1.8425F, 0.2239F, -0.03F);
                        G02(0.9732F, 1.8575F, 0F, -0.03F);
                        G02(1.1285F, 1.8503F, -0.1474F, -0.03F);
                        G02(1.5166F, 1.7828F, -0.4185F, -0.03F);
                        G02(1.7431F, 1.6952F, -0.0763F, -0.03F);
                        G02(1.8209F, 1.6294F, -0.0723F, -0.03F);
                        G02(1.8298F, 1.5925F, -0.0812F, -0.03F);
                        G02(1.8213F, 1.5564F, -0.3933F, -0.03F);
                        G03(1.7363F, 1.4324F, 4.0108F, -0.03F);
                        G02(1.466F, 1.0486F, -0.2403F, -0.03F);
                        G01(1.4102F, 0.9878F, 0.03F);
                        G01(1.4102F, 0.9878F, 0.06F);
                        G01(1.4609F, 1.0414F, 0.06F);
                        G01(1.4609F, 1.0414F, -0.03F);
                        G03(1.4609F, 1.0414F, 0.0551F, -0.03F);
                        G03(1.516F, 1.0328F, 0F, -0.03F);
                        G03(1.5721F, 1.0417F, -0.1249F, -0.03F);
                        G01(2.0805F, 1.3453F, -0.03F);
                        G03(2.0805F, 1.3453F, -2.2786F, -0.03F);
                        G03(2.2779F, 1.4869F, -0.0461F, -0.03F);
                        G03(2.3009F, 1.5123F, -0.0054F, -0.03F);
                        G03(2.2955F, 1.5126F, 0F, -0.03F);
                        G02(2.2813F, 1.5102F, -0.0801F, -0.03F);
                        G02(2.2062F, 1.4937F, 0.0016F, -0.03F);
                        G02(2.1991F, 1.5023F, 0.0087F, -0.03F);
                        G02(2.2F, 1.5063F, 0.1197F, -0.03F);
                        G02(2.2323F, 1.5479F, 0.2627F, -0.03F);
                        G01(2.3916F, 1.6358F, -0.03F);
                        G01(2.3285F, 1.6359F, -0.03F);
                        G01(2.3283F, 1.6359F, -0.03F);
                        G02(2.3283F, 1.6359F, 0F, -0.03F);
                        G02(2.3205F, 1.6437F, 0.0078F, -0.03F);
                        G02(2.3219F, 1.6482F, 0.1021F, -0.03F);
                        G02(2.3678F, 1.6823F, 0.5678F, -0.03F);
                        G02(2.6415F, 1.791F, 0.1891F, -0.03F);
                        G03(2.7842F, 1.8093F, -0.626F, -0.03F);
                        G03(3.244F, 1.9042F, -0.2741F, -0.03F);
                        G03(3.5066F, 2.0287F, -4.2803F, -0.03F);
                        G03(3.8088F, 2.2346F, -0.4652F, -0.03F);
                        G03(4.0971F, 2.7677F, -0.4073F, -0.03F);
                        G03(4.0977F, 2.7895F, -0.4079F, -0.03F);
                        G03(4.0709F, 2.935F, -0.37F, -0.03F);
                        G03(3.9228F, 3.111F, -0.5778F, -0.03F);
                        G01(3.7288F, 3.2144F, 0.03F);
                        G01(3.7288F, 3.2144F, 0.06F);
                        G01(3.9869F, 3.0586F, 0.06F);
                        G01(3.9869F, 3.0586F, -0.03F);
                        G03(3.9869F, 3.0586F, -0.0134F, -0.03F);
                        G03(3.9735F, 3.0601F, 0F, -0.03F);
                        G03(3.96F, 3.0586F, 0.039F, -0.03F);
                        G03(3.9058F, 3.0299F, 0.0054F, -0.03F);
                        G03(3.9007F, 3.0209F, 0.0105F, -0.03F);
                        G01(3.9485F, 2.9904F, -0.03F);
                        G02(3.9485F, 2.9904F, -0.0045F, -0.03F);
                        G02(3.9498F, 2.9868F, -0.0058F, -0.03F);
                        G02(3.9465F, 2.9817F, -0.1028F, -0.03F);
                        G03(3.8822F, 2.9604F, 0.2017F, -0.03F);
                        G02(3.7012F, 2.888F, -0.1015F, -0.03F);
                        G01(3.5956F, 2.8665F, -0.03F);
                        G02(3.5956F, 2.8665F, 0F, -0.03F);
                        G03(3.4599F, 2.8767F, -0.1967F, -0.03F);
                        G03(3.2632F, 2.8857F, 0F, -0.03F);
                        G02(3.1415F, 2.8822F, -0.0143F, -0.03F);
                        G02(3.1272F, 2.8819F, 0F, -0.03F);
                        G03(2.9542F, 2.9243F, -0.0172F, -0.03F);
                        G03(2.937F, 2.9283F, 0F, -0.03F);
                        G03(2.9335F, 2.9282F, 0.0049F, -0.03F);
                        G03(2.9279F, 2.9189F, 0.0105F, -0.03F);
                        G03(2.9284F, 2.9157F, 0.1032F, -0.03F);
                        G01(2.989F, 2.847F, -0.03F);
                        G03(2.989F, 2.847F, -0.1154F, -0.03F);
                        G01(2.876F, 2.8569F, -0.03F);
                        G03(2.876F, 2.8569F, 0F, -0.03F);
                        G03(2.7862F, 2.8196F, 0.0046F, -0.03F);
                        G03(2.784F, 2.8146F, 0.0068F, -0.03F);
                        G03(2.7871F, 2.8089F, 0.0284F, -0.03F);
                        G03(2.8155F, 2.8048F, 0F, -0.03F);
                        G02(2.826F, 2.8053F, 0.0152F, -0.03F);
                        G02(2.8412F, 2.8066F, 0F, -0.03F);
                        G02(2.8627F, 2.804F, -0.0399F, -0.03F);
                        G03(2.8461F, 2.7884F, 0.3562F, -0.03F);
                        G03(2.7207F, 2.699F, 0.7459F, -0.03F);
                        G02(2.5329F, 2.5057F, -0.0099F, -0.03F);
                        G02(2.523F, 2.5018F, 0F, -0.03F);
                        G01(2.4701F, 2.5801F, -0.03F);
                        G03(2.4701F, 2.5801F, -0.0803F, -0.03F);
                        G03(2.4069F, 2.6173F, -0.0371F, -0.03F);
                        G03(2.3698F, 2.6205F, 0F, -0.03F);
                        G03(2.2802F, 2.6013F, 0.0187F, -0.03F);
                        G03(2.255F, 2.5616F, 0.0439F, -0.03F);
                        G03(2.2568F, 2.5491F, 0.0767F, -0.03F);
                        G02(2.2811F, 2.5097F, -0.111F, -0.03F);
                        G02(2.3224F, 2.4631F, -0.1257F, -0.03F);
                        G02(2.3255F, 2.4351F, -0.1288F, -0.03F);
                        G03(2.3247F, 2.4213F, 0.0297F, -0.03F);
                        G03(2.3389F, 2.4013F, 0.0951F, -0.03F);
                        G02(2.4023F, 2.3793F, -0.0792F, -0.03F);
                        G01(2.6127F, 2.2095F, -0.03F);
                        G01(2.6127F, 2.2095F, 0.03F);
                        G01(2.6127F, 2.2095F, 0.06F);
                        G01(2.2647F, 2.5298F, 0.06F);
                        G01(2.2647F, 2.5298F, -0.03F);
                        G03(2.2647F, 2.5298F, -0.0504F, -0.03F);
                        G03(2.2143F, 2.5319F, 0F, -0.03F);
                        G03(1.9971F, 2.4911F, 0.1218F, -0.03F);
                        G03(1.8669F, 2.3992F, 0.1273F, -0.03F);
                        G03(1.821F, 2.2818F, 0.1732F, -0.03F);
                        G03(1.8211F, 2.2776F, 0.1371F, -0.03F);
                        G03(1.8612F, 2.1897F, 0.0641F, -0.03F);
                        G01(1.9053F, 2.1666F, 0.03F);
                        G01(1.9053F, 2.1666F, 0.06F);
                        G01(2.3247F, 2.4213F, 0.06F);
                        G01(2.3247F, 2.4213F, -0.03F);
                        G03(2.3247F, 2.4213F, 0.0068F, -0.03F);
                        G03(2.0425F, 2.3233F, 0.3037F, -0.03F);
                        G03(1.9053F, 2.1666F, 0.2184F, -0.03F);
                        G03(1.8997F, 2.1169F, 0.224F, -0.03F);
                        G03(1.9369F, 1.9934F, 0.0873F, -0.03F);
                        G03(2.0242F, 1.9505F, 0F, -0.03F);
                        G03(2.0423F, 1.952F, -0.0135F, -0.03F);
                        G01(2.1723F, 2.0767F, -0.03F);
                        G02(2.1723F, 2.0767F, 0.0775F, -0.03F);
                        G02(2.2498F, 2.1145F, 0F, -0.03F);
                        G01(2.2647F, 2.1134F, 0.03F);
                        G01(2.2647F, 2.1134F, 0.06F);
                        G01(2.0936F, 1.9714F, 0.06F);
                        G01(2.0936F, 1.9714F, -0.03F);
                        G03(2.0936F, 1.9714F, 0.2909F, -0.03F);
                        G03(2.1665F, 1.8209F, 0.1136F, -0.03F);
                        G03(2.271F, 1.7734F, 0.0287F, -0.03F);
                        G03(2.2997F, 1.7724F, 0F, -0.03F);
                        G03(2.4161F, 1.79F, -0.0419F, -0.03F);
                        G03(2.5071F, 1.8295F, -0.0582F, -0.03F);
                        G01(2.5378F, 1.8857F, 0.03F);
                        G01(2.5378F, 1.8857F, 0.06F);
                        G01(2.4023F, 2.3793F, 0.06F);
                        G01(2.4023F, 2.3793F, -0.03F);
                        G03(2.4023F, 2.3793F, 0.0208F, -0.03F);
                        G03(2.2423F, 2.3135F, 0.0626F, -0.03F);
                        G03(2.2053F, 2.236F, 0.0996F, -0.03F);
                        G03(2.2096F, 2.207F, 0.2165F, -0.03F);
                        G02(2.2971F, 2.086F, -0.3907F, -0.03F);
                        G02(2.368F, 2.0259F, -0.0103F, -0.03F);
                        G01(2.372F, 2.0101F, -0.03F);
                        G01(2.3416F, 2.006F, -0.03F);
                        G03(2.3416F, 2.006F, 0.011F, -0.03F);
                        G03(2.3404F, 2.0008F, 0.0122F, -0.03F);
                        G03(2.3453F, 1.991F, 0.2223F, -0.03F);
                        G03(2.456F, 1.9002F, 0.0787F, -0.03F);
                        G03(2.5347F, 1.8853F, 0F, -0.03F);
                        G03(2.6181F, 1.9021F, -0.0857F, -0.03F);
                        G03(2.7838F, 2.0933F, -0.2753F, -0.03F);
                        G03(2.7884F, 2.144F, -0.2799F, -0.03F);
                        G03(2.7616F, 2.2635F, -0.1311F, -0.03F);
                        G02(2.6748F, 2.3311F, 0.0378F, -0.03F);
                        G02(2.6186F, 2.3513F, 0.0043F, -0.03F);
                        G02(2.613F, 2.3602F, 0.0099F, -0.03F);
                        G01(2.6279F, 2.3792F, -0.03F);
                        G02(2.6279F, 2.3792F, 1.4993F, -0.03F);
                        G02(2.7949F, 2.5249F, 0.4382F, -0.03F);
                        G02(3.0709F, 2.6556F, 0.3776F, -0.03F);
                        G02(3.4485F, 2.7029F, 0F, -0.03F);
                        G02(3.5736F, 2.6978F, -0.0964F, -0.03F);
                        G01(4.047F, 2.5515F, 0.03F);
                        G01(4.047F, 2.5515F, 0.06F);
                        G01(2.2555F, 1.5672F, 0.06F);
                        G01(2.2555F, 1.5672F, -0.03F);
                        G02(2.2555F, 1.5672F, 0.7411F, -0.03F);
                        G02(2.0604F, 1.6899F, 0.5178F, -0.03F);
                        G01(1.735F, 2.0892F, -0.03F);
                        G03(1.735F, 2.0892F, -0.0425F, -0.03F);
                        G03(1.7161F, 2.1052F, -0.0026F, -0.03F);
                        G03(1.7135F, 2.1057F, 0F, -0.03F);
                        G03(1.7065F, 2.099F, 0.2159F, -0.03F);
                        G03(1.7041F, 2.0664F, 0.2183F, -0.03F);
                        G01(1.7141F, 1.9631F, -0.03F);
                        G01(1.7141F, 1.9631F, 0.03F);
                        G01(1.7141F, 1.9631F, 0.06F);
                        G01(1.7959F, 1.8505F, 0.06F);
                        G01(1.7959F, 1.8505F, -0.03F);
                        G03(1.7959F, 1.8505F, -1.2921F, -0.03F);
                        G03(1.3373F, 2.2999F, -0.6361F, -0.03F);
                        G03(1.1156F, 2.4009F, -0.1595F, -0.03F);
                        G03(0.9924F, 2.4257F, -0.0045F, -0.03F);
                        G03(0.9879F, 2.4259F, 0F, -0.03F);
                        G03(0.9674F, 2.4208F, 0.0114F, -0.03F);
                        G03(0.9634F, 2.4105F, 0.0154F, -0.03F);
                        G03(0.966F, 2.402F, 0.4119F, -0.03F);
                        G03(1.0623F, 2.2919F, 0.5625F, -0.03F);
                        G01(1.1356F, 2.2318F, 0.03F);
                        G01(1.1356F, 2.2318F, 0.06F);
                        G01(1.3201F, 2.1789F, 0.06F);
                        G01(1.3201F, 2.1789F, -0.03F);
                        G03(1.3201F, 2.1789F, -1.2712F, -0.03F);
                        G03(0.8721F, 2.2936F, -0.3696F, -0.03F);
                        G03(0.5025F, 2.3312F, 0F, -0.03F);
                        G03(0.4174F, 2.3292F, 0.03F, -0.03F);
                        G03(0.2054F, 2.2854F, 0.1508F, -0.03F);
                        G03(0.0732F, 2.1991F, 0.0667F, -0.03F);
                        G03(0.0503F, 2.1392F, 0.0896F, -0.03F);
                        G03(0.0629F, 2.0933F, 0.0675F, -0.03F);
                        G03(0.1027F, 2.064F, 0.234F, -0.03F);
                        G03(0.3367F, 2.028F, 0F, -0.03F);
                        G02(0.3511F, 2.0281F, 0.1199F, -0.03F);
                        G02(0.471F, 2.0309F, 0F, -0.03F);
                        G02(0.5739F, 2.0288F, -0.0263F, -0.03F);
                        G01(0.7415F, 1.98F, 0.03F);
                        G01(0.7415F, 1.98F, 0.06F);
                        G01(0.4041F, 2.0292F, 0.06F);
                        G01(0.4041F, 2.0292F, -0.03F);
                        G02(0.4041F, 2.0292F, -0.1974F, -0.03F);
                        G01(0.7664F, 1.8449F, 0.03F);
                        G01(0.7664F, 1.8449F, 0.06F);
                        G01(1.7363F, 1.4324F, 0.06F);
                        G01(1.7363F, 1.4324F, -0.03F);
                        G03(1.7363F, 1.4324F, 0.064F, -0.03F);
                        G03(1.8003F, 1.4277F, 0F, -0.03F);
                        G02(1.8873F, 1.4364F, 0.0743F, -0.03F);
                        G02(1.9616F, 1.4424F, 0F, -0.03F);
                        G02(1.9869F, 1.4417F, 0.0019F, -0.03F);
                        G02(1.9888F, 1.442F, 0F, -0.03F);
                        G02(1.9956F, 1.4352F, -0.0068F, -0.03F);
                        G02(1.9933F, 1.4302F, -0.1512F, -0.03F);
                        G03(1.9359F, 1.393F, 0.3012F, -0.03F);
                        G01(1.8621F, 1.349F, -0.03F);
                        G03(1.8621F, 1.349F, 0.0117F, -0.03F);
                        G03(1.8738F, 1.3478F, 0F, -0.03F);
                        G02(1.8876F, 1.3494F, 2.039F, -0.03F);
                        G02(2.0977F, 1.3922F, 0.0175F, -0.03F);
                        G02(2.1152F, 1.3935F, 0F, -0.03F);
                        G01(2.142F, 1.3877F, -0.03F);
                        G01(2.142F, 1.3877F, 0.03F);
                        G01(2.142F, 1.3877F, 0.06F);
                        G01(3.2265F, 1.4645F, 0.06F);
                        G01(3.2265F, 1.4645F, -0.03F);
                        G03(3.2265F, 1.4645F, 0.2698F, -0.03F);
                        G03(3.4733F, 1.3938F, 0.0641F, -0.03F);
                        G03(3.5374F, 1.3923F, 0F, -0.03F);
                        G01(3.9129F, 1.4198F, -0.03F);
                        G02(3.9129F, 1.4198F, 0.1503F, -0.03F);
                        G02(4.0632F, 1.4228F, 0F, -0.03F);
                        G02(4.1961F, 1.4205F, 0.0025F, -0.03F);
                        G02(4.1986F, 1.4206F, 0F, -0.03F);
                        G02(4.2145F, 1.4167F, -0.022F, -0.03F);
                        G03(4.2033F, 1.406F, 0.6695F, -0.03F);
                        G01(4.1225F, 1.3656F, -0.03F);
                        G01(4.2253F, 1.3572F, -0.03F);
                        G02(4.2253F, 1.3572F, -0.0197F, -0.03F);
                        G02(4.3029F, 1.3246F, -0.0439F, -0.03F);
                        G02(4.3246F, 1.2758F, -0.0656F, -0.03F);
                        G02(4.3176F, 1.2461F, -0.0082F, -0.03F);
                        G02(4.3094F, 1.2409F, 0F, -0.03F);
                        G03(4.3075F, 1.2411F, -0.1188F, -0.03F);
                        G03(4.2559F, 1.273F, -0.0035F, -0.03F);
                        G03(4.2524F, 1.2737F, 0F, -0.03F);
                        G03(4.2438F, 1.2651F, 0.0086F, -0.03F);
                        G02(4.245F, 1.2606F, -0.1169F, -0.03F);
                        G02(4.2535F, 1.2153F, -0.1254F, -0.03F);
                        G02(4.2394F, 1.1576F, -0.4871F, -0.03F);
                        G02(4.1493F, 1.0323F, -2.279F, -0.03F);
                        G03(3.9331F, 0.8207F, 0.795F, -0.03F);
                        G03(3.7569F, 0.6105F, 1.716F, -0.03F);
                        G01(3.6611F, 0.4249F, 0.03F);
                        G01(3.6611F, 0.4249F, 0.06F);
                        G01(3.7022F, 0.5078F, 0.06F);
                        G01(3.7022F, 0.5078F, -0.03F);
                        G03(3.7022F, 0.5078F, -0.0241F, -0.03F);
                        G03(3.8815F, 0.5804F, -0.4896F, -0.03F);
                        G02(4F, 0.7F, 3.2721F, -0.03F);
                        G02(4.1783F, 0.9111F, 0.3912F, -0.03F);
                        G02(4.3079F, 1.0151F, 0.2488F, -0.03F);
                        G02(4.4398F, 1.0534F, -0.0002F, -0.03F);
                        G01(4.4463F, 1.0465F, -0.03F);
                        G02(4.4463F, 1.0465F, -0.0586F, -0.03F);
                        G02(4.4261F, 1.007F, -0.1834F, -0.03F);
                        G01(4.4802F, 1.0093F, -0.03F);
                        G02(4.4802F, 1.0093F, 0.0843F, -0.03F);
                        G02(4.544F, 1.0237F, -0.0001F, -0.03F);
                        G02(4.6197F, 0.9879F, -0.0093F, -0.03F);
                        G02(4.6211F, 0.9826F, -0.0107F, -0.03F);
                        G01(4.5819F, 0.957F, -0.03F);
                        G01(4.5798F, 0.953F, -0.03F);
                        G03(4.5798F, 0.953F, 0.0241F, -0.03F);
                        G02(4.6033F, 0.9413F, -0.1341F, -0.03F);
                        G02(4.7049F, 0.9125F, -0.057F, -0.03F);
                        G01(4.7737F, 0.7991F, -0.03F);
                        G02(4.7737F, 0.7991F, -0.1161F, -0.03F);
                        G01(4.6912F, 0.7F, 0.03F);
                        G01(4.6912F, 0.7F, 0.06F);
                        G01(4.1028F, 0.688F, 0.06F);
                        G01(4.1028F, 0.688F, -0.03F);
                        G02(4.1028F, 0.688F, 0.5679F, -0.03F);
                        G01(4.2842F, 0.8366F, -0.03F);
                        G01(4.2619F, 0.791F, -0.03F);
                        G01(4.2548F, 0.7682F, -0.03F);
                        G01(4.2564F, 0.7598F, -0.03F);
                        G03(4.2564F, 0.7598F, 0.0034F, -0.03F);
                        G03(4.2598F, 0.7596F, 0F, -0.03F);
                        G02(4.279F, 0.7689F, 0.2211F, -0.03F);
                        G02(4.4095F, 0.832F, 0.0303F, -0.03F);
                        G02(4.4398F, 0.8355F, 0F, -0.03F);
                        G02(4.4718F, 0.8316F, -0.0055F, -0.03F);
                        G02(4.4763F, 0.8233F, -0.01F, -0.03F);
                        G03(4.4721F, 0.8152F, 0.0724F, -0.03F);
                        G01(4.4583F, 0.7829F, -0.03F);
                        G02(4.4583F, 0.7829F, -0.001F, -0.03F);
                        G02(4.597F, 0.7696F, -0.0264F, -0.03F);
                        G02(4.677F, 0.7296F, -0.045F, -0.03F);
                        G02(4.6921F, 0.6897F, -0.0601F, -0.03F);
                        G02(4.692F, 0.6847F, -0.2209F, -0.03F);
                        G02(4.6656F, 0.6014F, -0.4074F, -0.03F);
                        G02(4.5597F, 0.466F, -0.8951F, -0.03F);
                        G02(4.2787F, 0.2756F, -0.9052F, -0.03F);
                        G02(3.6611F, 0.0773F, -0.3662F, -0.03F);
                        G02(3.3685F, 0.056F, -0.0051F, -0.03F);
                        G02(3.3634F, 0.0559F, 0F, -0.03F);
                        G02(3.311F, 0.0642F, 0.0253F, -0.03F);
                        G02(3.2737F, 0.1153F, 0.2168F, -0.03F);
                        G02(3.2693F, 0.1593F, 0.2212F, -0.03F);
                        G02(3.3034F, 0.2772F, 0.4187F, -0.03F);
                        G03(3.3725F, 0.3668F, -3.3873F, -0.03F);
                        G03(3.7112F, 0.7316F, -0.8766F, -0.03F);
                        G03(3.8755F, 0.9929F, -0.2368F, -0.03F);
                        G03(3.9006F, 1.1047F, -0.2619F, -0.03F);
                        G01(3.882F, 1.195F, -0.03F);
                        G01(3.882F, 1.195F, 0.06F);
                        G01(3.882F, 1.195F, 0F);
                        G01(0F, 0F, 0F);*/
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
        public System.Drawing.Color dColor = Color.BlueViolet;
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
            double interval = Math.Pow(Math.Pow(Xstep, 2) + Math.Pow(Ystep, 2), 0.5);
            double endX = StartVertex.X;
            double endY = StartVertex.Y;
            GL.Vertex3(endX, endY, StartVertex.Z);
            while (total_drawn <= distance)
            {
                endX += Xstep;
                endY += Ystep;
                total_drawn += interval;
            }
            GL.Vertex3(endX, endY, StartVertex.Z);
            //GL.Vertex3(EndVertex.X, EndVertex.Y, EndVertex.Z);

            GL.End();
            Point3D ret = new Point3D((float)endX, (float)endY, (float)StartVertex.Z);
            return ret;

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
        public char Axis { get; set; }
        public bool CW { get; set; }
        public bool incremental { get; set; }
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

            float X = StartVertex.X;
            float Y = StartVertex.Y;
            float Xcode = EndVertex.X;
            float Ycode = EndVertex.Y;
            float Icode = OffsetVertex.X;
            float Jcode = OffsetVertex.Y;
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
                Angstart = -Math.PI / 2;
            }

            Xoffset = Xcode - Icode;
            Yoffset = Ycode - Jcode;

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
                Angfinish = -Math.PI / 2;
            }

            while((Angfinish > Angstart) && CW)
            {
                Angfinish -= (2 * Math.PI);
            }

            if (CW && (Angstart - Angfinish > (2 * Math.PI)))
            {
                Angstart -= (2 * Math.PI);
            }
            if (!CW && (Angfinish - Angstart > (2 * Math.PI)))
            {
                Angfinish -= (2 * Math.PI);
            }

            double TotalAngle = 0;

            if(CW)
                TotalAngle = Angstart - Angfinish;
            else
                TotalAngle = Angfinish - Angstart;


            radius = Math.Pow(Math.Pow(Yoffset, 2) + Math.Pow(Xoffset, 2), 0.5);
            ret.Add("AngStart", Angstart);
            ret.Add("AngFinish", Angfinish);
            ret.Add("X", X);
            ret.Add("Y", Y);
            ret.Add("ICode", Icode);
            ret.Add("JCode", Jcode);
            ret.Add("Radius", radius);
            ret.Add("TotalAngle", TotalAngle);
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
            if(CW)
                GL.Color3(Color.Wheat);
            else
                GL.Color3(Color.Crimson);
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
            GL.Color3(Color.Wheat);

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

        public IList<string> ListElements()
        {
            IList<string> ret = new List<string>();
            string t = "";
            foreach (var item in Elements)
            {
                t = "";
                Arc TEMPARC;
                LineSegment TEMPLINE;
                t += item.GetType() + ": ";
                if (item is Arc)
                {
                    TEMPARC = (Arc)item;
                    if (TEMPARC.CW) { t += "G02 "; }
                    else { t += "G03 "; }
                    t += " X: " + TEMPARC.StartVertex.X + " Y: " + TEMPARC.StartVertex.Y + " I: " + TEMPARC.OffsetVertex.X + " J: " + TEMPARC.OffsetVertex.Y + " Angle: " + String.Format("{0:G3}", TEMPARC.GetAngle()) + " Length: " + String.Format("{0:G3}", TEMPARC.GetLength());

                }
                else if (item is LineSegment)
                {
                    TEMPLINE = (LineSegment)item;
                    t += " X: " + TEMPLINE.EndVertex.X + " Y: " + TEMPLINE.EndVertex.Y + " Z: " + TEMPLINE.EndVertex.Z + " length: " + TEMPLINE.GetLength();
                }
                ret.Add(t += "\n");
            }
            return ret;

        }

        public void Draw()
        {
            foreach (Drawable item in Elements)
            {
                if (item is LineSegment)
                    item.Render();
                else if (item is Arc)
                {
                    Arc a = (Arc)item;
                    if (a.GetAngle() < Math.PI * 2 && a.GetAngle() > Math.PI * -2)
                    {
                        item.Render();
                    }
                }
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

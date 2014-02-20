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
        float InitialX = 0;
        float InitialY = 0;
        public float currentX;
        public float currentY;
        public float currentZ;

        //CAMERA CONTROL STUFF
        bool adjusting_camera = false;
        double rotate_angle = 0;
        Point3D rotation_axis = new Point3D(0, 0, 0);
        Point3D camera_anchor = new Point3D(0, 0, 0);
        float rotation_magnitude = 0;
        //END CAMERA CONTROL


        //ANIMATION STUFF
        Sculpture MAIN;
        bool MAIN_Loaded = false;
        public static bool animating = false;
        double distance_per_second = 1.0;
        public static double time_elapsed = 0;
        double timer_start = 0;
        //END ANIMATION STUFF

        float rotation = 0;
        int BaseDelay = 20;
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
            /*float deltaRotation = (float)milliseconds / 20.0f;
            rotation += deltaRotation;*/
            if (animating)
                time_elapsed = time_elapsed + milliseconds;
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
            int w = Arena.Width;
            int h = Arena.Height;
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
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
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
            if(rotation_magnitude > 0)
                GL.Rotate(rotation_magnitude, rotation_axis.X, rotation_axis.Y, rotation_axis.Z);

            if (MAIN_Loaded)
            {
                if (MainForm.animating)
                {
                    double distance=(time_elapsed + distance_per_second) / 1000;
                    MAIN.Draw((time_elapsed + distance_per_second) / 1000);
                    if (Math.Floor(time_elapsed) % 1000 == 0)
                        debugText.AppendText("tick: " + distance + "\n");
                }
                
            }
            /*foreach (Drawable item in Paths)
            {
                item.Render();
            }*/


            Arena.SwapBuffers();
        }


        private void showButton_Click(object sender, EventArgs e)
        {
            DrawAlien();
            MAIN = new Sculpture(Paths);
            timer_start = 0;
            MAIN_Loaded = true;
        }
        private void MoveHead(out Point3D HEAD_LOCATION, Point3D location)
        {
            //Paths.Add(new Dot(location.X, location.Y, location.Z));
            HEAD_LOCATION = location;
        }
        private void lineButton_Click(object sender, EventArgs e)
        {
            DrawSmile();
            MainForm.animating = true;
            if (MainForm.animating) debugText.AppendText("FLIP");
            MAIN = new Sculpture(Paths);
            MAIN_Loaded = true;
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
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(x, y, z)));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z));
        }
        private void G02(float x, float y, float i, float j)
        {
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z), new Point3D(i, j, HEAD_LOCATION.Z), 'Z', true));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z));
        }
        private void G03(float x, float y, float i, float j)
        {
            Paths.Add(new Arc(HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z), new Point3D(i, j, HEAD_LOCATION.Z), 'Z', false));
            MoveHead(out HEAD_LOCATION, new Point3D(x, y, HEAD_LOCATION.Z));
        }
        private void G00(float x, float y, float z)
        {
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(x, y, z)));
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
                foreach (KeyValuePair<int, IList<string>> entry in TempCodes)
                {
                    string entryValue = entry.Key.ToString() + ": ";
                    foreach (string command in entry.Value)
                    {
                        entryValue += command + " ";
                    }
                    debugText.AppendText(entryValue + "\n");
                }
                /*
                System.IO.StreamReader file = new System.IO.StreamReader(LoadGCODEFile.FileName);
                string line;
                IList<string> parsedLine;
                int counter = 0;
                debugText.ScrollBars = ScrollBars.Vertical;
                while ((line = file.ReadLine()) != null)
                {
                    if(line[0] == 'G'){
                        parsedLine = line.Split(' ').ToList<string>();
                        string debugNum = counter.ToString();
                        //debugText.Text = debugNum;
                        GCODECommands.Add(counter, parsedLine);
                        debugText.AppendText(debugNum + ": " + line + "\n");
                        counter++;
                    }
                }
                file.Close();
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
            string which = cmd[0];
            float x_component;
            float y_component;
            float z_component;
            float i_component;
            float j_component;
            float k_component;
            bool red = false;
            int index = this.index;
            Dictionary<string, float> args = new Dictionary<string, float>();
            foreach (string bit in cmd)
            {
                if (bit[0] == 'X')
                {
                    x_component = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    args.Add("X", x_component);
                }
                else if (bit[0] == 'Y')
                {
                    y_component = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    args.Add("Y", y_component);
                }
                else if (bit[0] == 'Z')
                {
                    z_component = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    args.Add("Z", z_component);
                }
                else if (bit[0] == 'I')
                {
                    i_component = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    args.Add("I", i_component);
                }
                else if (bit[0] == 'J')
                {
                    j_component = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    args.Add("J", j_component);
                }
                else if (bit[0] == 'K')
                {
                    k_component = float.Parse(bit.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    args.Add("K", k_component);
                }
            }
            debugText.Clear();
            //drawArea.Image = null;
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
            //debugText.AppendText("Executed up to command " + index + "\n");

            //debugText.AppendText(local_index.ToString() + " " + index.ToString());
        }
        private void RenderDrawable(Drawable item)
        {
            if (item is LineSegment)
            {
                LineSegment l = (LineSegment)item;
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.MidnightBlue);
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

                DeltaAngle = Totalangle / ((2 * Math.PI * radius * (Totalangle / (2 * Math.PI))) / 0.0050);

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
            if (MAIN_Loaded)
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
            camera_anchor.X = e.X;
            camera_anchor.Y = e.Y;
        }

        private void Arena_MouseMove(object sender, MouseEventArgs e)
        {
            if (adjusting_camera)
            {
                float dX = e.X - camera_anchor.X;
                float dY = e.Y - camera_anchor.Y;
                rotation_magnitude = (float)DistanceBetweenPoints(new Point3D(dX, dY, 0), new Point3D(0, 0, 0));
                rotation_axis.X = dY;
                rotation_axis.Y = dX;
            }
        }

        private void Arena_MouseUp(object sender, MouseEventArgs e)
        {
            adjusting_camera = false;
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
    }
    public class Drawable
    {
        public Drawable() { }
        public virtual void Render() { }
        public virtual void Render(double distance) { }
        public virtual double GetLength() {
            if(this is Arc)
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
        public override void Render()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.MidnightBlue);
            GL.Vertex3(StartVertex.X, StartVertex.Y, StartVertex.Z);
            GL.Vertex3(EndVertex.X, EndVertex.Y, EndVertex.Z);

            GL.End();

        }
        public override void Render(double distance)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.MidnightBlue);
            GL.Vertex3(StartVertex.X, StartVertex.Y, StartVertex.Z);
            GL.Vertex3(EndVertex.X, EndVertex.Y, EndVertex.Z);

            GL.End();

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
        //public Point3D CenterVertex { get; set; }
        public bool CW { get; set; }
        public bool incremental;
        public Arc() { }
        /*public Arc(float x1, float y1, float z1, float cx, float cy, float cz, bool clockwise) // TODO - fix this dumb constructor
        {
            StartVertex.X = x1;
            StartVertex.Y = y1;
            StartVertex.Z = z1;
            OffsetVertex.X = cx;
            OffsetVertex.Y = cy;
            OffsetVertex.Z = cz;
            CenterVertex.X = x1+cx;
            CenterVertex.Y = y1+cy;
            CenterVertex.Z = z1+cz;
            CW = clockwise;
        }*/
        public Arc(Point3D Start, Point3D End, Point3D Offset, char axis, bool clockwise) //Start = HEAD_LOCATION basically always
        {
            EndVertex = End;
            StartVertex = Start;
            OffsetVertex = Offset;
            Axis = axis;
            /*CenterVertex.X = Start.X + Offset.X;
            CenterVertex.Y = Start.Y + Offset.Y;
            CenterVertex.Z = Start.Z + Offset.Z;*/
            //CenterVertex = Center;
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
            /*CenterVertex.X = Start.X + Offset.X;
            CenterVertex.Y = Start.Y + Offset.Y;
            CenterVertex.Z = Start.Z + Offset.Z;*/
            //CenterVertex = Center;
            CW = clockwise;
        }
        public override void Render()
        {
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

            if (Angfinish < Angstart && !CW)
            {
                Angfinish += (2 * Math.PI);
            }
            else if (Angfinish > Angstart && CW)
            {
                Angfinish -= (2 * Math.PI);
            }
            radius = Math.Pow(Math.Pow(Yoffset, 2) + Math.Pow(Xoffset, 2), 0.5);

            Totalangle = Angstart - Angfinish;

            if (Totalangle < 0)
                Totalangle += 2 * Math.PI;

            DeltaAngle = Totalangle / ((2 * Math.PI * radius * (Totalangle / (2 * Math.PI))) / 0.0050);

            Angiterate = Angstart;
            Xiterate = X;
            Yiterate = Y;
            if (CW)
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
            GL.End();
        }
        public override void Render(double distance)
        {
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

            if (Angfinish < Angstart && !CW)
            {
                Angfinish += (2 * Math.PI);
            }
            else if (Angfinish > Angstart && CW)
            {
                Angfinish -= (2 * Math.PI);
            }
            radius = Math.Pow(Math.Pow(Yoffset, 2) + Math.Pow(Xoffset, 2), 0.5);

            Totalangle = Angstart - Angfinish;

            if (Totalangle < 0)
                Totalangle += 2 * Math.PI;

            DeltaAngle = Totalangle / ((2 * Math.PI * radius * (Totalangle / (2 * Math.PI))) / 0.0050);

            Angiterate = Angstart;
            Xiterate = X;
            Yiterate = Y;
            double completed = 0;
            if (CW)
            {
                while (Angiterate > (Angfinish - DeltaAngle))
                {
                    double xtemp = Xiterate;
                    double ytemp = Yiterate;
                    Xiterate = (radius * Math.Cos(Angiterate)) + Icode;
                    Yiterate = (radius * Math.Sin(Angiterate)) + Jcode;
                    completed += Math.Pow(Math.Pow(xtemp - Xiterate, 2) + Math.Pow(ytemp - Yiterate, 2), 0.5);
                    if (completed <= distance)
                        GL.Vertex3(Xiterate, Yiterate, 0);
                    Angiterate -= DeltaAngle;
                }
            }
            else
            {
                while (Angiterate < (Angfinish - DeltaAngle))
                {
                    double xtemp = Xiterate;
                    double ytemp = Yiterate;
                    Xiterate = (radius * Math.Cos(Angiterate)) + Icode;
                    Yiterate = (radius * Math.Sin(Angiterate)) + Jcode;
                    completed += Math.Pow(Math.Pow(xtemp - Xiterate, 2) + Math.Pow(ytemp - Yiterate, 2), 0.5);
                    if(completed<=distance)
                        GL.Vertex3(Xiterate, Yiterate, 0);
                    Angiterate += DeltaAngle;
                }
            }
            GL.End();
        }
        public override double GetLength()
        {
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

            if (Angfinish < Angstart && !CW)
            {
                Angfinish += (2 * Math.PI);
            }
            else if (Angfinish > Angstart && CW)
            {
                Angfinish -= (2 * Math.PI);
            }
            radius = Math.Pow(Math.Pow(Yoffset, 2) + Math.Pow(Xoffset, 2), 0.5);

            Totalangle = Angstart - Angfinish;

            if (Totalangle < 0)
                Totalangle += 2 * Math.PI;

            DeltaAngle = Totalangle / ((2 * Math.PI * radius * (Totalangle / (2 * Math.PI))) / 0.0050);

            Angiterate = Angstart;
            Xiterate = X;
            Yiterate = Y;
            double ret = 0;
            if (CW)
            {
                while (Angiterate > (Angfinish - DeltaAngle))
                {
                    double xtemp = Xiterate;
                    double ytemp = Yiterate;
                    Xiterate = (radius * Math.Cos(Angiterate)) + Icode;
                    Yiterate = (radius * Math.Sin(Angiterate)) + Jcode;
                    ret += Math.Pow(Math.Pow(xtemp - Xiterate, 2) + Math.Pow(ytemp - Yiterate, 2), 0.5);
                    Angiterate -= DeltaAngle;
                }
            }
            else
            {
                while (Angiterate < (Angfinish - DeltaAngle))
                {
                    double xtemp = Xiterate;
                    double ytemp = Yiterate;
                    Xiterate = (radius * Math.Cos(Angiterate)) + Icode;
                    Yiterate = (radius * Math.Sin(Angiterate)) + Jcode;
                    ret += Math.Pow(Math.Pow(xtemp - Xiterate, 2) + Math.Pow(ytemp - Yiterate, 2), 0.5);
                    Angiterate += DeltaAngle;
                }
            }
            return ret;
        }
    }
    public class Sculpture
    {
        public IList<Drawable> elements;
        public double total_drawn { get; set; }
        public double item_length { get; set; }
        public Sculpture() { }
        public Sculpture(IList<Drawable> in_elements)
        {
            elements = in_elements;
        }
        
        public void Draw()
        {
            foreach (Drawable item in elements)
            {
                item.Render();
            }
        }
        public void Draw(double distance)
        {
            total_drawn = 0;
            item_length = 0;
            foreach (Drawable item in elements)
            {
                item_length = item.GetLength();
                if (total_drawn + item_length < distance)
                {
                    item.Render();
                    total_drawn += item_length;
                }
                    /*
                else if (distance - total_drawn > 0)
                {
                    item.Render(distance - total_drawn);
                }*/
            }
        }
        public double GetLength()
        {
            double ret = 0;
            foreach (Drawable item in elements)
            {
                ret += item.GetLength();
            }
            return ret;
        }

    }
}

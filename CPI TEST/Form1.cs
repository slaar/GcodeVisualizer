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
        float rotation = 0;
        double rotate_angle = 0;
        int BaseDelay = 20;
        float GlobalScale = 0.2F;
        Point3D HEAD_LOCATION = new Point3D(1, 1, 0);
        bool incremental = false;
        Graphics g;
        //CPI.Plot3D.Plotter3D p;
        public MainForm()
        {
            InitializeComponent();
            Arena.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Arena_MouseWheel);
        }
        Stopwatch sw = new Stopwatch();
        private void Arena_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ChangeScale(e.Delta / 10000.0F);
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
            MouseWheelHandler.Add(this, MyOnMouseWheel);
            sw.Start();
            Application.Idle += Application_Idle;
        }

        private void MyOnMouseWheel(MouseEventArgs e)
        {
//            float t = e.Delta;
            //ChangeScale(t);
            debugText.AppendText("HI");
        }
        private void ChangeScale(float in_scale)
        {
            this.GlobalScale += in_scale;
        }
        private void Animate(double milliseconds)
        {
            float deltaRotation = (float)milliseconds / 20.0f;
            rotation += deltaRotation;
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
            Animate(milliseconds);            /*
            sw.Stop();
            double milliseconds = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            Arena.Invalidate();
             * */
            /*
            while (Arena.IsIdle)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Rotate(1, 1, 1, 0);
                foreach (Drawable item in Paths)
                {
                    RenderDrawable(item);
                }
                Arena.SwapBuffers();
            }
             * */
            /*GL.MatrixMode(MatrixMode.Modelview);
            GL.Rotate(rotate_angle,1, 1, 1);
            rotate_angle += 0.1;
            Invalidate();*/
            //debugText.AppendText("!");
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
            GL.Rotate(rotation, 0, 1, 0);

            foreach (Drawable item in Paths)
            {
                item.Render();
            }


            /*
            foreach (LineSegment l in ThingsToDraw)
            {
            
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.MidnightBlue);
                GL.Vertex3(l.StartVertex.X,l.StartVertex.Y,l.StartVertex.Z);
                GL.Vertex3(l.EndVertex.X,l.EndVertex.Y,l.EndVertex.Z);
                
                GL.End();
            
            }
             */
            /*
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.MidnightBlue);
            GL.Vertex3(-0.181434f, -1.541976f, 0.0f);
            GL.Vertex3(-0.181434f, 0.220898f, 0.0f);
            GL.End();
              */

            Arena.SwapBuffers();
        }


        private void showButton_Click(object sender, EventArgs e)
        {
            DrawAlien();
        }
        private void MoveHead(out Point3D HEAD_LOCATION, Point3D location)
        {
            Paths.Add(new Dot(location.X, location.Y, location.Z));
            HEAD_LOCATION = location;
        }
        private void lineButton_Click(object sender, EventArgs e)
        {
            //HEAD_LOCATION.X = 1;
            //HEAD_LOCATION.Y = 0;
            //Paths.Add(new Arc(new Point3D(-1, 0, 0), new Point3D(0, -1, 0), new Point3D(1, 0, 0), 'Z', false));
            //Paths.Add(new Arc(new Point3D(-2, 0, 0), new Point3D(0, -2, 0), new Point3D(2, 0, 0), 'Z', true));
            //Paths.Add(new Arc(new Point3D(2, 0, 0), new Point3D(-2, 2, 0), new Point3D(-2, 0, 0), 'Z', true));
            //Paths.Add(new Arc(new Point3D(1, 1, 0), new Point3D(-1, 1, 0), new Point3D(-1, 0, 0), 'Z', true));
            //Paths.Add(new Arc(new Point3D(2, 1, 0), new Point3D(3, 2, 0), new Point3D(0, 1, 0), 'Z', true));
            //Paths.Add(new LineSegment(new Point3D(0, 0, 0), new Point3D(1, 0, 0)));
            DrawSmile();
            //absolute test arcs
            //MoveHead(out HEAD_LOCATION, new Point3D(-1, 0, 0));
            //Paths.Add(new Arc(HEAD_LOCATION, new Point3D(1, 0, 0), new Point3D(0, 0, 0), 'Z', true));
            //MoveHead(out HEAD_LOCATION, new Point3D(-1, 1, 0));
            //Paths.Add(new Arc(HEAD_LOCATION, new Point3D(1, 1, 0), new Point3D(0, 1, 0), 'Z', true));
            //MoveHead(out HEAD_LOCATION, new Point3D(0.6F, 0, 0)); 
            //float xtemp = (float)((0.6 *Math.Cos(Math.PI * 1.8))+ 0.6);
            //float ytemp = (float)((0.6 *Math.Sin(Math.PI * 1.8)));
            //Paths.Add(new Arc(HEAD_LOCATION,new Point3D(xtemp,ytemp,0),new Point3D(0.2F,0,0),'Z',false));
            //Random r = new Random();
            //Point3D a = new Point3D((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            //Point3D b = new Point3D((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            //ThingsToDraw.Add(new LineSegment(a, b));
            //Paths.Add(new LineSegment(a, b));
            //Paths.Add(new LineSegment(new Point3D(-1, 0, 1), new Point3D(0, -1, 1)));
            //Paths.Add(new LineSegment(new Point3D(0, 0, 0), new Point3D(-1, -1, 2)));
            //Paths.Add(new Arc(new Point3D(1, 1, 0), new Point3D(-1, 1, 0), new Point3D(-1, -1, 0), 'Z', false));
            //Paths.Add(new Arc(new Point3D(1, 1, 0), new Point3D(-1, 1, 0), new Point3D(-1, -1, 0), 'Z', true));
            //Paths.Add(new Arc(new Point3D(1, 1, -0.5f), new Point3D(-1, 1, -0.5f), new Point3D(-1, -1, 0), 'Z', false));
            //Paths.Add(new Arc(new Point3D(1, 1, -0.1f), new Point3D(-1, 1, -0.1f), new Point3D(-1, -1, 0), 'Z', true));
            Arena.Invalidate();
            //AddLine(1.0f,1.0f,1.0f,2.0f,2.0f,2.0f);
            /*
            Graphics g = drawArea.CreateGraphics();
            DrawLine(g, p, 30, 50);
            debugText.AppendText(this.currentX + ", " + this.currentY + "\n");
            DrawLine(g, p, 70, 40);
            debugText.AppendText(this.currentX + ", " + this.currentY + "\n");
            DrawLine(g, p, 30, 20);
            debugText.AppendText(this.currentX + ", " + this.currentY + "\n");
            DrawLine(g, p, 70, 90);
            debugText.AppendText(this.currentX + ", " + this.currentY + "\n");
             */
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
        private void DrawApple()
        {
            HEAD_LOCATION.X = -0.764F;
            HEAD_LOCATION.Y = -2.4149F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.7966F, -2.4066F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.7966F;
            HEAD_LOCATION.Y = -2.4066F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.8275F, -2.3975F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.8275F;
            HEAD_LOCATION.Y = -2.3975F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.8568F, -2.3876F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.8568F;
            HEAD_LOCATION.Y = -2.3876F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.8847F, -2.3767F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.8847F;
            HEAD_LOCATION.Y = -2.3767F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.9113F, -2.3651F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.9113F;
            HEAD_LOCATION.Y = -2.3651F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.9369F, -2.3525F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.9369F;
            HEAD_LOCATION.Y = -2.3525F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.9616F, -2.3391F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.9616F;
            HEAD_LOCATION.Y = -2.3391F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.9856F, -2.3247F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.9856F;
            HEAD_LOCATION.Y = -2.3247F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.0091F, -2.3095F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.0091F;
            HEAD_LOCATION.Y = -2.3095F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.0322F, -2.2933F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.0322F;
            HEAD_LOCATION.Y = -2.2933F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.0551F, -2.2762F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.0551F;
            HEAD_LOCATION.Y = -2.2762F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.078F, -2.2581F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.078F;
            HEAD_LOCATION.Y = -2.2581F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.101F, -2.2391F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.101F;
            HEAD_LOCATION.Y = -2.2391F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.1244F, -2.2192F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.1244F;
            HEAD_LOCATION.Y = -2.2192F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.1483F, -2.1982F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.1483F;
            HEAD_LOCATION.Y = -2.1982F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.1728F, -2.1763F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.1728F;
            HEAD_LOCATION.Y = -2.1763F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.2792F, -2.0527F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.2792F;
            HEAD_LOCATION.Y = -2.0527F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.3795F, -1.9231F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.3795F;
            HEAD_LOCATION.Y = -1.9231F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4734F, -1.7879F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4734F;
            HEAD_LOCATION.Y = -1.7879F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.5605F, -1.6476F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.5605F;
            HEAD_LOCATION.Y = -1.6476F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.6403F, -1.5027F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.6403F;
            HEAD_LOCATION.Y = -1.5027F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.7124F, -1.3536F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.7124F;
            HEAD_LOCATION.Y = -1.3536F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.7764F, -1.201F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.7764F;
            HEAD_LOCATION.Y = -1.201F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.832F, -1.0453F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.832F;
            HEAD_LOCATION.Y = -1.0453F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.8785F, -0.8869F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.8785F;
            HEAD_LOCATION.Y = -0.8869F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.9157F, -0.7265F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.9157F;
            HEAD_LOCATION.Y = -0.7265F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.9432F, -0.5643F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.9432F;
            HEAD_LOCATION.Y = -0.5643F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.9604F, -0.4011F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.9604F;
            HEAD_LOCATION.Y = -0.4011F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.967F, -0.2372F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.967F;
            HEAD_LOCATION.Y = -0.2372F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.9627F, -0.0732F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.9627F;
            HEAD_LOCATION.Y = -0.0732F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.9468F, 0.0905F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.9468F;
            HEAD_LOCATION.Y = 0.0905F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.9192F, 0.2533F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.9192F;
            HEAD_LOCATION.Y = 0.2533F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.9026F, 0.3068F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.9026F;
            HEAD_LOCATION.Y = 0.3068F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.8851F, 0.3589F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.8851F;
            HEAD_LOCATION.Y = 0.3589F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.8664F, 0.4097F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.8664F;
            HEAD_LOCATION.Y = 0.4097F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.8466F, 0.4591F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.8466F;
            HEAD_LOCATION.Y = 0.4591F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.8254F, 0.5074F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.8254F;
            HEAD_LOCATION.Y = 0.5074F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.8028F, 0.5544F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.8028F;
            HEAD_LOCATION.Y = 0.5544F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.7788F, 0.6004F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.7788F;
            HEAD_LOCATION.Y = 0.6004F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.7531F, 0.6453F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.7531F;
            HEAD_LOCATION.Y = 0.6453F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.7258F, 0.6893F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.7258F;
            HEAD_LOCATION.Y = 0.6893F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.6968F, 0.7324F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.6968F;
            HEAD_LOCATION.Y = 0.7324F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.6659F, 0.7747F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.6659F;
            HEAD_LOCATION.Y = 0.7747F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.6331F, 0.8162F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.6331F;
            HEAD_LOCATION.Y = 0.8162F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.5982F, 0.857F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.5982F;
            HEAD_LOCATION.Y = 0.857F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.5613F, 0.8972F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.5613F;
            HEAD_LOCATION.Y = 0.8972F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.5221F, 0.9368F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.5221F;
            HEAD_LOCATION.Y = 0.9368F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4807F, 0.976F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4807F;
            HEAD_LOCATION.Y = 0.976F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4734F, 0.9811F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4734F;
            HEAD_LOCATION.Y = 0.9811F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4661F, 0.9863F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4661F;
            HEAD_LOCATION.Y = 0.9863F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4588F, 0.9914F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4588F;
            HEAD_LOCATION.Y = 0.9914F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4516F, 0.9966F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4516F;
            HEAD_LOCATION.Y = 0.9966F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4443F, 1.0018F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4443F;
            HEAD_LOCATION.Y = 1.0018F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.437F, 1.0069F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.437F;
            HEAD_LOCATION.Y = 1.0069F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4297F, 1.0121F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4297F;
            HEAD_LOCATION.Y = 1.0121F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4224F, 1.0173F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4224F;
            HEAD_LOCATION.Y = 1.0173F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4151F, 1.0224F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4151F;
            HEAD_LOCATION.Y = 1.0224F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4079F, 1.0276F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4079F;
            HEAD_LOCATION.Y = 1.0276F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.4006F, 1.0327F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.4006F;
            HEAD_LOCATION.Y = 1.0327F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.3933F, 1.0379F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.3933F;
            HEAD_LOCATION.Y = 1.0379F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.386F, 1.0431F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.386F;
            HEAD_LOCATION.Y = 1.0431F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.3787F, 1.0482F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.3787F;
            HEAD_LOCATION.Y = 1.0482F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.3715F, 1.0534F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.3715F;
            HEAD_LOCATION.Y = 1.0534F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.3642F, 1.0586F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.3642F;
            HEAD_LOCATION.Y = 1.0586F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.316F, 1.0842F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.316F;
            HEAD_LOCATION.Y = 1.0842F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.2684F, 1.108F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.2684F;
            HEAD_LOCATION.Y = 1.108F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.2213F, 1.1298F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.2213F;
            HEAD_LOCATION.Y = 1.1298F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.1745F, 1.1498F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.1745F;
            HEAD_LOCATION.Y = 1.1498F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.1278F, 1.1676F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.1278F;
            HEAD_LOCATION.Y = 1.1676F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.0811F, 1.1835F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.0811F;
            HEAD_LOCATION.Y = 1.1835F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-1.0343F, 1.1972F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -1.0343F;
            HEAD_LOCATION.Y = 1.1972F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.9872F, 1.2087F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.9872F;
            HEAD_LOCATION.Y = 1.2087F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.9397F, 1.218F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.9397F;
            HEAD_LOCATION.Y = 1.218F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.8916F, 1.225F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.8916F;
            HEAD_LOCATION.Y = 1.225F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.8427F, 1.2297F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.8427F;
            HEAD_LOCATION.Y = 1.2297F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.793F, 1.232F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.793F;
            HEAD_LOCATION.Y = 1.232F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.7422F, 1.2319F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.7422F;
            HEAD_LOCATION.Y = 1.2319F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.6902F, 1.2293F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.6902F;
            HEAD_LOCATION.Y = 1.2293F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.637F, 1.2241F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.637F;
            HEAD_LOCATION.Y = 1.2241F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5822F, 1.2164F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5822F;
            HEAD_LOCATION.Y = 1.2164F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5458F, 1.2057F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5458F;
            HEAD_LOCATION.Y = 1.2057F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5094F, 1.1945F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5094F;
            HEAD_LOCATION.Y = 1.1945F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.4733F, 1.1828F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.4733F;
            HEAD_LOCATION.Y = 1.1828F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.4372F, 1.1709F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.4372F;
            HEAD_LOCATION.Y = 1.1709F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.4012F, 1.1587F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.4012F;
            HEAD_LOCATION.Y = 1.1587F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.3652F, 1.1464F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.3652F;
            HEAD_LOCATION.Y = 1.1464F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.3293F, 1.1339F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.3293F;
            HEAD_LOCATION.Y = 1.1339F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.2934F, 1.1215F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.2934F;
            HEAD_LOCATION.Y = 1.1215F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.2575F, 1.109F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.2575F;
            HEAD_LOCATION.Y = 1.109F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.2215F, 1.0967F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.2215F;
            HEAD_LOCATION.Y = 1.0967F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.1855F, 1.0847F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.1855F;
            HEAD_LOCATION.Y = 1.0847F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.1494F, 1.0729F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.1494F;
            HEAD_LOCATION.Y = 1.0729F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.1132F, 1.0614F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.1132F;
            HEAD_LOCATION.Y = 1.0614F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0768F, 1.0504F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0768F;
            HEAD_LOCATION.Y = 1.0504F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0403F, 1.0399F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0403F;
            HEAD_LOCATION.Y = 1.0399F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0036F, 1.03F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0036F;
            HEAD_LOCATION.Y = 1.03F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0003F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0003F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0042F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0042F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0081F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0081F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.012F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.012F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.016F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.016F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0199F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0199F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0238F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0238F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0277F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0277F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0316F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0316F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0356F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0356F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0395F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0395F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0434F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0434F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0474F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0474F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0513F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0513F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0553F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0553F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0592F, HEAD_LOCATION.Y, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0592F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0675F, 1.0324F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0675F;
            HEAD_LOCATION.Y = 1.0324F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0758F, 1.0349F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0758F;
            HEAD_LOCATION.Y = 1.0349F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0841F, 1.0373F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0841F;
            HEAD_LOCATION.Y = 1.0373F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0924F, 1.0398F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0924F;
            HEAD_LOCATION.Y = 1.0398F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1006F, 1.0422F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1006F;
            HEAD_LOCATION.Y = 1.0422F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1089F, 1.0447F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1089F;
            HEAD_LOCATION.Y = 1.0447F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1172F, 1.0471F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1172F;
            HEAD_LOCATION.Y = 1.0471F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1255F, 1.0496F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1255F;
            HEAD_LOCATION.Y = 1.0496F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1337F, 1.052F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1337F;
            HEAD_LOCATION.Y = 1.052F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.142F, 1.0545F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.142F;
            HEAD_LOCATION.Y = 1.0545F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1503F, 1.0569F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1503F;
            HEAD_LOCATION.Y = 1.0569F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1585F, 1.0594F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1585F;
            HEAD_LOCATION.Y = 1.0594F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1668F, 1.0618F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1668F;
            HEAD_LOCATION.Y = 1.0618F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1751F, 1.0643F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1751F;
            HEAD_LOCATION.Y = 1.0643F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1834F, 1.0667F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1834F;
            HEAD_LOCATION.Y = 1.0667F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1916F, 1.0692F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1916F;
            HEAD_LOCATION.Y = 1.0692F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2399F, 1.0863F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2399F;
            HEAD_LOCATION.Y = 1.0863F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2875F, 1.1033F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2875F;
            HEAD_LOCATION.Y = 1.1033F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3345F, 1.12F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3345F;
            HEAD_LOCATION.Y = 1.12F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3811F, 1.1362F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3811F;
            HEAD_LOCATION.Y = 1.1362F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.4273F, 1.1519F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.4273F;
            HEAD_LOCATION.Y = 1.1519F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.4734F, 1.1669F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.4734F;
            HEAD_LOCATION.Y = 1.1669F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5194F, 1.1811F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5194F;
            HEAD_LOCATION.Y = 1.1811F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5655F, 1.1943F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5655F;
            HEAD_LOCATION.Y = 1.1943F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6119F, 1.2065F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6119F;
            HEAD_LOCATION.Y = 1.2065F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6585F, 1.2174F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6585F;
            HEAD_LOCATION.Y = 1.2174F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7056F, 1.227F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7056F;
            HEAD_LOCATION.Y = 1.227F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7533F, 1.2352F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7533F;
            HEAD_LOCATION.Y = 1.2352F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8017F, 1.2417F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8017F;
            HEAD_LOCATION.Y = 1.2417F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8509F, 1.2466F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8509F;
            HEAD_LOCATION.Y = 1.2466F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9011F, 1.2496F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9011F;
            HEAD_LOCATION.Y = 1.2496F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9524F, 1.2506F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9524F;
            HEAD_LOCATION.Y = 1.2506F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.0101F, 1.2427F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.0101F;
            HEAD_LOCATION.Y = 1.2427F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.0665F, 1.2336F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.0665F;
            HEAD_LOCATION.Y = 1.2336F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.1218F, 1.223F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.1218F;
            HEAD_LOCATION.Y = 1.223F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.1759F, 1.2108F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.1759F;
            HEAD_LOCATION.Y = 1.2108F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.2289F, 1.197F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.2289F;
            HEAD_LOCATION.Y = 1.197F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.2808F, 1.1813F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.2808F;
            HEAD_LOCATION.Y = 1.1813F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3317F, 1.1636F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3317F;
            HEAD_LOCATION.Y = 1.1636F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3815F, 1.1438F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3815F;
            HEAD_LOCATION.Y = 1.1438F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4303F, 1.1218F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4303F;
            HEAD_LOCATION.Y = 1.1218F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4782F, 1.0974F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4782F;
            HEAD_LOCATION.Y = 1.0974F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5251F, 1.0704F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5251F;
            HEAD_LOCATION.Y = 1.0704F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.571F, 1.0408F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.571F;
            HEAD_LOCATION.Y = 1.0408F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.616F, 1.0084F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.616F;
            HEAD_LOCATION.Y = 1.0084F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6602F, 0.9731F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6602F;
            HEAD_LOCATION.Y = 0.9731F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7035F, 0.9346F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7035F;
            HEAD_LOCATION.Y = 0.9346F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.746F, 0.893F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.746F;
            HEAD_LOCATION.Y = 0.893F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7511F, 0.8867F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7511F;
            HEAD_LOCATION.Y = 0.8867F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7569F, 0.8799F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7569F;
            HEAD_LOCATION.Y = 0.8799F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7632F, 0.8727F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7632F;
            HEAD_LOCATION.Y = 0.8727F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7699F, 0.8651F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7699F;
            HEAD_LOCATION.Y = 0.8651F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.777F, 0.8572F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.777F;
            HEAD_LOCATION.Y = 0.8572F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7842F, 0.849F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7842F;
            HEAD_LOCATION.Y = 0.849F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7914F, 0.8406F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7914F;
            HEAD_LOCATION.Y = 0.8406F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7985F, 0.8321F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7985F;
            HEAD_LOCATION.Y = 0.8321F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8054F, 0.8234F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8054F;
            HEAD_LOCATION.Y = 0.8234F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.812F, 0.8148F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.812F;
            HEAD_LOCATION.Y = 0.8148F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.818F, 0.8062F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.818F;
            HEAD_LOCATION.Y = 0.8062F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8234F, 0.7976F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8234F;
            HEAD_LOCATION.Y = 0.7976F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8281F, 0.7892F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8281F;
            HEAD_LOCATION.Y = 0.7892F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8319F, 0.7811F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8319F;
            HEAD_LOCATION.Y = 0.7811F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8347F, 0.7731F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8347F;
            HEAD_LOCATION.Y = 0.7731F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8364F, 0.7656F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8364F;
            HEAD_LOCATION.Y = 0.7656F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8162F, 0.7516F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8162F;
            HEAD_LOCATION.Y = 0.7516F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7984F, 0.7392F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7984F;
            HEAD_LOCATION.Y = 0.7392F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7829F, 0.7284F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7829F;
            HEAD_LOCATION.Y = 0.7284F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7692F, 0.7189F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7692F;
            HEAD_LOCATION.Y = 0.7189F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7573F, 0.7105F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7573F;
            HEAD_LOCATION.Y = 0.7105F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7468F, 0.7031F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7468F;
            HEAD_LOCATION.Y = 0.7031F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7374F, 0.6964F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7374F;
            HEAD_LOCATION.Y = 0.6964F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7291F, 0.6903F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7291F;
            HEAD_LOCATION.Y = 0.6903F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7214F, 0.6846F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7214F;
            HEAD_LOCATION.Y = 0.6846F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7141F, 0.6791F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7141F;
            HEAD_LOCATION.Y = 0.6791F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7071F, 0.6736F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7071F;
            HEAD_LOCATION.Y = 0.6736F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6999F, 0.668F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6999F;
            HEAD_LOCATION.Y = 0.668F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6925F, 0.662F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6925F;
            HEAD_LOCATION.Y = 0.662F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6845F, 0.6555F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6845F;
            HEAD_LOCATION.Y = 0.6555F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6758F, 0.6483F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6758F;
            HEAD_LOCATION.Y = 0.6483F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6659F, 0.6402F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6659F;
            HEAD_LOCATION.Y = 0.6402F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6355F, 0.6088F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6355F;
            HEAD_LOCATION.Y = 0.6088F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6066F, 0.5779F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6066F;
            HEAD_LOCATION.Y = 0.5779F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5792F, 0.5472F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5792F;
            HEAD_LOCATION.Y = 0.5472F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5533F, 0.5165F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5533F;
            HEAD_LOCATION.Y = 0.5165F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5288F, 0.4858F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5288F;
            HEAD_LOCATION.Y = 0.4858F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5058F, 0.4548F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5058F;
            HEAD_LOCATION.Y = 0.4548F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.484F, 0.4235F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.484F;
            HEAD_LOCATION.Y = 0.4235F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4636F, 0.3915F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4636F;
            HEAD_LOCATION.Y = 0.3915F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4444F, 0.3588F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4444F;
            HEAD_LOCATION.Y = 0.3588F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4265F, 0.3252F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4265F;
            HEAD_LOCATION.Y = 0.3252F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4097F, 0.2905F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4097F;
            HEAD_LOCATION.Y = 0.2905F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.394F, 0.2546F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.394F;
            HEAD_LOCATION.Y = 0.2546F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3795F, 0.2172F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3795F;
            HEAD_LOCATION.Y = 0.2172F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3659F, 0.1783F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3659F;
            HEAD_LOCATION.Y = 0.1783F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3534F, 0.1377F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3534F;
            HEAD_LOCATION.Y = 0.1377F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3418F, 0.0951F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3418F;
            HEAD_LOCATION.Y = 0.0951F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3352F, 0.0426F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3352F;
            HEAD_LOCATION.Y = 0.0426F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.33F, -0.009F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.33F;
            HEAD_LOCATION.Y = -0.009F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3265F, -0.0598F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3265F;
            HEAD_LOCATION.Y = -0.0598F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3245F, -0.1099F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3245F;
            HEAD_LOCATION.Y = -0.1099F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3243F, -0.1594F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3243F;
            HEAD_LOCATION.Y = -0.1594F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.326F, -0.2082F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.326F;
            HEAD_LOCATION.Y = -0.2082F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3295F, -0.2566F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3295F;
            HEAD_LOCATION.Y = -0.2566F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3349F, -0.3045F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3349F;
            HEAD_LOCATION.Y = -0.3045F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3425F, -0.3522F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3425F;
            HEAD_LOCATION.Y = -0.3522F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3521F, -0.3995F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3521F;
            HEAD_LOCATION.Y = -0.3995F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.364F, -0.4468F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.364F;
            HEAD_LOCATION.Y = -0.4468F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3782F, -0.4939F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3782F;
            HEAD_LOCATION.Y = -0.4939F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3947F, -0.541F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3947F;
            HEAD_LOCATION.Y = -0.541F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4137F, -0.5883F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4137F;
            HEAD_LOCATION.Y = -0.5883F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4352F, -0.6357F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4352F;
            HEAD_LOCATION.Y = -0.6357F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4594F, -0.6833F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4594F;
            HEAD_LOCATION.Y = -0.6833F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4796F, -0.7148F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4796F;
            HEAD_LOCATION.Y = -0.7148F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5005F, -0.7453F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5005F;
            HEAD_LOCATION.Y = -0.7453F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.522F, -0.7747F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.522F;
            HEAD_LOCATION.Y = -0.7747F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5442F, -0.8032F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5442F;
            HEAD_LOCATION.Y = -0.8032F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5671F, -0.8307F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5671F;
            HEAD_LOCATION.Y = -0.8307F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5907F, -0.8573F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5907F;
            HEAD_LOCATION.Y = -0.8573F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6151F, -0.883F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6151F;
            HEAD_LOCATION.Y = -0.883F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6401F, -0.9079F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6401F;
            HEAD_LOCATION.Y = -0.9079F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.666F, -0.9321F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.666F;
            HEAD_LOCATION.Y = -0.9321F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6926F, -0.9555F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6926F;
            HEAD_LOCATION.Y = -0.9555F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.72F, -0.9782F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.72F;
            HEAD_LOCATION.Y = -0.9782F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7482F, -1.0003F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7482F;
            HEAD_LOCATION.Y = -1.0003F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7773F, -1.0218F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7773F;
            HEAD_LOCATION.Y = -1.0218F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8072F, -1.0428F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8072F;
            HEAD_LOCATION.Y = -1.0428F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.838F, -1.0633F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.838F;
            HEAD_LOCATION.Y = -1.0633F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8696F, -1.0833F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8696F;
            HEAD_LOCATION.Y = -1.0833F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8757F, -1.0862F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8757F;
            HEAD_LOCATION.Y = -1.0862F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8818F, -1.0891F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8818F;
            HEAD_LOCATION.Y = -1.0891F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8879F, -1.092F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8879F;
            HEAD_LOCATION.Y = -1.092F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.894F, -1.0949F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.894F;
            HEAD_LOCATION.Y = -1.0949F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9F, -1.0979F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9F;
            HEAD_LOCATION.Y = -1.0979F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9061F, -1.1008F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9061F;
            HEAD_LOCATION.Y = -1.1008F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9122F, -1.1037F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9122F;
            HEAD_LOCATION.Y = -1.1037F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9183F, -1.1066F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9183F;
            HEAD_LOCATION.Y = -1.1066F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9244F, -1.1096F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9244F;
            HEAD_LOCATION.Y = -1.1096F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9305F, -1.1125F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9305F;
            HEAD_LOCATION.Y = -1.1125F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9366F, -1.1154F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9366F;
            HEAD_LOCATION.Y = -1.1154F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9427F, -1.1184F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9427F;
            HEAD_LOCATION.Y = -1.1184F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9488F, -1.1213F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9488F;
            HEAD_LOCATION.Y = -1.1213F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9549F, -1.1243F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9549F;
            HEAD_LOCATION.Y = -1.1243F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.961F, -1.1273F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.961F;
            HEAD_LOCATION.Y = -1.1273F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.967F, -1.1302F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.967F;
            HEAD_LOCATION.Y = -1.1302F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9544F, -1.1649F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9544F;
            HEAD_LOCATION.Y = -1.1649F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9416F, -1.1992F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9416F;
            HEAD_LOCATION.Y = -1.1992F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9285F, -1.2333F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9285F;
            HEAD_LOCATION.Y = -1.2333F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9153F, -1.2671F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9153F;
            HEAD_LOCATION.Y = -1.2671F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.9018F, -1.3006F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.9018F;
            HEAD_LOCATION.Y = -1.3006F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8881F, -1.334F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8881F;
            HEAD_LOCATION.Y = -1.334F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8741F, -1.3671F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8741F;
            HEAD_LOCATION.Y = -1.3671F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8599F, -1.4001F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8599F;
            HEAD_LOCATION.Y = -1.4001F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8453F, -1.433F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8453F;
            HEAD_LOCATION.Y = -1.433F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8305F, -1.4658F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8305F;
            HEAD_LOCATION.Y = -1.4658F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.8153F, -1.4984F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.8153F;
            HEAD_LOCATION.Y = -1.4984F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7997F, -1.5311F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7997F;
            HEAD_LOCATION.Y = -1.5311F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7838F, -1.5637F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7838F;
            HEAD_LOCATION.Y = -1.5637F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7675F, -1.5963F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7675F;
            HEAD_LOCATION.Y = -1.5963F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7508F, -1.6289F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7508F;
            HEAD_LOCATION.Y = -1.6289F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7337F, -1.6616F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7337F;
            HEAD_LOCATION.Y = -1.6616F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.7064F, -1.7055F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.7064F;
            HEAD_LOCATION.Y = -1.7055F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6788F, -1.7495F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6788F;
            HEAD_LOCATION.Y = -1.7495F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6507F, -1.7934F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6507F;
            HEAD_LOCATION.Y = -1.7934F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.6221F, -1.8371F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.6221F;
            HEAD_LOCATION.Y = -1.8371F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5928F, -1.8805F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5928F;
            HEAD_LOCATION.Y = -1.8805F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5629F, -1.9235F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5629F;
            HEAD_LOCATION.Y = -1.9235F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5322F, -1.9659F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5322F;
            HEAD_LOCATION.Y = -1.9659F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.5006F, -2.0077F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.5006F;
            HEAD_LOCATION.Y = -2.0077F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4682F, -2.0486F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4682F;
            HEAD_LOCATION.Y = -2.0486F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4347F, -2.0887F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4347F;
            HEAD_LOCATION.Y = -2.0887F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.4002F, -2.1278F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.4002F;
            HEAD_LOCATION.Y = -2.1278F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3646F, -2.1657F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3646F;
            HEAD_LOCATION.Y = -2.1657F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.3277F, -2.2024F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.3277F;
            HEAD_LOCATION.Y = -2.2024F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.2896F, -2.2377F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.2896F;
            HEAD_LOCATION.Y = -2.2377F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.25F, -2.2715F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.25F;
            HEAD_LOCATION.Y = -2.2715F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.2091F, -2.3037F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.2091F;
            HEAD_LOCATION.Y = -2.3037F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.1859F, -2.3167F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.1859F;
            HEAD_LOCATION.Y = -2.3167F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.1633F, -2.3287F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.1633F;
            HEAD_LOCATION.Y = -2.3287F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.1412F, -2.3399F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.1412F;
            HEAD_LOCATION.Y = -2.3399F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.1196F, -2.3501F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.1196F;
            HEAD_LOCATION.Y = -2.3501F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.0981F, -2.3595F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.0981F;
            HEAD_LOCATION.Y = -2.3595F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.0769F, -2.3679F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.0769F;
            HEAD_LOCATION.Y = -2.3679F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.0556F, -2.3755F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.0556F;
            HEAD_LOCATION.Y = -2.3755F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.0342F, -2.3823F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.0342F;
            HEAD_LOCATION.Y = -2.3823F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(1.0126F, -2.3882F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 1.0126F;
            HEAD_LOCATION.Y = -2.3882F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9905F, -2.3933F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9905F;
            HEAD_LOCATION.Y = -2.3933F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.968F, -2.3976F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.968F;
            HEAD_LOCATION.Y = -2.3976F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9449F, -2.4011F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9449F;
            HEAD_LOCATION.Y = -2.4011F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.921F, -2.4038F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.921F;
            HEAD_LOCATION.Y = -2.4038F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8963F, -2.4057F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8963F;
            HEAD_LOCATION.Y = -2.4057F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8706F, -2.4068F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8706F;
            HEAD_LOCATION.Y = -2.4068F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8437F, -2.4072F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8437F;
            HEAD_LOCATION.Y = -2.4072F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8033F, -2.3999F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8033F;
            HEAD_LOCATION.Y = -2.3999F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7634F, -2.3913F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7634F;
            HEAD_LOCATION.Y = -2.3913F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.724F, -2.3816F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.724F;
            HEAD_LOCATION.Y = -2.3816F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6849F, -2.3709F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6849F;
            HEAD_LOCATION.Y = -2.3709F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6462F, -2.3594F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6462F;
            HEAD_LOCATION.Y = -2.3594F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6077F, -2.3473F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6077F;
            HEAD_LOCATION.Y = -2.3473F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5694F, -2.3348F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5694F;
            HEAD_LOCATION.Y = -2.3348F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5313F, -2.3219F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5313F;
            HEAD_LOCATION.Y = -2.3219F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.4931F, -2.309F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.4931F;
            HEAD_LOCATION.Y = -2.309F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.4549F, -2.2962F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.4549F;
            HEAD_LOCATION.Y = -2.2962F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.4166F, -2.2836F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.4166F;
            HEAD_LOCATION.Y = -2.2836F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3781F, -2.2715F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3781F;
            HEAD_LOCATION.Y = -2.2715F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3394F, -2.2599F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3394F;
            HEAD_LOCATION.Y = -2.2599F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3003F, -2.2492F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3003F;
            HEAD_LOCATION.Y = -2.2492F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2609F, -2.2393F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2609F;
            HEAD_LOCATION.Y = -2.2393F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2209F, -2.2306F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2209F;
            HEAD_LOCATION.Y = -2.2306F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1731F, -2.2267F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1731F;
            HEAD_LOCATION.Y = -2.2267F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1267F, -2.225F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1267F;
            HEAD_LOCATION.Y = -2.225F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0815F, -2.2254F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0815F;
            HEAD_LOCATION.Y = -2.2254F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0374F, -2.2278F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0374F;
            HEAD_LOCATION.Y = -2.2278F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0057F, -2.2321F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0057F;
            HEAD_LOCATION.Y = -2.2321F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0481F, -2.2381F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0481F;
            HEAD_LOCATION.Y = -2.2381F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0899F, -2.2458F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0899F;
            HEAD_LOCATION.Y = -2.2458F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.1313F, -2.255F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.1313F;
            HEAD_LOCATION.Y = -2.255F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.1725F, -2.2655F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.1725F;
            HEAD_LOCATION.Y = -2.2655F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.2136F, -2.2773F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.2136F;
            HEAD_LOCATION.Y = -2.2773F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.2549F, -2.2903F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.2549F;
            HEAD_LOCATION.Y = -2.2903F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.2965F, -2.3043F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.2965F;
            HEAD_LOCATION.Y = -2.3043F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.3385F, -2.3191F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.3385F;
            HEAD_LOCATION.Y = -2.3191F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.3812F, -2.3348F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.3812F;
            HEAD_LOCATION.Y = -2.3348F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.4247F, -2.3511F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.4247F;
            HEAD_LOCATION.Y = -2.3511F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.4692F, -2.368F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.4692F;
            HEAD_LOCATION.Y = -2.368F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.488F, -2.3732F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.488F;
            HEAD_LOCATION.Y = -2.3732F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5065F, -2.3782F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5065F;
            HEAD_LOCATION.Y = -2.3782F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5249F, -2.3828F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5249F;
            HEAD_LOCATION.Y = -2.3828F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5432F, -2.3871F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5432F;
            HEAD_LOCATION.Y = -2.3871F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5614F, -2.391F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5614F;
            HEAD_LOCATION.Y = -2.391F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5795F, -2.3947F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5795F;
            HEAD_LOCATION.Y = -2.3947F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.5975F, -2.3981F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.5975F;
            HEAD_LOCATION.Y = -2.3981F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.6156F, -2.4011F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.6156F;
            HEAD_LOCATION.Y = -2.4011F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.6337F, -2.4039F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.6337F;
            HEAD_LOCATION.Y = -2.4039F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.6518F, -2.4063F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.6518F;
            HEAD_LOCATION.Y = -2.4063F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.6701F, -2.4085F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.6701F;
            HEAD_LOCATION.Y = -2.4085F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.6885F, -2.4103F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.6885F;
            HEAD_LOCATION.Y = -2.4103F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.707F, -2.4119F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.707F;
            HEAD_LOCATION.Y = -2.4119F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.7258F, -2.4132F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.7258F;
            HEAD_LOCATION.Y = -2.4132F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.7448F, -2.4142F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.7448F;
            HEAD_LOCATION.Y = -2.4142F;
            HEAD_LOCATION.X = 0.0352F;
            HEAD_LOCATION.Y = 1.2969F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0325F, 1.2972F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0325F;
            HEAD_LOCATION.Y = 1.2972F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0297F, 1.2975F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0297F;
            HEAD_LOCATION.Y = 1.2975F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0269F, 1.2979F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0269F;
            HEAD_LOCATION.Y = 1.2979F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0241F, 1.2982F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0241F;
            HEAD_LOCATION.Y = 1.2982F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0214F, 1.2985F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0214F;
            HEAD_LOCATION.Y = 1.2985F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0186F, 1.2988F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0186F;
            HEAD_LOCATION.Y = 1.2988F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0158F, 1.2992F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0158F;
            HEAD_LOCATION.Y = 1.2992F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.013F, 1.2995F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.013F;
            HEAD_LOCATION.Y = 1.2995F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0103F, 1.2998F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0103F;
            HEAD_LOCATION.Y = 1.2998F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0075F, 1.3001F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0075F;
            HEAD_LOCATION.Y = 1.3001F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0048F, 1.3004F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0048F;
            HEAD_LOCATION.Y = 1.3004F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.002F, 1.3007F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.002F;
            HEAD_LOCATION.Y = 1.3007F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0007F, 1.301F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0007F;
            HEAD_LOCATION.Y = 1.301F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0034F, 1.3013F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0034F;
            HEAD_LOCATION.Y = 1.3013F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0062F, 1.3015F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0062F;
            HEAD_LOCATION.Y = 1.3015F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0089F, 1.3018F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0089F;
            HEAD_LOCATION.Y = 1.3018F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0087F, 1.3135F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0087F;
            HEAD_LOCATION.Y = 1.3135F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0086F, 1.3251F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0086F;
            HEAD_LOCATION.Y = 1.3251F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0085F, 1.3367F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0085F;
            HEAD_LOCATION.Y = 1.3367F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0083F, 1.3484F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0083F;
            HEAD_LOCATION.Y = 1.3484F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0082F, 1.3601F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0082F;
            HEAD_LOCATION.Y = 1.3601F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0081F, 1.3717F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0081F;
            HEAD_LOCATION.Y = 1.3717F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0079F, 1.3834F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0079F;
            HEAD_LOCATION.Y = 1.3834F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0078F, 1.395F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0078F;
            HEAD_LOCATION.Y = 1.395F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0076F, 1.4067F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0076F;
            HEAD_LOCATION.Y = 1.4067F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0075F, 1.4183F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0075F;
            HEAD_LOCATION.Y = 1.4183F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0073F, 1.43F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0073F;
            HEAD_LOCATION.Y = 1.43F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0071F, 1.4416F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0071F;
            HEAD_LOCATION.Y = 1.4416F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.007F, 1.4533F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.007F;
            HEAD_LOCATION.Y = 1.4533F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0068F, 1.4649F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0068F;
            HEAD_LOCATION.Y = 1.4649F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0066F, 1.4766F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0066F;
            HEAD_LOCATION.Y = 1.4766F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(-0.0064F, 1.4882F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = -0.0064F;
            HEAD_LOCATION.Y = 1.4882F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0023F, 1.5363F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0023F;
            HEAD_LOCATION.Y = 1.5363F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0126F, 1.583F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0126F;
            HEAD_LOCATION.Y = 1.583F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0246F, 1.6285F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0246F;
            HEAD_LOCATION.Y = 1.6285F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0382F, 1.6728F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0382F;
            HEAD_LOCATION.Y = 1.6728F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0534F, 1.716F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0534F;
            HEAD_LOCATION.Y = 1.716F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0703F, 1.7582F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0703F;
            HEAD_LOCATION.Y = 1.7582F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0889F, 1.7993F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0889F;
            HEAD_LOCATION.Y = 1.7993F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1092F, 1.8396F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1092F;
            HEAD_LOCATION.Y = 1.8396F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1312F, 1.879F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1312F;
            HEAD_LOCATION.Y = 1.879F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1548F, 1.9177F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1548F;
            HEAD_LOCATION.Y = 1.9177F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1802F, 1.9556F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1802F;
            HEAD_LOCATION.Y = 1.9556F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2073F, 1.9929F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2073F;
            HEAD_LOCATION.Y = 1.9929F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2361F, 2.0295F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2361F;
            HEAD_LOCATION.Y = 2.0295F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2666F, 2.0657F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2666F;
            HEAD_LOCATION.Y = 2.0657F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2988F, 2.1014F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2988F;
            HEAD_LOCATION.Y = 2.1014F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3329F, 2.1367F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3329F;
            HEAD_LOCATION.Y = 2.1367F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3683F, 2.1635F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3683F;
            HEAD_LOCATION.Y = 2.1635F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.404F, 2.1894F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.404F;
            HEAD_LOCATION.Y = 2.1894F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.44F, 2.2145F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.44F;
            HEAD_LOCATION.Y = 2.2145F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.4764F, 2.2387F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.4764F;
            HEAD_LOCATION.Y = 2.2387F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5133F, 2.2618F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5133F;
            HEAD_LOCATION.Y = 2.2618F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5506F, 2.2836F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5506F;
            HEAD_LOCATION.Y = 2.2836F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5885F, 2.3043F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5885F;
            HEAD_LOCATION.Y = 2.3043F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.627F, 2.3235F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.627F;
            HEAD_LOCATION.Y = 2.3235F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6661F, 2.3412F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6661F;
            HEAD_LOCATION.Y = 2.3412F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7059F, 2.3574F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7059F;
            HEAD_LOCATION.Y = 2.3574F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7465F, 2.3718F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7465F;
            HEAD_LOCATION.Y = 2.3718F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7878F, 2.3845F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7878F;
            HEAD_LOCATION.Y = 2.3845F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.83F, 2.3952F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.83F;
            HEAD_LOCATION.Y = 2.3952F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.873F, 2.4039F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.873F;
            HEAD_LOCATION.Y = 2.4039F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.917F, 2.4105F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.917F;
            HEAD_LOCATION.Y = 2.4105F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.962F, 2.4149F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.962F;
            HEAD_LOCATION.Y = 2.4149F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9621F, 2.4064F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9621F;
            HEAD_LOCATION.Y = 2.4064F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9622F, 2.3978F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9622F;
            HEAD_LOCATION.Y = 2.3978F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9623F, 2.3893F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9623F;
            HEAD_LOCATION.Y = 2.3893F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9625F, 2.3807F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9625F;
            HEAD_LOCATION.Y = 2.3807F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9626F, 2.3721F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9626F;
            HEAD_LOCATION.Y = 2.3721F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9628F, 2.3636F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9628F;
            HEAD_LOCATION.Y = 2.3636F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9629F, 2.355F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9629F;
            HEAD_LOCATION.Y = 2.355F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9631F, 2.3464F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9631F;
            HEAD_LOCATION.Y = 2.3464F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9632F, 2.3378F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9632F;
            HEAD_LOCATION.Y = 2.3378F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9634F, 2.3292F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9634F;
            HEAD_LOCATION.Y = 2.3292F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9635F, 2.3206F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9635F;
            HEAD_LOCATION.Y = 2.3206F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9637F, 2.312F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9637F;
            HEAD_LOCATION.Y = 2.312F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9639F, 2.3034F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9639F;
            HEAD_LOCATION.Y = 2.3034F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.964F, 2.2948F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.964F;
            HEAD_LOCATION.Y = 2.2948F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9642F, 2.2862F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9642F;
            HEAD_LOCATION.Y = 2.2862F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9644F, 2.2776F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9644F;
            HEAD_LOCATION.Y = 2.2776F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9553F, 2.201F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9553F;
            HEAD_LOCATION.Y = 2.201F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9417F, 2.1259F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9417F;
            HEAD_LOCATION.Y = 2.1259F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9237F, 2.0527F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9237F;
            HEAD_LOCATION.Y = 2.0527F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.9013F, 1.9814F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.9013F;
            HEAD_LOCATION.Y = 1.9814F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8745F, 1.9122F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8745F;
            HEAD_LOCATION.Y = 1.9122F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8435F, 1.8453F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8435F;
            HEAD_LOCATION.Y = 1.8453F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.8084F, 1.781F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.8084F;
            HEAD_LOCATION.Y = 1.781F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7691F, 1.7193F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7691F;
            HEAD_LOCATION.Y = 1.7193F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.7258F, 1.6605F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.7258F;
            HEAD_LOCATION.Y = 1.6605F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6785F, 1.6047F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6785F;
            HEAD_LOCATION.Y = 1.6047F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.6273F, 1.5521F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.6273F;
            HEAD_LOCATION.Y = 1.5521F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5722F, 1.5029F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5722F;
            HEAD_LOCATION.Y = 1.5029F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.5134F, 1.4572F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.5134F;
            HEAD_LOCATION.Y = 1.4572F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.4508F, 1.4153F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.4508F;
            HEAD_LOCATION.Y = 1.4153F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3846F, 1.3774F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3846F;
            HEAD_LOCATION.Y = 1.3774F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.3148F, 1.3435F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.3148F;
            HEAD_LOCATION.Y = 1.3435F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2973F, 1.3382F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2973F;
            HEAD_LOCATION.Y = 1.3382F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2799F, 1.3334F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2799F;
            HEAD_LOCATION.Y = 1.3334F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2625F, 1.3289F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2625F;
            HEAD_LOCATION.Y = 1.3289F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2452F, 1.3248F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2452F;
            HEAD_LOCATION.Y = 1.3248F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.228F, 1.321F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.228F;
            HEAD_LOCATION.Y = 1.321F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.2108F, 1.3176F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.2108F;
            HEAD_LOCATION.Y = 1.3176F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1936F, 1.3144F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1936F;
            HEAD_LOCATION.Y = 1.3144F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1764F, 1.3116F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1764F;
            HEAD_LOCATION.Y = 1.3116F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1591F, 1.309F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1591F;
            HEAD_LOCATION.Y = 1.309F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1418F, 1.3066F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1418F;
            HEAD_LOCATION.Y = 1.3066F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1244F, 1.3045F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1244F;
            HEAD_LOCATION.Y = 1.3045F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.1068F, 1.3026F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.1068F;
            HEAD_LOCATION.Y = 1.3026F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0892F, 1.3009F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0892F;
            HEAD_LOCATION.Y = 1.3009F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0714F, 1.2994F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0714F;
            HEAD_LOCATION.Y = 1.2994F;
            Paths.Add(new LineSegment(HEAD_LOCATION, new Point3D(0.0534F, 1.2981F, HEAD_LOCATION.Z)));
            HEAD_LOCATION.X = 0.0534F;
            HEAD_LOCATION.Y = 1.2981F;
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
                TempCodes.Add(0, "G01 X0.133657 Y-1.54072".Split(' ').ToList<string>());
                TempCodes.Add(1, "G01 X0.224497 Y-1.570109".Split(' ').ToList<string>());
                TempCodes.Add(2, "G01 X0.319207 Y-1.572192".Split(' ').ToList<string>());

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
                float centerX = 0;
                float centerY = 0;
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
                GL.End();
                /*
                if (incremental)
                {
                    centerX = a.StartVertex.X + a.OffsetVertex.X;
                    centerY = a.StartVertex.Y + a.OffsetVertex.Y;
                }
                else
                {
                    centerX = a.OffsetVertex.X;
                    centerY = a.OffsetVertex.Y;
                }
                float Xoffset = a.StartVertex.X - centerX;
                float Yoffset = a.StartVertex.Y - centerY;
                //float Xoffset = a.StartVertex.X + a.OffsetVertex.X;
                //float Yoffset = a.StartVertex.Y + a.OffsetVertex.Y;
                if (Xoffset > 0 && Yoffset >= 0)
                {
                    Angstart = (Math.Atan(Yoffset / Xoffset));
                }
                else if (Xoffset > 0 && Yoffset < 0)
                {
                    Angstart = (Math.Atan(Yoffset / Xoffset) + (2 * Math.PI));
                }
                else if (Xoffset < 0 && Yoffset <= 0)
                {
                    Angstart = (Math.Atan(Yoffset / Xoffset) + (Math.PI));
                }
                else if (Xoffset < 0 && Yoffset > 0)
                {
                    Angstart = (Math.Atan(Yoffset / Xoffset) + (Math.PI));
                }
                else if (Xoffset == 0 && Yoffset > 0)
                {
                    Angstart = Math.PI / 2;
                }
                else if (Xoffset == 0 && Yoffset < 0)
                {
                    Angstart = -(Math.PI / 2);
                }
                debugText.AppendText("1) Xoffset " + Xoffset + " Yoffset " + Yoffset + "\n");
                double radius = 0;
                if (incremental) { radius = Math.Pow(Math.Pow(a.OffsetVertex.X, 2) + Math.Pow(a.OffsetVertex.Y, 2), 0.5); }
                else { radius = Math.Pow(Math.Pow(centerX - a.StartVertex.X, 2) + Math.Pow(centerY - a.StartVertex.Y, 2), 0.5); }
                debugText.AppendText("!" + radius+"\n");
                Xoffset = (a.EndVertex.X-centerX);
                Yoffset = (a.EndVertex.Y-centerY);

                if (Xoffset > 0 && Yoffset >= 0)
                {
                    Angfinish = (Math.Atan(Yoffset / Xoffset));
                }
                else if (Xoffset > 0 && Yoffset < 0)
                {
                    Angfinish = (Math.Atan(Yoffset / Xoffset) + (2 * Math.PI));
                }
                else if (Xoffset < 0 && Yoffset <= 0)
                {
                    Angfinish = (Math.Atan(Yoffset / Xoffset) + (Math.PI));
                }
                else if (Xoffset < 0 && Yoffset > 0)
                {
                    Angfinish = (Math.Atan(Yoffset / Xoffset) + (Math.PI));
                }
                else if (Xoffset == 0 && Yoffset > 0)
                {
                    Angfinish = Math.PI / 2;
                }
                else if (Xoffset == 0 && Yoffset < 0)
                {
                    Angfinish = -(Math.PI / 2);
                }
                if (Angfinish < Angstart && !a.CW)
                {
                    Angfinish += (2 * Math.PI);
                }
                else if (Angfinish > Angstart && a.CW)
                {
                    Angfinish -= (2 * Math.PI);
                }
                debugText.AppendText("2) Xoffset " + Xoffset + " Yoffset " + Yoffset + "\n");
                //debugText.AppendText("Start " + Angstart % Math.PI + " Finish " + Angfinish % Math.PI);
                double TotalAngle = Angstart - Angfinish;
                if (TotalAngle < 0)
                {
                    TotalAngle += (2 * Math.PI);
                }
                double DeltaAngle = 1 / (50 * radius);
                //double DeltaAngle = TotalAngle / ((2 * Math.PI * radius * (TotalAngle / (2 * Math.PI))) / 0.05);
                double Angiterate = Angstart;
                double Xiterate;
                double Yiterate;
                debugText.AppendText("start " + Angstart + " end " + Angfinish+"\n");
                bool report = true;
                //while (Angiterate > (Angfinish - DeltaAngle))
                if (!a.CW)
                {
                    while (Angiterate < (Angfinish - DeltaAngle))
                    {
                        Xiterate = (radius * Math.Cos(Angiterate)) + centerX;
                        Yiterate = (radius * Math.Sin(Angiterate)) + centerY;
                        if (report)
                        {
                            debugText.AppendText("radius is " + radius + "x/y is " + Xiterate + "/" + Yiterate);
                            report = false;
                        }

                        GL.Vertex3(Xiterate, Yiterate, HEAD_LOCATION.Z);
                        //Angiterate -= DeltaAngle;
                        Angiterate += DeltaAngle;
                    }
                }
                else
                {
                    while (Angiterate > (Angfinish - DeltaAngle))
                    {
                        Xiterate = (radius * Math.Cos(Angiterate)) + centerX;
                        Yiterate = (radius * Math.Sin(Angiterate)) + centerY;
                        if (report)
                        {
                            debugText.AppendText("radius is " + radius + "x/y is " + Xiterate + "/" + Yiterate);
                            report = false;
                        }

                        GL.Vertex3(Xiterate, Yiterate, HEAD_LOCATION.Z);
                        Angiterate -= DeltaAngle;
                        //Angiterate += DeltaAngle;
                    }
                }
                GL.End();
                 */
                /*
                Point3D center = a.CenterVertex;
                Vector3 Base = new Vector3(0,0,1);
                int steps = 70;
                double radius = 0;
                double ta = 0;
                double ea = 0;
                if (a.CW)
                {
                    Point3D Start = a.StartVertex;
                    Point3D End = a.EndVertex;
                    Point3D Offset = a.OffsetVertex;
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Color3(Color.Wheat);
                    Point3D CenterOfArc = new Point3D(Start.X + Offset.X, Start.Y + Offset.Y, Start.Z + Offset.Z);
                    radius = DistanceBetweenPoints(Start, CenterOfArc);
#if debug
		                    debugText.AppendText("Center: " + CenterOfArc.X + "," + CenterOfArc.Y + "," + CenterOfArc.Z + "\n");
                    debugText.AppendText("Radius: " + radius + "\n");
  
	#endif 
                    Point3D TransposedStart = new Point3D(Start.X - CenterOfArc.X, Start.Y - CenterOfArc.Y, Start.Z - CenterOfArc.Z);
                    Point3D TransposedEnd = new Point3D(End.X - CenterOfArc.X, End.Y - CenterOfArc.Y, End.Z - CenterOfArc.Z);
                    //Point3D TrandposedCenter = new Point3D(0, 0, 0);
                    //ta = Math.PI / 4;//starting angle

                    // THIS ASSUMES ONLY ROTATION AROUND Z AXIS
                    if (a.Axis == 'Z')
                    {
                        //debugText.AppendText("Y/X = " + TransposedStart.Y + "/" + TransposedStart.X + "\n");
                        ta = Math.Atan(TransposedStart.Y / TransposedStart.X);
                        ea = Math.Atan(TransposedEnd.Y / TransposedEnd.X);
#if debug
                        debugText.AppendText("Start Angle = " + ta + "\n");
                        debugText.AppendText("End Angle = " + ea + "\n"); 
#endif
                        if (TransposedStart.X < 0)
                        {
                            ta = Math.Atan(TransposedStart.Y / TransposedStart.X) + Math.PI;
                        }
                        if (TransposedEnd.X < 0)
                        {
                            ea = Math.Atan(TransposedEnd.Y / TransposedEnd.X) + Math.PI;
                        }
#if debug
		                        debugText.AppendText("Start Angle = " + ta + "\n");
                        debugText.AppendText("End Angle = " + ea + "\n");
                        //debugText.AppendText("Test Angle = " + Math.Atan(1) + "\n");
                        debugText.AppendText("AngleSwept = " + AngleSweptOut + "*pi\n");
  
#endif                       
                        double AngleSweptOut = ea - ta;//GET THIS DONE is it done? it looks done
                        double DeltaTheta = AngleSweptOut / steps;
                        //END OF ASSUMING DUMB AREA

                        for (int i = 0; i <= steps; i++)
                        {
                            //oh wait no this assumes it also shoot...
                            GL.Vertex3(CenterOfArc.X + (radius * Math.Cos(ta)), CenterOfArc.Y + (radius * Math.Sin(ta)), CenterOfArc.Z + (radius * 0));
                            //ok now it's over
                            ta += DeltaTheta;
                            //debugText.AppendText("Added: (" + CenterOfArc.X + (radius * Math.Cos(ta)) + "," + CenterOfArc.Y + (radius * Math.Sin(ta)) + "," + CenterOfArc.Z + (radius * 0) + ")\n");
                        }
                        GL.End();
                        
                    }
                    else if(a.Axis == 'Y'){
                        //debugText.AppendText("Y/X = " + TransposedStart.Y + "/" + TransposedStart.X + "\n");
                        ta = Math.Atan(TransposedStart.Y / TransposedStart.X);
                        ea = Math.Atan(TransposedEnd.Y / TransposedEnd.X);
                        //debugText.AppendText("Start Angle = " + ta + "\n");
                        //debugText.AppendText("End Angle = " + ea + "\n");
                        if (TransposedStart.X < 0)
                        {
                            ta = Math.Atan(TransposedStart.Y / TransposedStart.X) + Math.PI;
                        }
                        if (TransposedEnd.X < 0)
                        {
                            ea = Math.Atan(TransposedEnd.Y / TransposedEnd.X) + Math.PI;
                        }
                        //debugText.AppendText("Start Angle = " + ta + "\n");
                        //debugText.AppendText("End Angle = " + ea + "\n");
                        //debugText.AppendText("Test Angle = " + Math.Atan(1) + "\n");
                        double AngleSweptOut = ea - ta;//GET THIS DONE is it done? it looks done
                        //debugText.AppendText("AngleSwept = " + AngleSweptOut + "*pi\n");
                        double DeltaTheta = AngleSweptOut / steps;
                        //END OF ASSUMING DUMB AREA

                        for (int i = 0; i <= steps; i++)
                        {
                            //oh wait no this assumes it also shoot...
                            GL.Vertex3(CenterOfArc.X + (radius * Math.Cos(ta)), CenterOfArc.Y + (radius * Math.Sin(ta)), CenterOfArc.Z + (radius * 0));
                            //ok now it's over
                            ta += DeltaTheta;
                            //debugText.AppendText("Added: (" + CenterOfArc.X + (radius * Math.Cos(ta)) + "," + CenterOfArc.Y + (radius * Math.Sin(ta)) + "," + CenterOfArc.Z + (radius * 0) + ")\n");
                        }
                        GL.End();
                    }
                }
//              Point3D HEAD_LOCATION = new Point3D(1,1,0);
//              Paths.Add(new Arc(new Point3D(1,1,0), new Point3D(-1, 1, 0), new Point3D(-1, -1, 0), true));
                else
                {
                    Point3D Start = a.StartVertex;
                    Point3D End = a.EndVertex;
                    Point3D Offset = a.OffsetVertex;
                    /*
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(Color.White);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(Start.X, Start.Y, Start.Z);
                    GL.End();
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(Color.White);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(End.X, End.Y, End.Z);
                    GL.End();
                     
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Color3(Color.SpringGreen);
                    Point3D CenterOfArc = new Point3D(Start.X + Offset.X, Start.Y+Offset.Y, Start.Z + Offset.Z);
                    radius = DistanceBetweenPoints(Start, CenterOfArc);
                    //debugText.AppendText("Center: " + CenterOfArc.X + "," + CenterOfArc.Y + "," + CenterOfArc.Z + "\n");
                    //debugText.AppendText("Radius: " + radius + "\n");
                    Point3D TransposedStart = new Point3D(Start.X - CenterOfArc.X, Start.Y - CenterOfArc.Y, Start.Z - CenterOfArc.Z);
                    Point3D TransposedEnd = new Point3D(End.X - CenterOfArc.X, End.Y - CenterOfArc.Y, End.Z - CenterOfArc.Z);
                    Point3D TrandposedCenter = new Point3D(0, 0, 0);
                    //ta = Math.PI / 4;//starting angle
                    //debugText.AppendText("Y/X = " + TransposedStart.Y + "/" + TransposedStart.X + "\n");
                    ta = Math.Atan(TransposedStart.Y / TransposedStart.X);
                    ea = Math.Atan(TransposedEnd.Y / TransposedEnd.X);
                    //debugText.AppendText("Start Angle = " + ta + "\n");
                    //debugText.AppendText("End Angle = " + ea + "\n");
                    if (TransposedStart.X < 0)
                    {
                        ta = Math.Atan(TransposedStart.Y / TransposedStart.X) + Math.PI;
                    }
                    if (TransposedEnd.X < 0)
                    {
                        ea = Math.Atan(TransposedEnd.Y / TransposedEnd.X)+Math.PI;
                    }
                    //debugText.AppendText("Start Angle = " + ta + "\n");
                    //debugText.AppendText("End Angle = " + ea + "\n");
                    //debugText.AppendText("Test Angle = " + Math.Atan(1) + "\n");
                    double AngleSweptOut = ea-ta-(2*Math.PI);//GET THIS DONE is it done? it looks done
                    //debugText.AppendText("AngleSwept = " + AngleSweptOut + "*pi\n");
                    double DeltaTheta = AngleSweptOut / steps;
                    for (int i = 0; i <= steps; i++)
                    {
                        GL.Vertex3(CenterOfArc.X + (radius * Math.Cos(ta)), CenterOfArc.Y + (radius * Math.Sin(ta)), CenterOfArc.Z + (radius * 0));
                        ta += DeltaTheta;
                      //  debugText.AppendText("Added: (" + CenterOfArc.X + (radius * Math.Cos(ta)) + "," + CenterOfArc.Y + (radius * Math.Sin(ta)) + "," + CenterOfArc.Z + (radius * 0) + ")\n");
                    }
                    GL.End();
                }*/
            }
        }
        private double DistanceBetweenPoints(Point3D a, Point3D b)
        {
            return Math.Pow(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2), 0.5);
        }

        private double CalculateAngle(Point3D P1, Point3D P2, Point3D Center, Vector3 Axis, bool Clockwise)
        {
            //for now axis is (0,0,1)
            //for now P1 is the start
            //for now P2 is the end
            //if angle to P1 smaller than angle to P2 AND NOT clockwise then return Math.Acos(CosineTheta)
            //else if angle to P2 smaller than angle to P1 AND clockwise then return Math.Acos(CosineTheta)
            //else return (2 * Math.PI) - Math.Acos(CosineTheta)
            Vector3 A = new Vector3(0, 0, 1);
            double HypotenuseLengthSquared = Math.Pow(P1.X - P2.X, 2) + Math.Pow(P1.Y - P2.Y, 2) + Math.Pow(P1.Y - P2.Y, 2);
            double SideLengthSquared = Math.Pow(P1.X - Center.X, 2) + Math.Pow(P1.Y - Center.Y, 2) + Math.Pow(P1.Y - Center.Y, 2);
            double CosineTheta = 1 - (HypotenuseLengthSquared / (2 * SideLengthSquared));
            return Math.Acos(CosineTheta);
        }
        private void forwardButton_Click(object sender, EventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            rotate_angle += 1;
            /*
            foreach (Drawable item in Paths)
            {
                RenderDrawable(item);
            }*/
            Arena.SwapBuffers();
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

            /*
            CPI.Plot3D.Plotter3D p = new CPI.Plot3D.Plotter3D(g);
            p.PenUp();
            Point3D loc = new Point3D(currentX, currentY, 0);
            p.MoveTo(loc);
            p.PenDown();

            p.PenColor = Color.Black;
            if (red)
            {
                p.PenColor = Color.Red;
            }
            if (cmd == "G01")
            {
                DrawLine(g, p, x_component, y_component);
            }
            this.currentX = x_component;
            this.currentY = y_component;
            debugOut.Text = "pos: " + p.Location;
            */
        }

        private void Arena_MouseClick(object sender, MouseEventArgs e)
        {
            ChangeScale(0.1F);
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
    }
    public static class MouseWheelHandler
    {
        public static void Add(Control ctrl, Action<MouseEventArgs> onMouseWheel)
        {
            if (ctrl == null || onMouseWheel == null)
                throw new ArgumentNullException();

            var filter = new MouseWheelMessageFilter(ctrl, onMouseWheel);
            Application.AddMessageFilter(filter);
            ctrl.Disposed += (s, e) => Application.RemoveMessageFilter(filter);
        }

        class MouseWheelMessageFilter : IMessageFilter
        {
            private readonly Control _ctrl;
            private readonly Action<MouseEventArgs> _onMouseWheel;

            public MouseWheelMessageFilter(Control ctrl, Action<MouseEventArgs> onMouseWheel)
            {
                _ctrl = ctrl;
                _onMouseWheel = onMouseWheel;
            }

            public bool PreFilterMessage(ref Message m)
            {
                var parent = _ctrl.Parent;
                if (parent != null && m.Msg == 0x20a) // WM_MOUSEWHEEL, find the control at screen position m.LParam
                {
                    var pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);

                    var clientPos = _ctrl.PointToClient(pos);

                    if (_ctrl.ClientRectangle.Contains(clientPos)
                     && ReferenceEquals(_ctrl, parent.GetChildAtPoint(parent.PointToClient(pos))))
                    {
                        var wParam = m.WParam.ToInt32();
                        Func<int, MouseButtons, MouseButtons> getButton =
                            (flag, button) => ((wParam & flag) == flag) ? button : MouseButtons.None;

                        var buttons = getButton(wParam & 0x0001, MouseButtons.Left)
                                    | getButton(wParam & 0x0010, MouseButtons.Middle)
                                    | getButton(wParam & 0x0002, MouseButtons.Right)
                                    | getButton(wParam & 0x0020, MouseButtons.XButton1)
                                    | getButton(wParam & 0x0040, MouseButtons.XButton2)
                                    ; // Not matching for these /*MK_SHIFT=0x0004;MK_CONTROL=0x0008*/

                        var delta = wParam >> 16;
                        var e = new MouseEventArgs(buttons, 0, clientPos.X, clientPos.Y, delta);
                        _onMouseWheel(e);

                        return true;
                    }
                }
                return false;
            }
        }
    }
}

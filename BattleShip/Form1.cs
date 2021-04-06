using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;


namespace BattleShip
{
    public partial class Form1 : Form
    {
        Bitmap View;
        Bitmap SeaView;
        Graphics g;
        int ani, SeaClock, Clock;
        double fadeInter;

        Ship CurrShip;
        int ShipIndex;
        System.Collections.ArrayList Ships;
        System.Collections.ArrayList Enemy;
        System.Collections.ArrayList Shells;
        System.Collections.ArrayList EnemyShells;
        System.Collections.ArrayList Explosions;
        System.Collections.ArrayList Wake;

        int ViewX, ViewY, DragX, DragY;
        bool DragOn;

        Color HUColor;
        Font HUFont;
        Brush HUBrush;

        //minimap
        int mmX, mmY, MMWidth, MMHeight;
        float MMScaleWidth, MMScaleHeight;

        Color MMap, MMCurrShip, MMShip, MMEnemy;

        //Foam
        Random R;


        public Form1()
        {
            try
            {
                InitializeComponent();
                HUColor = Color.WhiteSmoke;
                HUBrush = new SolidBrush(HUColor);
                HUFont = new Font("San Serif", 8.0F, FontStyle.Bold);
                GetSea();
                View = new Bitmap(pictureBox1.Width, pictureBox1.Height);

                //Fleet

                Ships = new System.Collections.ArrayList();
                Ships.Add(new Ship("Missouri", 0, 0, 45, 1.0f, 1000, "missouri-s2.bmp"));
                Ships.Add(new Ship("Iowa", 0, 100 / 2, 45, 1.0f, 1000, "missouri-s2.bmp"));
                Ships.Add(new Ship("Wisconsin", 100, 0, 45, 1.0f, 1000, "missouri-s2.bmp"));
                CurrShip = (Ship)Ships[0];
                ShipIndex = 0;

                //Enemy Fleet

                Enemy = new System.Collections.ArrayList();
                Enemy.Add(new Ship("Burt", 1000, 1100, 225, .5f, 1000, "Kaga.bmp"));
                Enemy.Add(new Ship("Ernie", 4000, 4300, 225, .25f, 1000, "Kaga.bmp"));
                Enemy.Add(new Ship("Kaga", 4000, 4200, 225, .75f, 1000, "Kaga.bmp"));

                Shells = new System.Collections.ArrayList();
                EnemyShells = new System.Collections.ArrayList();
                Explosions = new System.Collections.ArrayList();
                Wake = new System.Collections.ArrayList();
                //Scrolling
                ViewX = 0;
                ViewY = 0;
                DragOn = false;

                //minimap
                mmX = 0; mmY = 0;
                MMWidth = 200;
                MMHeight = 200;

                MMScaleWidth = 10000;
                MMScaleHeight = 10000;

                MMap = Color.PowderBlue;
                MMCurrShip = Color.DarkBlue;

                MMShip = Color.DarkGray;
                MMEnemy = Color.Red;

                //Animation
                fadeInter = 0;
                timer1.Enabled = true;
                Clock = 0;
                R = new Random();

            }
            catch (InvalidCastException e)
            {
                MessageBox.Show(e.Message, "public Form1()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetSea()
        {
            try
            {
                SeaView = new Bitmap(new Bitmap("Sea4.jpg"), 250, 4000);
                ani = -251;
                SeaClock = 0;
            }
            catch (InvalidCastException e)
            {
                MessageBox.Show(e.Message, "GetSea()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void DrawCircle(Graphics g, float x, float y)
        {
            try
            {
                g.FillEllipse(new SolidBrush(Color.Red), x - 3, y - 3, 6, 6);
            }
            catch (InvalidCastException e)
            {
                MessageBox.Show(e.Message, "DrawCircle()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void DrawExplosion(Graphics g, Explosion E)
        {
            double r = Math.Sqrt(E.Size * E.Frame);
            int green = 255 - (E.Frame - 1) * 50; // assumes 5 frames
            g.FillEllipse(new SolidBrush(Color.FromArgb(255, green, 0)), (float)(E.X - ViewX + pictureBox1.Width / 2 - r), (float)(ViewY - E.Y + pictureBox1.Height / 2 - r), (float)(2 * r), (float)(2 * r));
        }

        private void DrawView()
        {
            try
            {
                g = Graphics.FromImage(View);

                //animate Sea
                for (int i = ani; i <= pictureBox1.Width; i = i + 250) { g.DrawImage(SeaView, i, 1 + ani); }
                SeaClock = SeaClock + 1;
                ani = ani + SeaClock / 10;
                if (ani >= 0) ani = -251;
                if (SeaClock >= 10) SeaClock = 0;


                //MiniMap
                Bitmap MiniMap = new Bitmap(MMWidth, MMHeight);
                Graphics MMg = Graphics.FromImage(MiniMap);
                MMg.Clear(MMap);

                mmX = View.Width - MMWidth - 100;
                mmY = 100;
                float UnitPer = MMScaleWidth / MMWidth;

                int xc = ViewX - View.Width / 2;
                int yc = ViewY + View.Height / 2;

                int x1 = Convert.ToInt16(MMWidth / 2.0F + (xc / UnitPer));
                int y1 = Convert.ToInt16(MMHeight / 2.0F - (yc / UnitPer));

                int xw = Convert.ToInt16(View.Width / UnitPer);
                int yw = Convert.ToInt16(View.Height / UnitPer);

                MMg.DrawRectangle(new Pen(Color.DarkGray), x1, y1, xw, yw);
                //draw foam
                foreach (Ship s in Ships)
                {
                    int Rate = 1000000; // Rate = Infinity
                    if (s.Speed > 0.05)
                    {
                        Rate = Convert.ToInt32(1 / s.Speed);
                    }
                    if (Clock % Rate == 0)
                    {
                        double a = -1 * (s.Heading - 90);
                        a = (Math.PI / 180) * a; // convert to radians
                                                 // Starboard wake
                        float x = -100 * (float)Math.Cos(a + 0.03);
                        float y = -100 * (float)Math.Sin(a + 0.03);
                        float sp = (400 - R.Next(200)) / 10000.0F; // Convert to function of speed of ship
                        double dx = sp * Math.Cos(a + Math.PI / 2);
                        double dy = sp * Math.Sin(a + Math.PI / 2);
                        float Radius = (R.Next(50) + 1) / 25.0F;
                        int Life = 400 + R.Next(200);
                        Wake.Add(new Foam(s.X + x, s.Y + y, Radius, -dx, -dy, Life, Color.White));
                        // Port wake
                        x = -100 * (float)Math.Cos(a - 0.03);
                        y = -100 * (float)Math.Sin(a - 0.03);
                        Radius = (R.Next(50) + 1) / 25.0F;
                        // Recompute speed?
                        Life = 400 + R.Next(200);
                        Wake.Add(new Foam(s.X + x, s.Y + y, Radius, dx, dy, Life, Color.White));
                    }
                }
                foreach (Ship s in Enemy)
                {
                    int Rate = 1000000; // Rate = Infinity
                    if (s.Speed > 0.05)
                    {
                        Rate = Convert.ToInt32(1 / s.Speed);
                    }
                    if (Clock % Rate == 0)
                    {
                        double a = -1 * (s.Heading - 90);
                        a = (Math.PI / 180) * a; // convert to radians
                                                 // Starboard wake
                        float x = -100 * (float)Math.Cos(a + 0.03);
                        float y = -100 * (float)Math.Sin(a + 0.03);
                        float sp = (400 - R.Next(200)) / 10000.0F; // Convert to function of speed of ship
                        double dx = sp * Math.Cos(a + Math.PI / 2);
                        double dy = sp * Math.Sin(a + Math.PI / 2);
                        float Radius = (R.Next(50) + 1) / 25.0F;
                        int Life = 400 + R.Next(200);
                        Wake.Add(new Foam(s.X + x, s.Y + y, Radius, -dx, -dy, Life, Color.White));
                        // Port wake
                        x = -100 * (float)Math.Cos(a - 0.03);
                        y = -100 * (float)Math.Sin(a - 0.03);
                        Radius = (R.Next(50) + 1) / 25.0F;
                        // Recompute speed?
                        Life = 400 + R.Next(200);
                        Wake.Add(new Foam(s.X + x, s.Y + y, Radius, dx, dy, Life, Color.White));
                    }
                }
                foreach (Foam f in Wake)
                {
                    float BitmapX = ConvertCartesianXtoBitmapX(View, f.X);
                    float BitmapY = ConvertCartesianYtoBitmapY(View, f.Y);
                    f.Draw(g, BitmapX, BitmapY, 1.0F);
                }
                //Draw ships
                foreach (Ship ship in Ships)
                {
                    if (ship.DesiredHeading != ship.Heading)
                    {
                        //Show Desired Heading
                        double x = ship.X - ViewX + pictureBox1.Width / 2 + (125 * Math.Cos(((Math.PI / 180.0) * (ship.DesiredHeading - 90))));
                        double y = -ship.Y + ViewY + pictureBox1.Height / 2 + (125 * Math.Sin(((Math.PI / 180.0) * (ship.DesiredHeading - 90))));
                        g.DrawLine(new Pen(Color.FromArgb(Convert.ToInt16(75), Color.White), 2), ship.X - ViewX + pictureBox1.Width / 2, ViewY - ship.Y + pictureBox1.Height / 2, (float)x, (float)y);
                    }

                    int MiniX = Convert.ToInt16(MMWidth / 2.0F + (ship.X / UnitPer));
                    int MiniY = Convert.ToInt16(MMHeight / 2.0F - (ship.Y / UnitPer));

                    if (ship == CurrShip)
                    {

                        MMg.FillRectangle(new SolidBrush(MMCurrShip), MiniX, MiniY, 3, 3);

                    }
                    else
                    {

                        MMg.FillRectangle(new SolidBrush(MMShip), MiniX, MiniY, 3, 3);
                    }

                    g.DrawImage(ship.Image, ship.X - ship.Image.Width / 2 - ViewX + pictureBox1.Width / 2, ViewY - ship.Y - ship.Image.Height / 2 + pictureBox1.Height / 2);

                    DrawCircle(g, ship.X - ViewX + pictureBox1.Width / 2, ViewY - ship.Y + pictureBox1.Height / 2);
                }

                foreach (Ship ship in Enemy)
                {
                    int MiniX = Convert.ToInt16(MMWidth / 2.0F + (ship.X / UnitPer));
                    int MiniY = Convert.ToInt16(MMHeight / 2.0F - (ship.Y / UnitPer));
                    MMg.FillRectangle(new SolidBrush(MMEnemy), MiniX, MiniY, 3, 3);

                    g.DrawImage(ship.Image, ship.X - ship.Image.Width / 2 - ViewX + pictureBox1.Width / 2, ViewY - ship.Y - ship.Image.Height / 2 + pictureBox1.Height / 2);
                    DrawCircle(g, ship.X - ViewX + pictureBox1.Width / 2, ViewY - ship.Y + pictureBox1.Height / 2);
                    g.DrawString(ship.Name, HUFont, HUBrush, ship.HUX - ViewX + pictureBox1.Width / 2, -ship.HUY + ViewY + pictureBox1.Height / 2);
                    g.DrawString(ship.HP.ToString(), HUFont, HUBrush, ship.HUX - ViewX + pictureBox1.Width / 2, -ship.HUY + ViewY + pictureBox1.Height / 2 + 15);
                }
                // Age foam
                System.Collections.ArrayList DeadFoam = new System.Collections.ArrayList();
                foreach (Foam f in Wake)
                {
                    f.Tick();
                    if (f.Visible == false)
                        DeadFoam.Add(f);
                }
                foreach (Foam f in DeadFoam)
                    Wake.Remove(f);


                foreach (Shell s in Shells)
                {
                    DrawCircle(g, (float)(s.X - ViewX + pictureBox1.Width / 2), (float)(ViewY - s.Y + pictureBox1.Height / 2));
                }

                foreach (Shell s in EnemyShells)
                {
                    DrawCircle(g, (float)(s.X - ViewX + pictureBox1.Width / 2), (float)(ViewY - s.Y + pictureBox1.Height / 2));
                }

                //Explosions
                System.Collections.ArrayList DeadExplosions = new System.Collections.ArrayList();

                foreach (Explosion E in Explosions)
                {
                    DrawExplosion(g, E);
                    E.Grow();
                    if (E.Frame ==6)
                    {
                        DeadExplosions.Add(E);
                    }
                }

                foreach (Explosion E in DeadExplosions )
                {
                    Explosions.Remove(E);
                }

                

                g.DrawImage(MiniMap, mmX, mmY);

                //Debug for center screen
                //DrawCircle(g, pictureBox1.Width/2, pictureBox1.Height / 2);            

                //Calculation for Displaying infro
                float displayX = CurrShip.X - ViewX + pictureBox1.Width / 2;
                float displayY = -CurrShip.Y + ViewY + pictureBox1.Height / 2;

                float tempX = CurrShip.HUX - ViewX + pictureBox1.Width / 2;
                float tempY = -CurrShip.HUY + ViewY + pictureBox1.Height / 2;

                //Blinking circle
                fadeInter = fadeInter + .1;
                if (fadeInter >= 4.5) fadeInter = -1.5;
                double se = (200 * (Math.Sin(fadeInter) + 1) / 2);
                g.DrawEllipse(new Pen(Color.FromArgb(Convert.ToInt16(se), Color.Red), 3), displayX - 125, displayY - 125, 250, 250);

                //Ship info
                g.DrawString(CurrShip.Name, HUFont, HUBrush, tempX, tempY);
                g.DrawString("HP: " + CurrShip.HP.ToString("N0"), HUFont, HUBrush, tempX, tempY + 15);
                g.DrawString("S: " + CurrShip.Speed.ToString("N2"), HUFont, HUBrush, tempX, tempY + 30);

                //HUD
                g.DrawString("(" + ViewX + ", " + ViewY + ")", HUFont, HUBrush, 100, 100);
                g.DrawString(CurrShip.Name, HUFont, HUBrush, 100, 120);
                g.DrawString("H: " + CurrShip.Heading.ToString("N1"), HUFont, HUBrush, 100, 140);
                g.DrawString("S: " + CurrShip.Speed.ToString("N2"), HUFont, HUBrush, 100, 160);
                g.DrawString("Loc: " + "(" + CurrShip.X.ToString("n1") + ", " + CurrShip.Y.ToString("n1") + ")", HUFont, HUBrush, 100, 180);

                pictureBox1.Image = View;
            }
            catch (InvalidCastException e)
            {
                MessageBox.Show(e.Message, "DrawView", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private float ConvertCartesianXtoBitmapX(Bitmap b, double CartesianX)
        {
            return b.Width / 2.0F + (float)(CartesianX - ViewX);
        }



        private float ConvertCartesianYtoBitmapY(Bitmap b, double CartesianY)
        {
            return b.Height / 2.0F - (float)(CartesianY - ViewY);
        }

        private void IndicateMiss(double X, double Y)
        {
            // Add initial animation?
            for (int i = 0; i < 18; i++) // wake
            {
                double a = (i * 20) * Math.PI / 180.0; // angle in radians
                //double r = R.Next(100) * 0.05; // radius 0 to 5
                double r = 3.0 + R.Next(10) * 0.1;
                double x = r * Math.Cos(a);
                double y = r * Math.Sin(a);
                float Radius = (R.Next(30) + 10) / 20.0F;
                // Note: fast dx, dy looks like explosion
                double dx = 0.035 * Math.Cos(a);
                double dy = 0.035 * Math.Sin(a);
                Wake.Add(new Foam(X + x, Y + y, Radius, dx, dy, R.Next(300) + 50,Color.White));
            }
            Wake.Add(new Foam(X, Y, 5.0F, 0, 0, 12,Color.White)); // blast
        }

        private void IndicateSunk(double X, double Y)
        {
            // Add initial animation?
            for (int i = 0; i < 18; i++) // wake
            {
                double a = (i * 20) * Math.PI / 180.0; // angle in radians
                //double r = R.Next(100) * 0.05; // radius 0 to 5
                double r = 3.0 + R.Next(50) * 0.1;
                double x = r * Math.Cos(a);
                double y = r * Math.Sin(a);
                float Radius = (R.Next(100) + 50) / 20.0F;
                // Note: fast dx, dy looks like explosion
                double dx = 0.035 * Math.Cos(a);
                double dy = 0.035 * Math.Sin(a);
                Wake.Add(new Foam(X + x, Y + y, Radius, dx, dy, R.Next(300) + 300,Color.DarkGray));

    
            }
            for (int i = 0; i < 18; i++) // wake
            {
                double a = (i * 20) * Math.PI / 180.0; // angle in radians
                //double r = R.Next(100) * 0.05; // radius 0 to 5
                double r = 3.0 + R.Next(250) * 0.1;
                double x = r * Math.Cos(a);
                double y = r * Math.Sin(a);
                float Radius = (R.Next(100) + 500) / 20.0F;
                // Note: fast dx, dy looks like explosion
                double dx = 0.035 * Math.Cos(a);
                double dy = 0.035 * Math.Sin(a);
                Wake.Add(new Foam(X + x, Y + y, Radius, dx, dy, R.Next(300) + 500, Color.DarkGray));


            }


            Wake.Add(new Foam(X, Y, 5.0F, 0, 0, 50,Color.DarkGray)); // blast
        }



        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                DragOn = true;
                //First Drag location
                DragX = e.X;
                DragY = e.Y;
            }
            catch (InvalidCastException e2)
            {
                MessageBox.Show(e2.Message, "pictureBox1_MouseDown", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            DragOn = false;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (DragOn)
                {
                    ViewX += (DragX - e.X);
                    ViewY -= (DragY - e.Y);
                    DragX = e.X;
                    DragY = e.Y;
                }
            }
            catch (InvalidCastException e2)
            {
                MessageBox.Show(e2.Message, " pictureBox1_MouseMove", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Width = ClientSize.Width;
                pictureBox1.Height = ClientSize.Height;
                if (View != null)
                {
                    if (pictureBox1.Height < 1)
                    {
                        timer1.Enabled = false;
                    }
                    else
                    {
                        View.Dispose();
                        View = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                        DrawView();
                        timer1.Enabled = true;
                    }
                }
            }
            catch (InvalidCastException e2)
            {
                MessageBox.Show(e2.Message, "Form1_Resize", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Width = ClientSize.Width;
                pictureBox1.Height = ClientSize.Height;
                View.Dispose();
                View = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                DrawView();
            }
            catch (InvalidCastException e2)
            {
                MessageBox.Show(e2.Message, " Form1_Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                ++Clock;

                foreach (Ship ship in Ships)
                {
                    ship.Move();
                    ship.Gun.Operate(Clock, Shells);
                }

                foreach (Ship ship in Enemy)
                {
                    ship.Move();
                    ship.Gun.Operate(Clock, EnemyShells);
                }

                SetTargets(Ships, Enemy);
                SetTargets(Enemy, Ships);

                ResolveHits(Ships,Shells, Enemy);
                ResolveHits(Enemy, EnemyShells, Ships);

                DrawView();
            }
            catch (InvalidCastException e2)
            {
                MessageBox.Show(e2.Message, " timer1_Tick", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                //Heading
                if ((e.X >= this.pictureBox1.Width - 100) && (e.Y <= 100))
                {
                    CurrShip.DesiredHeading += 45;
                    CurrShip.Rudder = 1;
                    return;
                }
                if ((e.X <= 100) && (e.Y <= 100))
                {
                    CurrShip.DesiredHeading -= 45;
                    CurrShip.Rudder = -1;
                    return;
                }
                if (e.X > 100 && e.X < pictureBox1.Width - 100 && e.Y <= 50)
                {
                    CurrShip.DesiredHeading = CurrShip.Heading;
                    CurrShip.Rudder = 0;
                    return;
                }
                //Engine
                if ((e.X <= 100) && (e.Y >= pictureBox1.Height - 100))
                {
                    CurrShip.EngineDown();
                    return;
                }
                if ((e.X >= this.pictureBox1.Width - 100) && (e.Y >= pictureBox1.Height - 100))
                {
                    CurrShip.EngineUp();
                    return;
                }
                if (e.X > 100 && e.X < pictureBox1.Width - 100 && e.Y >= pictureBox1.Height - 50)
                {
                    CurrShip.EngineHold();
                    return;
                }
                //Change Current Ship
                if ((e.X >= this.pictureBox1.Width - 50) && (e.Y > 100) && (e.Y < pictureBox1.Height - 100))
                {
                    //CurrShip = Ships[0];
                    ShipIndex++;
                    if (ShipIndex >= Ships.Count) ShipIndex = 0;
                    CurrShip = (Ship)Ships[ShipIndex];
                    return;

                }
                if ((e.X <= 50) && (e.Y > 100) && (e.Y < pictureBox1.Height - 100))
                {
                    //CurrShip = Ships[0];
                    ShipIndex--;
                    if (ShipIndex < 0) ShipIndex = Ships.Count - 1;
                    CurrShip = (Ship)Ships[ShipIndex];
                    return;

                }
                //Minimap
                if ((e.X >= mmX) && (e.X <= (mmX + MMWidth)) && (e.Y >= mmY) && (e.Y <= (mmY + MMHeight))) {

                    int px = e.X - (mmX + MMWidth / 2);
                    int py = e.Y - (mmY + MMHeight / 2);

                    float UPP = MMScaleWidth / MMWidth;

                    ViewX = (int)(px * UPP);
                    ViewY = -(int)(py * UPP);
                    return;

                }
            }
            catch (InvalidCastException e2)
            {
                MessageBox.Show(e2.Message, "pictureBox1_MouseClick", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void SetTargets(System.Collections.ArrayList Firing, System.Collections.ArrayList Targets)
        {
            try
            {
               

                foreach (Ship ship in Firing)
                {
                    double distance = 10000.0;
                    ship.Gun.Target = null;
                    foreach (Ship target in Targets)
                    {
                        double d = Math.Sqrt((ship.X - target.X) * (ship.X - target.X) + (ship.Y - target.Y) * (ship.Y - target.Y));
                        if (d <= distance)
                        {
                            distance = d;
                            ship.Gun.Target = target;
                        }
                    }
                }
            }
            catch (InvalidCastException e2)
            {
                MessageBox.Show(e2.Message, "SetTargets", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ResolveHits(System.Collections.ArrayList Firing, System.Collections.ArrayList FiredShells, System.Collections.ArrayList Targets)
        {
            System.Collections.ArrayList DeadShells = new System.Collections.ArrayList();
            System.Collections.ArrayList DeadEnemy = new System.Collections.ArrayList();

            foreach (Shell s in FiredShells)
            {
                if (Clock >= s.Time) //hits impact point
                {
                    try
                    {
                        foreach (Ship target in Targets)
                        {
                            double d = Math.Sqrt((target.X - s.X) * (target.X - s.X) + (target.Y - s.Y) * (target.Y - s.Y));
                            if (d <= 10)
                            {
                                target.HP -= s.Damage;
                                Explosions.Add(new Explosion(target.X, target.Y, 50));
                                

                                SoundPlayer player = new SoundPlayer("Bomb.wav");
                                player.Play();

                                if (target.HP <= 0)
                                {
                                    DeadEnemy.Add(target);
                                    IndicateSunk(s.X, s.Y);
                                }
                            }
                            else
                            {
                                IndicateMiss(s.X, s.Y);
                            }
                            
                            DeadShells.Add(s);
                        }
                    }
                    catch (InvalidCastException e2)
                    {
                        MessageBox.Show(e2.Message, " ResolveHits-1", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            foreach (Shell s in DeadShells)
            {
                FiredShells.Remove(s);
            }
            foreach (Ship s in DeadEnemy)
            {
                Targets.Remove(s);
            }
        }

    }


    class Ship
    {
        String ShipName;

        float x, y; //Location of ship
        Point sXY;
        float h, s; //Heading & Speed
        float hx, hy; //Location for Ship info
        float m; //Manuver Rate
        int engine;
        float rudder; // left-1, straight = 0, right = 1
        // The original is the bitmap load from the file.  rotBitMap is the bitmap rotated to match the current heading.  If the heading = DesiredHead, no need to rotate the bitmap again.
        Bitmap original, rotBitMap;
        float dh;//desired heading
        float ds; //desired speed
        int hp; //Ships condition/damage

        Gun gun;


        public Ship(String pName, float pX, float pY, float pStartHeading, float pSpeed, int pHitPoints, String pImage)
        {
            ShipName = pName;
            X = pX;  //Center X
            Y = pY;  //Center Y
            Heading = pStartHeading;
            DesiredHeading = Heading;
            Speed = pSpeed;  //Sets Engine, initial speed, desired speed;
            HP= pHitPoints;
            LoadImage = pImage;

            m = 0.2f;
            gun = new Gun("16 in/50 Caliber Mk 7", this, 2000, 8.0f, 200);

        }
        public String Name
        {
            get { return ShipName; }
        }
        public float X
        {
            get { return x; }
            set { x = value; }
        }
        public float Y
        {
            get { return y; }
            set { y = value; }
        }
        /*/ public Point ShipLoc
         {
             get { return sXY; }
             set { sXY = value; }
         }
        /*/
        public float HUX
        {
            get { return hx; }
        }
        public float HUY
        {
            get { return hy; }
        }

        public float Heading
        {
            get
            {
                //heading is measured in 360 degrees in a circle Heading is never greater than 360 or less than 0
                if (h < 0) { return 360 + (h % 360); } //h is negative
                if (h > 360) { return h % 360; } // greater than 360 return mod
                if (h == 0) { return 360; } //Could be zero degrees just as well
                return (float)Math.Round(h, 1); ;
            }
            set { h = value;
                if (Heading == DesiredHeading) Rudder = 0;
            }
        }
        public float Rudder
        {
            set { rudder = value; }
        }
        public float DesiredHeading
        {
            get
            {
                if (dh < 0) { return 360 + (dh % 360); }
                if (dh > 360) { return dh % 360; }
                if (dh == 0) { return 360; }
                //Math.Round(dh,1)
                return (float)Math.Round(dh, 1);
            }
            set
            {
                dh = value;
            }
        }
        public float Speed
        {
            get { return s; }

            private set {
                s = value;
                ds = s;
                switch (s)
                {
                    case 0: engine = 0; break;
                    case .25f: engine = 1; break;
                    case .5f: engine = 2; break;
                    case .75f: engine = 3; break;
                    case 1: engine = 4; break;
                    default: engine = 0; break;
                }
            }
        }
        public float XCorner
        {
            get { return X - (original.Width / 2); }
        }
        public float YCorner
        {
            get { return Y - (original.Height / 2); }
        }
        public Bitmap Image
        {
            get
            {
                //If heading not changed use last bitmap
                //else rotate a new bitmap
                if (dh == h)
                { return rotBitMap; }
                else
                {
                    rotBitMap = rotateImage(original, h);
                    return rotBitMap;
                }
            }
        }
        public int HP
        {
            get { return hp; }
            set { hp = value; }
        }
        public Gun Gun
        {
            get { return gun; }
        }

        private string LoadImage
        {

            set
            {
                string iName = value;
                if (original != null) { original.Dispose(); }
                original = new Bitmap(iName);
                original = new Bitmap(new Bitmap(iName), original.Width, original.Height);
                original.MakeTransparent(original.GetPixel(0, 0));
                rotBitMap = rotateImage(original, Heading);
            }
        }
        private Bitmap rotateImage(Bitmap b, float angle)
        {
            //create a new empty bitmap to hold rotated image
            Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(returnBitmap);
            //move rotation point to center of image
            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
            //rotate
            g.RotateTransform(angle);
            //move image back
            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
            //draw passed in image onto graphics object
            g.DrawImage(b, 0, 0);
            return returnBitmap;
        }
        public void Move()
        {
            //Check Heading and Change if needed.
            if (Heading != DesiredHeading)
            {
                if (rudder > 0)
                {
                    Heading += (float)Math.Min(m , Math.Abs(DesiredHeading - Heading)); ;
                }
                else
                {
                    Heading -= (float)Math.Min(m, Math.Abs(DesiredHeading - Heading)); ;
                }
            }

            //Convert Heading to Degrees
            double Degrees = 90 - h;
            double Radians = Degrees * (Math.PI / 180);
            //Check Speed
            if (s != ds)
            {
                if (ds < s)
                {
                    s -= (float)Math.Min(.001F, s - ds);
                }
                else
                {
                    s += (float)Math.Min(.01F, ds - s);
                }
            }

            //Calc X AND Y
            float dx = s * (float)Math.Cos(Radians);
            float dy = s * (float)Math.Sin(Radians);

            x += dx;
            y += dy;
            float dist = 260;

            //set spot for ship info
            if (Heading <= 90) { dist = 170 - (90 - Heading); }
            if (Heading > 90 && Heading <= 180) { dist = 260 - (Heading); }
            if (Heading > 180 && Heading <= 270) { dist = 80 + (Heading) - 180; }
            if (Heading > 270 && Heading <= 360) { dist = 440 - (Heading); }

            Radians = 180 * (Math.PI / 180);
            dx = dist * (float)Math.Cos(Radians);
            dy = dist * (float)Math.Sin(Radians);

            hx = x + dx;
            hy = y - dy;
        }
        public void EngineUp()
        {
            if (engine < 4)
            {

                ++engine;
                switch (engine)
                {
                    case 0: ds = 0; break;
                    case 1: ds = .25F; break;
                    case 2: ds = .5F; break;
                    case 3: ds = .75F; break;
                    case 4: ds = 1; break;
                }
            }
        }
        public void EngineDown()
        {
            if (ds > 0)
            {
                --engine;
                switch (engine)
                {
                    case 0: ds = 0; break;
                    case 1: ds = .25F; break;
                    case 2: ds = .5F; break;
                    case 3: ds = .75F; break;
                    case 4: ds = 1; break;
                }
            }
        }
        public void EngineHold()
        {
            ds = s;
        }
    }


    class Shell // Munition, Torpedo, Bomb, Projectile, Shot, Round, Ordnance, Ammunition, Weapon, Rocket, Missle
    {
        string type; // "16 in/50 caliber Mark 7"
        int t; // time of impact
        double x, y; // impact location in global coordinates
        int damage;
        public Shell(string Type, int Time, double X, double Y, int Damage)
        {
            type = Type;
            t = Time;
            x = X;
            y = Y;
            damage = Damage;
        }
        public string Type
        {
            get { return type; }
        }
        public int Time
        {
            get { return t; }
        }
        public double X
        {
            get { return x; }
        }
        public double Y
        {
            get { return y; }
        }
        public int Damage
        {
            get { return damage; }
        }
    }


    class Gun
    {
        bool loaded;
        string type;
        double range; // in cartesian units
        float velocity; // in cartesian untis
        int loadtime;
        int rttl; // Remaining Time To Load
        Ship parent;
        Ship target;
        // number of shells?
        // bitmap of gun
        // location relative to ship center
        // orientation of gun
        // firing animation

        Random r;

        public Gun(string Type, Ship Parent, double Range, float Velocity, int LoadDuration)
        {
            type = Type;
            range = Range; // 2000.0
            velocity = Velocity; // 8.0
            loadtime = LoadDuration; // 200
            rttl = 0;
            loaded = true;
            parent = Parent;
            target = null;
            r = new Random();
        }
        public bool Loaded
        {
            get { return loaded; }
            set { loaded = value; }
        }
        public Ship Target
        {
            get { return target; }
            set { target = value; }
        }
        public double Range // Read-Only
        {
            get { return range; }
        }
        public void Operate(int Clock, System.Collections.ArrayList Shells)
        {
            if (loaded)
            {
                if (target != null)
                {
                    double d = Math.Sqrt((parent.X - target.X) * (parent.X - target.X) + (parent.Y - target.Y) * (parent.Y - target.Y)); // in cartesian units
                    if (d < range) // Fire!
                    {

                        SoundPlayer player = new SoundPlayer("Gun.wav");
                        player.Play();
                        int flighttime = Convert.ToInt32(d / velocity);
                        // predict location (d=rt)
                        // Determine cartesian impact coordinates
                        double degrees = -1 * (target.Heading - 90.0F); // target ship cartesain heading
                        double radians = degrees * (Math.PI / 180.0);
                        double ix = target.X + target.Speed * flighttime * Math.Cos(radians);

                        

                        double iy = target.Y + target.Speed * flighttime * Math.Sin(radians);
                        
                        //add a little random
                       // ix = ix + r.Next(1, 30) - 15;
                      //  iy = iy + r.Next(1, 30) - 15;

                        // Create Shell, start Reload
                        //   Shells.Add(new Shell("16in/50 caliber Mark 7", Clock + flighttime, ix, iy, 200));
                        Shells.Add(new Shell(parent.Name, Clock + flighttime, ix, iy, 200));  
                           loaded = false;
                        rttl = loadtime;
                    }
                }
            }
            else
            {
                --rttl;
                if (rttl == 0)
                    loaded = true;
            }
        }
    }

    class Explosion
    {
        double x;
        double y;
        int frame;
        double size;
        public Explosion(double X, double Y, double Size)
        {
            x = X;
            y = Y;
            size = Size;
            frame = 1;
        }
        public double X // Read Only
        {
            get { return x; }
        }
        public double Y
        {
            get { return y; }
        }
        public int Frame
        {
            get { return frame; }
            set { frame = value; }
        }
        public double Size
        {
            get { return size; }
        }
        public void Grow()
        {
            ++frame;
        }
    }

    class Foam
    {
        double x, y; // global cartesian location
        int age;
        int ma; // max age
        float r;
        double dx, dy; // direction
        Color C;
        public Foam(double X, double Y, float Radius, double Dx, double Dy, int Life, Color Clr)
        {
            x = X;
            y = Y;
            age = 0;
            ma = Life;
            r = Radius;
            dx = Dx;
            dy = Dy;
            C = Clr;
        }
        public double X
        {
            get { return x; }
        }
        public double Y
        {
            get { return y; }
        }
        public void Tick()
        {
            ++age;
            // move foam
            x += dx;
            y += dy;
        }
        public void Draw(Graphics g, float bx, float by, float Scale) // Need global view coordinates or bitmap coordinates
        {
            int alpha = 255 - Convert.ToInt32(255 * ((float)age / ma));
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;
            SolidBrush b = new SolidBrush(Color.FromArgb(alpha, C));
            float r1 = r * Scale;
            float d = 2 * r;
            g.FillEllipse(b, bx - r1, by - r1, d, d);
        }
        public bool Visible
        {
            get
            {
                if (age < ma)
                    return true;
                else
                    return false;
            }
        }
    } // end class Foam

}

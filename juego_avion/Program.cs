//*********************************************//
//  INSTITUCION : UNIVERSIDAD                  //
//  VERSION : 2.0.0.0                          //
//  FECHA DE CREACION: 01/07/2013              //
//  DESARROLLADO : JUAN CARLOS PINTO L.        //
//*********************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;

namespace Juego_Aviones
{
    //************ EJECUCION DEL PROGRAMA *************//
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public partial class Form1 : Form
    {
        //************ VARIABLES GLOBALES *************
        PictureBox navex = new PictureBox();
        PictureBox naveRival = new PictureBox();
        PictureBox contiene = new PictureBox();
        System.Windows.Forms.Timer tiempo;
        System.Windows.Forms.Timer obstaculosTimer;
        readonly Random random = new Random();
        int Dispara = 0;
        bool flag = false;
        float angulo = 0;
        System.Windows.Forms.Label label1 =new System.Windows.Forms.Label();
        System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
        System.Windows.Forms.Label labelProgreso = new System.Windows.Forms.Label();
        readonly List<Color> escenarios = new List<Color>
        {
            Color.AliceBlue,
            Color.DarkSlateGray,
            Color.MidnightBlue,
            Color.Black
        };
        readonly List<PictureBox> asteroides = new List<PictureBox>();
        readonly List<PictureBox> enemigos = new List<PictureBox>();
        readonly List<PictureBox> disparosEnemigos = new List<PictureBox>();
        readonly HashSet<PictureBox> enemigosEnPicado = new HashSet<PictureBox>();
        readonly Dictionary<PictureBox, PointF> velocidadPicado = new Dictionary<PictureBox, PointF>();
        int nivelActual = 1;
        int puntaje = 0;
        int framesEscenario = 0;
        int framesEnemigos = 0;

        //************ DIAGRAMAR DEL MISIL ************//
        public void CrearMisil(int AngRotar, Color pintar, string nombre, int x, int y)
        {
            dynamic Balas = new PictureBox();
            int PosX = 1;
            int PosY = 1;
            int largoM = 11;
            int anchoM = 8;
            //declarar los array de puntos.
            Point[] myMisil1 = { new Point(4 * PosX, 0 * PosY), new Point(5 * PosX, 1 * PosY), new
            Point(6 * PosX, 2 * PosY), new Point(6 * PosX, 7 * PosY), new Point(7 * PosX, 8 * PosY), new
            Point(8 * PosX, 9 * PosY), new Point(7 * PosX, 9 * PosY), new Point(6 * PosX, 10 * PosY), new
            Point(2 * PosX, 10 * PosY), new Point(1 * PosX, 9 * PosY), new Point(0 * PosX, 9 * PosY), new
            Point(1 * PosX, 8 * PosY), new Point(2 * PosX, 7 * PosY), new Point(2 * PosX, 2 * PosY), new
            Point(3 * PosX, 1 * PosY), new Point(4 * PosX, 0 * PosY) };

            Point[] myMisil = new Point[myMisil1.Count()];
            for (int i = 0; i < myMisil1.Count(); i++)
            {
                myMisil[i].X = myMisil1[i].X;
                if (AngRotar == 180)
                    myMisil[i].Y = largoM - myMisil1[i].Y;
                else
                    myMisil[i].Y = myMisil1[i].Y;
            }
            GraphicsPath ObjGrafico = new GraphicsPath();
            ObjGrafico.AddPolygon(myMisil);
            Balas.Location = new Point(x, y);
            Balas.BackColor = pintar;
            Balas.Size = new Size(anchoM * PosX, largoM * PosY);
            Balas.Region = new Region(ObjGrafico);
            contiene.Controls.Add(Balas);
            Balas.Visible = true;
            Balas.Tag = nombre;
            //*************** DIBUJAR COLORES *************//
            Bitmap flag = new Bitmap(anchoM, largoM);
            Graphics flagImagen = Graphics.FromImage(flag);
            flagImagen.FillRectangle(Brushes.Orange, 2, 8, 5, 1);
            flagImagen.FillRectangle(Brushes.Yellow, 3, 10, 3, 1);
            Balas.Image = flag;
        }

        //***************DESTRUCTOR DEL MISIL********************//
        private void ImpactarTick(object sender, EventArgs e)
        {
            // VARIABLES LOCALES
            int X = naveRival.Location.X;
            int Y = naveRival.Location.Y;
            int W = naveRival.Width;
            int H = naveRival.Height;
            int PH = 10;
            int X2 = navex.Location.X;
            int Y2 = navex.Location.Y;
            int W2 = navex.Width;
            int H2 = navex.Height;
            int x = naveRival.Location.X;
            int y = naveRival.Location.Y;

            Dispara++;
            // ACCION DE DISPARAR DEL RIVAL
            if (Dispara == 100 && naveRival.Visible == true)
            {
                int xRival = naveRival.Location.X + (naveRival.Width / 2);
                int yRival = naveRival.Location.Y + (naveRival.Height / 2);
                CrearMisil(180, Color.OrangeRed, "Rival", xRival, yRival);
                Dispara = 0;
            }
            // MOVIMIENTO DE LA NAVE A DESTRUIR
            if (flag == false)
            {
                if (contiene.Width == x + naveRival.Width)
                    flag = true;
                x++;
            }
            else
            {
                if (contiene.Location.X == x)
                    flag = false;
                x--;
            }
            naveRival.Location = new Point(x, y);

            // ELIMINACION DEL MISIL Y DESCONTAR PUNTOS DE IMPACTO DE LA NAVE RIVAL
            foreach (Control c in contiene.Controls)
            {
                if (c is PictureBox)
                {
                    int X1 = ((PictureBox)c).Location.X;
                    int Y1 = ((PictureBox)c).Location.Y;
                    int W1 = ((PictureBox)c).Width;
                    int H1 = ((PictureBox)c).Height;
                    string nombre = ((PictureBox)c).Tag.ToString();

                    // ACTIVIDAD DE IMPACTO CON LA NAVE RIVAL
                    if (X < X1 && X1 + W1 < X + W && Y < Y1 && Y1 + H1 < Y + H && nombre == "Misil")
                    {
                        if (X + PH < X1 && X1 + W1 < X + W - PH)
                        {
                            ((PictureBox)c).Dispose();
                            naveRival.Tag = int.Parse(naveRival.Tag.ToString()) - 10;
                            puntaje += 10;
                        }
                        else
                        {
                            ((PictureBox)c).Dispose();
                            naveRival.Tag = int.Parse(naveRival.Tag.ToString()) - 1;
                            puntaje += 5;
                        }
                        label1.Text = "Vida del Rival : " + naveRival.Tag.ToString();
                        ActualizarProgreso();
                        //tiempo.Stop();
                    }
                    else if (int.Parse(naveRival.Tag.ToString()) <= 0)
                    {
                        naveRival.Dispose();
                        Bitmap NuevoImg = new Bitmap(contiene.Width, contiene.Height);
                        Graphics flagImagen = Graphics.FromImage(NuevoImg);
                        // Crear cadena para dibujar.
                        String drawString = "Felicitaciones Ganaste_!";
                        // Crear la fuente y el pincel.
                        Font drawFont = new Font("Arial", 16);
                        SolidBrush drawBrush = new SolidBrush(Color.Blue);
                        Point drawPoint = new Point(40, 150);
                        // Dibujar cadena en pantalla.
                        flagImagen.DrawString(drawString, drawFont, drawBrush, drawPoint);
                        contiene.Image = NuevoImg;
                        tiempo.Stop();
                    }

                    // IMPACTO DE MISIL CON ASTEROIDES
                    if (nombre == "Misil")
                    {
                        Rectangle misilRect = new Rectangle(((PictureBox)c).Location, ((PictureBox)c).Size);
                        foreach (PictureBox ast in asteroides.ToList())
                        {
                            Rectangle astRect = new Rectangle(ast.Location, ast.Size);
                            if (misilRect.IntersectsWith(astRect))
                            {
                                ((PictureBox)c).Dispose();
                                asteroides.Remove(ast);
                                ast.Dispose();
                                puntaje += 3;
                                ActualizarProgreso();
                                break;
                            }
                        }

                        foreach (PictureBox enemigo in enemigos.ToList())
                        {
                            Rectangle enemigoRect = new Rectangle(enemigo.Location, enemigo.Size);
                            if (misilRect.IntersectsWith(enemigoRect))
                            {
                                ((PictureBox)c).Dispose();
                                enemigos.Remove(enemigo);
                                enemigosEnPicado.Remove(enemigo);
                                velocidadPicado.Remove(enemigo);
                                enemigo.Dispose();
                                puntaje += 8;
                                ActualizarProgreso();
                                break;
                            }
                        }
                    }

                    // ACTIVIDAD DE IMPACTO CON MI NAVE
                    if (X2 < X1 && X1 + W1 < X2 + W2 && Y2 < Y1 && Y1 + H1 < Y2 + H2 && nombre == "Rival")
                    {
                        if (X2 + PH < X1 && X1 + W1 < X2 + W2 - PH)
                        {
                            ((PictureBox)c).Dispose();
                            navex.Tag = int.Parse(navex.Tag.ToString()) - 10;
                        }
                        else
                        {
                            ((PictureBox)c).Dispose();
                            navex.Tag = int.Parse(navex.Tag.ToString()) - 1;
                        }
                        label2.Text = "Mi Nave : " + navex.Tag.ToString();
                        ActualizarProgreso();
                    }
                    else if (int.Parse(navex.Tag.ToString()) <= 0)
                    {
                        navex.Dispose();
                        Bitmap NuevoImg = new Bitmap(contiene.Width, contiene.Height);
                        Graphics flagImagen = Graphics.FromImage(NuevoImg);
                        // Crear cadena para dibujar.
                        String drawString = "Perdiste el juego";
                        // Crear la fuente y el pincel.
                        Font drawFont = new Font("Arial", 16);
                        SolidBrush drawBrush = new SolidBrush(Color.Red);
                        Point drawPoint = new Point(70, 150);
                        // Dibujar cadena en pantalla.
                        flagImagen.DrawString(drawString, drawFont, drawBrush, drawPoint);
                        contiene.Image = NuevoImg;
                        tiempo.Stop();
                    }

                    if (((PictureBox)c).Location.Y <= 0 && nombre == "Misil")
                    {
                        ((PictureBox)c).Dispose();
                    }
                    if (((PictureBox)c).Location.Y >= contiene.Height && nombre == "Rival")
                    {
                        ((PictureBox)c).Dispose();
                    }
                    if (nombre == "Misil")
                    {
                        ((PictureBox)c).Top -= 10;
                    }
                    if (nombre == "Rival")
                    {
                        ((PictureBox)c).Top += 10;
                    }
                    if (W >= X2 && H >= Y2 && W2 >= X && H2 >= Y)
                    {
                        naveRival.Dispose();
                        navex.Dispose();
                    }
                }
                else
                    tiempo.Stop();

            }
        }

        //************ DIAGRAMAR NAVE***************//
        public void CrearNave(PictureBox Avion, int AngRotar, int Tipox, Color Pintar, int Vida)
        {
            int largoN = 0;
            int anchoN = 0;
            Point[] myNave1 = { new Point(29, 0), new Point(30, 1), new Point(30, 6), new Point(31, 6),
            new Point(31, 11), new Point(32, 11), new Point(32, 17), new Point(35, 17), new Point(35, 16), new
            Point(37, 16), new Point(37, 17), new Point(38, 18), new Point(38, 28), new Point(39, 28), new
            Point(42, 39), new Point(44, 45), new Point(50, 51), new Point(51, 51), new Point(51, 52), new
            Point(58, 59), new Point(58, 66), new Point(44, 66), new Point(39, 71), new Point(35, 71), new
            Point(35, 74), new Point(32, 77), new Point(26, 77), new Point(23, 74), new Point(23, 71), new
            Point(19, 71), new Point(14, 66), new Point(0, 66), new Point(0, 59), new Point(7, 52), new Point(7, 51),
            new Point(8, 51), new Point(14, 45), new Point(16, 39), new Point(19, 28), new Point(20, 28), new
            Point(20, 18), new Point(21, 17), new Point(21, 16), new Point(23, 16), new Point(23, 17), new
            Point(26, 17), new Point(26, 11), new Point(27, 11), new Point(27, 6), new Point(28, 6), new
            Point(28, 1), new Point(29, 0) };

            Point[] myNave2 = {
            new Point(16, 0), new Point(17, 1), new Point(18, 2), new Point(18, 3),
            new Point(19, 3), new Point(20, 3), new Point(21, 4), new Point(22, 5),
            new Point(23, 6), new Point(24, 7), new Point(25, 8), new Point(26, 9),
            new Point(25, 9), new Point(24, 9), new Point(23, 9), new Point(22, 9),
            new Point(21, 10), new Point(20, 11), new Point(20, 12), new Point(20, 13),
            new Point(21, 14), new Point(22, 15), new Point(22, 16), new Point(22, 17),
            new Point(22, 18), new Point(22, 19), new Point(22, 20), new Point(22, 20),
            new Point(22, 21), new Point(22, 22), new Point(23, 23), new Point(24, 24),
            new Point(25, 25), new Point(26, 26), new Point(27, 27), new Point(28, 28),
            new Point(29, 29), new Point(30, 30), new Point(31, 31), new Point(32, 32),
            new Point(33, 33), new Point(33, 34), new Point(33, 35), new Point(33, 36),
            new Point(33, 37), new Point(33, 38), new Point(33, 39), new Point(33, 40),
            new Point(32, 40), new Point(31, 40), new Point(30, 40), new Point(29, 41),
            new Point(28, 42), new Point(27, 42), new Point(26, 42), new Point(25, 41),
            new Point(24, 40), new Point(23, 40), new Point(22, 40), new Point(21, 40),
            new Point(20, 40), new Point(19, 40), new Point(18, 40), new Point(17, 41),
            new Point(16, 42), new Point(15, 41), new Point(14, 40), new Point(13, 40),
            new Point(12, 40), new Point(11, 40), new Point(10, 40), new Point(9, 40),
            new Point(8, 41), new Point(7, 42), new Point(6, 42), new Point(5, 42),
            new Point(4, 41), new Point(3, 40), new Point(2, 40), new Point(1, 40),
            new Point(0, 40), new Point(0, 39), new Point(0, 38), new Point(0, 37),
            new Point(0, 36), new Point(0, 35), new Point(0, 34), new Point(0, 33),
            new Point(1, 32), new Point(2, 31), new Point(3, 30), new Point(4, 29),
            new Point(5, 28), new Point(6, 27), new Point(7, 26), new Point(8, 25),
            new Point(9, 24), new Point(10, 23), new Point(11, 22), new Point(11, 21),
            new Point(11, 20), new Point(11, 19), new Point(11, 18), new Point(11, 17),
            new Point(11, 16), new Point(11, 15), new Point(11, 14), new Point(12, 13),
            new Point(12, 12), new Point(12, 11), new Point(11, 10), new Point(10, 9),
            new Point(9, 9), new Point(8, 9), new Point(7, 9), new Point(6, 9),
            new Point(7, 8), new Point(8, 7), new Point(9, 6), new Point(10, 5),
            new Point(11, 4), new Point(12, 3), new Point(13, 3), new Point(14, 3),
            new Point(14, 2), new Point(15, 1), new Point(16, 0)};

            Point[] myNave3 = {
            new Point(25, 54), new Point(26, 54), new Point(26, 50), new Point(26, 50),
            new Point(27, 50), new Point(28, 50), new Point(29, 50), new Point(30, 51),
            new Point(31, 51), new Point(32, 52), new Point(32, 49), new Point(31, 48),
            new Point(30, 47), new Point(29, 46), new Point(28, 45), new Point(27, 44),
            new Point(27, 36), new Point(28, 35), new Point(28, 25), new Point(29, 25),
            new Point(30, 25), new Point(31, 25), new Point(32, 26), new Point(33, 26),
            new Point(34, 27), new Point(35, 28), new Point(36, 28), new Point(37, 29),
            new Point(38, 30), new Point(39, 30), new Point(40, 31), new Point(41, 32),
            new Point(42, 32), new Point(43, 33), new Point(44, 34), new Point(45, 35),
            new Point(46, 36), new Point(47, 36), new Point(48, 36), new Point(49, 37),
            new Point(50, 37), new Point(51, 38), new Point(51, 37), new Point(51, 36),
            new Point(51, 35), new Point(50, 35), new Point(37, 22), new Point(37, 15),
            new Point(36, 14), new Point(35, 14), new Point(34, 15), new Point(34, 21),
            new Point(28, 15), new Point(28, 7), new Point(27, 6), new Point(26, 5),
            new Point(25, 5), new Point(24, 6), new Point(23, 7), new Point(23, 15),
            new Point(17, 21), new Point(17, 15), new Point(16, 14), new Point(15, 14),
            new Point(14, 15), new Point(14, 22), new Point(1, 35), new Point(0, 35),
            new Point(0, 36), new Point(0, 37), new Point(0, 38), new Point(1, 37),
            new Point(2, 37), new Point(3, 36), new Point(4, 36), new Point(5, 36),
            new Point(6, 35), new Point(7, 34), new Point(8, 33), new Point(9, 32),
            new Point(10, 32), new Point(11, 31), new Point(12, 30), new Point(13, 30),
            new Point(14, 29), new Point(15, 28), new Point(16, 28), new Point(17, 27),
            new Point(18, 26), new Point(19, 26), new Point(20, 25), new Point(21, 25),
            new Point(22, 25), new Point(23, 25), new Point(23, 35), new Point(24, 36),
            new Point(24, 44), new Point(23, 45), new Point(22, 46), new Point(21, 47),
            new Point(20, 48), new Point(19, 49), new Point(19, 52), new Point(20, 51),
            new Point(21, 51), new Point(22, 50), new Point(23, 50), new Point(24, 50),
            new Point(25, 50), new Point(25, 54) };

            Point[] myNave;

            //********************INSERTAR EL OBJETO***********//
            GraphicsPath ObjGrafico = new GraphicsPath();
            if (Tipox == 1)
            {
                largoN = 77;
                anchoN = 58;
                // Rotación
                myNave = new Point[myNave1.Count()];
                for (int i = 0; i < myNave1.Count(); i++)
                {
                    myNave[i].X = myNave1[i].X;
                    if (AngRotar == 180)
                        myNave[i].Y = largoN - myNave1[i].Y;
                    else
                        myNave[i].Y = myNave1[i].Y;
                }
                ObjGrafico.AddPolygon(myNave);
            }
            else if (Tipox == 2)
            {
                largoN = 42;
                anchoN = 33;
                // Rotación
                myNave = new Point[myNave2.Count()];
                for (int i = 0; i < myNave2.Count(); i++)
                {
                    myNave[i].X = myNave2[i].X;
                    if (AngRotar == 180)
                        myNave[i].Y = largoN - myNave2[i].Y;
                    else
                        myNave[i].Y = myNave2[i].Y;
                }
                ObjGrafico.AddPolygon(myNave);
            }
            else if (Tipox == 3)
            {
                largoN = 54;
                anchoN = 51;
                // Rotación
                myNave = new Point[myNave3.Count()];
                for (int i = 0; i < myNave3.Count(); i++)
                {
                    myNave[i].X = myNave3[i].X;
                    if (AngRotar == 180)
                        myNave[i].Y = largoN - myNave3[i].Y;
                    else
                        myNave[i].Y = myNave3[i].Y;
                }
                ObjGrafico.AddPolygon(myNave);
            }

            Avion.Location = new Point(0, 0);
            Avion.BackColor = Pintar;
            Avion.Size = new Size(anchoN, largoN);
            Avion.Region = new Region(ObjGrafico);

            //***********INSERTAR LA NAVE AL CONTENEDOR************//
            contiene.Controls.Add(Avion);
            NaveCorre(Avion, Tipox, AngRotar);
            Avion.Tag = Vida;
            Avion.Visible = true;
        }

        //********* EFECTOS DE LA NAVE PRINCIPAL **********//

        public void NaveCorre(PictureBox Avion, int AngRotar, int velox)

        {
            Bitmap Imagen = new Bitmap(58, 77);
            Graphics PintaImg = Graphics.FromImage(Imagen);
            Point[] puntoDer = { new Point(35, 28), new Point(35, 30), new Point(36, 30), new
         Point(37, 31), new Point(37, 37), new Point(38, 38), new Point(38, 40), new Point(39, 41), new
         Point(39, 44), new Point(40, 45), new Point(40, 46), new Point(42, 48), new Point(43, 48), new
         Point(44, 49), new Point(44, 64), new Point(43, 65), new Point(42, 65), new Point(41, 66), new
         Point(40, 66), new Point(38, 68), new Point(36, 68), new Point(36, 69), new Point(36, 68), new
         Point(35, 62), new Point(35, 28) };

            Point[] puntoIzq = { new Point(23, 28), new Point(23, 30), new Point(22, 30), new
         Point(21, 31), new Point(21, 37), new Point(20, 38), new Point(20, 40), new Point(19, 41), new
         Point(19, 44), new Point(18, 45), new Point(18, 46), new Point(16, 48), new Point(15, 48), new
         Point(14, 49), new Point(14, 64), new Point(15, 65), new Point(16, 65), new Point(17, 66), new
         Point(18, 66), new Point(20, 68), new Point(22, 68), new Point(22, 69), new Point(22, 63), new
         Point(23, 62), new Point(23, 28) };

            Point[] puntoAtr = { new Point(29, 21), new Point(31, 19), new Point(32, 19), new
         Point(33, 20), new Point(33, 25), new Point(32, 26), new Point(32, 63), new Point(34, 65), new
         Point(34, 68), new Point(33, 69), new Point(33, 74), new Point(32, 73), new Point(31, 73), new
         Point(29, 71), new Point(27, 73), new Point(26, 73), new Point(25, 74), new Point(25, 69), new
         Point(24, 68), new Point(24, 65), new Point(26, 63), new Point(26, 26), new Point(25, 25), new
         Point(25, 20), new Point(26, 19), new Point(27, 19), new Point(29, 21) };

            PintaImg.FillPolygon(Brushes.DarkGreen, puntoDer);
            PintaImg.FillPolygon(Brushes.DarkGreen, puntoIzq);
            PintaImg.FillPolygon(Brushes.DarkGreen, puntoAtr);
            PintaImg.FillRectangle(Brushes.Silver, 25, 35, 1, 15);
            PintaImg.FillRectangle(Brushes.Silver, 32, 35, 1, 15);
            PintaImg.FillRectangle(Brushes.Silver, 29, 58, 1, 13);
            if (velox == 1)
            {
                PintaImg.FillRectangle(Brushes.DarkOrange, 35, 68, 6, 1);
                PintaImg.FillRectangle(Brushes.Orange, 36, 69, 4, 1);
                PintaImg.FillRectangle(Brushes.Yellow, 37, 70, 2, 1);
                PintaImg.FillRectangle(Brushes.DarkOrange, 17, 68, 6, 1);
                PintaImg.FillRectangle(Brushes.Orange, 18, 69, 4, 1);
                PintaImg.FillRectangle(Brushes.Yellow, 19, 70, 2, 1);
            }
            else if (velox == 2)
            {
                PintaImg.FillRectangle(Brushes.DarkRed, 15, 30, 1, 8);
                PintaImg.FillRectangle(Brushes.DarkRed, 25, 28, 1, 16);
                PintaImg.FillRectangle(Brushes.DarkRed, 35, 30, 1, 8);
            }
            else if (velox == 3)
            {
                PintaImg.FillRectangle(Brushes.DarkRed, 15, 30, 1, 8);
                PintaImg.FillRectangle(Brushes.DarkRed, 25, 28, 1, 16);
                PintaImg.FillRectangle(Brushes.DarkRed, 35, 30, 1, 8);
            }
            Avion.Image = RotateImage(Imagen, AngRotar);
        }

        //*********************CREAR ANGULO DE ROTACION*****************//
        public static Image RotateImage(Image img, float rotationAngle)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
            gfx.RotateTransform(rotationAngle);
            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gfx.DrawImage(img, new Point(0, 0));
            gfx.Dispose();
            return bmp;
        }

        //***********MOVIMIENTO DEL TECLADO DEL USUARIO***********//
        public void ActividadTecla(object sender, KeyEventArgs e)
        {
            //INSTRUCCIONES DE LOS BOTONES
            switch (e.KeyValue)
            {
                case 37: // flecha hacia la izquierda
                    {
                        if (contiene.Left < navex.Left) // PARAMETROS DE LIMITE
                            navex.Left -= 10;
                        angulo = -45;
                        NaveCorre(navex, angulo, 0);
                        break;
                    }
                case 38: // flecha hacia arriba
                    {
                        if (contiene.Top < navex.Top)
                            navex.Top -= 10;
                        angulo = 0;
                        NaveCorre(navex, angulo, 1);
                        break;
                    }
                case 39: // flecha hacia la derecha
                    {
                        if (contiene.Right > navex.Right)
                            navex.Left += 10;
                        angulo = 45;
                        NaveCorre(navex, angulo, 0);
                        break;
                    }
                case 40: // flecha hacia abajo
                    {
                        if (contiene.Bottom > navex.Bottom)
                            navex.Top += 10;
                        angulo = 0;
                        NaveCorre(navex, angulo, 1);
                        break;
                    }
                case 87: // W
                    {
                        if (contiene.Top < navex.Top)
                            navex.Top -= 10;
                        angulo = 0;
                        NaveCorre(navex, angulo, 1);
                        break;
                    }
                case 83: // S
                    {
                        if (contiene.Bottom > navex.Bottom)
                            navex.Top += 10;
                        angulo = 0;
                        NaveCorre(navex, angulo, 1);
                        break;
                    }
                case 65: // A
                    {
                        if (contiene.Left < navex.Left) // PARAMETROS DE LIMITE
                            navex.Left -= 10;
                        angulo = -45;
                        NaveCorre(navex, angulo, 0);
                        break;
                    }
                case 68: // D
                    {
                        if (contiene.Right > navex.Right)
                            navex.Left += 10;
                        angulo = 45;
                        NaveCorre(navex, angulo, 0);
                        break;
                    }
                case 13: // Enter - disparo
                    {
                        tiempo.Start();
                        int x = navex.Location.X + (navex.Width / 2);
                        int y = navex.Location.Y + (navex.Height / 2);
                        CrearMisil(0, Color.DarkOrange, "Misil", x, y);
                        break;
                    }
                case 32: // Espacio - disparo
                    {
                        tiempo.Start();
                        int x = navex.Location.X + (navex.Width / 2);
                        int y = navex.Location.Y + (navex.Height / 2);
                        CrearMisil(0, Color.DarkOrange, "Misil", x, y);
                        break;
                    }
            }
        }

        //************** ACTIVAR ACCIONES DE INICIALIZACION **************//
        public void Iniciar()
        {
            //******************************************************//
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Width = 345;
            this.Height = 450;
            this.Text = "JUEGO DE AVIONES";
            label1.Text = "Mi Rival";
            label2.Text = "Mi Avion";
            labelProgreso.Text = "Nivel 1 - Puntaje 0";
            this.KeyDown += new KeyEventHandler(ActividadTecla);
            //*****************************************************//
            contiene.Location = new Point(0, 0);
            contiene.BackColor = escenarios[0];
            contiene.BackgroundImage = CrearFondo(nivelActual);
            contiene.Size = new Size(300, 420);
            contiene.Dock = DockStyle.Fill;
            Controls.Add(contiene);
            contiene.Visible = true;
            label1.Location = new Point(10, 10);
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            contiene.Controls.Add(label1);
            label2.Location = new Point(10, 30);
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            contiene.Controls.Add(label2);
            labelProgreso.Location = new Point(10, 50);
            labelProgreso.AutoSize = true;
            labelProgreso.BackColor = Color.Transparent;
            contiene.Controls.Add(labelProgreso);
            //******Contenido del formulario*******//
            Random r = new Random();
            int aleatY = r.Next(250, 330);
            int aleatX = r.Next(50, 250);
            CrearNave(navex, 0, 1, Color.SeaGreen, 20);
            //ELEGIR NAVE DE SALIDA RIVAL
            Random sal = new Random();
            int sale = sal.Next(1, 3);
            CrearNave(naveRival, 180, sale, Color.DarkBlue, 50);
            //Modulo.Escenario(contiene, I);
            navex.Location = new Point(aleatX, aleatY);

            tiempo = new System.Windows.Forms.Timer();
            tiempo.Interval = 1;
            tiempo.Enabled = true;
            tiempo.Tick += new EventHandler(ImpactarTick);

            obstaculosTimer = new System.Windows.Forms.Timer();
            obstaculosTimer.Interval = 20;
            obstaculosTimer.Enabled = true;
            obstaculosTimer.Tick += new EventHandler(MoverAsteroidesTick);

            ActualizarProgreso();
        }

        private void MoverAsteroidesTick(object sender, EventArgs e)
        {
            framesEscenario++;
            if (framesEscenario % 40 == 0 && asteroides.Count < nivelActual + 3)
            {
                CrearAsteroide();
            }

            framesEnemigos++;
            if (framesEnemigos % 60 == 0 && enemigos.Count < nivelActual + 4)
            {
                CrearEnemigo();
            }

            if (framesEnemigos % 45 == 0)
            {
                DispararEnemigos();
            }

            if (framesEnemigos % 120 == 0)
            {
                LanzarPicadoGalaga();
            }

            foreach (PictureBox ast in asteroides.ToList())
            {
                ast.Top += 2 + nivelActual;
                if (ast.Top > contiene.Height)
                {
                    asteroides.Remove(ast);
                    ast.Dispose();
                    continue;
                }

                Rectangle naveRect = new Rectangle(navex.Location, navex.Size);
                Rectangle astRect = new Rectangle(ast.Location, ast.Size);
                if (naveRect.IntersectsWith(astRect))
                {
                    asteroides.Remove(ast);
                    ast.Dispose();
                    navex.Tag = int.Parse(navex.Tag.ToString()) - 5;
                    label2.Text = "Mi Nave : " + navex.Tag.ToString();
                    ActualizarProgreso();
                }
            }

            MoverEnemigos();
            MoverDisparosEnemigos();

            CambiarEscenarioSiEsNecesario();
        }

        private void CambiarEscenarioSiEsNecesario()
        {
            int nuevoNivel = 1 + (puntaje / 30);
            if (nuevoNivel != nivelActual && nuevoNivel - 1 < escenarios.Count)
            {
                nivelActual = nuevoNivel;
                contiene.BackColor = escenarios[nivelActual - 1];
                contiene.BackgroundImage = CrearFondo(nivelActual);
                ActualizarProgreso();
            }
        }

        private void CrearAsteroide()
        {
            int ancho = random.Next(15, 30);
            int alto = random.Next(15, 30);
            PictureBox asteroide = new PictureBox();
            asteroide.Size = new Size(ancho, alto);
            asteroide.BackColor = Color.Transparent;
            asteroide.Tag = "Asteroide" + asteroides.Count;
            asteroide.Location = new Point(random.Next(0, Math.Max(1, contiene.Width - ancho)), -alto);

            Bitmap imagen = new Bitmap(ancho, alto);
            Graphics g = Graphics.FromImage(imagen);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillEllipse(Brushes.Gray, 0, 0, ancho, alto);
            g.FillEllipse(Brushes.DimGray, ancho / 4, alto / 4, ancho / 2, alto / 2);
            g.Dispose();
            asteroide.Image = imagen;

            contiene.Controls.Add(asteroide);
            asteroides.Add(asteroide);
            asteroide.BringToFront();
        }

        private void CrearEnemigo()
        {
            int ancho = 26;
            int alto = 18;
            PictureBox enemigo = new PictureBox();
            enemigo.Size = new Size(ancho, alto);
            enemigo.BackColor = Color.Transparent;
            enemigo.Tag = "Enemigo";
            enemigo.Location = new Point(random.Next(10, Math.Max(20, contiene.Width - ancho - 10)), -alto);

            Bitmap imagen = new Bitmap(ancho, alto);
            Graphics g = Graphics.FromImage(imagen);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPolygon(Brushes.DarkRed, new[] { new Point(ancho / 2, 0), new Point(ancho, alto), new Point(0, alto) });
            g.FillEllipse(Brushes.OrangeRed, ancho / 4, alto / 3, ancho / 2, alto / 2);
            g.Dispose();
            enemigo.Image = imagen;

            contiene.Controls.Add(enemigo);
            enemigos.Add(enemigo);
            enemigo.BringToFront();
        }

        private void MoverEnemigos()
        {
            foreach (PictureBox enemigo in enemigos.ToList())
            {
                if (enemigosEnPicado.Contains(enemigo))
                {
                    if (!velocidadPicado.TryGetValue(enemigo, out PointF velocidad))
                    {
                        enemigosEnPicado.Remove(enemigo);
                        continue;
                    }

                    int nuevoX = (int)(enemigo.Location.X + velocidad.X);
                    int nuevoY = (int)(enemigo.Location.Y + velocidad.Y);
                    enemigo.Location = new Point(nuevoX, nuevoY);
                }
                else
                {
                    double fase = (framesEnemigos + enemigo.Location.X) * 0.1;
                    int nuevoX = enemigo.Location.X + (int)(Math.Sin(fase) * 4);
                    nuevoX = Math.Max(0, Math.Min(contiene.Width - enemigo.Width, nuevoX));
                    enemigo.Location = new Point(nuevoX, enemigo.Location.Y + 2 + nivelActual);
                }

                Rectangle naveRect = new Rectangle(navex.Location, navex.Size);
                Rectangle enemigoRect = new Rectangle(enemigo.Location, enemigo.Size);
                if (naveRect.IntersectsWith(enemigoRect))
                {
                    velocidadPicado.Remove(enemigo);
                    enemigosEnPicado.Remove(enemigo);
                    enemigos.Remove(enemigo);
                    enemigo.Dispose();
                    navex.Tag = int.Parse(navex.Tag.ToString()) - 8;
                    label2.Text = "Mi Nave : " + navex.Tag.ToString();
                    ActualizarProgreso();
                    continue;
                }

                if (enemigo.Location.Y > contiene.Height)
                {
                    velocidadPicado.Remove(enemigo);
                    enemigosEnPicado.Remove(enemigo);
                    enemigos.Remove(enemigo);
                    enemigo.Dispose();
                }
            }
        }

        private void LanzarPicadoGalaga()
        {
            if (enemigos.Count == 0 || navex == null || navex.IsDisposed)
            {
                return;
            }

            PictureBox? candidato = enemigos.OrderBy(_ => random.Next()).FirstOrDefault(e => !enemigosEnPicado.Contains(e));
            if (candidato == null)
            {
                return;
            }

            Point centroNave = new Point(navex.Left + (navex.Width / 2), navex.Top + (navex.Height / 2));
            Point centroEnemigo = new Point(candidato.Left + (candidato.Width / 2), candidato.Top + (candidato.Height / 2));
            float dx = centroNave.X - centroEnemigo.X;
            float dy = centroNave.Y - centroEnemigo.Y;
            float magnitud = (float)Math.Sqrt((dx * dx) + (dy * dy));
            if (magnitud <= 0.01f)
            {
                return;
            }

            float velocidad = 4f + nivelActual;
            PointF paso = new PointF((dx / magnitud) * velocidad, (dy / magnitud) * velocidad);
            velocidadPicado[candidato] = paso;
            enemigosEnPicado.Add(candidato);
        }

        private void DispararEnemigos()
        {
            foreach (PictureBox enemigo in enemigos)
            {
                PictureBox disparo = new PictureBox();
                disparo.Size = new Size(6, 12);
                disparo.BackColor = Color.Yellow;
                disparo.Tag = "DisparoEnemigo";
                disparo.Location = new Point(enemigo.Location.X + (enemigo.Width / 2) - 3, enemigo.Location.Y + enemigo.Height);
                contiene.Controls.Add(disparo);
                disparosEnemigos.Add(disparo);
                disparo.BringToFront();
            }
        }

        private void MoverDisparosEnemigos()
        {
            foreach (PictureBox disparo in disparosEnemigos.ToList())
            {
                disparo.Top += 6 + nivelActual;
                if (disparo.Top > contiene.Height)
                {
                    disparosEnemigos.Remove(disparo);
                    disparo.Dispose();
                    continue;
                }

                Rectangle naveRect = new Rectangle(navex.Location, navex.Size);
                Rectangle disparoRect = new Rectangle(disparo.Location, disparo.Size);
                if (naveRect.IntersectsWith(disparoRect))
                {
                    disparosEnemigos.Remove(disparo);
                    disparo.Dispose();
                    navex.Tag = int.Parse(navex.Tag.ToString()) - 5;
                    label2.Text = "Mi Nave : " + navex.Tag.ToString();
                    ActualizarProgreso();
                }
            }
        }

        private Image CrearFondo(int nivel)
        {
            Bitmap fondo = new Bitmap(contiene.Width > 0 ? contiene.Width : 320, contiene.Height > 0 ? contiene.Height : 460);
            Color baseColor = escenarios[Math.Max(0, Math.Min(escenarios.Count - 1, nivel - 1))];
            using (Graphics g = Graphics.FromImage(fondo))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, fondo.Width, fondo.Height),
                           ControlPaint.Light(baseColor), ControlPaint.Dark(baseColor), LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 0, 0, fondo.Width, fondo.Height);
                }

                for (int i = 0; i < 35 + (nivel * 5); i++)
                {
                    int x = random.Next(0, fondo.Width);
                    int y = random.Next(0, fondo.Height);
                    g.FillEllipse(Brushes.WhiteSmoke, x, y, 2, 2);
                }
            }

            return fondo;
        }

        private void ActualizarProgreso()
        {
            labelProgreso.Text = $"Nivel {nivelActual} - Puntaje {puntaje} - Escenario {nivelActual}";
        }
        //****ARGUMENTOS GENERADOS POR EL PROGRAMA*******//
        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(320, 460);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "JUEGO DE AVIONES";
            KeyPreview = true;
            DoubleBuffered = true;
            Load += new EventHandler(Form1_Load);
            ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Iniciar();
        }
    }
}

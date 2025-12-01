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
        private class EntityState
        {
            public string Tipo { get; set; }
            public int Vida { get; set; }

            public EntityState(string tipo, int vida)
            {
                Tipo = tipo;
                Vida = vida;
            }
        }

        //************ VARIABLES GLOBALES *************
        PictureBox navex = new PictureBox();
        PictureBox naveRival = new PictureBox();
        List<PictureBox> enemigosPequenos = new List<PictureBox>();
        PictureBox contiene = new PictureBox();
        System.Windows.Forms.Timer tiempo;
        System.Windows.Forms.Timer asteroideTimer;
        System.Windows.Forms.Timer enemigoPequenoTimer;
        int Dispara = 0;
        bool flag = false;
        float angulo = 0;
        int vidasJugador = 3;
        int tipoNaveActual = 1;
        System.Windows.Forms.Label label1 = new System.Windows.Forms.Label();
        System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
        System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
        List<PictureBox> asteroides = new List<PictureBox>();
        Random generador = new Random();

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
            if (navex.IsDisposed)
            {
                tiempo.Stop();
                return;
            }

            Dispara++;
            // ACCION DE DISPARAR DEL RIVAL
            if (Dispara == 100 && naveRival.Visible)
            {
                int xRival = naveRival.Location.X + (naveRival.Width / 2);
                int yRival = naveRival.Location.Y + (naveRival.Height / 2);
                CrearMisil(180, Color.OrangeRed, "MisilEnemigo", xRival, yRival);
                Dispara = 0;
            }

            MoverNaveRival();
            MoverEnemigosPequenos();

            List<PictureBox> paraEliminar = new List<PictureBox>();

            foreach (PictureBox c in contiene.Controls.OfType<PictureBox>().ToList())
            {
                EntityState estado = c.Tag as EntityState;
                string nombre = c.Tag as string;

                if (nombre == "MisilJugador")
                {
                    c.Top -= 10;
                    if (c.Bottom <= 0)
                    {
                        paraEliminar.Add(c);
                        continue;
                    }

                    foreach (PictureBox enemigo in ObtenerEnemigos().ToList())
                    {
                        if (c.Bounds.IntersectsWith(enemigo.Bounds))
                        {
                            var estadoEnemigo = enemigo.Tag as EntityState;
                            if (estadoEnemigo != null)
                            {
                                estadoEnemigo.Vida -= estadoEnemigo.Tipo == "EnemigoGrande" ? 10 : 5;
                                if (estadoEnemigo.Vida <= 0)
                                {
                                    EliminarEnemigo(enemigo);
                                }
                                ActualizarIndicadores();
                            }
                            paraEliminar.Add(c);
                            break;
                        }
                    }
                }
                else if (nombre == "MisilEnemigo")
                {
                    c.Top += 7;
                    if (c.Top >= contiene.Height)
                    {
                        paraEliminar.Add(c);
                        continue;
                    }

                    if (navex.Visible && c.Bounds.IntersectsWith(navex.Bounds))
                    {
                        var estadoJugador = navex.Tag as EntityState;
                        if (estadoJugador != null)
                        {
                            estadoJugador.Vida -= 5;
                            ActualizarIndicadores();
                            RevisarVidaJugador();
                        }
                        paraEliminar.Add(c);
                    }
                }
            }

            foreach (PictureBox elemento in paraEliminar)
            {
                elemento.Dispose();
            }
        }

        private void MoverNaveRival()
        {
            if (naveRival.IsDisposed || !naveRival.Visible)
                return;

            int x = naveRival.Location.X;
            int y = naveRival.Location.Y;

            if (!flag)
            {
                if (contiene.Width <= x + naveRival.Width + 2)
                    flag = true;
                x++;
            }
            else
            {
                if (x <= 2)
                    flag = false;
                x--;
            }

            naveRival.Location = new Point(x, y);
        }

        private IEnumerable<PictureBox> ObtenerEnemigos()
        {
            if (naveRival != null && !naveRival.IsDisposed && naveRival.Visible)
                yield return naveRival;

            foreach (PictureBox e in enemigosPequenos.ToList())
            {
                if (e != null && !e.IsDisposed && e.Visible)
                {
                    yield return e;
                }
            }
        }

        private void EliminarEnemigo(PictureBox enemigo)
        {
            if (enemigo == naveRival)
            {
                naveRival.Dispose();
                MostrarMensajeFinal("Felicitaciones Ganaste_!");
                tiempo.Stop();
            }
            else
            {
                enemigosPequenos.Remove(enemigo);
                enemigo.Dispose();
            }
        }

        private void MoverEnemigosPequenos()
        {
            List<PictureBox> paraEliminar = new List<PictureBox>();
            foreach (PictureBox enemigo in enemigosPequenos.ToList())
            {
                if (enemigo.IsDisposed)
                {
                    paraEliminar.Add(enemigo);
                    continue;
                }

                int desplazamientoHorizontal = (int)(Math.Sin((Environment.TickCount + enemigo.GetHashCode()) / 200.0) * 2);
                enemigo.Left = Math.Max(0, Math.Min(contiene.Width - enemigo.Width, enemigo.Left + desplazamientoHorizontal));
                enemigo.Top += 3;

                if (enemigo.Top > contiene.Height)
                {
                    paraEliminar.Add(enemigo);
                }
                else if (navex.Visible && enemigo.Bounds.IntersectsWith(navex.Bounds))
                {
                    var estadoJugador = navex.Tag as EntityState;
                    if (estadoJugador != null)
                    {
                        estadoJugador.Vida -= 5;
                        ActualizarIndicadores();
                        RevisarVidaJugador();
                    }
                    paraEliminar.Add(enemigo);
                }
            }

            foreach (PictureBox enemigo in paraEliminar)
            {
                enemigosPequenos.Remove(enemigo);
                enemigo.Dispose();
            }
        }

        private void RevisarVidaJugador()
        {
            var estadoJugador = navex.Tag as EntityState;
            if (estadoJugador == null)
                return;

            if (estadoJugador.Vida <= 0)
            {
                vidasJugador--;
                if (vidasJugador > 0)
                {
                    estadoJugador.Vida = 30;
                    navex.Location = new Point(Math.Max(10, contiene.Width / 2), Math.Max(10, contiene.Height - 120));
                    ActualizarIndicadores();
                }
                else
                {
                    navex.Dispose();
                    MostrarMensajeFinal("Perdiste el juego");
                    tiempo.Stop();
                }
            }
        }

        private void MostrarMensajeFinal(string texto)
        {
            Bitmap NuevoImg = new Bitmap(contiene.Width, contiene.Height);
            Graphics flagImagen = Graphics.FromImage(NuevoImg);
            Font drawFont = new Font("Arial", 16);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            SizeF tamTexto = flagImagen.MeasureString(texto, drawFont);
            Point drawPoint = new Point((int)((contiene.Width - tamTexto.Width) / 2), (int)((contiene.Height - tamTexto.Height) / 2));
            flagImagen.DrawString(texto, drawFont, drawBrush, drawPoint);
            contiene.Image = NuevoImg;
        }

        private void ActualizarIndicadores()
        {
            var estadoRival = naveRival.Tag as EntityState;
            var estadoJugador = navex.Tag as EntityState;

            label1.Text = "Vida del Rival : " + (estadoRival != null ? estadoRival.Vida.ToString() : "0");
            label2.Text = "Mi Nave HP : " + (estadoJugador != null ? estadoJugador.Vida.ToString() : "0");
            label3.Text = "Vidas : " + vidasJugador.ToString();
        }

        private void Contenedor_SizeChanged(object sender, EventArgs e)
        {
            contiene.Image = CrearFondoEspacial(contiene.Width, contiene.Height);
            AjustarDentroDeContenedor(navex);
            AjustarDentroDeContenedor(naveRival);
            enemigosPequenos.ForEach(AjustarDentroDeContenedor);
            asteroides.ForEach(AjustarDentroDeContenedor);
        }

        private void AjustarDentroDeContenedor(PictureBox elemento)
        {
            if (elemento == null || elemento.IsDisposed)
                return;

            int nuevoX = Math.Max(0, Math.Min(contiene.Width - elemento.Width, elemento.Left));
            int nuevoY = Math.Max(0, Math.Min(contiene.Height - elemento.Height, elemento.Top));
            elemento.Location = new Point(nuevoX, nuevoY);
        }

        private void CambiarNave(int nuevoTipo)
        {
            tipoNaveActual = nuevoTipo;
            var estadoJugador = navex.Tag as EntityState;
            int vida = estadoJugador != null ? estadoJugador.Vida : 30;
            Point posicionActual = navex.Location;
            CrearNave(navex, 0, tipoNaveActual, Color.SeaGreen, vida, "Jugador");
            navex.Location = posicionActual;
            ActualizarIndicadores();
        }

        private void ConfigurarIndicadores()
        {
            label1.AutoSize = true;
            label2.AutoSize = true;
            label3.AutoSize = true;

            label1.ForeColor = Color.White;
            label2.ForeColor = Color.White;
            label3.ForeColor = Color.White;

            label1.BackColor = Color.Black;
            label2.BackColor = Color.Black;
            label3.BackColor = Color.Black;

            label1.Location = new Point(10, 10);
            label2.Location = new Point(10, 30);
            label3.Location = new Point(10, 50);

            label1.Parent = contiene;
            label2.Parent = contiene;
            label3.Parent = contiene;
            label1.BringToFront();
            label2.BringToFront();
            label3.BringToFront();
        }

        //************ DIAGRAMAR NAVE***************//
        public void CrearNave(PictureBox Avion, int AngRotar, int Tipox, Color Pintar, int Vida, string tipo)
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
            if (!contiene.Controls.Contains(Avion))
            {
                contiene.Controls.Add(Avion);
            }
            NaveCorre(Avion, Tipox, AngRotar);
            Avion.Tag = new EntityState(tipo, Vida);
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

        private Bitmap CrearFondoEspacial(int ancho, int alto)
        {
            Bitmap fondo = new Bitmap(ancho, alto);
            using (Graphics grafico = Graphics.FromImage(fondo))
            {
                grafico.Clear(Color.Black);
                int cantidadEstrellas = (ancho * alto) / 2500;
                for (int i = 0; i < cantidadEstrellas; i++)
                {
                    int x = generador.Next(0, ancho);
                    int y = generador.Next(0, alto);
                    int radio = generador.Next(1, 3);
                    Color colorEstrella = generador.Next(0, 2) == 0 ? Color.White : Color.LightGray;
                    grafico.FillEllipse(new SolidBrush(colorEstrella), x, y, radio, radio);
                }
            }
            return fondo;
        }

        private PictureBox CrearAsteroide()
        {
            PictureBox asteroide = new PictureBox();
            int tamano = generador.Next(18, 28);
            asteroide.Size = new Size(tamano, tamano);
            asteroide.BackColor = Color.Transparent;
            Bitmap imagenAsteroide = new Bitmap(tamano, tamano);
            using (Graphics grafico = Graphics.FromImage(imagenAsteroide))
            {
                grafico.SmoothingMode = SmoothingMode.AntiAlias;
                Point[] puntos =
                {
                    new Point(tamano / 2, 0),
                    new Point(tamano - 3, tamano / 3),
                    new Point(tamano - 2, (tamano * 2) / 3),
                    new Point(tamano / 2, tamano - 1),
                    new Point(2, (tamano * 2) / 3),
                    new Point(2, tamano / 3)
                };
                grafico.FillPolygon(Brushes.DimGray, puntos);
                grafico.DrawPolygon(new Pen(Color.Gray, 1), puntos);
            }
            asteroide.Image = imagenAsteroide;
            asteroide.Tag = "Asteroide";
            int posicionX = generador.Next(0, Math.Max(1, contiene.Width - tamano));
            int posicionY = generador.Next(-400, -tamano);
            asteroide.Location = new Point(posicionX, posicionY);
            contiene.Controls.Add(asteroide);
            asteroides.Add(asteroide);
            asteroide.BringToFront();
            return asteroide;
        }

        private PictureBox CrearEnemigoPequeno()
        {
            PictureBox enemigo = new PictureBox();
            enemigo.Size = new Size(32, 24);
            enemigo.BackColor = Color.Transparent;

            Bitmap imagen = new Bitmap(enemigo.Width, enemigo.Height);
            using (Graphics g = Graphics.FromImage(imagen))
            {
                g.FillPolygon(Brushes.CadetBlue, new[] { new Point(16, 0), new Point(31, 8), new Point(16, 15), new Point(1, 8) });
                g.FillRectangle(Brushes.Aquamarine, new Rectangle(8, 8, 16, 8));
                g.FillRectangle(Brushes.LightBlue, new Rectangle(12, 12, 8, 4));
            }

            enemigo.Image = imagen;
            enemigo.Tag = new EntityState("EnemigoPequeno", 10);
            int posX = generador.Next(0, Math.Max(1, contiene.Width - enemigo.Width));
            enemigo.Location = new Point(posX, -enemigo.Height);
            enemigosPequenos.Add(enemigo);
            contiene.Controls.Add(enemigo);
            enemigo.BringToFront();
            return enemigo;
        }

        private void IniciarAsteroides(int cantidad)
        {
            asteroides.ForEach(a => a.Dispose());
            asteroides.Clear();
            for (int i = 0; i < cantidad; i++)
            {
                CrearAsteroide();
            }

            asteroideTimer = new System.Windows.Forms.Timer();
            asteroideTimer.Interval = 20;
            asteroideTimer.Tick += MoverAsteroides;
            asteroideTimer.Start();
        }

        private void IniciarEnemigosPequenos()
        {
            enemigosPequenos.ForEach(e => e.Dispose());
            enemigosPequenos.Clear();

            enemigoPequenoTimer = new System.Windows.Forms.Timer();
            enemigoPequenoTimer.Interval = 1800;
            enemigoPequenoTimer.Tick += (s, e) =>
            {
                if (enemigosPequenos.Count < 6)
                {
                    CrearEnemigoPequeno();
                }
            };
            enemigoPequenoTimer.Start();
        }

        private void MoverAsteroides(object sender, EventArgs e)
        {
            if (navex.IsDisposed)
                return;

            Rectangle nave = navex.Bounds;
            foreach (PictureBox asteroide in asteroides.ToList())
            {
                if (asteroide.IsDisposed)
                    continue;

                asteroide.Top += 3;
                if (asteroide.Bottom >= contiene.Height)
                {
                    asteroide.Top = -asteroide.Height;
                    asteroide.Left = generador.Next(0, Math.Max(1, contiene.Width - asteroide.Width));
                }

                if (asteroide.Bounds.IntersectsWith(nave))
                {
                    asteroide.Top = -asteroide.Height;
                    asteroide.Left = generador.Next(0, Math.Max(1, contiene.Width - asteroide.Width));
                    var estadoJugador = navex.Tag as EntityState;
                    if (estadoJugador != null)
                    {
                        estadoJugador.Vida = Math.Max(0, estadoJugador.Vida - 2);
                        ActualizarIndicadores();
                        RevisarVidaJugador();
                    }
                }
            }
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
                        NaveCorre(navex, 1, 0);
                        break;
                    }
                case 38: // flecha hacia arriba
                    {
                        if (contiene.Top < navex.Top)
                            navex.Top -= 10;
                        NaveCorre(navex, 0, 1);
                        break;
                    }
                case 39: // flecha hacia la derecha
                    {
                        if (contiene.Right > navex.Right)
                            navex.Left += 10;
                        NaveCorre(navex, 1, 0);
                        break;
                    }
                case 40: // flecha hacia abajo
                    {
                        if (contiene.Bottom > navex.Bottom)
                            navex.Top += 10;
                        NaveCorre(navex, 0, 1);
                        break;
                    }
                case 87: // W
                    {
                        if (contiene.Top < navex.Top)
                            navex.Top -= 10;
                        NaveCorre(navex, 0, 1);
                        break;
                    }
                case 83: // S
                    {
                        if (contiene.Bottom > navex.Bottom)
                            navex.Top += 10;
                        NaveCorre(navex, 0, 1);
                        break;
                    }
                case 65: // A
                    {
                        if (contiene.Left < navex.Left) // PARAMETROS DE LIMITE
                            navex.Left -= 10;
                        NaveCorre(navex, 1, 0);
                        break;
                    }
                case 68: // D
                    {
                        if (contiene.Right > navex.Right)
                            navex.Left += 10;
                        NaveCorre(navex, 1, 0);
                        break;
                    }
                case 13: // Enter - disparo
                    {
                        tiempo.Start();
                        int x = navex.Location.X + (navex.Width / 2);
                        int y = navex.Location.Y + (navex.Height / 2);
                        CrearMisil(0, Color.DarkOrange, "MisilJugador", x, y);
                        break;
                    }
                case 32: // Espacio - disparo
                    {
                        tiempo.Start();
                        int x = navex.Location.X + (navex.Width / 2);
                        int y = navex.Location.Y + (navex.Height / 2);
                        CrearMisil(0, Color.DarkOrange, "MisilJugador", x, y);
                        break;
                    }
                case 49: // 1 - cambiar nave
                    {
                        CambiarNave(1);
                        break;
                    }
                case 50: // 2 - cambiar nave
                    {
                        CambiarNave(2);
                        break;
                    }
                case 51: // 3 - cambiar nave
                    {
                        CambiarNave(3);
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
            this.KeyDown += new KeyEventHandler(ActividadTecla);
            //*****************************************************//
            contiene.Location = new Point(0, 0);
            contiene.Size = new Size(300, 420);
            contiene.Dock = DockStyle.Fill;
            contiene.BackColor = Color.Black;
            contiene.Image = CrearFondoEspacial(contiene.Width, contiene.Height);
            contiene.SizeChanged += Contenedor_SizeChanged;
            Controls.Add(contiene);
            contiene.Visible = true;
            //******Contenido del formulario*******//
            Random r = new Random();
            int aleatY = r.Next(250, 330);
            int aleatX = r.Next(50, 250);
            CrearNave(navex, 0, tipoNaveActual, Color.SeaGreen, 30, "Jugador");
            //ELEGIR NAVE DE SALIDA RIVAL
            Random sal = new Random();
            int sale = sal.Next(1, 3);
            CrearNave(naveRival, 180, sale, Color.DarkBlue, 60, "EnemigoGrande");
            //Modulo.Escenario(contiene, I);
            navex.Location = new Point(aleatX, aleatY);
            IniciarAsteroides(6);
            IniciarEnemigosPequenos();

            tiempo = new System.Windows.Forms.Timer();
            tiempo.Interval = 1;
            tiempo.Enabled = true;
            tiempo.Tick += new EventHandler(ImpactarTick);

            ConfigurarIndicadores();
            ActualizarIndicadores();

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

using Cliente.Properties.Langs;
using System;
using System.Windows.Threading;

namespace Cliente
{
    public class TableroBuscaminas
    {
        public event EventHandler EventoCambioContadorBanderas;
        public event EventHandler EventoTemporizador;
        public event EventHandler<ArgumentosDeEventosCelda> EventoClicCelda;
        public int ancho { get; private set; }
        public int alto { get; private set; }
        public int numeroMinas { get; private set; }
        public int tiempoTranscurrido { get; private set; }
        public Celda[,] celdas;
        private int marcasCorrectas;
        private int marcasIncorrectas;
        public int numeroMinasMarcadas { get { return (this.marcasCorrectas + this.marcasIncorrectas); } }
        private DispatcherTimer cronometroJuego;

        public TableroBuscaminas(int ancho, int alto, int numeroMinas)
        {
            this.ancho = ancho;
            this.alto = alto;
            this.numeroMinas = numeroMinas;
        }

        public bool ValidarPosicionCeldaDentroCuadricula(int filaPosicion, int columnaPosicion)
        {
            return ((filaPosicion >= 0) && (filaPosicion < this.ancho) && (columnaPosicion >= 0) && (columnaPosicion < this.alto));
        }

        public bool VerificarSiCeldaEsBomba(int filaPosicion, int columnaPosicion)
        {
            bool esBomba = false;
            if (this.ValidarPosicionCeldaDentroCuadricula(filaPosicion, columnaPosicion))
            {
                if(this.celdas[filaPosicion, columnaPosicion].esMinada)
                {
                    esBomba = true;
                }
                else
                {
                    esBomba = false;
                }
            }
            return esBomba;
        }

        public bool VerificarSiCeldaEstaMarcada(int filaPosicion, int columnaPosicion)
        {
            bool esMarcada = false;
            if (this.ValidarPosicionCeldaDentroCuadricula(filaPosicion, columnaPosicion))
            {
                if(this.celdas[filaPosicion, columnaPosicion].esMarcada)
                {
                    esMarcada = true;
                }
                else
                {
                    esMarcada = false;
                }
            }
            return esMarcada;
        }

        public int BuscarMinasCercanas(int filaPosicion, int columnaPosicion)
        {
            if (this.ValidarPosicionCeldaDentroCuadricula(filaPosicion, columnaPosicion))
            {
                int numeroMinasEncontradas = this.celdas[filaPosicion, columnaPosicion].ContarMinasCeldasProximas();
                ComprobarFinalizacionTablero();
                return numeroMinasEncontradas;
            }
            throw new BuscaminasExcepcion(Lang.ErrorLlamadaReferenciaBuscaminas_MSJCONST);
        }

        public void MarcarODesmarcarCelda(int filaPosicion, int columnaPosicion)
        {
            if (!this.ValidarPosicionCeldaDentroCuadricula(filaPosicion, columnaPosicion))
            {
                throw new BuscaminasExcepcion(Lang.ErrorLlamadaReferenciaBuscaminas_MSJCONST);
            }

            Celda celdaActual = this.celdas[filaPosicion, columnaPosicion];
            if (!celdaActual.esMarcada)
            {
                if (celdaActual.esMinada)
                {
                    this.marcasCorrectas++;
                }
                else
                {
                    this.marcasIncorrectas++;
                }
            }
            else
            {
                if (celdaActual.esMinada)
                {
                    this.marcasCorrectas--;
                }
                else
                {
                    this.marcasIncorrectas--;
                }
            }
            celdaActual.esMarcada = !celdaActual.esMarcada;
            ComprobarFinalizacionTablero();

            this.EnCambioContadorBanderas(new EventArgs());
        }

        public void DestaparCelda(int filaPosicion, int columnaPosicion)
        {
            if (this.ValidarPosicionCeldaDentroCuadricula(filaPosicion, columnaPosicion) && !this.celdas[filaPosicion, columnaPosicion].esRevelada)
            {
                this.DestaparCeldaEnClic(new ArgumentosDeEventosCelda(filaPosicion, columnaPosicion));
            }
        }

        private void ComprobarFinalizacionTablero()
        {
            bool esJuegoFinalizado = false;
            if (this.marcasIncorrectas == 0 && this.numeroMinasMarcadas == this.numeroMinas)
            {
                esJuegoFinalizado = true;
                foreach (Celda elementoCelda in this.celdas)
                {
                    if (!elementoCelda.esRevelada && !elementoCelda.esMinada)
                    {
                        esJuegoFinalizado = false;
                        break;
                    }
                }
            }
            if (esJuegoFinalizado) 
            {
                cronometroJuego.Stop();
            }
        }

        public void EjecutarJuego()
        {
            this.marcasCorrectas = 0;
            this.marcasIncorrectas = 0;
            this.tiempoTranscurrido = 0;

            this.celdas = new Celda[ancho, alto];

            for (int fila = 0; fila < ancho; fila++)
            {
                for (int columna = 0; columna < alto; columna++)
                {
                    Celda celda = new Celda(this, fila, columna);
                    this.celdas[fila, columna] = celda;
                }
            }

            int contadorMinas = 0;
            Random posicionMinas = new Random();

            while (contadorMinas < numeroMinas)
            {
                int filas = posicionMinas.Next(ancho);
                int columnas = posicionMinas.Next(alto);

                Celda celda = this.celdas[filas, columnas];

                if (!celda.esMinada)
                {
                    celda.esMinada = true;
                    contadorMinas++;
                }
            }

            cronometroJuego = new DispatcherTimer();
            cronometroJuego.Tick += new EventHandler(ActualizarCronometroEnTiempoTranscurrido);
            cronometroJuego.Interval = new TimeSpan(0, 0, 1);
            cronometroJuego.Start();
        }

        public void EjecutarJuegoVacio()
        {
            this.marcasCorrectas = 0;
            this.marcasIncorrectas = 0;
            this.tiempoTranscurrido = 0;

            this.celdas = new Celda[ancho, alto];

            for (int fila = 0; fila < ancho; fila++)
            {
                for (int columna = 0; columna < alto; columna++)
                {
                    Celda celda = new Celda(this, fila, columna);
                    this.celdas[fila, columna] = celda;
                }
            }

            cronometroJuego = new DispatcherTimer();
            cronometroJuego.Tick += new EventHandler(ActualizarCronometroEnTiempoTranscurrido);
            cronometroJuego.Interval = new TimeSpan(0, 0, 1);
            cronometroJuego.Start();
        }

        public void DetenerConometro()
        {
            cronometroJuego.Stop();
        }

        protected virtual void EnCambioContadorBanderas(EventArgs e)
        {
            EventHandler handler = EventoCambioContadorBanderas;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void ActualizarCronometroEnTiempoTranscurrido(object sender, EventArgs e)
        {
            this.tiempoTranscurrido++;
            EventHandler handler = EventoTemporizador;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void DestaparCeldaEnClic(ArgumentosDeEventosCelda e)
        {
            EventHandler<ArgumentosDeEventosCelda> handler = EventoClicCelda;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}

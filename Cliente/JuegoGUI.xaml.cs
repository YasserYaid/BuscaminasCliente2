using Cliente.Properties.Langs;
using Cliente.ServidorBuscaminasServicio;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cliente
{
    public partial class JuegoGUI : Page, IChatServiceDuplexCallback, IJuegoServiceDuplexCallback
    {
        private ServidorBuscaminasServicio.ChatServiceDuplexClient chatServiceDuplex;
        private ServidorBuscaminasServicio.JuegoServiceDuplexClient juegoServiceDuplex;
        private ServidorBuscaminasServicio.SalaServiceClient salaService;
        private ServidorBuscaminasServicio.Sala sala;
        private ServidorBuscaminasServicio.Tablero tableroServidor;
        string nombreUsuario;
        string codigoAccesoSala;
        string nombreOponente;
        bool esMiturno;
        int turnoAsignado;
        public TableroBuscaminas tableroBuscaMinas { get; private set; }
        private int numeroMinas;
        private bool esJuegoIniciado;
        private Color[] colorTextoMina;

        public JuegoGUI(string nombreUsuario, string codigoAccesoSala, string nombreOponente, int turnoAsignado)
        {
            InitializeComponent();
            this.nombreUsuario = nombreUsuario;
            this.codigoAccesoSala = codigoAccesoSala;
            this.nombreOponente = nombreOponente;
            this.turnoAsignado = turnoAsignado;
            InstanceContext instanceContext = new InstanceContext(this);
            this.chatServiceDuplex = new ServidorBuscaminasServicio.ChatServiceDuplexClient(instanceContext);
            this.juegoServiceDuplex = new ServidorBuscaminasServicio.JuegoServiceDuplexClient(instanceContext);
            this.salaService = new SalaServiceClient();
            this.sala = salaService.BuscarSala(codigoAccesoSala);
            this.chatServiceDuplex.AgregarJugadorChatCallback(codigoAccesoSala, nombreUsuario);
            this.juegoServiceDuplex.AgregarJugadorJuegoCallback(codigoAccesoSala, nombreUsuario);
            esJuegoIniciado = false;
            this.numeroMinas = 15;
            colorTextoMina = new Color[] { Colors.White, Colors.Blue, Colors.DarkGreen, Colors.Red, Colors.DarkBlue, Colors.DarkViolet, Colors.DarkCyan, Colors.Brown, Colors.Black };
            ConfigurarJuego();
            CargarBombasDesdeServidor();
            ActualizarEtiquetasInicio();
        }

        public void RecibirMensaje(string nombreUsuarioEmisor, string mensaje)
        {
            ListBoxItem mensajeListBoxItem = new ListBoxItem();
            mensajeListBoxItem.Content = nombreUsuarioEmisor + Lang.FormatoChat_MSJCONST + mensaje;
            MensajesChatListBox.Items.Add(mensajeListBoxItem);
        }

        private void SalirButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Lang.AvisoSalida_MSJ);
            Environment.Exit(0);
        }

        private void EnviarMensajeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                chatServiceDuplex.EnviarMensajeChat(codigoAccesoSala, nombreUsuario, MensajeChatTextbox.Text);
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(Lang.ErrorNoSeEncontroServidor_MSJ);
                Environment.Exit(0);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Lang.ErrorObjetoComunicacionConServidor_MSJ);
                Environment.Exit(0);
            }
        }

        public void RecibirConclusion(string nombreUsuario)
        {
            esJuegoIniciado = false;
            if (nombreOponente.Equals(nombreUsuario))
            {
                NombreGanadorTextBox.Text = this.nombreOponente;
            }
            else if (!nombreOponente.Equals(nombreUsuario))
            {
                NombreGanadorTextBox.Text = nombreUsuario;
            }
        }

        public void RecibirTurno(string nombreUsuarioClic, int turnoUsuarioClic, ServidorBuscaminasServicio.Celda celdaPulsada)
        {
            sala.turno = turnoUsuarioClic;
            string nombreBoton = Lang.NombreBoton_MSJCONST + celdaPulsada.filaPosicion + "" + celdaPulsada.columnaPosicion;
            Button botonPulsado = (TableroGrid.FindName(nombreBoton) as Button);
            botonPulsado.IsEnabled = ValidarAcivacionDesactivacionBoton(celdaPulsada.filaPosicion, celdaPulsada.columnaPosicion);
            if (celdaPulsada.esMarcada)
            {
                EjecutarPulsacionDerecha(celdaPulsada.filaPosicion, celdaPulsada.columnaPosicion, botonPulsado);
            }
            else
            {
                EjecutarPulsacionIzquierda(celdaPulsada.filaPosicion, celdaPulsada.columnaPosicion, botonPulsado, new RoutedEventArgs());
            }
            sala.turno = turnoUsuarioClic;
            ActualizarTurno(nombreUsuarioClic, turnoUsuarioClic);
        }

        public void ActualizarTurno(string nombreUsuarioClic, int turnoUsuarioClic)
        {
            if(turnoUsuarioClic == 1)
            {
                sala.turno = 2;
            }if(turnoUsuarioClic == 2)
            {
                sala.turno = 1;
            }
            if(sala.turno == turnoAsignado)
            {
                esMiturno = true;
            }
            else
            {
                esMiturno = false;
            }
            if (nombreUsuarioClic.Equals(this.nombreOponente))
            {
                NombreTurnoTextBox.Text = this.nombreUsuario;
            }
            if(nombreUsuarioClic.Equals(this.nombreUsuario))
            {
                NombreTurnoTextBox.Text = this.nombreOponente;
            }
        }

        public void ActualizarEtiquetasInicio()
        {
            NombreOponenteTextBox.Text = nombreOponente;
            NombreUsuarioTextBox.Text = nombreUsuario;
            if(turnoAsignado == 1){
                NombreTurnoTextBox.Text = nombreUsuario;
                esMiturno=true;
            }else if(turnoAsignado == 2)
            {
                NombreTurnoTextBox.Text = nombreOponente;
                esMiturno=false;
            }
        }

        public void CargarBombasDesdeServidor()
        {
            try
            {
                tableroServidor = sala.tablero;
                foreach (var bombaServidor in tableroServidor.bombas)
                {
                    int fila = bombaServidor.filaPosicion;
                    int columna = bombaServidor.columnaPosicion;
                    bool esMina = bombaServidor.esMinada;
                    tableroBuscaMinas.celdas[fila, columna].esMinada = esMina;
                }

            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(Lang.ErrorNoSeEncontroServidor_MSJ);
                Environment.Exit(0);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Lang.ErrorObjetoComunicacionConServidor_MSJ);
                Environment.Exit(0);
            }
        }

        private void ConfigurarJuego()
        {
            tableroBuscaMinas = new TableroBuscaminas(10, 10, numeroMinas);
            foreach (Button boton in TableroGrid.Children)
            {
                boton.Content = "";
                boton.IsEnabled = true;
            }
            tableroBuscaMinas.EventoCambioContadorBanderas += ActualizarContadorBanderasGUI;
            BanderasTextBox.Text = numeroMinas.ToString();

            tableroBuscaMinas.EventoClicCelda += AbrirCeldaEnBotonClic;

            tableroBuscaMinas.EventoTemporizador += ActualizarCronometroGUI;
            TiempoTextBox.Text = Lang.Cero_MSJCONST;

            tableroBuscaMinas.EjecutarJuegoVacio();

            esJuegoIniciado = true;
        }
        private void Boton_Izquierdo_Click(object sender, RoutedEventArgs e)
        {
            if(esMiturno)
            {
                Button boton = (Button)sender;
                int fila = AnalizarFilaBoton(boton);
                int columna = AnalizarColumnaBoton(boton);
                boton.IsEnabled = ValidarAcivacionDesactivacionBoton(fila, columna);
                EjecutarPulsacionIzquierda(fila, columna, boton, e);
                ServidorBuscaminasServicio.Celda celdaPulsada = new ServidorBuscaminasServicio.Celda();
                celdaPulsada.filaPosicion = fila;
                celdaPulsada.columnaPosicion = columna;
                juegoServiceDuplex.PasarTurno(codigoAccesoSala, nombreUsuario, turnoAsignado, celdaPulsada);
            }
            sala.turno = turnoAsignado;
        }

        private void Boton_Derecho_Click(object sender, MouseButtonEventArgs e)
        {
            if (esMiturno)
            {
                Button boton = (Button)sender;
                int fila = AnalizarFilaBoton(boton);
                int columna = AnalizarColumnaBoton(boton);
                EjecutarPulsacionDerecha(fila, columna, boton);
                ServidorBuscaminasServicio.Celda celdaMarcada = new ServidorBuscaminasServicio.Celda();
                celdaMarcada.filaPosicion = fila;
                celdaMarcada.columnaPosicion = columna;
                celdaMarcada.esMarcada = true;
                tableroBuscaMinas.MarcarODesmarcarCelda(fila, columna);
                juegoServiceDuplex.PasarTurno(codigoAccesoSala, nombreUsuario, turnoAsignado, celdaMarcada);
            }
            sala.turno = turnoAsignado;
        }

        private int AnalizarFilaBoton(Button button)
        {
            if (button.Name.IndexOf(Lang.NombreBoton_MSJCONST) != 0)
            {
                throw new BuscaminasExcepcion(Lang.ErrorBMExcepcionNombreIncorrectoBoton_MSJ);
            }
            return int.Parse(button.Name.Substring(13, (button.Name.Length - 13) / 2));
        }

        private int AnalizarColumnaBoton(Button button)
        {
            if (button.Name.IndexOf(Lang.NombreBoton_MSJCONST) != 0)
            {
                throw new BuscaminasExcepcion(Lang.ErrorBMExcepcionNombreIncorrectoBoton_MSJ);
            }
            return int.Parse(button.Name.Substring(13 + (button.Name.Length - 13) / 2, (button.Name.Length - 13) / 2));
        }

        private void ActualizarContadorBanderasGUI(object sender, EventArgs e)
        {
            BanderasTextBox.Text = (this.numeroMinas - tableroBuscaMinas.numeroMinasMarcadas).ToString();
        }

        private void ActualizarCronometroGUI(object sender, EventArgs e)
        {
            TiempoTextBox.Text = tableroBuscaMinas.tiempoTranscurrido.ToString();
        }

        private void AbrirCeldaEnBotonClic(object sender, ArgumentosDeEventosCelda e)
        {
            string nombreBoton = Lang.NombreBoton_MSJCONST;
            if (tableroBuscaMinas.ancho <= 10 && tableroBuscaMinas.alto <= 10)
            {
                nombreBoton += String.Format(Lang.FormatoCoordenadasUnDigito_MSJCONST, e.CeldaFila, e.CeldaColumna);
            }
            else
            {
                nombreBoton += String.Format(Lang.FormatoCoordenadasDosDigito_MSJCONST, e.CeldaFila, e.CeldaColumna);
            }

            Button botonEmisor = (TableroGrid.FindName(nombreBoton) as Button);
            if (botonEmisor == null)
            {
                throw new BuscaminasExcepcion(Lang.ErrorBMExcepcionReferenciaInvalidaBoton_MSJ);
            }

            this.Boton_Izquierdo_Click(botonEmisor, new RoutedEventArgs());
        }

        private bool ValidarAcivacionDesactivacionBoton(int fila, int columna)
        {
            bool esActivo = false;
            if (!tableroBuscaMinas.ValidarPosicionCeldaDentroCuadricula(fila, columna))
            {
                esActivo = true;
                throw new BuscaminasExcepcion(Lang.ErrorBMExcepcionReferenciaInvalidaBoton_MSJ);
            }

            if (tableroBuscaMinas.VerificarSiCeldaEstaMarcada(fila, columna))
            {
                esActivo = true;
            }
            return esActivo;
        }

        private void EjecutarPulsacionIzquierda(int fila, int columna, Button boton, RoutedEventArgs e)
        {
            if (tableroBuscaMinas.VerificarSiCeldaEsBomba(fila, columna))
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                Image imagenBomba = new Image();
                BitmapImage mapaBitsImagenBomba = new BitmapImage();
                mapaBitsImagenBomba.BeginInit();
                mapaBitsImagenBomba.UriSource = new Uri(@"/Imagenes/JuegoGUI/Bomba.png", UriKind.Relative);
                mapaBitsImagenBomba.EndInit();
                imagenBomba.Source = mapaBitsImagenBomba;
                stackPanel.Children.Add(imagenBomba);
                boton.Content = stackPanel;
                tableroBuscaMinas.DetenerConometro();

                if (esJuegoIniciado)
                {
                    esJuegoIniciado = false;
                    foreach (Button button in TableroGrid.Children)
                    {
                        if (button.IsEnabled) this.Boton_Izquierdo_Click(button, e);
                    }
                }
                NombreTurnoTextBox.Text = "";
                NombreGanadorTextBox.Text = nombreOponente;
                juegoServiceDuplex.EnviarConclusion(codigoAccesoSala, nombreUsuario);
            }
            else
            {
                int numeroMinasProximas = tableroBuscaMinas.BuscarMinasCercanas(fila, columna);
                if (numeroMinasProximas > 0)
                {
                    boton.Foreground = new SolidColorBrush(colorTextoMina[numeroMinasProximas]);
                    boton.FontWeight = FontWeights.Bold;
                    boton.Content = numeroMinasProximas.ToString();
                }
                sala.turno = turnoAsignado;
            }
        }

        private void EjecutarPulsacionDerecha(int fila, int columna, Button botonMarcado)
        {
            if (!tableroBuscaMinas.ValidarPosicionCeldaDentroCuadricula(fila, columna))
            {
                throw new BuscaminasExcepcion(Lang.ErrorBMExcepcionReferenciaInvalidaBoton_MSJ);
            }

            if (tableroBuscaMinas.VerificarSiCeldaEstaMarcada(fila, columna))
            {
                botonMarcado.Content = "";
            }
            else
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                Image imagenBandera = new Image();
                BitmapImage mapaBitsImagenBandera = new BitmapImage();
                mapaBitsImagenBandera.BeginInit();
                mapaBitsImagenBandera.UriSource = new Uri(@"/Imagenes/JuegoGUI/Bandera.png", UriKind.Relative);
                mapaBitsImagenBandera.EndInit();
                imagenBandera.Source = mapaBitsImagenBandera;
                stackPanel.Children.Add(imagenBandera);
                botonMarcado.Content = stackPanel;
            }
            tableroBuscaMinas.MarcarODesmarcarCelda(fila, columna);
            sala.turno = turnoAsignado;
        }
    }
}

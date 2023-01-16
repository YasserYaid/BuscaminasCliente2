using Cliente.Properties.Langs;
using System.ServiceModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Cliente
{
    public partial class IniciarSesionGUI : Page
    {
        
        private ServidorBuscaminasServicio.CuentaUsuarioServiceMgtClient cuentaUsuarioServiceMgt;
        
        public IniciarSesionGUI()
        {
            InitializeComponent();
            cuentaUsuarioServiceMgt = new ServidorBuscaminasServicio.CuentaUsuarioServiceMgtClient();
        }
        
        private void IniciarSesionButton_Click(object sender, RoutedEventArgs e)
        {            
            
            string nombreUsuario = NombreUsuarioTextBox.Text;
            string contrasenia = ContraseniaPasswordBox.Password;

            if (!string.IsNullOrEmpty(nombreUsuario) && !string.IsNullOrEmpty(contrasenia))
            {
                try
                {
                    bool esAutenticacionCorrecta = cuentaUsuarioServiceMgt.ValidarAuntentificacion(nombreUsuario, contrasenia);
                    if (esAutenticacionCorrecta)
                    {
                        bool esPermitida = cuentaUsuarioServiceMgt.ComprobarEstado(nombreUsuario);
                        if (esPermitida)
                        {
                            MessageBox.Show(Lang.AvisoBienvenido_MSJ + nombreUsuario);
                            MenuPrincipalGUI menuPrincipalGUI = new MenuPrincipalGUI(nombreUsuario);
                            Application.Current.MainWindow.Content = menuPrincipalGUI;
                        }
                        else
                        {
                            MessageBox.Show(Lang.AlertaJugadorBaneado_MSJ);
                        }
                    }
                    else
                    {
                        MessageBox.Show(Lang.ErrorDatosIncorrectos_MSJ);
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
            else
            {
                MessageBox.Show(Lang.AlertaCamposVacios_MSJ);
            }
        }

        private void RegistrarseTextBlockHyperlink_Click(object sender, RoutedEventArgs e)
        {
            RegistrarCuentaGUI registrarCuentaGUI = new RegistrarCuentaGUI();
            Application.Current.MainWindow.Content = registrarCuentaGUI;
        }

    }

}

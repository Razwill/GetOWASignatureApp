using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace ZadanieDcs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var loginWindow = new LoginWindow();

            // Display the dialog box and read the response
            bool? result = loginWindow.ShowDialog();

            if (result != true)
            {
                Application.Current.Shutdown();
                Environment.Exit(0);
            }

            InitializeAsync();
        }

        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async();
            try
            {
                string email = Application.Current.Properties["email"].ToString();
                string password = Application.Current.Properties["password"].ToString();
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Email and password should not be empty.");
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                }

                ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
                ExchangeService service = new ExchangeService
                {
                    Credentials = new WebCredentials(email, password),
                    UseDefaultCredentials = false
                };

                // uncomment those lines if you want to diagnose problems
                // service.TraceEnabled = true;
                // service.TraceFlags = TraceFlags.All;

                service.AutodiscoverUrl(email, RedirectionUrlValidationCallback);

                var OWAConfig = UserConfiguration.Bind(service, "OWA.UserOptions", WellKnownFolderName.Root,
                    UserConfigurationProperties.All);

                string signature = "ERROR: Can't access signature";

                if (OWAConfig.Dictionary.ContainsKey("signaturehtml"))
                {
                    signature = OWAConfig.Dictionary["signaturehtml"].ToString();
                }
                else if (OWAConfig.Dictionary.ContainsKey("signaturetext"))
                {
                    signature = OWAConfig.Dictionary["signaturetext"].ToString();
                }

                canvas.Visibility = Visibility.Hidden;
                canvas.Opacity = 0;
                label.Visibility = Visibility.Hidden;
                label.Opacity = 0;
                webView.Visibility = Visibility.Visible;
                webView.NavigateToString(signature);

            }
            catch (Exception e)
            {
                MessageBox.Show($"Login failed. Error: {e.Message}");
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);
            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        private static bool CertificateValidationCallBack(
            object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                            (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }
            else
            {
                // In all other cases, return false.
                return false;
            }
        }
    }
}

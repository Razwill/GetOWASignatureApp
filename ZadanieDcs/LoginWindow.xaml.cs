using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace ZadanieDcs
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties.Add("email", email.Text);
            Application.Current.Properties.Add("password", password.Text);
            DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) =>
            DialogResult = false;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                DialogResult = false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


namespace CurrencyConverter_DB
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, double> currencyRates;

        public MainWindow()
        {
            InitializeComponent();
            ClearControls();
            BindCurrency();
        }

        #region Bind Currency From and To Combobox
        private void BindCurrency()
        {
            // Initialize currency rates
            currencyRates = new Dictionary<string, double>
            {
                { "--SELECT--", 0 },
                { "INR", 1 },
                { "USD", 75 },
                { "EUR", 85 },
                { "SAR", 20 },
                { "POUND", 5 },
                { "DEM", 43 }
            };

            // Bind currency to ComboBoxes
            cmbFromCurrency.ItemsSource = currencyRates.Keys;
            cmbToCurrency.ItemsSource = currencyRates.Keys;

            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
        }
        #endregion

        #region Button Click Event
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double convertedValue;

            // Check if the amount is null or empty
            if (string.IsNullOrWhiteSpace(txtCurrency.Text))
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }

            // Check if From currency is selected properly
            if (cmbFromCurrency.SelectedItem == null || cmbFromCurrency.SelectedItem.ToString() == "--SELECT--")
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();
                return;
            }

            // Check if To currency is selected properly
            if (cmbToCurrency.SelectedItem == null || cmbToCurrency.SelectedItem.ToString() == "--SELECT--")
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();
                return;
            }

            // Check if From and To currency are the same
            if (cmbFromCurrency.SelectedItem.ToString() == cmbToCurrency.SelectedItem.ToString())
            {
                convertedValue = double.Parse(txtCurrency.Text);
                lblCurrency.Content = $"{cmbToCurrency.SelectedItem.ToString()} {convertedValue:N3}";
            }
            else
            {
                try
                {
                    double fromCurrencyValue = currencyRates[cmbFromCurrency.SelectedItem.ToString()];
                    double toCurrencyValue = currencyRates[cmbToCurrency.SelectedItem.ToString()];
                    double amount = double.Parse(txtCurrency.Text);

                    // Currency conversion formula
                    convertedValue = (fromCurrencyValue * amount) / toCurrencyValue;
                    lblCurrency.Content = $"{cmbToCurrency.SelectedItem.ToString()} {convertedValue:N3}";
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid currency amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Clear button click event
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }
        #endregion

        #region Extra Events
        private void ClearControls()
        {
            txtCurrency.Clear();
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        // Only allow numbers in the text box
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]*[.,]?[0-9]*$"); // Updated regex to allow decimal values
            e.Handled = !regex.IsMatch(e.Text);
        }
        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace CurrencyConverter_API
{
    public partial class MainWindow : Window
    {
        private Root val = new Root();
        private Dictionary<string, double> currencyRates = new Dictionary<string, double>();

        public class Root
        {
            public Rate rates { get; set; }
            public long timestamp;
            public string license;
        }

        public class Rate
        {
            public double INT { get; set; }
            public double JPY { get; set; }
            public double USD { get; set; }
            public double NZD { get; set; }
            public double EUR { get; set; }
            public double CAD { get; set; }
            public double ISK { get; set; }
            public double PHP { get; set; }
            public double DKK { get; set; }
            public double CZK { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            ClearControls();
            GetValue();
        }

        private async void GetValue()
        {
            val = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=7e3b95a4528a4a0ca19a168046d97b5e");
            if (val?.rates != null)
            {
                // Initialize currency rates
                currencyRates = new Dictionary<string, double>
                {
                    { "INT", val.rates.INT },
                    { "JPY", val.rates.JPY },
                    { "USD", val.rates.USD },
                    { "NZD", val.rates.NZD },
                    { "EUR", val.rates.EUR },
                    { "CAD", val.rates.CAD },
                    { "ISK", val.rates.ISK },
                    { "PHP", val.rates.PHP },
                    { "DKK", val.rates.DKK },
                    { "CZK", val.rates.CZK }
                };
                BindCurrency();
            }
        }

        public static async Task<Root> GetData<T>(string url)
        {
            var myRoot = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<Root>(responseString);

                        // Display the information in a message box
                        //MessageBox.Show("Timestamp: " + responseObject.timestamp, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //MessageBox.Show("Rates: " + responseString, "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        return responseObject;
                    }
                    else
                    {
                        MessageBox.Show("Failed to retrieve data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return myRoot;
        }

        #region Bind Currency From and To Combobox
        private void BindCurrency()
        {
            // Bind currency to ComboBoxes
            cmbFromCurrency.ItemsSource = currencyRates.Keys;
            cmbToCurrency.ItemsSource = currencyRates.Keys;

            if (currencyRates.Count > 0)
            {
                cmbFromCurrency.SelectedIndex = 0;
                cmbToCurrency.SelectedIndex = 0;
            }
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
            cmbFromCurrency.SelectedIndex = -1; // Set to -1 for no selection
            cmbToCurrency.SelectedIndex = -1; // Set to -1 for no selection
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Configuration;

namespace CurrencyConverter_Static
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper;
        private int CurrencyId = 0;
        private double FromAmount = 0;
        private double ToAmount = 0;

        public MainWindow()
        {
            InitializeComponent();
            ClearControls();
            dbHelper = new DatabaseHelper();
            BindCurrency();
            GetData();
        }

        private void BindCurrency()
        {
            string query = "SELECT Id, CurrencyName FROM Currency_Master";
            DataTable dt = dbHelper.ExecuteQuery(query);

            DataRow newRow = dt.NewRow();
            newRow["Id"] = 0;
            newRow["CurrencyName"] = "--SELECT--";
            dt.Rows.InsertAt(newRow, 0);

            if (dt.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dt.DefaultView;
                cmbToCurrency.ItemsSource = dt.DefaultView;
            }

            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedValue = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedValue = 0;
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double ConvertedValue;

                if (string.IsNullOrWhiteSpace(txtCurrency.Text))
                {
                    MessageBox.Show("Please enter currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrency.Focus();
                    return;
                }

                if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
                {
                    MessageBox.Show("Please select from currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    cmbFromCurrency.Focus();
                    return;
                }

                if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
                {
                    MessageBox.Show("Please select to currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    cmbToCurrency.Focus();
                    return;
                }

                if (cmbFromCurrency.SelectedValue.Equals(cmbToCurrency.SelectedValue))
                {
                    ConvertedValue = double.Parse(txtCurrency.Text);
                    lblCurrency.Content = $"{cmbToCurrency.Text} {ConvertedValue:N3}";
                }
                else
                {
                    FromAmount = (double)dbHelper.ExecuteScalar("SELECT Amount FROM Currency_Master WHERE Id = @FromCurrencyId", new SqlParameter("@FromCurrencyId", cmbFromCurrency.SelectedValue));
                    ToAmount = (double)dbHelper.ExecuteScalar("SELECT Amount FROM Currency_Master WHERE Id = @ToCurrencyId", new SqlParameter("@ToCurrencyId", cmbToCurrency.SelectedValue));
                    ConvertedValue = FromAmount * double.Parse(txtCurrency.Text) / ToAmount;

                    lblCurrency.Content = $"{cmbToCurrency.Text} {ConvertedValue:N3}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void GetData()
        {
            string query = "SELECT * FROM Currency_Master";
            DataTable dt = dbHelper.ExecuteQuery(query);
            dgvCurrency.ItemsSource = dt.Rows.Count > 0 ? dt.DefaultView : null;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the fields are filled
                if (string.IsNullOrWhiteSpace(txtAmount.Text) || string.IsNullOrWhiteSpace(txtCurrencyName.Text))
                {
                    MessageBox.Show("Bitte füllen Sie alle Felder aus.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                double amount;
                // Try to convert the amount to a double value
                if (!double.TryParse(txtAmount.Text, out amount))
                {
                    MessageBox.Show("Bitte geben Sie einen gültigen Betrag ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtAmount.Focus();
                    return;
                }

                string query;
                SqlParameter[] parameters;

                // Determine whether to Update or Insert
                if (CurrencyId > 0) // Update
                {
                    query = "UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id";
                    parameters = new SqlParameter[]
                    {
                new SqlParameter("@Amount", amount),
                new SqlParameter("@CurrencyName", txtCurrencyName.Text),
                new SqlParameter("@Id", CurrencyId) // Id parameter for update
                    };

                    // Debugging: Check parameter values during update
                    MessageBox.Show($"Update - Amount: {amount}, CurrencyName: {txtCurrencyName.Text}, Id: {CurrencyId}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Insert
                {
                    query = "INSERT INTO Currency_Master (Amount, CurrencyName) VALUES (@Amount, @CurrencyName)";
                    parameters = new SqlParameter[]
                    {
                new SqlParameter("@Amount", amount),
                new SqlParameter("@CurrencyName", txtCurrencyName.Text)
                    };

                    // Debugging: Check parameter values during insert
                    MessageBox.Show($"Insert - Amount: {amount}, CurrencyName: {txtCurrencyName.Text}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Execute database query
                dbHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Währung erfolgreich gespeichert", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearMaster(); // Reset form fields
            }
            catch (Exception ex)
            {
                // Display error details
                MessageBox.Show($"Ein Fehler ist aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearMaster()
        {
            txtAmount.Text = string.Empty;
            txtCurrencyName.Text = string.Empty;
            btnSave.Content = "Save";
            GetData();
            CurrencyId = 0;
            BindCurrency();
            txtAmount.Focus();
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (dgvCurrency.CurrentItem is DataRowView rowSelected)
                {
                    CurrencyId = int.Parse(rowSelected["Id"].ToString());

                    if (dgvCurrency.SelectedCells.Count > 0)
                    {
                        if (dgvCurrency.SelectedCells[0].Column.DisplayIndex == 0)
                        {
                            txtAmount.Text = rowSelected["Amount"].ToString();
                            txtCurrencyName.Text = rowSelected["CurrencyName"].ToString();
                            btnSave.Content = "Update"; // Change button text
                        }
                        else if (dgvCurrency.SelectedCells[0].Column.DisplayIndex == 1)
                        {
                            if (MessageBox.Show("Are you sure you want to delete?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                dbHelper.ExecuteNonQuery("DELETE FROM Currency_Master WHERE Id = @Id", new SqlParameter("@Id", CurrencyId));
                                MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ClearMaster();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbFromCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbFromCurrency.SelectedValue is int currencyFromId && currencyFromId != 0)
                {
                    FromAmount = (double)dbHelper.ExecuteScalar("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyFromId", new SqlParameter("@CurrencyFromId", currencyFromId));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbToCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbToCurrency.SelectedValue is int currencyToId && currencyToId != 0)
                {
                    ToAmount = (double)dbHelper.ExecuteScalar("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyToId", new SqlParameter("@CurrencyToId", currencyToId));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

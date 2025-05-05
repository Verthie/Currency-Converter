using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using static System.Net.Mime.MediaTypeNames;

namespace Currency_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection connection = new SqlConnection();
        SqlCommand command = new SqlCommand();
        SqlDataAdapter adapter = new SqlDataAdapter();

        private int currencyId = 0;
        private double? fromAmount = 0;
        private double? toAmount = 0;

        public MainWindow()
        {
            InitializeComponent();
            BindCurrency();
        }

        public void establishConnection()
        {
            String connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        private void BindCurrency()
        {
            establishConnection();

            DataTable dt = new DataTable();

            string query = "SELECT Id, CurrencyName FROM Currency_Master";
            command = new SqlCommand(query, connection);
            command.CommandType = CommandType.Text;

            adapter = new SqlDataAdapter(command);
            adapter.Fill(dt);

            DataRow newRow = dt.NewRow(); // Adding new row to DataTable
            newRow["Id"] = 0;
            newRow["CurrencyName"] = "--SELECT--";

            dt.Rows.InsertAt(newRow, 0);

            if (dt != null && dt.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dt.DefaultView;
                cmbToCurrency.ItemsSource = dt.DefaultView;
            }

            connection.Close();

            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void OnConvertClick(object sender, RoutedEventArgs e)
        {
            try
            {

                double convertedValue;

                if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
                {
                    MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrency.Focus();
                    return;
                }
                else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
                {
                    MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    cmbFromCurrency.Focus();
                    return;
                }
                else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
                {
                    MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    cmbToCurrency.Focus();
                    return;
                }

                txtCurrency.Text = CorrectTextFormat(txtCurrency.Text);

                int commaAmount = txtCurrency.Text.Count(c => c == ',');

                if (commaAmount > 1)
                {
                    MessageBox.Show("Too many commas used, only 1 comma is allowed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtCurrency.Focus();
                    return;
                }

                if (cmbFromCurrency.Text == cmbToCurrency.Text)
                {
                    // using double.parse to convert String datatype to Double
                    convertedValue = double.Parse(txtCurrency.Text);

                    lblCurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N3"); // N3 Represents the 3 point precision
                }
                else
                {
                    if (fromAmount != null && fromAmount != 0 && toAmount != null && toAmount != 0)
                    {
                        //Calculation for currency converter is From currency value Multiplied(*) with amount textbox value and then that total is divided(/) with To currency value.
                        convertedValue = (double)(fromAmount * double.Parse(txtCurrency.Text) / toAmount);

                        lblCurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N3");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        private string CorrectTextFormat(string text)
        {
            string formattedText = "";

            if (text.Contains("."))
            {
                formattedText = text.Replace('.', ',');
            }
            else
            {
                return text;
            }

            return formattedText;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[0-9]+");
            //e.Handled = !regex.IsMatch(e.Text);
            //----------------------------------//

            TextBox textBox = sender as TextBox;

            // Predict the result after input
            string futureText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            /*
            - textBox.Text: the current text in the box
            - e.Text: the character the user just typed
            - textBox.CaretIndex: the current cursor position
            
            By using .Insert(), we simulate what the text will look like if the typed character is allowed
             */

            Regex regex = new Regex(@"^[0-9]*(?:[.,]{0,1}[0-9]*)?$");
            e.Handled = !regex.IsMatch(futureText); // Block input if it doesn't match
        }

        private void ClearControls()
        {
            try
            {
                txtCurrency.Text = string.Empty;
                if (cmbFromCurrency.Items.Count > 0)
                    cmbFromCurrency.SelectedIndex = 0;
                if (cmbToCurrency.Items.Count > 0)
                    cmbToCurrency.SelectedIndex = 0;
                lblCurrency.Content = "";
                txtCurrency.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearMaster()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                currencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GetData()
        {
            establishConnection();

            DataTable dt = new DataTable();

            command = new SqlCommand("SELECT * FROM Currency_Master", connection);
            command.CommandType = CommandType.Text;

            adapter = new SqlDataAdapter(command);
            adapter.Fill(dt);

            if (dt != null && dt.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dt.DefaultView;
            }
            else
            {
                dgvCurrency.ItemsSource = null;
            }
            connection.Close();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter currency name", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtCurrencyName.Focus();
                    return;
                }

                if (new Regex(@"\s+").IsMatch(txtCurrencyName.Text))
                {
                    MessageBox.Show("Spaces are not allowed in Currency Name", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtCurrencyName.Focus();
                    return;
                }

                // Updating
                if (currencyId > 0)
                {
                    // Updating only if the user confirms his choice
                    if (MessageBox.Show("Are you sure you want to update ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        establishConnection();
                        DataTable dt = new DataTable();

                        command = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", connection);
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@Id", currencyId);
                        command.Parameters.AddWithValue("@Amount", txtAmount.Text);
                        command.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text.ToUpper());
                        command.ExecuteNonQuery();
                        connection.Close();

                        MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                // Saving
                else
                {
                    // Saving only if the user confirms his choice
                    if (MessageBox.Show("Are you sure you want to save ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        establishConnection();

                        command = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", connection);
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@Amount", txtAmount.Text);
                        command.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text.ToUpper());
                        command.ExecuteNonQuery();
                        connection.Close();

                        MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                ClearMaster();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
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

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid grd = (DataGrid)sender;
                DataRowView row_selected = grd.CurrentItem as DataRowView;

                if (row_selected != null)
                {
                    if (dgvCurrency.Items.Count > 0)
                    {
                        if (grd.SelectedCells.Count > 0)
                        {
                            currencyId = Int32.Parse(row_selected["Id"].ToString());

                            if (grd.SelectedCells[0].Column.DisplayIndex == 0)
                            {
                                txtAmount.Text = row_selected["Amount"].ToString();
                                txtCurrencyName.Text = row_selected["CurrencyName"].ToString();
                                btnSave.Content = "Update";
                            }

                            if (grd.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                if (MessageBox.Show("Are you sure you want to delete ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    establishConnection();
                                    DataTable dt = new DataTable();

                                    command = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", connection);
                                    command.CommandType = CommandType.Text;
                                    command.Parameters.AddWithValue("@Id", currencyId);

                                    command.ExecuteNonQuery();
                                    connection.Close();

                                    MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearMaster();
                                }
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
                if (cmbFromCurrency.SelectedValue != null && int.Parse(cmbFromCurrency.SelectedValue.ToString()) != 0 && cmbFromCurrency.SelectedIndex != 0)
                {
                    int? CurrencyFromId = int.Parse(cmbFromCurrency.SelectedValue.ToString());

                    establishConnection();
                    DataTable dt = new DataTable();

                    command = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyFromId", connection);
                    command.CommandType = CommandType.Text;

                    if (CurrencyFromId != null && CurrencyFromId != 0)
                    {
                        command.Parameters.AddWithValue("@CurrencyFromId", CurrencyFromId);
                    }
                    adapter = new SqlDataAdapter(command);

                    adapter.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        fromAmount = double.Parse(dt.Rows[0]["Amount"].ToString());
                    }
                    connection.Close();
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
                if (cmbToCurrency.SelectedValue != null && int.Parse(cmbToCurrency.SelectedValue.ToString()) != 0 && cmbToCurrency.SelectedIndex != 0)
                {
                    int? CurrencyToId = int.Parse(cmbToCurrency.SelectedValue.ToString());

                    establishConnection();

                    DataTable dt = new DataTable();

                    command = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyToId", connection);
                    command.CommandType = CommandType.Text;

                    if (CurrencyToId != null && CurrencyToId != 0)
                    {
                        command.Parameters.AddWithValue("@CurrencyToId", CurrencyToId);
                    }

                    adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        toAmount = double.Parse(dt.Rows[0]["Amount"].ToString());
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbFromCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //If the user presses Tab or Enter key then trigger event cmbFromCurrency_SelectionChanged
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbFromCurrency_SelectionChanged(sender, null);
            }
        }

        private void cmbToCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //If the user press Tab or Enter key then trigger event cmbToCurrency_SelectionChanged
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbToCurrency_SelectionChanged(sender, null);
            }
        }

        private void WordValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[A-Za-z]+");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}

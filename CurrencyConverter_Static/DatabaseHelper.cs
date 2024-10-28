using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows;
using System.Linq;

public class DatabaseHelper : IDisposable
{
    private readonly SqlConnection con;

    public DatabaseHelper()
    {
        string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        con = new SqlConnection(connString);
    }

    private void OpenConnection()
    {
        if (con.State == ConnectionState.Closed)
        {
            con.Open();
        }
    }

    private void CloseConnection()
    {
        if (con.State == ConnectionState.Open)
        {
            con.Close();
        }
    }

    // Execute a query that returns a DataTable
    public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
    {
        DataTable dt = new DataTable();
        try
        {
            OpenConnection();
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddRange(parameters);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Ausführen der Abfrage: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            CloseConnection();
        }
        return dt;
    }

    // Execute a scalar query
    public object ExecuteScalar(string query, params SqlParameter[] parameters)
    {
        try
        {
            OpenConnection();
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Abrufen des Wertes: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
        finally
        {
            CloseConnection();
        }
    }

    // Execute a non-query command (INSERT, UPDATE, DELETE)
    public void ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        try
        {
            OpenConnection();
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddRange(parameters);

                // Debugging: Show the executed SQL query with parameters
                string parameterInfo = string.Join(", ", parameters.Select(p => $"{p.ParameterName}: {p.Value}"));
                MessageBox.Show($"Auszuführende Abfrage: {query}\nParameter: {parameterInfo}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                int rowsAffected = cmd.ExecuteNonQuery();

                // Check if a record was inserted or updated
                if (rowsAffected == 0)
                {
                    MessageBox.Show("Kein Datensatz wurde eingefügt oder aktualisiert.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"{rowsAffected} Datensatz(e) erfolgreich bearbeitet.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (SqlException sqlEx)
        {
            MessageBox.Show($"Datenbankfehler: {sqlEx.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ein Fehler ist aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            CloseConnection();
        }
    }

    public void Dispose()
    {
        CloseConnection();
        con?.Dispose();
    }
}

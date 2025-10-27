using System.Data.SqlClient;
using System.Diagnostics;

namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class created_queries
    {
        private string instanceName = "claim_system";
        private string databaseName = "contract_claims_database";
        private string connectionStringToInstance => $@"Server=(localdb)\{instanceName};Integrated Security=true;";
        private string connectionStringToDatabase => $@"Server=(localdb)\{instanceName};Database={databaseName};Integrated Security=true;";

                public void InitializeSystem()
        {
            try
            {
                //  Check and create LocalDB instance
                CreateClaimSystemInstance();

                //  Check and create Database
                CreateDatabase();

                // Check and create Tables
                CreateTables();

                Console.WriteLine(" LocalDB instance, database, and tables verified successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error initializing system: {ex.Message}");
            }
        }

        // -----------------------------
        // LocalDB Instance Handling
        // -----------------------------
        private void CreateClaimSystemInstance()
        {
            if (CheckInstanceExists())
            {
                Console.WriteLine($" LocalDB instance '{instanceName}' already exists.");
                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c sqllocaldb create \"{instanceName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                    Console.WriteLine($" LocalDB instance '{instanceName}' created successfully!");
                else
                    Console.WriteLine($" Error creating instance: {error}");
            }
        }

        private bool CheckInstanceExists()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c sqllocaldb info \"{instanceName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error) &&
                    error.Contains($"LocalDB instance \"{instanceName}\" doesn't exist", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return !string.IsNullOrWhiteSpace(output)
                    && !output.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase);
            }
        }


        // -----------------------------
        // Database Handling
        // -----------------------------
        private void CreateDatabase()
        {
            string createDbQuery = $@"
         IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
         BEGIN
             CREATE DATABASE [{databaseName}];
         END";

            using (var connection = new SqlConnection(connectionStringToInstance))
            {
                connection.Open();
                using (var command = new SqlCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine($" Database '{databaseName}' verified or created.");
        }


        // CreateTables method
        private void CreateTables()
        {
            string createUsersTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
            BEGIN
                CREATE TABLE Users (
                    userID INT PRIMARY KEY IDENTITY(1,1),
                    Full_Name VARCHAR(100),
                    Email_address VARCHAR(255) UNIQUE,
                    password VARCHAR(255),
                    Role VARCHAR(50)
                );
            END";

            string createClaimsTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Claims' AND xtype='U')
            BEGIN
                CREATE TABLE Claims (
                    claimID INT PRIMARY KEY IDENTITY(1,1),
                    lecturerID INT,
                    Email_Address VARCHAR(255),
                    Claim_Date DATE,
                    Faculty VARCHAR(100),
                    Module VARCHAR(100),
                    Hours_Worked INT,
                    Hourly_Rate DECIMAL(10,2),
                    Calculated_Amount DECIMAL(10,2),
                    Supporting_Documents VARCHAR(500),
                    Status VARCHAR(50) DEFAULT 'Pending',
                    RejectionReason VARCHAR(500),
                    SubmittedDate DATETIME DEFAULT GETDATE(),
                    ProcessedDate DATETIME NULL,
                    ProcessedBy VARCHAR(255) NULL,
                    FOREIGN KEY (lecturerID) REFERENCES Users(userID)
                );
            END";

            using (var connection = new SqlConnection(connectionStringToDatabase))
            {
                connection.Open();
                using (var cmd = new SqlCommand(createUsersTable, connection))
                    cmd.ExecuteNonQuery();
                using (var cmd = new SqlCommand(createClaimsTable, connection))
                    cmd.ExecuteNonQuery();
            }
            Console.WriteLine(" Tables 'Users' and 'Claims' verified or created.");
        }

        public void store_user(string Full_Name, string Email_Address, string Password, string Role)
{
    try
    {
        //open connection
        using (SqlConnection connect = new SqlConnection(connectionStringToDatabase))
        {
            connect.Open();
            //query to insert into a table
            string query = @"insert into users values('" + Full_Name + "', '" + Email_Address + "', '" + Password + "', '" + Role + "' )";

            //use the using function to command queries
            using (SqlCommand insert = new SqlCommand(query, connect))
            {
                //execute the query
                insert.ExecuteNonQuery();
                Console.WriteLine("user inserted successfully");
            }

            connect.Close();
        }
    }
    catch (Exception error)
    {
        Console.WriteLine(error.Message);
    }
}
        //method to login the user
        public bool login_user(string Email_Address, string Password, string Role)
        {
            bool found = false;
            try
            {
                //use the using to open the connection
                using (SqlConnection connect = new SqlConnection(connectionStringToDatabase))
                {
                    //open the connection
                    connect.Open();

                    //query to read from a table
                    string query = @"select * from Users where Email_Address='" + Email_Address + "' and Password='" + Password + "' and Role='" + Role + "';";

                    //use the using function to command queries
                    using (SqlCommand insert = new SqlCommand(query, connect))
                    {

                        //execute the query

                        using (SqlDataReader find = insert.ExecuteReader())
                        {

                            while (find.Read())
                            {
                                Console.WriteLine(find["userID"]);
                                Console.WriteLine(find["Full_Name"]);
                                Console.WriteLine(find["Email_Address"]);
                                Console.WriteLine(find["Password"]);
                                Console.WriteLine(find["Role"]);

                                found = true;
                            }

                            Console.WriteLine("user found");

                        }

                    }

                    //close the connection
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine();
            }
            return found;
        }//end of login user method


        // Store claim with status
        public void store_claim(string Email_Address, DateTime Claim_Date, string Faculty, string Module,
                              int Hours_Worked, decimal Hourly_Rate, decimal Calculated_Amount,
                              string Supporting_Documents)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connectionStringToDatabase))
                {
                    connect.Open();
                    string query = @"INSERT INTO Claims ('" +Email_Address + "', '" +Claim_Date+ "', '" + Faculty+ "', '" + Module + "' , '"+ Hours_Worked+ "' , '" + Hourly_Rate + "' , '" + Calculated_Amount + "' , '" + Supporting_Documents + "', 'Pending' )";
                    using (SqlCommand insert = new SqlCommand(query, connect))
                    {

                        insert.ExecuteNonQuery();
                        Console.WriteLine("Claim inserted successfully with Pending status");
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error storing claim: {error.Message}");
                throw;
            }
        }

        // Get claims for approval (for PC/Admin)
        public List<claim> GetPendingClaims()
        {
            var claims = new List<claim>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connectionStringToDatabase))
                {
                    connect.Open();
                    string query = @"SELECT * FROM Claims WHERE Status = 'Pending' ORDER BY SubmittedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            claims.Add(new claim
                            {
                                ClaimID = reader.GetInt32(reader.GetOrdinal("claimID")),
                                Email_Address = reader.GetString(reader.GetOrdinal("Email_Address")),
                                Claim_Date = reader.GetDateTime(reader.GetOrdinal("Claim_Date")),
                                Faculty = reader.GetString(reader.GetOrdinal("Faculty")),
                                Module = reader.GetString(reader.GetOrdinal("Module")),
                                Hours_Worked = reader.GetInt32(reader.GetOrdinal("Hours_Worked")),
                                Hourly_Rate = reader.GetDecimal(reader.GetOrdinal("Hourly_Rate")),
                                Calculated_Amount = reader.GetDecimal(reader.GetOrdinal("Calculated_Amount")),
                                Supporting_Documents = reader.GetString(reader.GetOrdinal("Supporting_Documents")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                SubmittedDate = reader.GetDateTime(reader.GetOrdinal("SubmittedDate"))
                            });
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error getting pending claims: {error.Message}");
            }
            return claims;
        }

        // Get claims by user email
        public List<claim> GetUserClaims(string email)
        {
            var claims = new List<claim>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connectionStringToDatabase))
                {
                    connect.Open();
                    string query = @"SELECT * FROM Claims WHERE Email_Address = @Email ORDER BY SubmittedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                claims.Add(new claim
                                {
                                    ClaimID = reader.GetInt32(reader.GetOrdinal("claimID")),
                                    Email_Address = reader.GetString(reader.GetOrdinal("Email_Address")),
                                    Claim_Date = reader.GetDateTime(reader.GetOrdinal("Claim_Date")),
                                    Faculty = reader.GetString(reader.GetOrdinal("Faculty")),
                                    Module = reader.GetString(reader.GetOrdinal("Module")),
                                    Hours_Worked = reader.GetInt32(reader.GetOrdinal("Hours_Worked")),
                                    Hourly_Rate = reader.GetDecimal(reader.GetOrdinal("Hourly_Rate")),
                                    Calculated_Amount = reader.GetDecimal(reader.GetOrdinal("Calculated_Amount")),
                                    Supporting_Documents = reader.GetString(reader.GetOrdinal("Supporting_Documents")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    RejectionReason = reader.IsDBNull(reader.GetOrdinal("RejectionReason")) ?
                                        "" : reader.GetString(reader.GetOrdinal("RejectionReason")),
                                    SubmittedDate = reader.GetDateTime(reader.GetOrdinal("SubmittedDate")),
                                    ProcessedDate = reader.IsDBNull(reader.GetOrdinal("ProcessedDate")) ?
                                        null : reader.GetDateTime(reader.GetOrdinal("ProcessedDate"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error getting user claims: {error.Message}");
            }
            return claims;
        }

        // Approve claim
        public bool ApproveClaim(int claimId, string processedBy)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connectionStringToDatabase))
                {
                    connect.Open();
                    string query = @"UPDATE Claims SET Status = 'Approved', ProcessedDate = GETDATE(), 
                                    ProcessedBy = @ProcessedBy WHERE claimID = @ClaimID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ClaimID", claimId);
                        cmd.Parameters.AddWithValue("@ProcessedBy", processedBy);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error approving claim: {error.Message}");
                return false;
            }
        }

        // Reject claim
        public bool RejectClaim(int claimId, string reason, string processedBy)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connectionStringToDatabase))
                {
                    connect.Open();
                    string query = @"UPDATE Claims SET Status = 'Rejected', RejectionReason = @Reason, 
                                    ProcessedDate = GETDATE(), ProcessedBy = @ProcessedBy 
                                    WHERE claimID = @ClaimID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ClaimID", claimId);
                        cmd.Parameters.AddWithValue("@Reason", reason);
                        cmd.Parameters.AddWithValue("@ProcessedBy", processedBy);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error rejecting claim: {error.Message}");
                return false;
            }
        }
    }
}

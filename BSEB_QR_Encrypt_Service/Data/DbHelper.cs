using BSEB_QR_Encrypt_Service.Models;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace BSEBExamResult_QRGenerate.Data
{
    public class DbHelper
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DbHelper> _logger;

        public DbHelper(AppDBContext context, ILogger<DbHelper> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================================
        // ✅ 1. STREAM ALL ROLL CODES (NO MEMORY LOAD)
        // ============================================================
        public async IAsyncEnumerable<(string RollCode, string RollNo)> GetAllRollCodesStreamAsync()
        {
            var conn = _context.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            int offset = 0;
            int pageSize = 5; // 🔥 adjust (2000–10000)
            //int pageSize = 5000; // 🔥 adjust (2000–10000)

            while (true)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "dbo.GetAllRollCodesForQR";
                cmd.CommandType = CommandType.StoredProcedure;

                // ✅ Pass parameters
                cmd.Parameters.Add(new SqlParameter("@Offset", offset));
                cmd.Parameters.Add(new SqlParameter("@PageSize", pageSize));

                using var reader = await cmd.ExecuteReaderAsync();

                bool hasData = false;

                while (await reader.ReadAsync())
                {
                    hasData = true;

                    yield return (
                        reader["RollCode"]?.ToString() ?? "",
                        reader["RollNo"]?.ToString() ?? ""
                    );
                }

                // ✅ Stop when no more data
                if (!hasData)
                    yield break;

                // ✅ Move to next page
                offset += pageSize;
            }
        }

        // ============================================================
        // ✅ 2. GET STUDENT RESULT (LoginSp)
        // ============================================================
        public async Task<StudentResult?> GetStudentResultAsync(string rollcode, string rollno)
        {
            try
            {
                // ✅ Create NEW connection (IMPORTANT FIX)
                using var conn = new SqlConnection(_context.Database.GetConnectionString());

                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "LoginSp";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@rollcode", rollcode));
                cmd.Parameters.Add(new SqlParameter("@rollno", rollno));

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return null;

                var student = new StudentResult
                {
                    Status = reader.GetInt32(reader.GetOrdinal("status")),
                    IsCCEMarks = reader.GetInt32(reader.GetOrdinal("IsCCEMarks")),
                    RollCode = reader["rollcode"]?.ToString(),
                    RollNo = reader["rollno"]?.ToString(),
                    BsebUniqueID = reader["BsebUniqueID"]?.ToString(),
                    NameoftheCandidate = reader["NameoftheCandidate"]?.ToString(),
                    FathersName = reader["FathersName"]?.ToString(),
                    CollegeName = reader["CollegeName"]?.ToString(),
                    RegistrationNo = reader["RegistrationNo"]?.ToString(),
                    Faculty = reader["FACULTY"]?.ToString(),
                    TotalAggregateMarkinNumber = reader["TotalAggregateMarkinNumber"]?.ToString(),
                    TotalAggregateMarkinWords = reader["TotalAggregateMarkinWords"]?.ToString(),
                    Division = reader["DIVISION"]?.ToString()
                };

                while (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        student.SubjectResults.Add(new SubjectResult
                        {
                            Sub = reader["Sub"]?.ToString(),
                            MaxMark = reader["maxMark"] == DBNull.Value ? null : Convert.ToInt32(reader["maxMark"]),
                            PassMark = reader["passMark"] == DBNull.Value ? null : Convert.ToInt32(reader["passMark"]),
                            Theory = reader["theory"]?.ToString(),
                            OB_PR = reader["OB_PR"]?.ToString(),
                            GRC_THO = reader["GRC_THO"]?.ToString(),
                            GRC_PR = reader["GRC_PR"]?.ToString(),
                            TotSub = reader["TOT_SUB"]?.ToString(),
                            CCEMarks = reader["CCEMarks"]?.ToString(),
                            SubjectGroupName = reader["SubjectGroupName"]?.ToString()
                        });
                    }
                }

                return student;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching student RC:{rc}, RN:{rn}", rollcode, rollno);
                return null;
            }
        }

        // ============================================================
        // ✅ 3. BULK INSERT (FAST FOR LAKH RECORDS)
        // ============================================================
        public async Task BulkSaveEncryptedDataAsync(List<QREncryptedData> records)
        {
            if (records == null || records.Count == 0)
                return;

            try
            {
                var conn = _context.Database.GetDbConnection();

                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "InsertQREncryptedDataBulk";
                cmd.CommandType = CommandType.StoredProcedure;

                // ✅ Prepare DataTable
                var dt = new DataTable();
                dt.Columns.Add("RollCode", typeof(string));
                dt.Columns.Add("RollNo", typeof(string));
                dt.Columns.Add("EncryptedData", typeof(string));
                dt.Columns.Add("CreatedOn", typeof(DateTime));

                foreach (var r in records)
                {
                    dt.Rows.Add(
                        r.RollCode,
                        r.RollNo,
                        r.EncryptedData,
                        r.CreatedOn
                    );
                }

                // ✅ TVP Parameter
                var param = new SqlParameter
                {
                    ParameterName = "@Data",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.QREncryptedDataType",
                    Value = dt
                };

                cmd.Parameters.Add(param);

                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation("✅ SP Bulk Insert Success: {count}", records.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SP Bulk Insert Failed. Records: {count}", records?.Count);
                throw;
            }
        }
    }
}
using BSEB_QR_Encrypt_Service.Models;
using BSEBExamResult_QRGenerate.Data;

public class ProcessingService
{
    private readonly DbHelper _dbHelper;
    private readonly EncryptionService _encryptionService;
    private readonly ILogger<ProcessingService> _logger;

    public ProcessingService(DbHelper dbHelper, EncryptionService encryptionService, ILogger<ProcessingService> logger)
    {
        _dbHelper = dbHelper;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task ProcessAsync(CancellationToken token)
    {
        _logger.LogInformation("Bulk QR Encryption (No Image) started at {Time}", DateTime.Now);

        int success = 0, fail = 0, skip = 0;

        const int BATCH_SIZE = 5000; // 🔥 increased like controller
        var batch = new List<QREncryptedData>(BATCH_SIZE);

        await foreach (var (rollCode, rollNo) in _dbHelper.GetAllRollCodesStreamAsync())
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogWarning("Process cancelled!");
                break;
            }

            try
            {
                // Step 1: Fetch student data
                var student = await _dbHelper.GetStudentResultAsync(rollCode, rollNo);

                if (student == null || student.Status != 1)
                {
                    skip++;
                    continue;
                }

                // Step 2: Encrypt full payload
                string encrypted = _encryptionService.EncryptStudent(student);

                if (string.IsNullOrWhiteSpace(encrypted))
                {
                    skip++;
                    continue;
                }

                // Step 3: Add to batch
                batch.Add(new QREncryptedData
                {
                    RollCode = rollCode,
                    RollNo = rollNo,
                    EncryptedData = encrypted,
                    CreatedOn = DateTime.Now
                });

                success++;

                // Step 4: Flush batch
                if (batch.Count >= BATCH_SIZE)
                {
                    await _dbHelper.BulkSaveEncryptedDataAsync(batch);

                    _logger.LogInformation(
                        "Batch flushed. Total saved so far: {Count}", success);

                    batch.Clear();
                }
            }
            catch (Exception ex)
            {
                fail++;

                _logger.LogError(ex,
                    "Failed for RollCode: {RC}, RollNo: {RN}",
                    rollCode, rollNo);
            }
        }

        // Step 5: Save remaining
        if (batch.Count > 0)
        {
            await _dbHelper.BulkSaveEncryptedDataAsync(batch);
            batch.Clear();
        }

        _logger.LogInformation(
            "DONE. Success: {S}, Failed: {F}, Skipped: {SK}",
            success, fail, skip);
    }
}
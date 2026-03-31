using BSEB_QR_Encrypt_Service.Models;
using BSEB_QR_Encrypt_Service.Utilities;

public class EncryptionService
{
    public string EncryptStudent(StudentResult student)
    {
        return QrUtility.GenerateEncryptedPayloadFull(student);
    }
}
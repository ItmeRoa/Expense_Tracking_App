using System.Security.Cryptography;

namespace expense_tracker.Util;

public class OtpGenerator
{

    public  string GenerateSecureOtp()
    {

        byte[] randomNumber = new byte[4];
        
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        int value = BitConverter.ToInt32(randomNumber, 0);

        int otp = Math.Abs(value % 900000) + 100000;

        return otp.ToString();
    }

}

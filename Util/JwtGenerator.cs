using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ILogger = Serilog.ILogger;

namespace expense_tracker.Util;

public class JwtGenerator
{
    private readonly string _publicKeyPath;
    private readonly string _symmetricKey;
    private readonly Dictionary<string, IEnumerable<Claim>> _roleClaims;
    private readonly ILogger _logger;

    public JwtGenerator(IConfiguration config, ILogger logger)
    {
        _logger = logger;
        _publicKeyPath = config["PUBLIC_KEY"] ?? string.Empty;
        _symmetricKey = config["JWT_KEY"] ?? string.Empty;
        _roleClaims = new Dictionary<string, IEnumerable<Claim>>
        {
            {
                "Admin", new List<Claim>
                {
                    new("Permission", "CanManageUsers"),
                    new("Permission", "CanEditReports")
                }
            },
            {
                "Premium Consumer", new List<Claim>
                {
                    new("Permission", "CanViewReports"),
                    new("Permission", "CanAccessPremiumFeatures")
                }
            },
            {
                "Consumer", new List<Claim>
                {
                    new("Permission", "CanViewReports")
                }
            }
        };
    }

    private static byte[] GetPublicKeyBytes(string pemKeyPath)
    {
        var keyContent = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), pemKeyPath));
        keyContent = keyContent.Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "");
        return Convert.FromBase64String(keyContent);
    }

    public string GenerateAsymmetricAccessToken(string userId, string email, string role)
    {
        var publicKey = GetPublicKeyBytes(_publicKeyPath);

        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKey, out _);

        var credential = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        if (_roleClaims.TryGetValue(role, out var roleSpecification))
        {
            claims.AddRange(roleSpecification);
        }
        else
        {
            _logger.Warning($"Role '{role}' not found in role claims.");
        }

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Issuer = "roa.io",
            Audience = "personal-finance-app",
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credential
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    public string GenerateAccessToken(string userId, string email, string role)
    {
        var credential = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_symmetricKey)),
            SecurityAlgorithms.HmacSha256Signature);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        if (_roleClaims.TryGetValue(role, out var roleSpecification))
        {
            claims.AddRange(roleSpecification);
        }
        else
        {
            _logger.Warning($"Role '{role}' not found in role claims.");
        }

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Issuer = "roa.io",
            Audience = "personal-finance-app",
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credential
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
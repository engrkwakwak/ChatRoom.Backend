using Service.Contracts;
using Shared.DataTransferObjects.Email;
using Shared.DataTransferObjects.Users;
using Razor.Templating.Core;
using RedisCacheService;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Contracts;

namespace Service;

public sealed class EmailService(IRedisCacheManager cache, ISmtpClientManager smtp) : IEmailService {
    private readonly IRedisCacheManager _cache = cache;
    private readonly ISmtpClientManager _smtpClient = smtp;

    public async Task<bool> SendVerificationEmail(UserDto user, string verificationLink, string token)
    {
        EmailVerificationViewModel viewModel = new() { User = user, VerificationLink=verificationLink};
        EmailDto email = new()
        {
            Body = await RazorTemplateEngine.RenderAsync("/Views/EmailTemplates/EmailVerification.cshtml", viewModel),
            To = user.Email,
            Subject = "Complete Your Registration with Chatroom",
            User = user
        };

        await CacheToken(token);

        bool isMessageSent = await _smtpClient.SendEmailAsync(email);
        return isMessageSent;
    }

    public async Task<bool> SendPasswordResetLink(UserDto user, string passwordResetLink, string token)
    {
        PasswordResetEmailViewModel viewModel = new() { User = user, PasswordResetLink = passwordResetLink};
        EmailDto email = new()
        {
            Body = await RazorTemplateEngine.RenderAsync("/Views/EmailTemplates/PasswordResetEmail.cshtml", viewModel),
            To = user.Email,
            Subject = "Password Reset",
            User = user
        };

        await CacheToken(token);

        bool isMessageSent = await _smtpClient.SendEmailAsync(email);
        return isMessageSent;
    }

    public async Task RemoveTokenFromCache(string token)
    {
        string cacheToken = $"email-{token}";
        await _cache.RemoveDataAsync(cacheToken);
    }

    public async Task<bool> IsEmailTokenUsed(string token)
    {
        string? _token = await _cache.GetCachedDataAsync<string>($"email-{token}");
        return _token.IsNullOrEmpty();
    }

    private async Task CacheToken(string token)
    {
        JwtPayload payload = GetTokenPayload(token);
        DateTimeOffset exp = DateTimeOffset.FromUnixTimeSeconds(payload.Expiration!.Value);
        TimeSpan span = TimeSpan.FromSeconds(exp.Subtract(DateTimeOffset.UtcNow).TotalSeconds);
        await _cache.SetCachedDataWithAbsoluteExpAsync($"email-{token}", token, span);
    }

    private static JwtPayload GetTokenPayload(string token) {
        JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        JwtSecurityToken securityToken = jwtSecurityTokenHandler.ReadJwtToken(token);

        return securityToken.Payload;
    }
}

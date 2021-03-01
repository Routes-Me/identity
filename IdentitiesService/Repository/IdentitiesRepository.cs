using IdentitiesService.Models;
using IdentitiesService.Abstraction;
using IdentitiesService.Models.Common;
using IdentitiesService.Models.DBModels;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace IdentitiesService.Repository
{
    public class IdentitiesRepository : IIdentitiesRepository
    {
        private readonly AppSettings _appSettings;
        private readonly IdentitiesServiceContext _context;
        public IdentitiesRepository(IOptions<AppSettings> appSettings, IdentitiesServiceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic RevokeRefreshToken(string refreshToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            if (string.IsNullOrEmpty(refreshToken) || !tokenHandler.CanReadToken(refreshToken))
                throw new ArgumentNullException(CommonMessage.InvalidData);

            JwtSecurityToken refreshTokenData = tokenHandler.ReadToken(refreshToken) as JwtSecurityToken;

            if (refreshTokenData.ValidTo < DateTime.UtcNow)
                throw new SecurityTokenExpiredException(CommonMessage.TokenExpired);

            string tokenreference = refreshTokenData.Claims.First(c => c.Type == "sub").Value;
            if (_context.RevokedRefreshTokens.Where(r => r.RefreshTokenReference == tokenreference).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.TokenAlreadyRevoked);

            RevokedRefreshTokens revokedToken = new RevokedRefreshTokens
            {
                RefreshTokenReference =  refreshTokenData.Claims.First(c => c.Type == "ref").Value,
                UserId = Convert.ToInt32(refreshTokenData.Claims.First(c => c.Type == "sub").Value),
                ExpiryAt = refreshTokenData.ValidTo,
                RevokedAt = DateTime.Now
            };

            return revokedToken;
        }
    }
}

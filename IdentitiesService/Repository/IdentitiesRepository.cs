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
            JwtSecurityToken refreshTokenData = ValidateToken(refreshToken);

            string tokenreference = refreshTokenData.Claims.First(c => c.Type == "ref").Value;
            if (_context.RevokedRefreshTokens.Where(r => r.RefreshTokenReference == tokenreference).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.TokenAlreadyRevoked);

            RevokedRefreshTokens revokedToken = new RevokedRefreshTokens
            {
                RefreshTokenReference =  refreshTokenData.Claims.First(c => c.Type == "ref").Value,
                IdentityId = Convert.ToInt32(refreshTokenData.Claims.First(c => c.Type == "sub").Value),
                ExpiryAt = refreshTokenData.ValidTo,
                RevokedAt = DateTime.Now
            };
            return revokedToken;
        }

        public dynamic RevokeAccessToken(string accessToken)
        {
            JwtSecurityToken accessTokenData = ValidateToken(accessToken);

            string tokenreference = accessTokenData.Claims.First(c => c.Type == "ref").Value;
            if (_context.RevokedAccessTokens.Where(r => r.AccessTokenReference == tokenreference).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.TokenAlreadyRevoked);

            RevokedAccessTokens revokedToken = new RevokedAccessTokens
            {
                AccessTokenReference = accessTokenData.Claims.First(c => c.Type == "ref").Value,
                IdentityId = Convert.ToInt32(accessTokenData.Claims.First(c => c.Type == "sub").Value),
                ExpiryAt = accessTokenData.ValidTo,
                RevokedAt = DateTime.Now
            };
            return revokedToken;
        }

        private dynamic ValidateToken(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            if (string.IsNullOrEmpty(token) || !tokenHandler.CanReadToken(token))
                throw new ArgumentNullException(CommonMessage.InvalidData);

            JwtSecurityToken tokenData = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (tokenData.ValidTo < DateTime.UtcNow)
                throw new SecurityTokenExpiredException(CommonMessage.TokenExpired);

            return tokenData;
        }
    }
}

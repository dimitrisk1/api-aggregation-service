using Core_Api.Controllers;
using Core_Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiAggregator.Test
{
    public class SecurityTests
    {
        [Fact]
        public void Aggregate_And_Stats_Endpoints_Should_Require_Authorization()
        {
            typeof(AggregateController).Should().BeDecoratedWith<AuthorizeAttribute>();
            typeof(StatsController).Should().BeDecoratedWith<AuthorizeAttribute>();
        }

        [Fact]
        public void Auth_Endpoint_Should_Allow_Anonymous_Access()
        {
            typeof(AuthController).Should().BeDecoratedWith<AllowAnonymousAttribute>();
        }

        [Fact]
        public void JwtIdentityService_Should_Create_Token_With_Expected_Claims()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "this-is-a-long-enough-development-signing-key",
                    ["Jwt:Issuer"] = "core-api",
                    ["Jwt:Audience"] = "core-api-users"
                })
                .Build();

            var service = new JwtIdentityService(configuration);
            var token = service.GenerateToken("alice", "Admin");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Issuer.Should().Be("core-api");
            jwt.Audiences.Should().Contain("core-api-users");
            jwt.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Name && claim.Value == "alice");
            jwt.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        }
    }
}

﻿using Bogus;
using FluentAssertions;
using MatchDayApp.Domain.Entities.Enum;
using MatchDayApp.Domain.Resources;
using MatchDayApp.Infra.CrossCutting.Contract.V1.Request.Auth;
using MatchDayApp.Infra.CrossCutting.Contract.V1.Response.Auth;
using MatchDayApp.Infra.CrossCutting.V1;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MatchDayApp.IntegrationTest.Controller.AuthenticationController
{
    [Trait("AuthenticationController", "Register")]
    public class RegisterTest : ControllerTest
    {
        private readonly string _requestUri = ApiRoutes.Authentication.Register;

        private readonly ITestOutputHelper _output;
        private readonly RegisterRequest _registerRequest;
        private readonly Faker _faker;

        public RegisterTest(CustomWebApplicationFactory factory,
            ITestOutputHelper output) : base(factory)
        {
            _output = output;
            _faker = new Faker("pt_BR");

            _registerRequest = new RegisterRequest
            {
                FirstName = "Mateus",
                LastName = "Silva",
                UserName = "mateus.silva",
                Email = "mateussilva@email.com",
                Password = "Mateus@123",
                ConfirmPassword = "Mateus@123",
                UserType = UserType.Player,
                Avatar = "avatar.png"
            };
        }

        #region FirstName Validation

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Register_AuthenticationController_FailedResponseIfFirstNameIsNullOrEmpty(string invalidFirstName)
        {
            _registerRequest.FirstName = invalidFirstName;

            var response = await HttpClientTest
                .PostAsJsonAsync(_requestUri, _registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var authResponse = await response.Content
                .ReadAsAsync<AuthFailedResponse>();

            authResponse.Errors.Should()
                .HaveCount(1).And
                .BeEquivalentTo(new[] { Dictionary.MV005 });

            _output.WriteLine($@"Valor entrada: {JsonSerializer.Serialize(_registerRequest)} 
                              | Resultado teste: {response.StatusCode}");
        }

        [Fact]
        public async Task Register_AuthenticationController_FailedResponseIfFirstNameIsLessThan4()
        {
            _registerRequest.FirstName = _faker.Random.String2(1, 3);

            var response = await HttpClientTest
                .PostAsJsonAsync(_requestUri, _registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var authResponse = await response.Content
                .ReadAsAsync<AuthFailedResponse>();

            authResponse.Errors.Should()
                .HaveCount(1).And
                .BeEquivalentTo(new[] { Dictionary.MV006 });

            _output.WriteLine($@"Valor entrada: {JsonSerializer.Serialize(_registerRequest)} 
                              | Resultado teste: {response.StatusCode}");
        }

        [Fact]
        public async Task Register_AuthenticationController_FailedResponseIfFirstNameHasSpecialCaractersOrNumber()
        {
            _registerRequest.FirstName = _faker.Random.String2(5, 10, chars: "abc123#@$");

            var response = await HttpClientTest
                .PostAsJsonAsync(_requestUri, _registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var authResponse = await response.Content
                .ReadAsAsync<AuthFailedResponse>();

            authResponse.Errors.Should()
                .HaveCount(1).And
                .BeEquivalentTo(new[] { Dictionary.MV009 });

            _output.WriteLine($@"Valor entrada: {JsonSerializer.Serialize(_registerRequest)} 
                              | Resultado teste: {response.StatusCode}");
        }

        #endregion

        #region LastName Validation

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Register_AuthenticationController_FailedResponseIfLastNameIsNullOrEmpty(string invalidLastName)
        {
            _registerRequest.LastName = invalidLastName;

            var response = await HttpClientTest
                .PostAsJsonAsync(_requestUri, _registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var authResponse = await response.Content
                .ReadAsAsync<AuthFailedResponse>();

            authResponse.Errors.Should()
                .HaveCount(1).And
                .BeEquivalentTo(new[] { Dictionary.MV007 });

            _output.WriteLine($@"Valor entrada: {JsonSerializer.Serialize(_registerRequest)} 
                              | Resultado teste: {response.StatusCode}");
        }

        [Fact]
        public async Task Register_AuthenticationController_FailedResponseIfLastNameIsLessThan4()
        {
            _registerRequest.LastName = _faker.Random.String2(1, 3);

            var response = await HttpClientTest
                .PostAsJsonAsync(_requestUri, _registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var authResponse = await response.Content
                .ReadAsAsync<AuthFailedResponse>();

            authResponse.Errors.Should()
                .HaveCount(1).And
                .BeEquivalentTo(new[] { Dictionary.MV008 });

            _output.WriteLine($@"Valor entrada: {JsonSerializer.Serialize(_registerRequest)} 
                              | Resultado teste: {response.StatusCode}");
        }

        [Fact]
        public async Task Register_AuthenticationController_FailedResponseIfLastNameHasSpecialCaractersOrNumber()
        {
            _registerRequest.LastName = _faker.Random.String2(5, 10, chars: "abc123#@$");

            var response = await HttpClientTest
                .PostAsJsonAsync(_requestUri, _registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var authResponse = await response.Content
                .ReadAsAsync<AuthFailedResponse>();

            authResponse.Errors.Should()
                .HaveCount(1).And
                .BeEquivalentTo(new[] { Dictionary.MV010 });

            _output.WriteLine($@"Valor entrada: {JsonSerializer.Serialize(_registerRequest)} 
                              | Resultado teste: {response.StatusCode}");
        }

        #endregion

        #region Email Validation

        [Fact]
        public async Task Register_AuthenticationController_FailedResponseIfEmailIsInvalid()
        {
            _registerRequest.Email = _faker.Random.String2(15,20);

            var response = await HttpClientTest
                .PostAsJsonAsync(_requestUri, _registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var authResponse = await response.Content
                .ReadAsAsync<AuthFailedResponse>();

            authResponse.Errors.Should()
                .HaveCount(1).And
                .BeEquivalentTo(new[] { Dictionary.MV011 });

            _output.WriteLine($@"Valor entrada: {JsonSerializer.Serialize(_registerRequest)} 
                              | Resultado teste: {response.StatusCode}");
        }

        #endregion

    }
}

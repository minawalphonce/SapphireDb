﻿using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class LoginCommandHandler : AuthCommandHandlerBase, ICommandHandler<LoginCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly JwtOptions jwtOptions;
        private readonly JwtIssuer jwtIssuer;

        public LoginCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer, JwtOptions jwtOptions, JwtIssuer jwtIssuer, IServiceProvider serviceProvider)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.jwtOptions = jwtOptions;
            this.jwtIssuer = jwtIssuer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, LoginCommand command)
        {
            if (string.IsNullOrEmpty(command.Username) || string.IsNullOrEmpty(command.Password))
            {
                await websocketConnection.Send(new LoginResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("Username and password cannot be empty")
                });
                return;
            }

            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            IdentityUser userToVerify = await usermanager.FindByNameAsync(command.Username) ??
                await usermanager.FindByEmailAsync(command.Username);

            if (userToVerify != null)
            {
                if ((bool)await (dynamic)contextTypeContainer.UserManagerType.GetMethod("CheckPasswordAsync").Invoke(usermanager, new object[] { userToVerify, command.Password }))
                {
                    RefreshToken rT = new RefreshToken()
                    {
                        UserId = userToVerify.Id
                    };

                    IRealtimeAuthContext context = GetContext();
                    context.RefreshTokens.RemoveRange(context.RefreshTokens.Where(rt => rt.CreatedOn.Add(jwtOptions.ValidFor) < DateTime.UtcNow));
                    context.RefreshTokens.Add(rT);
                    context.SaveChanges();

                    LoginResponse loginResponse = new LoginResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        AuthToken = await jwtIssuer.GenerateEncodedToken(userToVerify),
                        ExpiresAt = jwtOptions.Expiration,
                        ValidFor = jwtOptions.ValidFor.TotalSeconds,
                        RefreshToken = rT.RefreshKey,
                        UserData = await ModelHelper.GenerateUserData(userToVerify, contextTypeContainer, usermanager)
                    };

                    await websocketConnection.Send(loginResponse);
                    return;
                }
            }

            await websocketConnection.Send(new LoginResponse()
            {
                ReferenceId = command.ReferenceId,
                Error = new Exception("Login failed")
            });
        }
    }
}

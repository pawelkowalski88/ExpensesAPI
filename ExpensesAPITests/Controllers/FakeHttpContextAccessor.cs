using ExpensesAPI.Models;
using ExpensesAPI.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace ExpensesAPITests.Controllers
{
    class FakeHttpContextAccessor : IHttpContextAccessor
    {
        private readonly MainDbContext context;
        private readonly bool noUser;

        public FakeHttpContextAccessor(MainDbContext context) : this(context, false) { }

        public FakeHttpContextAccessor(MainDbContext context, bool noUser)
        {
            this.context = context;
            this.noUser = noUser;
        }

        public HttpContext HttpContext
        {
            get
            {
                return new FakeHttpContext(context, noUser);
            }

            set => throw new NotImplementedException();
        }
    }

    class FakeHttpContext : HttpContext
    {
        private readonly MainDbContext context;
        private readonly bool noUser;

        public FakeHttpContext(MainDbContext context, bool noUser)
        {
            this.context = context;
            this.noUser = noUser;
        }
        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request => throw new NotImplementedException();

        public override HttpResponse Response => throw new NotImplementedException();

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override ClaimsPrincipal User
        {
            get
            {
                var user = new User { FirstName = "Zenek" };
                if (noUser == false)
                    return new ClaimsPrincipal(new List<ClaimsIdentity>
                        {
                            new ClaimsIdentity(new List<Claim>{ new Claim("id", user.Id) })
                        });
                else
                    return null;
            }

            set => throw new NotImplementedException();
        }
        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
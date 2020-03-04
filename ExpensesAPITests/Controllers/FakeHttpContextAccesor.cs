using ExpensesAPI.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace ExpensesAPITests.Controllers
{
    class FakeHttpContextAccesor : IHttpContextAccessor
    {
        private readonly MainDbContext context;

        public FakeHttpContextAccesor(MainDbContext context)
        {
            this.context = context;
        }

        public HttpContext HttpContext
        {
            get
            {
                return new FakeHttpContext(context);
            }

            set => throw new NotImplementedException();
        }
    }

    class FakeHttpContext : HttpContext
    {
        private readonly MainDbContext context;

        public FakeHttpContext(MainDbContext context)
        {
            this.context = context;
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
                var user = context.Users.FirstOrDefault(s => s.FirstName == "Zenek");
                if (user != null)
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
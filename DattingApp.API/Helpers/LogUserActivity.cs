using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DattingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DattingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            int userId = int.Parse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var repo = context.HttpContext.RequestServices.GetService<IDatingRepository>();

            var userFromRepo = await repo.GetUser(userId);
            userFromRepo.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }
}
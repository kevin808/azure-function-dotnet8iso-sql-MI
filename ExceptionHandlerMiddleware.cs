using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.ServiceModel;
using FunctionApp.Helper;
using FunctionApp.Models;
using FunctionApp.Models.Common;

namespace ALD_LogicProcessing.Middleware
{
    public class ExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly AppDbContext _appDbContext;
        public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            _logger.LogInformation("[START] Handle request={Request}",
                context.FunctionDefinition.Name);
            DateTime startedTime = DateTime.Now;
            //FunctionAppLoggings model =
            //            new(startedTime, null, null, false, context.FunctionDefinition.Name);
            //await _appDbContext.FunctionAppLoggings.AddAsync(model);
            await _appDbContext.SaveChangesAsync();
            try
            {
                await next.Invoke(context);
                DateTime finishedTime = DateTime.Now;
                //model.FinishedTime = finishedTime;
                //model.ExcutionTime = (finishedTime - startedTime).TotalSeconds;
                //model.IsCompleted = true;
                //_appDbContext.FunctionAppLoggings.Update(model);
                await _appDbContext.SaveChangesAsync();
                _logger.LogInformation("[END] Handle request={Request}",
                context.FunctionDefinition.Name);
            }
            catch (Exception exception)
            {
                var req = await context.GetHttpRequestDataAsync();
                var res = req!.CreateResponse();
                //model.FinishedTime = DateTime.Now;
                //model.Response = StringHelper.GenerateException(exception);
                _logger.LogError($"[Exception] Exception occurs: {context.FunctionDefinition.Name} &&  {StringHelper.GenerateException(exception)}");
                //_appDbContext.FunctionAppLoggings.Update(model);
                await _appDbContext.SaveChangesAsync();
                req = await context.GetHttpRequestDataAsync();
                res = req!.CreateResponse();
                res.StatusCode = HttpStatusCode.InternalServerError;
                await res.WriteStringAsync(StringHelper.GenerateException(exception));
                context.GetInvocationResult().Value = res;
            }
        }
    }
}
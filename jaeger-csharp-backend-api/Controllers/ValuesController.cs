using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTracing;
using System;
using System.Collections.Generic;

namespace jaeger_csharp_backend_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> logger;
        private readonly ITracer tracer;

        public ValuesController(ILoggerFactory loggerFactory, ITracer tracer)
        {
            logger = loggerFactory?.CreateLogger<ValuesController>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
            this.tracer = tracer;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            using (IScope scope = tracer.BuildSpan("waitingForValues")
                //.WithStartTimestamp(DateTimeOffset.Now)
                .StartActive(finishSpanOnDispose: true))
            {
                //var span = scope.Span;

                logger.LogError("Capturado via logger.");

                //span.Log(DateTimeOffset.Now, "Evento capturado via span.");

                return new string[] { "value1", "value2" };
            }
        }
    }
}

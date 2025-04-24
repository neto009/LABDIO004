using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnValidaBoleto
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("barcode-validate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string resultObject = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(resultObject);

            string barcodeData = data?.barcodeData;

            if (string.IsNullOrEmpty(barcodeData))
            {
                return new BadRequestObjectResult("O campo barcode é obrigatorio!");
            }

            if (barcodeData.Length != 44)
            {
                return new BadRequestObjectResult("O campo barcode deve ter 44 caracteres!");
            }

            string datePart = barcodeData.Substring(3, 8);

            if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dateObj))
            {
                return new BadRequestObjectResult("O campo barcode deve ter 8 caracteres para a data!");
            }

            var resultOkay = new
            {
                valido = true,
                message = "Boleto Valido",
                vencimento = dateObj.ToString("dd/MM/yyyy")
            };
            return new OkObjectResult(resultOkay);
        }
    }
}

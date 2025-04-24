using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BarcodeStandard;

namespace fnGeradorBoletos
{
    public class GeradorCodigoBarras
    {
        private readonly ILogger<GeradorCodigoBarras> _logger;
        private readonly string _serviceBusConnectionString;
        private readonly string _queueName = "geradorboletos";

        public GeradorCodigoBarras(ILogger<GeradorCodigoBarras> logger)
        {
            _logger = logger;
            _serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        }

        [Function("barcode-generate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                string resultObject = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(resultObject);

                string valor = data?.valor;
                string dataVencimento = data?.dataVencimento;

                string barcodeData;

                if (string.IsNullOrEmpty(valor) || string.IsNullOrEmpty(dataVencimento))
                {
                    return new BadRequestObjectResult("Valor e Data de Vencimento são obrigatórios.");
                }

                // Validar formato da Data de vencimento YYYY-MM-DD
                if (!DateTime.TryParse(dataVencimento.ToString(), out DateTime vencimento))
                {
                    return new BadRequestObjectResult("Data de Vencimento inválida.");
                }

                string dateStr = data.ToString("yyyyMMdd");

                //Conversao do valor para centavos e formataçã até 8 digitos
                if (!decimal.TryParse(valor.ToString(), out decimal valorDecimal))
                {
                    return new BadRequestObjectResult("Valor inválido.");
                }

                int valorCentavos = (int)(valorDecimal * 10);
                string valorStr = valorCentavos.ToString("D8");

                string bankCode = "008";
                string baseCode = string.Concat(bankCode, dateStr, valorStr);

                barcodeData = baseCode.Length < 44 ? baseCode.PadRight(44, '0') : baseCode.Substring(0, 44);
                _logger.LogInformation($"Código de barras gerado: {barcodeData}");

                Barcode barcode = new Barcode();
                var skImage = barcode.Encode(BarcodeStandard.Type.Code128, barcodeData);

                using (var encodeDate = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100)) { 
                    var image = encodeDate.ToArray();

                    string base64image = Convert.ToBase64String(image);

                    var result = new
                    {
                        barcode = "12345678",
                        valorOriginal = valorStr,
                        dataVencimento = DateTime.Now.AddDays(5),
                        ImagemBase64 = base64image
                    };

                    await SendFileFallback(result, _serviceBusConnectionString, _queueName);
                    return new OkObjectResult(result);
                }
            }
            catch (Exception ex) 
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task SendFileFallback(object resultObject, string serviceBusConnectionString, string queueName)
        {
            await using var client = new ServiceBusClient(serviceBusConnectionString);

            ServiceBusSender sender = client.CreateSender(queueName);

            string messageBody = JsonConvert.SerializeObject(resultObject);

            await sender.SendMessageAsync(new ServiceBusMessage(messageBody));

            _logger.LogInformation($"Message sent to queue: {queueName}");
        }
    }
}

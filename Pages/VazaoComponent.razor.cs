using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Components;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;

namespace LoRaDashboard.Pages
{
    public class VazaoComponent : ComponentBase, IDisposable
    {
        public int amount1 = 10;

        [Inject]
        private MqttFactory _mqttFactory { get; set; } = default!;
        private IManagedMqttClient _client = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_client == null) {
                MqttNetEventLogger logger = new MqttNetEventLogger("MyCustomLogger");

                logger.LogMessagePublished += (sender, args) =>
                {
                    var output = new StringBuilder();
                    output.AppendLine($">> [{args.LogMessage.Timestamp:O}] [{args.LogMessage.ThreadId}] [{args.LogMessage.Source}] [{args.LogMessage.Level}]: {args.LogMessage.Message}");
                    if (args.LogMessage.Exception != null)
                    {
                        output.AppendLine(args.LogMessage.Exception.ToString());
                    }

                    Console.Write(output);
                };

                _client = _mqttFactory.CreateManagedMqttClient(logger);
                X509Certificate2Collection caChain = new X509Certificate2Collection();
                caChain.ImportFromPem(mosquitto_org); // from https://test.mosquitto.org/ssl/mosquitto.org.crt
                var clientCert = new X509Certificate2(@"caminho-certificad-pfx-versao-doc-MQTTnet", "123456");
                var options = new ManagedMqttClientOptionsBuilder()
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer("test.mosquitto.org", 8884)
                        .WithTlsOptions(new MqttClientTlsOptionsBuilder()
                            .WithTrustChain(caChain)
                            .WithClientCertificates(new List<X509Certificate2>()
                            {
                                clientCert
                            })
                            .UseTls(true)
                            .WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12)
                            .Build())
                    .Build())
                .Build();
                await _client.StartAsync(options);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void UpdateValue(ChangeEventArgs e)
        {
            amount1 = int.Parse(e.Value.ToString());
            StateHasChanged(); // Atualiza a UI
        }

        public async Task SendVazaoData() {
            var applicationMessage = new MqttApplicationMessageBuilder()
              .WithTopic("/UFMG/Pampulha/sprinkler/vazao")
              .WithPayload(amount1.ToString())
              .Build();
            await _client.EnqueueAsync(applicationMessage);
        }
        public void Dispose()
        {
            _client?.Dispose();
        }

        const string mosquitto_org = @"
        -----BEGIN CERTIFICATE-----
        MIIEAzCCAuugAwIBAgIUBY1hlCGvdj4NhBXkZ/uLUZNILAwwDQYJKoZIhvcNAQEL
        BQAwgZAxCzAJBgNVBAYTAkdCMRcwFQYDVQQIDA5Vbml0ZWQgS2luZ2RvbTEOMAwG
        A1UEBwwFRGVyYnkxEjAQBgNVBAoMCU1vc3F1aXR0bzELMAkGA1UECwwCQ0ExFjAU
        BgNVBAMMDW1vc3F1aXR0by5vcmcxHzAdBgkqhkiG9w0BCQEWEHJvZ2VyQGF0Y2hv
        by5vcmcwHhcNMjAwNjA5MTEwNjM5WhcNMzAwNjA3MTEwNjM5WjCBkDELMAkGA1UE
        BhMCR0IxFzAVBgNVBAgMDlVuaXRlZCBLaW5nZG9tMQ4wDAYDVQQHDAVEZXJieTES
        MBAGA1UECgwJTW9zcXVpdHRvMQswCQYDVQQLDAJDQTEWMBQGA1UEAwwNbW9zcXVp
        dHRvLm9yZzEfMB0GCSqGSIb3DQEJARYQcm9nZXJAYXRjaG9vLm9yZzCCASIwDQYJ
        KoZIhvcNAQEBBQADggEPADCCAQoCggEBAME0HKmIzfTOwkKLT3THHe+ObdizamPg
        UZmD64Tf3zJdNeYGYn4CEXbyP6fy3tWc8S2boW6dzrH8SdFf9uo320GJA9B7U1FW
        Te3xda/Lm3JFfaHjkWw7jBwcauQZjpGINHapHRlpiCZsquAthOgxW9SgDgYlGzEA
        s06pkEFiMw+qDfLo/sxFKB6vQlFekMeCymjLCbNwPJyqyhFmPWwio/PDMruBTzPH
        3cioBnrJWKXc3OjXdLGFJOfj7pP0j/dr2LH72eSvv3PQQFl90CZPFhrCUcRHSSxo
        E6yjGOdnz7f6PveLIB574kQORwt8ePn0yidrTC1ictikED3nHYhMUOUCAwEAAaNT
        MFEwHQYDVR0OBBYEFPVV6xBUFPiGKDyo5V3+Hbh4N9YSMB8GA1UdIwQYMBaAFPVV
        6xBUFPiGKDyo5V3+Hbh4N9YSMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEL
        BQADggEBAGa9kS21N70ThM6/Hj9D7mbVxKLBjVWe2TPsGfbl3rEDfZ+OKRZ2j6AC
        6r7jb4TZO3dzF2p6dgbrlU71Y/4K0TdzIjRj3cQ3KSm41JvUQ0hZ/c04iGDg/xWf
        +pp58nfPAYwuerruPNWmlStWAXf0UTqRtg4hQDWBuUFDJTuWuuBvEXudz74eh/wK
        sMwfu1HFvjy5Z0iMDU8PUDepjVolOCue9ashlS4EB5IECdSR2TItnAIiIwimx839
        LdUdRudafMu5T5Xma182OC0/u/xRlEm+tvKGGmfFcN0piqVl8OrSPBgIlb+1IKJE
        m/XriWr/Cq4h/JfB7NTsezVslgkBaoU=
        -----END CERTIFICATE-----
        ";

    }
}
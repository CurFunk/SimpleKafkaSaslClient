using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using Confluent.Kafka;

namespace SimpleKafkaSaslClient
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args).WithParsedAsync<Options>(async (Options opt) =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Console.WriteLine("Kafka: Verbindung wird beendet.");
                    // Bei einer Tastenunterbrechung (z.B. CTRL+C) die Cancellation durchführen
                    eventArgs.Cancel = true; // Verhindert, dass das Programm beendet wird
                    cancellationTokenSource.Cancel();
                };

                var config = new ConsumerConfig
                {
                    BootstrapServers = opt.BootstrapServers, 
                    GroupId = opt.GroupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest, // Offset-Einstellung für den Consumer
                    SecurityProtocol = SecurityProtocol.SaslSsl, // Sicherheitsprotokoll
                    SaslMechanism = SaslMechanism.Plain, // Mechanismus für die Authentifizierung (z.B. Plain, ScramSha256, etc.)
                    SaslUsername = opt.SaslUsername, 
                    SaslPassword = opt.SaslPassword,
                };

                try
                {
                    using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
                    {
                        Console.WriteLine($"Kafka: Es wird das Topic '{opt.Topic}' abonniert.");
                        consumer.Subscribe(opt.Topic);

                        try
                        {
                            while (!cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                var consumeResult = consumer.Consume(cancellationTokenSource.Token); // Daten vom Kafka-Cluster abrufen
                                // Nachricht in JSON umwandeln und in die Konsole ausgeben
                                var jsonOptions = new JsonSerializerOptions
                                {
                                    WriteIndented = true, // Bessere Lesbarkeit durch Einrückung
                                    //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                };
                                JsonDocument jsonDocument = JsonDocument.Parse(consumeResult.Message.Value);
                                JsonElement root = jsonDocument.RootElement;

                                var jsonMessage = JsonSerializer.Serialize(root, jsonOptions);
                                Console.WriteLine($"Kafka: Nachricht erhalten:"); // Nachricht verarbeiten
                                Console.WriteLine($"'{jsonMessage}'"); // Nachricht verarbeiten
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Bei Bedarf aufgefangene Ausnahme
                        }
                        finally
                        {
                            consumer.Close(); // Consumer schließen, wenn er nicht mehr benötigt wird
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Environment.Exit(-1);
                }
            });
            Console.WriteLine("Anwendung wird beendet.");
            return 0;
        }
    }
}

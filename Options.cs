using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleKafkaSaslClient
{
    public class Options
    {
        [Option(Required = true, HelpText = "Adresse und Port des Kafka Brokers.")]
        public string? BootstrapServers { get; set; }

        [Option(Required = true, HelpText = "Gruppen-ID des Consumers.")]
        public string? GroupId { get; set; }

        [Option(Required = true, HelpText = " Benutzername für die SASL-Authentifizierung.")]
        public string? SaslUsername { get; set; }

        [Option(Required = true, HelpText = "Passwort für die SASL-Authentifizierung.")]
        public string? SaslPassword { get; set; }

        [Option(Required = true, HelpText = "Topic, dem der Consumer beitreten soll.")]
        public string? Topic { get; set; }

    }
}

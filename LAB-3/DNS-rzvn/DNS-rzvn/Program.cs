using System;
using System.Net;
using DnsClient;

namespace DNSResolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var lookup = new LookupClient();
            string dnsServer = string.Empty; // DNS server-ul folosit initial va fi cel indicat de sistem

            Console.WriteLine("Introduceti o comanda:\n1. Resolve domain\n2. Resolve ip\n3. Use dns <ip>");

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine();

                if (keyInfo.KeyChar == '1')
                {
                    Console.WriteLine("Introduceti numele de domeniu:");
                    string domain = Console.ReadLine();
                    IPHostEntry entry = lookup.GetHostEntry(domain);
                    Console.WriteLine("Adrese IP asociate domeniului {0}:", domain);
                    foreach (IPAddress ip in entry.AddressList)
                    {
                        Console.WriteLine(ip.ToString()); // Afiseaza adresele IP asociate domeniului
                    }
                }
                else if (keyInfo.KeyChar == '2')
                {
                    Console.WriteLine("Introduceti adresa IP:");
                    string ip = Console.ReadLine();
                    IPAddress ipAddress = IPAddress.Parse(ip);

                    IPHostEntry entry2 = lookup.GetHostEntry(ipAddress);
                    Console.WriteLine("Domenii asociate adresei IP {0}:\n", ipAddress);
                    Console.WriteLine(lookup.GetHostName(ipAddress).ToString()); // Afiseaza numele de domeniu asociat adresei IP
                    foreach (var name in entry2.Aliases)
                    {
                        Console.WriteLine(name.ToString()); // Afiseaza aliasurile domeniului
                    }
                }
                else if (keyInfo.KeyChar == '3')
                {
                    Console.WriteLine("Introduceti adresa IP server DNS:");
                    string dnsIp = Console.ReadLine();

                    if (IPAddress.TryParse(dnsIp, out IPAddress dnsIpAddress))
                    {
                        dnsServer = dnsIp;
                        var endpoint = new IPEndPoint(dnsIpAddress, 53);
                        lookup = new LookupClient(endpoint);
                        Console.WriteLine("DNS server-ul a fost schimbat la {0}", dnsServer); // Afiseaza mesajul de schimbare a DNS server-ului
                    }
                    else
                    {
                        Console.WriteLine("Adresa IP introdusa este invalida.");
                    }
                }
                else
                {
                    Console.WriteLine("Tasta apasata nu corespunde niciunei comenzi valide.");
                }
            }
        }
    }
}

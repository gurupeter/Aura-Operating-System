﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.IPV4.UDP.DNS
{
    public class DNSClient
    {
        // TODO: Once we support more than just IPv4, we really need to base all the IPv4 classes on abstract classes
        // that represent the required functionality, then we can generalize the stack to be independent from IPv4 or IPv6
        internal class DataGram
        {
            internal byte[] data;
            internal IPV4.EndPoint source;

            internal DataGram(byte[] data, IPV4.EndPoint src)
            {
                this.data = data;
                this.source = src;
            }
        }

        private static TempDictionary<DNSClient> clients;

        protected Int32 localPort;
        protected IPV4.Address destination;
        protected Int32 destinationPort;

        private Queue<DataGram> rxBuffer;

        static DNSClient()
        {
            clients = new TempDictionary<DNSClient>();
        }

        internal static DNSClient Client(ushort destPort)
        {
            if (clients.ContainsKey((UInt32)destPort) == true)
            {
                return clients[(UInt32)destPort];
            }

            return null;
        }

        public void Close()
        {
            if (DNSClient.clients.ContainsKey((UInt32)this.localPort))
            {
                DNSClient.clients.Clear();
            }
        }

        public DNSClient()
            : this(0)
        { }

        public DNSClient(Int32 localPort)
        {
            this.rxBuffer = new Queue<DataGram>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                DNSClient.clients.Add((UInt32)localPort, this);
            }
        }

        public DNSClient(IPV4.Address dest, Int32 destPort)
            : this(0)
        {
            this.destination = dest;
            this.destinationPort = destPort;
        }

        public void Connect(IPV4.Address dest, Int32 destPort)
        {
            this.destination = dest;
            this.destinationPort = destPort;
        }

        public DNSPacketAsk askpacket;

        public bool ReceivedResponse;

        public string URL;
        public Address address;

        public void Ask(string dns_name)
        {
            if (!Kernel.DNScache.ContainsKey(dns_name))
            {
                Utils.Settings settings = new Utils.Settings(@"0:\System\resolv.conf");
                Address primary_dns_server = Address.Parse(settings.Get("primary_dns"));
                Address source = Config.FindNetwork(primary_dns_server);

                askpacket = new DNSPacketAsk(source, primary_dns_server, 0x1234, 0x0100, 1, dns_name);

                OutgoingBuffer.AddPacket(askpacket);
                NetworkStack.Update();
            }          
            else
            {
                URL = dns_name;
                address = Kernel.DNScache[dns_name];

                ReceivedResponse = true;
            }
        }

        internal void receiveData(DNSPacketAnswer packet)
        {
            Console.WriteLine();
            EndPoint source = new EndPoint(packet.SourceIP, 53);

            URL = askpacket.Url;
            address = packet.address;

            Kernel.DNScache.Add(URL, address);

            ReceivedResponse = true;
        }
    }
}

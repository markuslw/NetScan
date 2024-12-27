using System;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;


class Program
{
    static void Main()
    {
        // Get the default gateway
        string gatewayIP = GetDefaultGateway();
        if (gatewayIP == "0.0.0.0")
        {
            Console.WriteLine("Default gateway not found.");
            return;
        }
        Console.WriteLine($"Default Gateway: {gatewayIP}");

        List<long> RTTListLAN = LAN(gatewayIP);
        AnalyzeRTT(RTTListLAN);

        //Console.WriteLine("Enter IP: ");
        //string? userInput = Console.ReadLine();
        //GWAN(userInput ?? "0.0.0.0");
    }

    static void AnalyzeRTT(List<long> RTTList)
    {
        if (RTTList.Count > 0)
        {
            // Calculate sum, average, min, max
            long sum = RTTList.Sum();
            double avgRTT = (double)sum / RTTList.Count;
            long minRTT = RTTList.Min();
            long maxRTT = RTTList.Max();

            // Display the results
            Console.WriteLine($"\nFrom {RTTList.Count} tries");
            Console.WriteLine($"\tMax RTT: \t{maxRTT} ms");
            Console.WriteLine($"\tAvg RTT: \t{avgRTT} ms");
            Console.WriteLine($"\tMin RTT: \t{minRTT} ms");

            // Sort the RTT list to find quartile values
            RTTList.Sort();
            Console.WriteLine($"Upper quartile RTT values: {RTTList[^1]} ms, {RTTList[^2]} ms, {RTTList[^3]} ms, {RTTList[^4]} ms, {RTTList[^5]} ms");
            Console.WriteLine($"Lower quartile RTT values: {RTTList[0]} ms, {RTTList[1]} ms, {RTTList[2]} ms, {RTTList[3]} ms, {RTTList[4]} ms");

            // Display all RTT values
            Console.WriteLine("All RTT values: ");
            foreach (long rtt in RTTList)
            {
                Console.Write(rtt + " ");
            }
        }
        else
        {
            Console.WriteLine("No successful pings were recorded.");
        }
    }

    static string GetDefaultGateway()
    {

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ipconfig",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine("Starting ipconfig process...");

        Process? process = Process.Start(psi);
        if (process == null)
        {
            Console.WriteLine("Failed to start the ipconfig process\n Manually write Default Gateway: ");
            string? userInput = Console.ReadLine();

            return userInput ?? "0.0.0.0";
        }

        Console.WriteLine("Process started");

        using (process) // Use the process safely within the using block
        {
            string output = process.StandardOutput.ReadToEnd();
            var match = Regex.Match(output, @"Default Gateway[\s.]*: (\d+\.\d+\.\d+\.\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                Console.WriteLine("Failed to find the default gateway in the output of ipconfig\n Manually write Default Gateway: ");
                string? userInput = Console.ReadLine();

                return userInput ?? "0.0.0.0";
            }
        }
    }

    static List<long> LAN(string gatewayIP)
    {
        List<long> RTTList = new List<long>();
        Ping pingSender = new Ping();

        for (int i = 0; i < 300; i++)
        {
            try
            {
                PingReply reply = pingSender.Send(gatewayIP);
                if (reply.Status == IPStatus.Success)
                {
                    RTTList.Add(reply.RoundtripTime);
                }
                else
                {
                    Console.WriteLine($"Ping failed: {reply.Status}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending ping: " + ex.Message);
            }
        }
        return RTTList;
    }

    static void GWAN(string IP)
    {
        byte[] buffer = new byte[32];
        int i = 1;

        Ping pingSender = new Ping();
        PingOptions options = new PingOptions(1, true);
        PingReply reply = pingSender.Send(IP, 10000, buffer, options);
        Console.WriteLine($"\t{i++} {reply.RoundtripTime} ms {reply.Address.ToString()}");

        while(reply.Address.ToString() != IP)
        {
            if (options.Ttl < 10) {
                options.Ttl += 1;
            }
            reply = pingSender.Send(IP, 5000, buffer, options);
            Console.WriteLine($"\t{i++} {reply.RoundtripTime} ms at {reply.Address.ToString()}");            
        }
    }
}
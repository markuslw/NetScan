using System;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;


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
        Console.WriteLine("Default Gateway: " + gatewayIP);

        // Send pings and collect RTT values
        List<long> RTTList = SendPings(gatewayIP, 50);

        // Calculate and display RTT statistics
        if (RTTList.Count > 0)
        {
            long sum = RTTList.Sum();
            double avgRTT = (double)sum / RTTList.Count;
            long minRTT = RTTList.Min();
            long maxRTT = RTTList.Max();
            Console.WriteLine($"\nFrom {RTTList.Count} tries\n\tMax RTT: \t{maxRTT} ms\n\tAvg RTT: \t{avgRTT} ms\n\tMin RTT: \t{minRTT} ms");
            RTTList.Sort();
            Console.WriteLine($"Upper quartile RTT values: {RTTList[^1]} ms, {RTTList[^2]} ms, {RTTList[^3]} ms, {RTTList[^4]} ms, {RTTList[^5]} ms");
            Console.WriteLine($"Lower quartile RTT values: {RTTList[0]} ms, {RTTList[1]} ms, {RTTList[2]} ms, {RTTList[3]} ms, {RTTList[4]} ms");
            // All RTT values
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

        Process? process = Process.Start(psi);
        if (process == null)
        {
            Console.WriteLine("Failed to start the ipconfig process\n Manually write Default Gateway: ");
            string? userInput = Console.ReadLine();

            return userInput ?? "0.0.0.0";
        }

        using (process) // Use the process safely within the using block
        {
            string output = process.StandardOutput.ReadToEnd();
            var match = Regex.Match(output, @"Default Gateway[\s.]*: (\d+\.\d+\.\d+\.\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }
        return "0.0.0.0";
    }

    static List<long> SendPings(string gatewayIP, int pingCount)
    {
        List<long> RTTList = new List<long>();
        for (int i = 0; i < pingCount; i++)
        {
            Ping pingSender = new Ping();
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
}
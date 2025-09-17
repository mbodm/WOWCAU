using System.Diagnostics;
using WOWCAU.Core.Parts.Domain.Defaults;

var httpClient = new HttpClient();
var domainLogic = new DomainLogic(httpClient);

var title = $"wowcaucmd.exe {domainLogic.GetApplicationVersion()} (by MBODM 09/2025)";
Console.WriteLine();
Console.WriteLine(title);
Console.WriteLine();

await domainLogic.LoadSettingsAsync().ConfigureAwait(false);

var message = "Progressing addons...";
var left = message.Length + 1;
Console.Write(message);
Console.CursorVisible = false;
Console.SetCursorPosition(left, Console.CursorTop);
Console.Write("0%");
var syncRoot = new object();
var progress = new Progress<byte>((b) =>
{
    lock (syncRoot)
    {
        Console.SetCursorPosition(left, Console.CursorTop);
        Console.Write($"{b}%");
    }
});
var stopwatch = new Stopwatch();
stopwatch.Start();
var updatedAddons = await domainLogic.ProcessAddonsAsync(progress).ConfigureAwait(false);
stopwatch.Stop();
Console.SetCursorPosition(left, Console.CursorTop);
Console.WriteLine("100%");
Console.CursorVisible = true;

var roundedSeconds = (uint)Math.Round((double)stopwatch.ElapsedMilliseconds / 1000, MidpointRounding.AwayFromZero);
var addonsTerm = domainLogic.PluralizeWordByCount("addon", updatedAddons);
var secondsTerm = domainLogic.PluralizeWordByCount("second", roundedSeconds);
Console.WriteLine();
Console.WriteLine($"Udpated {updatedAddons} {addonsTerm} after {roundedSeconds} {secondsTerm}");
Console.WriteLine();
Console.WriteLine("Have a nice day.");

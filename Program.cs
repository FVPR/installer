using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

const string Version = "2.0";

var originalColor = Console.ForegroundColor;

#region Methods

void Write(string test, ConsoleColor color)
{
	Console.ForegroundColor = color;
	Console.Write(test);
}

void WriteLine(string test, ConsoleColor color)
{
	Console.ForegroundColor = color;
	Console.WriteLine(test);
}

void Exit(int code = 0)
{
	Console.WriteLine();
	if (code != 0)
		WriteLine("!!!  Installation failed  !!!", ConsoleColor.Red);
	WriteLine("=== Press any key to exit ===", ConsoleColor.DarkGray);
	Console.ForegroundColor = originalColor;
	Console.ReadKey(true);
	Environment.Exit(code);
}

void Try(Action action, string errorMessage)
{
	try
	{
		action();
	}
	catch
	{
		Console.WriteLine(errorMessage);
		Exit(1);
	}
}

bool AwaitYes(string question, bool failOnEscape = true)
{
	// Console.Write($"{question} (Y/n)");
	Write(question, ConsoleColor.Gray);
	Write(" (Y/n)", ConsoleColor.Cyan);

	Await:
	var input = Console.ReadKey(true).Key;

	if (input is ConsoleKey.N or ConsoleKey.Escape)
	{
		Console.WriteLine();
		if (failOnEscape)
			Exit(1);
		return false;
	}

	if (input is not ConsoleKey.Y)
		goto Await;

	Console.WriteLine();
	return true;
}

#endregion

// Console Header
Console.Title = $"FVPR Installer v{Version}";
WriteLine($"""
===================================
|| FVPR Installer || Version {Version} ||
===================================

""", ConsoleColor.DarkYellow);

// Check if "CreatorCompanion" is running
if (System.Diagnostics.Process.GetProcessesByName("CreatorCompanion").Length > 0)
{
	// Tell the user that it's necessary to close "CreatorCompanion" before continuing
	// And ask if they want to close it
	WriteLine("The Creator Companion app is currently running, please close it before continuing.",
		ConsoleColor.Gray);
	AwaitYes("Do you want to close it now?");
	// Close "CreatorCompanion"
	WriteLine("Closing Creator Companion...\n", ConsoleColor.DarkGray);
	while (System.Diagnostics.Process.GetProcessesByName("CreatorCompanion").Length > 0)
	{
		foreach (var process in System.Diagnostics.Process.GetProcessesByName("CreatorCompanion"))
			process.Kill();
		Thread.Sleep(10);
	}
}

// Check if the %localappdata%/VRChatCreatorCompanion/settings.json file exists
var path = Path.Combine(
	Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
	"VRChatCreatorCompanion", "settings.json"
);
if (!File.Exists(path))
{
	// Tell the user that they need to have the "CreatorCompanion" app installed, and need to run it at least once
	WriteLine("The Creator Companion app is not installed, or has not been run at least once!", ConsoleColor.DarkRed);
	Exit(1);
}

// Read the settings.json file
string raw = null!;
JObject settings = null!;
Try(() => raw = File.ReadAllText(path), "Failed to read the settings.json file!");
Try(() => settings = JObject.Parse(raw), "Failed to parse the settings.json file!");

// Check if the "--dev" argument was passed
var useDev = args.Any(x => x == "--dev");
var displayName = useDev ? "FVPR Dev" : "FVPR";
var repo = useDev
	? "https://dev.vpm.foxscore.de/api/v1/index"
	: "https://api.fvpr.dev/index";
var tosUrl = useDev
	? "https://dev.vpm.foxscore.de/tos"
	: "https://fvpr.dev/tos";

// Check if the userRepos array exists
JArray? userRepos = null;
try
{
	userRepos = settings["userRepos"] as JArray;
	if (userRepos is not { Type: JTokenType.Array })
		userRepos = new JArray();
}
catch
{
	// If it doesn't exist, create it
	settings["userRepos"] = new JArray();
}

// Check every userRepo (object) in the array, if the "url" property is equal to the repo url
if (settings["userRepos"]!.Any(x => x["url"]?.ToString() == repo))
{
	// Ask the user if they want to remove the repo
	Write("You're about to ", ConsoleColor.Gray);
	Write("remove", ConsoleColor.Red);
	Write($" the {displayName} repository ", ConsoleColor.Gray);
	WriteLine($"({repo}).", ConsoleColor.DarkGray);
	AwaitYes("Are you sure you want to continue?");
	Console.WriteLine();

	// Remove the repo from the userRepos array
	var index = userRepos!.IndexOf(userRepos.First(x => x["url"]?.ToString() == repo));
	userRepos.RemoveAt(index);
	settings["userRepos"] = userRepos;
	Try(
		() => File.WriteAllText(path, settings.ToString()),
		"Failed to write to the settings.json file!"
	);
	WriteLine($"Successfully removed the {displayName} repository!", ConsoleColor.DarkGreen);
}
else
{
	#region Install repository

	// Ask the user if they want to install the repo
	Write("You're about to ", ConsoleColor.Gray);
	Write("install", ConsoleColor.Green);
	Write($" the {displayName} repository ", ConsoleColor.Gray);
	WriteLine($"({repo}).", ConsoleColor.DarkGray);
	AwaitYes("Do you want to continue?");
	Console.WriteLine();

	// Accept the terms of service
	Write("FVPR Terms of Service: ", ConsoleColor.Gray);
	WriteLine(tosUrl, ConsoleColor.DarkCyan);
	AwaitYes("By pressing Y, you confirm that you have read, understand, and agree to the terms of service");
	Console.WriteLine();

	// Add the repo to the userRepos array
	userRepos!.Add(new JObject
	{
		["url"] = repo,
		["name"] = displayName
	});
	settings["userRepos"] = userRepos;
	Try(
		() => File.WriteAllText(path, settings.ToString()),
		"Failed to write to the settings.json file!"
	);
	WriteLine($"Successfully installed the {displayName} repository!", ConsoleColor.DarkGreen);

	#endregion

	#region Install user-repo fix

	var installationPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Programs", "VRChat Creator Companion"
	);

	if (!File.Exists(Path.Combine(installationPath, "CreatorCompanion.exe")))
	{
		Console.WriteLine();
		WriteLine(
			"It is highly recommended to to install the VccUserRepoFix mod, but it is not possible to do so automatically.",
			ConsoleColor.DarkYellow
		);
		Write("Please install it manually: ", ConsoleColor.Gray);
		WriteLine("https://github.com/foxscore/vcc-user-repo-fix", ConsoleColor.DarkCyan);
		goto AfterInstall;
	}

	// Get the version of the Creator Companion
	var version = FileVersionInfo.GetVersionInfo(Path.Combine(installationPath, "CreatorCompanion.exe"))
		.FileVersion;
	if (version is null) goto AfterInstall;

	// If the version does not start with "2019.4.31", we assume it's the new release of the Creator Companion
	// which should have the issue already fixed
	if (!version.StartsWith("2019.4.31")) goto AfterInstall;

	var modsPath = Path.Combine(installationPath, "Mods");
	var dllPath = Path.Combine(modsPath, "VccUserRepoFix.dll");

	// Check if the VccUserRepoFix.dll file exists, but only on windows
	if (!File.Exists(dllPath) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
	{
		var didInstallMelonLoader = false;

		// Ask the user if they want to install the user-repo fix
		Console.WriteLine();
		WriteLine("The VccUserRepoFix mod is not installed, but highly recommended.", ConsoleColor.Gray);
		WriteLine("Without it, using the Creator Companion with user repositories extremely slow.", ConsoleColor.Gray);
		Write("Source code: ", ConsoleColor.Gray);
		WriteLine("https://github.com/foxscore/vcc-user-repo-fix", ConsoleColor.DarkGray);
		if (!AwaitYes("Do you want to install it?", false))
		{
			WriteLine("Skipping installation of the VccUserRepoFix mod...", ConsoleColor.DarkGray);
			Exit();
		}

		// If MelonLoader is not installed, install it
		if (!Directory.Exists(Path.Combine(installationPath, "MelonLoader")))
		{
			didInstallMelonLoader = true;

			// Download MelonLoader
			WriteLine("Downloading MelonLoader...", ConsoleColor.DarkGray);
			var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				new WebClient().DownloadFile(
					"https://github.com/LavaGang/MelonLoader/releases/latest/download/MelonLoader.x64.zip",
					tempPath
				);
			}
			catch (Exception e)
			{
				WriteLine("Failed to download MelonLoader!", ConsoleColor.DarkRed);
				WriteLine($"Error: {e.Message}", ConsoleColor.DarkRed);
				Exit(1);
			}

			// Extract MelonLoader, overwriting existing files
			WriteLine("Extracting MelonLoader...", ConsoleColor.DarkGray);
			try
			{
				ZipFile.ExtractToDirectory(
					tempPath,
					installationPath,
					true
				);
			}
			catch (Exception e)
			{
				WriteLine("Failed to extract MelonLoader!", ConsoleColor.DarkRed);
				WriteLine($"Error: {e.Message}", ConsoleColor.DarkRed);
				Exit(1);
			}

			// Delete the temporary file (no fail condition)
			try
			{
				File.Delete(tempPath);
			}
			catch
			{
				// ignored
			}
		}

		// Create the Mods folder
		Directory.CreateDirectory(modsPath);

		// Download the VccUserRepoFix mod
		WriteLine("Downloading VccUserRepoFix...", ConsoleColor.DarkGray);
		try
		{
			new WebClient().DownloadFile(
				"https://github.com/foxscore/vcc-user-repo-fix/releases/latest/download/VccUserRepoFix.dll",
				dllPath
			);
		}
		catch (Exception e)
		{
			WriteLine("Failed to download VccUserRepoFix!", ConsoleColor.DarkRed);
			WriteLine($"Error: {e.Message}", ConsoleColor.DarkRed);
			Exit(1);
		}

		// Done
		WriteLine(
			didInstallMelonLoader
				? "Successfully installed MelonLoader and the VccUserRepoFix mod!"
				: "Successfully installed the VccUserRepoFix mod!", ConsoleColor.DarkGreen);

		#endregion
	}
}

// Exit the program
AfterInstall:
Exit();
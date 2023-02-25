using System.ComponentModel;
using Newtonsoft.Json.Linq;

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

void AwaitYes(string question)
{
	// Console.Write($"{question} (Y/n)");
	Write(question, ConsoleColor.Gray);
	Write(" (Y/n)", ConsoleColor.Cyan);	

	Await:
	var input = Console.ReadKey(true).Key;
	if (input is ConsoleKey.N or ConsoleKey.Escape)
	{
		Console.WriteLine();
		Exit(1);
	}
	else if (input is not ConsoleKey.Y)
		goto Await;
	
	Console.WriteLine();
}

#endregion

// Console Header
Console.Title = "FVPR Installer v1.0";
WriteLine("""
===================================
|| FVPR Installer || Version 1.0 ||
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
var repo = useDev
	? "https://dev.vpm.foxscore.de/api/v1/index"
	: "https://vpm.foxscore.de/api/v1/index";
var displayName = useDev ? "FVPR Dev" : "FVPR";

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
	// Ask the user if they want to install the repo
	Write("You're about to ", ConsoleColor.Gray);
	Write("install", ConsoleColor.Green);
	Write($" the {displayName} repository ", ConsoleColor.Gray);
	WriteLine($"({repo}).", ConsoleColor.DarkGray);
	AwaitYes("Do you want to continue?");
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
}

// Exit the program
Exit();
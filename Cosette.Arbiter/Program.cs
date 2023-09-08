using Cosette.Arbiter.Settings;
using Cosette.Arbiter.Tournament;

SettingsLoader.Init("settings.json");
new TournamentArbiter().Run();

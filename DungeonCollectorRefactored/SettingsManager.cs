public class SettingsManager
{
    private string settingsPath;

    public SettingsManager(string settingsPath)
    {
        this.settingsPath = settingsPath;
    }

    public void SaveDifficulty(Difficulty difficulty)
    {
        try
        {
            File.WriteAllText(settingsPath, difficulty.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    public Difficulty LoadDifficulty()
    {
        if (!File.Exists(settingsPath))
            return Difficulty.Normal;

        try
        {
            string text = File.ReadAllText(settingsPath).Trim();

            if (Enum.TryParse(text, out Difficulty difficulty))
                return difficulty;
            else
                return Difficulty.Normal;
        }
        catch
        {
            return Difficulty.Normal;
        }
    }
}
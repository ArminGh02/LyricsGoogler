using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS;

namespace LyricsGoogler;

internal class Config
{
    public readonly HotKey HotKey;

    private static readonly Dictionary<string, HOT_KEY_MODIFIERS> _modifiers = new()
    {
        ["win"] = MOD_WIN,
        ["Win"] = MOD_WIN,
        ["windows"] = MOD_WIN,
        ["Windows"] = MOD_WIN,
        ["shift"] = MOD_SHIFT,
        ["Shift"] = MOD_SHIFT,
        ["alt"] = MOD_ALT,
        ["Alt"] = MOD_ALT,
        ["control"] = MOD_CONTROL,
        ["Control"] = MOD_CONTROL,
        ["ctrl"] = MOD_CONTROL,
        ["Ctrl"] = MOD_CONTROL,
    };

    private static readonly Config _default = new(new HotKey(
        Key: Keys.L,
        Modifiers: MOD_WIN | MOD_SHIFT)
    );

    private Config(HotKey hotKey)
    {
        HotKey = hotKey;
    }

    public static Config Parse(string configFilename)
    {
        string configText;
        try
        {
            configText = File.ReadAllText(configFilename);
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show("Config file not found. Using default hotkey: Win+Shift+L");
            FixConfigFile(configFilename);
            return _default;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance)
            .Build();

        var config = deserializer.Deserialize<ConfigInternal>(configText);
        if (string.IsNullOrEmpty(config.HotKey))
        {
            MessageBox.Show("hotkey is not set in config file. Using default hotkey: Win+Shift+L");
            FixConfigFile(configFilename);
            return _default;
        }

        var parts = config.HotKey.Split('+');
        if (parts.Length < 2)
        {
            MessageBox.Show("hotkey in config file contains less than 2 keys. Using default hotkey: Win+Shift+L");
            FixConfigFile(configFilename);
            return _default;
        }

        var keyPart = parts[^1];
        var modifiersParts = parts[0..^1];

        if (!Enum.TryParse(keyPart, true, out Keys key))
        {
            MessageBox.Show("Invalid hotkey in config file. Using default hotkey: Win+Shift+L");
            FixConfigFile(configFilename);
            return _default;
        }

        HOT_KEY_MODIFIERS modifiers = 0;
        foreach (var modifier in modifiersParts)
        {
            if (!_modifiers.TryGetValue(modifier, out HOT_KEY_MODIFIERS val))
            {
                MessageBox.Show("Invalid hotkey modifier key in config file. Using default hotkey: Win+Shift+L");
                FixConfigFile(configFilename);
                return _default;
            }
            modifiers |= val;
        }

        return new Config(
            new HotKey(
                Key: key,
                Modifiers: modifiers
            )
        );
    }

    private class ConfigInternal
    {
        public string? HotKey { get; set; }
    }

    private static void FixConfigFile(string configFilename)
    {
        const string _config = @"
# The keyboard shortcut used to invoke the script.
# It should be of this format: [ModifierKey+]ModifierKey+KeyName
# Which indicates a plus-seperated list of keys which ends with a regular key
# and contains at least one modifier key before it.
# modifiers can include:
#  win
#  shift
#  ctrl
#  alt
hotkey: win+shift+l
";
        File.WriteAllText(configFilename, _config);
    }
}

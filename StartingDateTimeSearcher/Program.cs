using static System.Console;

var mac = LoadSetting();
if (mac is null) return;
var targets = LoadTargetSeed();
if (targets is null) return;
WriteLine($"Load {targets.Length} seeds");

var frames = SelectFrames();

var version = SelectVersion();
var useParallel = SelectUseParallel();

SearchStartingDateTime.Execute(version, mac, frames, targets.ToHashSet(), useParallel);

// --------------------
// loaders
// --------------------
static uint[]? LoadSetting()
{
    var exists = File.Exists("./setting.txt");
    if (!exists) return NullWithMessage("Missing `setting.txt`");

    var setting = File.ReadAllText("./setting.txt").Replace("\r\n", "\n").Split('\n');
    if (setting.Length == 0) return NullWithMessage("Invalid MAC Address");

    var mac = setting[0].Split(' ').Select(TryParseHex).OfType<uint>().ToArray();
    if (mac.Length != 6) return NullWithMessage("Invalid MAC Address");

    return mac;
}

static uint[]? LoadTargetSeed()
{
    if (!File.Exists("./seedList.txt")) return NullWithMessage("Missing `seedList.txt`");
    var target = File.ReadAllText("./seedList.txt")
        .Replace("\r\n", "\n").Split('\n')
        .Select(TryParseHex)
        .OfType<uint>()
        .Distinct().ToArray();

    if (target.Length == 0) return NullWithMessage("`seedList.txt` is empty");

    return target;
}

static uint SelectFrames()
{
    WriteLine("Select Frames (maybe `6` or `8`)");
    while (true)
    {
        Write("> ");
        var input = TryParseHex(ReadLine()?.Trim().ToLower()!);
        if (input is uint frames) return frames;
    }
}

static Version SelectVersion()
{
    var dict = new Dictionary<string, Version>
    {
        ["b"] = Constants.JPB1,
        ["w"] = Constants.JPW1,
        ["b2"] = Constants.JPB2,
        ["w2"] = Constants.JPW2,
    };

    WriteLine("Select version (`b` or `w` or `b2` or `w2`)");
    while (true)
    {
        Write("> ");
        var input = ReadLine()?.Trim().ToLower();
        if (input is not null && dict.ContainsKey(input)) return dict[input];
    }
}

static bool SelectUseParallel()
{
    WriteLine("Use parallel? (Y/n)");
    while (true)
    {
        Write("> ");
        var input = ReadLine()?.Trim().ToLower();
        if (input is null || input == string.Empty || input == "y") return true;
        if (input == "n") return false;
    }
}

// --------------------
// utils
// --------------------

static uint[]? NullWithMessage(string message)
{
    WriteLine(message);
    return null;
}

static uint? TryParseHex(string hex)
{
    try { return Convert.ToUInt32(hex, 16); }
    catch { return null; }
}

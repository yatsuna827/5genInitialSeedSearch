
record Version(string Label, uint[] NazoValues, uint VCount, uint[] DefaultTimer0Range);

static class Constants
{
    public static readonly Version JPB1 = new(
        "Black JP",
        NazoValues: new[] { 0x2215F10u, 0x221600Cu, 0x221600Cu, 0x2216058u, 0x2216058u }, 
        VCount: 0x60,
        DefaultTimer0Range: new[] { 0xC79u, 0xC7Au }
    );
    public static readonly Version JPW1 = new(
        "White JP",
        NazoValues: new[] { 0x2215f30u, 0x221602Cu, 0x221602Cu, 0x2216078u, 0x2216078u },
        VCount: 0x5F,
        DefaultTimer0Range: new[] { 0xC68u, 0xC69u }
    );
    public static readonly Version JPB2 = new(
        "Black2 JP",
        NazoValues: new uint[] { 0x209A8DC, 0x2039AC9, 0x21FF9B0, 0x21FFA04, 0x21FFA04 }, 
        VCount: 0x82,
        DefaultTimer0Range: new[] { 0x1102u, 0x1103u, 0x1104u, 0x1105u, 0x1106u, 0x1107u, 0x1108u }
    );
    public static readonly Version JPW2 = new(
        "White2 JP",
        NazoValues: new uint[] { 0x209A8FC, 0x2039AF5, 0x21FF9D0, 0x21FFA24, 0x21FFA24 },
        VCount: 0x82,
        DefaultTimer0Range: new[] { 0x10F4u, 0x10F5u, 0x10F6u, 0x10F7u, 0x10F8u, 0x10F9u, 0x10FAu, 0x10FBu }
    );
}

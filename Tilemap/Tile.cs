namespace Pure.Tilemap;

/// <summary>
/// Represents a tile in a tilemap.
/// </summary>
public struct Tile
{
    /// <summary>
    /// Gets or sets the identifier of the tile.
    /// </summary>
    public int ID { get; set; }
    /// <summary>
    /// Gets or sets the tint of the tile.
    /// </summary>
    public uint Tint { get; set; }
    /// <summary>
    /// Gets or sets the amount of 90 degree rotations, wrapping in intervals of 4.
    /// Positive values indicate clockwise rotation,
    /// negative values indicate counter-clockwise rotation.
    /// </summary>
    public sbyte Angle { get; set; }
    /// <summary>
    /// Gets or sets a tuple indicating whether the tile is flipped horizontally or vertically.
    /// </summary>
    public (bool isHorizontal, bool isVertical) Flips { get; set; }

    /// <summary>
    /// Initializes a new tile instance with the specified identifier, 
    /// tint, angle, and flips.
    /// </summary>
    /// <param name="id">The identifier of the tile.</param>
    /// <param name="tint">The tint of the tile (defaults to white).</param>
    /// <param name="angle">The amount of 90 degree rotations, wrapping in intervals of 4.
    /// Positive values indicate clockwise rotation,
    /// negative values indicate counter-clockwise rotation.</param>
    /// <param name="flips">A tuple indicating whether the tile is flipped 
    /// horizontally or vertically.</param>
    public Tile(int id, uint tint = uint.MaxValue, sbyte angle = default,
        (bool isHorizontal, bool isVertical) flips = default)
    {
        ID = id;
        Tint = tint;
        Angle = angle;
        Flips = flips;
    }
    public Tile(byte[] bytes)
    {
        var offset = 0;

        ID = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        Tint = BitConverter.ToUInt32(GetBytesFrom(bytes, 4, ref offset));
        Angle = (sbyte)GetBytesFrom(bytes, 1, ref offset)[0];
        Flips = (
            BitConverter.ToBoolean(GetBytesFrom(bytes, 1, ref offset)),
            BitConverter.ToBoolean(GetBytesFrom(bytes, 1, ref offset)));
    }

    /// <returns>
    /// A bundle tuple containing the identifier, tint, angle and flips of the tile.</returns>
    public (int id, uint tint, sbyte angle, bool isFlippedHorizontally, bool isFlippedVertically)
        ToBundle() =>
        this;
    /// <returns>
    /// A string representation of this tile.".</returns>
    public override string ToString()
    {
        return $"ID({ID}) Tint({Tint}) Angle({Angle}) Flips{Flips}";
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();

        result.AddRange(BitConverter.GetBytes(ID));
        result.AddRange(BitConverter.GetBytes(Tint));
        result.Add((byte)Angle);
        result.AddRange(BitConverter.GetBytes(Flips.isHorizontal));
        result.AddRange(BitConverter.GetBytes(Flips.isVertical));

        return result.ToArray();
    }

    public override int GetHashCode() => base.GetHashCode();
    public override bool Equals(object? obj) => base.Equals(obj);

    /// <summary>
    /// Implicitly converts an identifier to a white, not rotated and not flipped tile.
    /// </summary>
    /// <param name="id">The identifier of the tile.</param>
    public static implicit operator Tile(int id) => new(id);
    /// <summary>
    /// Implicitly converts a bundle tuple of values to a tile with the 
    /// specified identifier, tint, angle, and flips.
    /// </summary>
    /// <param name="bundle">A bundle tuple of values representing the identifier, 
    /// tint, angle, and flips of the tile.</param>
    public static implicit operator Tile(
        (int id, uint tint, sbyte angle, bool isFlippedHorizontally, bool isFlippedVertically)
            bundle)
    {
        var (tile, tint, angle, flipH, flipV) = bundle;
        return new(tile, tint, angle, (flipH, flipV));
    }
    /// <summary>
    /// Implicitly converts a tile to a bundle tuple of values representing its 
    /// identifier, tint, angle, and flips.
    /// </summary>
    /// <param name="tile">The tile to convert.</param>
    public static implicit operator (int id, uint tint, sbyte angle, bool isFlippedHorizontally,
        bool isFlippedVertically)(Tile tile)
    {
        return (
            tile.ID,
            tile.Tint,
            tile.Angle,
            tile.Flips.isHorizontal,
            tile.Flips.isVertical);
    }

    public static bool operator ==(Tile a, Tile b)
    {
        return a.ID == b.ID && a.Tint == b.Tint && a.Angle == b.Angle &&
               a.Flips.isHorizontal == b.Flips.isHorizontal &&
               a.Flips.isVertical == b.Flips.isVertical;
    }
    public static bool operator !=(Tile a, Tile b)
    {
        return a.ID != b.ID || a.Tint != b.Tint || a.Angle != b.Angle ||
               a.Flips.isHorizontal != b.Flips.isHorizontal ||
               a.Flips.isVertical != b.Flips.isVertical;
    }

#region General
    public const int EMPTY = 0,
        FULL = 10,
#endregion

#region Shade
        SHADE_TRANSPARENT = 0,
        SHADE_1 = 1,
        SHADE_2 = 2,
        SHADE_3 = 3,
        SHADE_4 = 4,
        SHADE_5 = 5,
        SHADE_6 = 6,
        SHADE_7 = 7,
        SHADE_8 = 8,
        SHADE_9 = 9,
        SHADE_OPAQUE = 10,
#endregion

#region Pattern
        PATTERN_1 = 11,
        PATTERN_2 = 12,
        PATTERN_3 = 13,
        PATTERN_4 = 14,
        PATTERN_5 = 15,
        PATTERN_6 = 16,
        PATTERN_7 = 17,
        PATTERN_8 = 18,
        PATTERN_9 = 19,
        PATTERN_10 = 20,
        PATTERN_11 = 21,
        PATTERN_12 = 22,
        PATTERN_13 = 23,
        PATTERN_14 = 24,
        PATTERN_15 = 25,
        PATTERN_16 = 26,
        PATTERN_17 = 27,
        PATTERN_18 = 28,
        PATTERN_19 = 29,
        PATTERN_20 = 30,
        PATTERN_21 = 31,
        PATTERN_22 = 32,
        PATTERN_23 = 33,
        PATTERN_24 = 34,
        PATTERN_25 = 35,
        PATTERN_26 = 36,
        PATTERN_27 = 37,
        PATTERN_28 = 38,
        PATTERN_29 = 39,
        PATTERN_30 = 40,
        PATTERN_31 = 41,
        PATTERN_32 = 42,
        PATTERN_33 = 43,
        PATTERN_34 = 44,
        PATTERN_35 = 45,
        PATTERN_36 = 46,
        PATTERN_37 = 47,
        PATTERN_38 = 48,
        PATTERN_39 = 49,
        PATTERN_40 = 50,
        PATTERN_41 = 51,
        PATTERN_42 = 52,
        PATTERN_43 = 53,
        PATTERN_44 = 54,
        PATTERN_45 = 55,
        PATTERN_46 = 56,
        PATTERN_47 = 57,
        PATTERN_48 = 58,
        PATTERN_49 = 59,
        PATTERN_50 = 60,
        PATTERN_51 = 61,
        PATTERN_52 = 62,
        PATTERN_53 = 63,
        PATTERN_54 = 64,
        PATTERN_55 = 65,
        PATTERN_56 = 66,
        PATTERN_57 = 67,
        PATTERN_58 = 68,
        PATTERN_59 = 69,
        PATTERN_60 = 70,
        PATTERN_61 = 71,
        PATTERN_62 = 72,
        PATTERN_63 = 73,
        PATTERN_64 = 74,
        PATTERN_65 = 75,
        PATTERN_66 = 76,
        PATTERN_67 = 77,
#endregion

#region Uppercase
        UPPERCASE_A = 78,
        UPPERCASE_B = 79,
        UPPERCASE_C = 80,
        UPPERCASE_D = 81,
        UPPERCASE_E = 82,
        UPPERCASE_F = 83,
        UPPERCASE_G = 84,
        UPPERCASE_H = 85,
        UPPERCASE_I = 86,
        UPPERCASE_J = 87,
        UPPERCASE_K = 88,
        UPPERCASE_L = 89,
        UPPERCASE_M = 80,
        UPPERCASE_N = 91,
        UPPERCASE_O = 92,
        UPPERCASE_P = 93,
        UPPERCASE_Q = 94,
        UPPERCASE_R = 95,
        UPPERCASE_S = 96,
        UPPERCASE_T = 97,
        UPPERCASE_U = 98,
        UPPERCASE_V = 99,
        UPPERCASE_W = 100,
        UPPERCASE_X = 101,
        UPPERCASE_Y = 102,
        UPPERCASE_Z = 103,
#endregion

#region Lowercase
        LOWERCASE_A = 104,
        LOWERCASE_B = 105,
        LOWERCASE_C = 106,
        LOWERCASE_D = 107,
        LOWERCASE_E = 108,
        LOWERCASE_F = 109,
        LOWERCASE_G = 110,
        LOWERCASE_H = 111,
        LOWERCASE_I = 112,
        LOWERCASE_J = 113,
        LOWERCASE_K = 114,
        LOWERCASE_L = 115,
        LOWERCASE_M = 116,
        LOWERCASE_N = 117,
        LOWERCASE_O = 118,
        LOWERCASE_P = 119,
        LOWERCASE_Q = 120,
        LOWERCASE_R = 121,
        LOWERCASE_S = 122,
        LOWERCASE_T = 123,
        LOWERCASE_U = 124,
        LOWERCASE_V = 125,
        LOWERCASE_W = 126,
        LOWERCASE_X = 127,
        LOWERCASE_Y = 128,
        LOWERCASE_Z = 129,
#endregion

#region Number
        NUMBER_0 = 130,
        NUMBER_1 = 131,
        NUMBER_2 = 132,
        NUMBER_3 = 133,
        NUMBER_4 = 134,
        NUMBER_5 = 135,
        NUMBER_6 = 136,
        NUMBER_7 = 137,
        NUMBER_8 = 138,
        NUMBER_9 = 139,
#endregion

#region Fragments
        FRACTION_ONE_EIGHT = 140,
        FRACTION_ONE_SEVENTH = 141,
        FRACTION_ONE_SIXTH = 142,
        FRACTION_ONE_FIFTH = 143,
        FRACTION_ONE_FOURTH = 144,
        FRACTION_ONE_THIRD = 145,
        FRACTION_THREE_EIGHTS = 146,
        FRACTION_TWO_FIFTHS = 147,
        FRACTION_ONE_HALF = 148,
        FRACTION_THREE_FIFTHS = 149,
        FRACTION_FIVE_EIGHTS = 150,
        FRACTION_TWO_THIRDS = 151,
        FRACTION_THREE_FOURTHS = 152,
        FRACTION_FOUR_FIFTHS = 153,
        FRACTION_FIVE_SIXTHS = 154,
        FRACTION_SEVEN_EIGHTS = 155,
#endregion

#region Subscript
        SUBSCRIPT_0 = 156,
        SUBSCRIPT_1st = 157,
        SUBSCRIPT_2nd = 158,
        SUBSCRIPT_3rd = 159,
        SUBSCRIPT_4th = 160,
        SUBSCRIPT_5th = 161,
        SUBSCRIPT_6th = 162,
        SUBSCRIPT_7th = 163,
        SUBSCRIPT_8th = 164,
        SUBSCRIPT_9th = 165,
        SUBSCRIPT_10th = 166,
        SUBSCRIPT_11th = 167,
        SUBSCRIPT_12th = 168,
#endregion

#region Superscript
        SUPERSCRIPT_0_TH = 169,
        SUPERSCRIPT_1_ST = 170,
        SUPERSCRIPT_2_ND = 171,
        SUPERSCRIPT_3_RD = 172,
        SUPERSCRIPT_4_TH = 173,
        SUPERSCRIPT_5_TH = 174,
        SUPERSCRIPT_6_TH = 175,
        SUPERSCRIPT_7_TH = 176,
        SUPERSCRIPT_8_TH = 177,
        SUPERSCRIPT_9_TH = 178,
        SUPERSCRIPT_10_TH = 179,
        SUPERSCRIPT_11_TH = 180,
        SUPERSCRIPT_12_TH = 181,
#endregion

#region Math
        MATH_PLUS = 182,
        MATH_MINUS = 183,
        MATH_MULTIPLICATION = 184,
        MATH_OVER = 185,
        MATH_DIVISION = 186,
        MATH_PERCENT = 187,
        MATH_EQUAL = 188,
        MATH_NOT_EQUAL = 189,
        MATH_APPROXIMATE = 190,
        MATH_SQUARE_ROOT = 191,
        MATH_FUNCTION = 192,
        MATH_INTEGRAL = 193,
        MATH_SUMMATION = 194,
        MATH_EPSILON = 195,
        MATH_EULER = 196,
        MATH_GOLDEN_RATIO = 197,
        MATH_PI = 198,
        MATH_SILVER_RATIO = 199,
        MATH_INFINITY = 200,
        MATH_MUCH_LESS = 204,
        MATH_MUCH_GREATER = 205,
        MATH_LESS_EQUAL = 206,
        MATH_GREATER_EQUAL = 207,
        MATH_LESS = 208,
        MATH_GREATER = 209,
#endregion

#region Brackets
        BRACKET_ROUND_LEFT = 210,
        BRACKET_ROUND_RIGHT = 211,
        BRACKET_SQUARE_LEFT = 212,
        BRACKET_SQUARE_RIGHT = 213,
        BRACKET_CURLY_LEFT = 214,
        BRACKET_CURLY_RIGHT = 215,
        BRACKET_ANGLE_LEFT = 208,
        BRACKET_ANGLE_RIGHT = 209,
#endregion

#region Geometry
        GEOMETRY_PERPENDICULAR = 216,
        GEOMETRY_PARALLEL = 217,
        GEOMETRY_ANGLE = 218,
        GEOMETRY_ANGLE_RIGHT = 219,
        GEOMETRY_SIMILAR = 220,
        GEOMETRY_DEGREE = 221,
#endregion

#region TextSymbols
        SYMBOL_CELCIUS = 222,
        SYMBOL_FAHRENHEIT = 223,
        SYMBOL_ASTERISK = 224,
        SYMBOL_CARAT = 225,
        SYMBOL_HASH = 226,
        SYMBOL_NUMBER = 227,
        SYMBOL_DOLLAR = 228,
        SYMBOL_EURO = 229,
        SYMBOL_POUND = 230,
        SYMBOL_YEN = 231,
        SYMBOL_CENT = 232,
        SYMBOL_CURRENCY = 233,
        SYMBOL_REGISTERED = 251,
        SYMBOL_COPYRIGHT_AUDIO = 252,
        SYMBOL_COPYRIGHT = 253,
        SYMBOL_TRADE_MARK = 254,
#endregion

#region Punctuation
        PUNCTUATION_TILDE = 220,
        PUNCTUATION_EXCLAMATION_MARK = 234,
        PUNCTUATION_QUESTION_MARK = 235,
        PUNCTUATION_DOT = 236,
        PUNCTUATION_COMMA = 237,
        PUNCTUATION_ELLIPSIS = 238,
        PUNCTUATION_COLON = 239,
        PUNCTUATION_SEMICOLON = 240,
        PUNCTUATION_QUOTATION_MARK = 241,
        PUNCTUATION_APOSTROPHE = 242,
        PUNCTUATION_BACKTICK = 243,
        PUNCTUATION_DASH = 244,
        PUNCTUATION_UNDERSCORE = 245,
        PUNCTUATION_PIPE = 246,
        PUNCTUATION_SLASH = 247,
        PUNCTUATION_BACKSLASH = 248,
        PUNCTUATION_AT = 249,
        PUNCTUATION_AMPERSAND = 250,
#endregion

#region Box
        BOX_DEFAULT_STRAIGHT = 260,
        BOX_DEFAULT_CORNER = 261,
        BOX_DEFAULT_T_SHAPED = 262,
        BOX_DEFAULT_CROSS = 263,
        BOX_GRID_STRAIGHT = 264,
        BOX_GRID_CORNER = 265,
        BOX_GRID_T_SHAPED = 266,
        BOX_GRID_CROSS = 267,
        BOX_PIPE_STRAIGHT = 268,
        BOX_PIPE_CORNER = 269,
        BOX_PIPE_T_SHAPED = 270,
        BOX_PIPE_CROSS = 271,
        BOX_HOLLOW_STRAIGHT = 272,
        BOX_HOLLOW_CORNER = 273,
        BOX_HOLLOW_T_SHAPED = 274,
        BOX_HOLLOW_CROSS = 275,
        BOX_SOLID_STRAIGHT = 276,
        BOX_SOLID_CORNER = 277,
        BOX_SOLID_T_SHAPED = 278,
        BOX_SOLID_CROSS = 279,
        BOX_PIPE_BIG_STRAIGHT = 280,
        BOX_PIPE_BIG_CORNER = 281,
        BOX_PIPE_BIG_T_SHAPED = 282,
        BOX_PIPE_BIG_CROSS = 283,
        BOX_CORNER_ROUND = 285,
#endregion

#region Bar
        BAR_DEFAULT_EDGE = 286,
        BAR_DEFAULT_STRAIGHT = 287,
        BAR_HOLLOW_EDGE = 288,
        BAR_HOLLOW_STRAIGHT = 289,
        BAR_STRIP_EDGE = 290,
        BAR_STRIP_STRAIGHT = 291,
        BAR_GRID_EDGE = 292,
        BAR_GRID_STRAIGHT = 293,
        BAR_BIG_EDGE = 294,
        BAR_BIG_STRAIGHT = 295,
        BAR_HOLLOW_BIG_EDGE = 296,
        BAR_HOLLOW_BIG_STRAIGHT = 297,
        BAR_STRIP_BIG_EDGE = 298,
        BAR_STRIP_BIG_STRAIGHT = 299,
        BAR_GRID_BIG_EDGE = 300,
        BAR_GRID_BIG_STRAIGHT = 301,
        BAR_SPIKE_EDGE = 302,
        BAR_SPIKE_STRAIGHT = 303,
#endregion

#region Arrows
        ARROW = 304,
        ARROW_THIN = 305,
        ARROW_BIG = 306,
        ARROW_HOLLOW = 307,
        ARROW_DIAGONAL = 308,
        ARROW_DIAGONAL_THIN = 309,
        ARROW_DIAGONAL_BIG = 310,
        ARROW_DIAGONAL_HOLLOW = 311,
        ARROW_NO_TAIL = 352,
#endregion

#region Icon
        ICON_WAVE = 220,
        ICON_WAVE_DOUBLE = 190,
        ICON_HOME = 312,
        ICON_SETTINGS = 313,
        ICON_SAVE_LOAD = 314,
        ICON_INFO = 315,
        ICON_WAIT = 316,
        ICON_FILE = 317,
        ICON_FOLDER = 318,
        ICON_DELETE = 319,
        ICON_LOCK = 320,
        ICON_KEY = 321,
        ICON_PIN = 322,
        ICON_MARK = 323,
        ICON_GLOBE = 324,
        ICON_TALK = 325,
        ICON_LETTER = 326,
        ICON_BELL = 327,
        ICON_CALENDAR = 328,
        ICON_CONNECTION_BAD = 329,
        ICON_CONNECTION_GOOD = 330,
        ICON_PERSON = 331,
        ICON_PEOPLE = 332,
        ICON_TROPHY = 333,
        ICON_STAR = 334,
        ICON_STAR_HOLLOW = 335,
        ICON_INPUT_MOUSE = 336,
        ICON_INPUT_KEYBOARD = 337,
        ICON_INPUT_CONTROLLER = 338,
        ICON_TICK = 339,
        ICON_BACK = 340,
        ICON_LOOP = 341,
        ICON_SIZE_REDUCE = 342,
        ICON_SIZE_INCREASE = 343,
        ICON_SORT_LIST = 344,
        ICON_SORT_GRID = 345,
        ICON_BOLT = 346,
        ICON_EYE_OPENED = 347,
        ICON_EYE_CLOSED = 348,
        ICON_MINUS = 349,
        ICON_PLUS = 350,
        ICON_FLOW_PAUSE = 351,
        ICON_FLOW_PLAY = 352,
        ICON_FLOW_SKIP = 353,
        ICON_AUDIO_VOLUME_MUTE = 354,
        ICON_AUDIO_VOLUME_LOW = 355,
        ICON_AUDIO_VOLUME_HIGH = 356,
        ICON_MUSIC_NOTE_QUARTER = 357,
        ICON_MUSIC_NOTE_SEIGHT = 358,
        ICON_MUSIC_NOTES_BEAMED_EIGHT = 359,
        ICON_MUSIC_NOTES_BEAMED_SIXTEENTH = 360,
        ICON_MUSIC_SIGN_FLAT = 361,
        ICON_MUSIC_SIGN_NATURAL = 362,
        ICON_MUSIC_SIGN_SHARP = 363,
#endregion

#region Face
        FACE_SMILING = 364,
        FACE_LAUGHING = 365,
        FACE_SAD = 366,
        FACE_TERRIFIED = 367,
        FACE_ANGRY = 368,
        FACE_EMOTIONLESS = 369,
        FACE_SIGHING = 370,
        FACE_BORED = 370,
        FACE_HAPPY = 371,
        FACE_IN_LOVE = 372,
        FACE_RELIEVED = 373,
        FACE_DISSATISFIED = 374,
        FACE_EGOISTIC = 375,
        FACE_ANNOYED = 376,
        FACE_SURPRISED = 377,
        FACE_SLEEPING = 378,
        FACE_KISSING = 379,
        FACE_AWW = 380,
        FACE_WHOLESOME = 381,
        FACE_CRYING = 382,
        FACE_TANTRUM = 383,
        FACE_INTERESTED = 384,
        FACE_EVIL = 385,
        FACE_WINKING = 386,
        FACE_CONFIDENT = 387,
        FACE_SUSPICIOUS = 388,
        FACE_DISGUISED = 389,
#endregion

#region Game
        GAME_DICE_1 = 390,
        GAME_DICE_2 = 391,
        GAME_DICE_3 = 392,
        GAME_DICE_4 = 393,
        GAME_DICE_5 = 394,
        GAME_DICE_6 = 395,
        GAME_CARD_SPADE = 396,
        GAME_CARD_HEART = 397,
        GAME_CARD_CLUB = 398,
        GAME_CARD_DIAMOND = 399,
        GAME_CARD_SPADE_HOLLOW = 400,
        GAME_CARD_HEART_HOLLOW = 401,
        GAME_CARD_CLUB_HOLLOW = 402,
        GAME_CARD_DIAMOND_HOLLOW = 403,
        GAME_CHESS_PAWN = 404,
        GAME_CHESS_ROOK = 405,
        GAME_CHESS_KNIGHT = 406,
        GAME_CHESS_BISHOP = 407,
        GAME_CHESS_QUEEN = 408,
        GAME_CHESS_KING = 409,
        GAME_CHESS_PAWN_HOLLOW = 410,
        GAME_CHESS_ROOK_HOLLOW = 411,
        GAME_CHESS_KNIGHT_HOLLOW = 412,
        GAME_CHESS_BISHOP_HOLLOW = 413,
        GAME_CHESS_QUEEN_HOLLOW = 414,
        GAME_CHESS_KING_HOLLOW = 415,
#endregion

#region Shape
        SHAPE_SQUARE_SMALL = 416,
        SHAPE_SQUARE = 417,
        SHAPE_SQUARE_BIG = 418,
        SHAPE_SQUARE_SMALL_HOLLOW = 419,
        SHAPE_SQUARE_HOLLOW = 420,
        SHAPE_SQUARE_BIG_HOLLOW = 421,
        SHAPE_CIRCLE_SMALL = 422,
        SHAPE_CIRCLE = 423,
        SHAPE_CIRCLE_BIG = 424,
        SHAPE_CIRCLE_SMALL_HOLLOW = 425,
        SHAPE_CIRCLE_HOLLOW = 426,
        SHAPE_CIRCLE_BIG_HOLLOW = 427,
        SHAPE_TRIANGLE = 428,
        SHAPE_TRIANGLE_BIG = 429,
        SHAPE_TRIANGLE_HOLLOW = 430,
        SHAPE_TRIANGLE_BIG_HOLLOW = 431,
        SHAPE_LINE = 432,
#endregion

#region Cursor
        CURSOR_ARROW = 442,
        CURSOR_ARROW_WAIT = 443,
        CURSOR_WAIT = 444,
        CURSOR_TEXT = 445,
        CURSOR_HAND = 446,
        CURSOR_RESIZE = 447,
        CURSOR_RESIZE_DIAGONAL = 448,
        CURSOR_MOVE = 459,
        CURSOR_CROSSHAIR = 450,
        CURSOR_HELP = 451,
        CURSOR_DISABLE = 452;
#endregion

#region Backend
    internal const int BYTE_SIZE = 14;

    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}
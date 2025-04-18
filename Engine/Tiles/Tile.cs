global using TileBundle = (ushort id, uint tint, byte pose);

namespace Pure.Engine.Tiles;

public enum Pose : byte
{
    Default = 0, Right = 1, Down = 2, Left = 3, Flip = 4, FlipRight = 5, FlipDown = 6, FlipLeft = 7
}

public struct Tile(ushort id, uint tint = uint.MaxValue, Pose pose = Pose.Default)
{
    public ushort Id { get; set; } = id;
    public uint Tint { get; set; } = tint;
    public Pose Pose { get; set; } = pose;

    public Tile(TileBundle bundle) : this(bundle.id, bundle.tint, (Pose)bundle.pose)
    {
    }

    public Tile Rotate(int times)
    {
        var angle = (int)Pose;

        if (angle > 3)
            angle -= 4;

        angle = (angle + times) % 4;

        if ((int)Pose > 3)
            angle += 4;

        return new(Id, Tint, (Pose)angle);
    }
    public Tile FlipHorizontally()
    {
        switch (Pose)
        {
            default:
            case Pose.Default: return new(Id, Tint, Pose.Flip);
            case Pose.Right: return new(Id, Tint, Pose.FlipLeft);
            case Pose.Down: return new(Id, Tint, Pose.FlipDown);
            case Pose.Left: return new(Id, Tint, Pose.FlipRight);
            case Pose.Flip: return new(Id, Tint);
            case Pose.FlipRight: return new(Id, Tint, Pose.Left);
            case Pose.FlipDown: return new(Id, Tint, Pose.Down);
            case Pose.FlipLeft: return new(Id, Tint, Pose.Right);
        }
    }
    public Tile FlipVertically()
    {
        switch (Pose)
        {
            default:
            case Pose.Default: return new(Id, Tint, Pose.FlipDown);
            case Pose.Right: return new(Id, Tint, Pose.FlipRight);
            case Pose.Down: return new(Id, Tint, Pose.Flip);
            case Pose.Left: return new(Id, Tint, Pose.FlipLeft);
            case Pose.Flip: return new(Id, Tint, Pose.Down);
            case Pose.FlipRight: return new(Id, Tint, Pose.Right);
            case Pose.FlipDown: return new(Id, Tint);
            case Pose.FlipLeft: return new(Id, Tint, Pose.Left);
        }
    }

    public TileBundle ToBundle()
    {
        return (Id, Tint, (byte)Pose);
    }
    public override string ToString()
    {
        return $"Id({Id}) Tint({Tint}) Pose({Pose})";
    }
    public override bool Equals(object? obj)
    {
        return obj is Tile other && this == other;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Tint, Pose);
    }

    public static implicit operator ushort(Tile tile)
    {
        return tile.Id;
    }
    public static implicit operator Tile(ushort id)
    {
        return new(id);
    }
    public static implicit operator Tile(TileBundle bundle)
    {
        return new(bundle);
    }
    public static implicit operator TileBundle(Tile tile)
    {
        return tile.ToBundle();
    }
    public static bool operator ==(Tile a, Tile b)
    {
        return a.Id == b.Id && a.Tint == b.Tint && a.Pose == b.Pose;
    }
    public static bool operator !=(Tile a, Tile b)
    {
        return a.Id != b.Id || a.Tint != b.Tint || a.Pose != b.Pose;
    }

#region General
    public const ushort EMPTY = 0,
        FULL = 10,
#endregion

#region Shades
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

#region Patterns
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

#region Uppercase Letters
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

#region Lowercase Letters
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

#region Numbers
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

#region Subscripts
        SUBSCRIPT_0_TH = 156,
        SUBSCRIPT_1_ST = 157,
        SUBSCRIPT_2_ND = 158,
        SUBSCRIPT_3_RD = 159,
        SUBSCRIPT_4_TH = 160,
        SUBSCRIPT_5_TH = 161,
        SUBSCRIPT_6_TH = 162,
        SUBSCRIPT_7_TH = 163,
        SUBSCRIPT_8_TH = 164,
        SUBSCRIPT_9_TH = 165,
        SUBSCRIPT_10_TH = 166,
        SUBSCRIPT_11_TH = 167,
        SUBSCRIPT_12_TH = 168,
#endregion

#region Superscripts
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

#region Punctuations
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

#region Pipes
        PIPE_SOLID_STRAIGHT = 260,
        PIPE_SOLID_CORNER = 261,
        PIPE_SOLID_T_SHAPED = 262,
        PIPE_SOLID_CROSS = 263,
        PIPE_GRID_STRAIGHT = 264,
        PIPE_GRID_CORNER = 265,
        PIPE_GRID_T_SHAPED = 266,
        PIPE_GRID_CROSS = 267,
        PIPE_STRAIGHT = 268,
        PIPE_CORNER = 269,
        PIPE_T_SHAPED = 270,
        PIPE_CROSS = 271,
        PIPE_HOLLOW_STRAIGHT = 272,
        PIPE_HOLLOW_CORNER = 273,
        PIPE_HOLLOW_T_SHAPED = 274,
        PIPE_HOLLOW_CROSS = 275,
        PIPE_SOLID_BIG_STRAIGHT = 276,
        PIPE_SOLID_BIG_CORNER = 277,
        PIPE_SOLID_BIG_T_SHAPED = 278,
        PIPE_SOLID_BIG_CROSS = 279,
        PIPE_BIG_STRAIGHT = 280,
        PIPE_BIG_CORNER = 281,
        PIPE_BIG_T_SHAPED = 282,
        PIPE_BIG_CROSS = 283,
#endregion

#region Bars
        BAR_EDGE = 286,
        BAR_STRAIGHT = 287,
        BAR_SHADOW_EDGE = 288,
        BAR_SHADOW_STRAIGHT = 289,
        BAR_HOLLOW_EDGE = 290,
        BAR_HOLLOW_STRAIGHT = 291,
        BAR_STRIP_EDGE = 292,
        BAR_STRIP_STRAIGHT = 293,
        BAR_GRID_EDGE = 294,
        BAR_GRID_STRAIGHT = 295,
        BAR_BIG_EDGE = 296,
        BAR_BIG_STRAIGHT = 297,
        BAR_SHADOW_BIG_EDGE = 298,
        BAR_SHADOW_BIG_STRAIGHT = 299,
        BAR_HOLLOW_BIG_EDGE = 300,
        BAR_HOLLOW_BIG_STRAIGHT = 301,
        BAR_DASH_BIG_EDGE = 302,
        BAR_DASH_BIG_STRAIGHT = 303,
        BAR_GRID_BIG_EDGE = 304,
        BAR_GRID_BIG_STRAIGHT = 305,
        BAR_SQUARE_EDGE = 306,
        BAR_SQUARE_STRAIGHT = 307,
        BAR_DOT_EDGE = 308,
        BAR_DOT_STRAIGHT = 309,
        BAR_SPIKE_EDGE = 310,
        BAR_SPIKE_STRAIGHT = 311,
#endregion

#region Boxes
        BOX_CORNER = 312,
        BOX_EDGE = 313,
        BOX_SHADOW_CORNER = 314,
        BOX_SHADOW_EDGE = 315,
        BOX_OUTLINE_CORNER = 316,
        BOX_OUTLINE_EDGE = 317,
        BOX_OUTLINE_DASH_CORNER = 318,
        BOX_OUTLINE_DASH_EDGE = 319,
        BOX_OUTLINE_GRID_CORNER = 320,
        BOX_OUTLINE_GRID_EDGE = 321,
        BOX_BIG_CORNER = 322,
        BOX_BIG_EDGE = 323,
        BOX_SHADOW_BIG_CORNER = 324,
        BOX_SHADOW_BIG_EDGE = 325,
        BOX_OUTLINE_BIG_CORNER = 326,
        BOX_OUTLINE_BIG_EDGE = 327,
        BOX_OUTLINE_DASH_BIG_CORNER = 328,
        BOX_OUTLINE_DASH_BIG_EDGE = 329,
        BOX_OUTLINE_GRID_BIG_CORNER = 330,
        BOX_OUTLINE_GRID_BIG_EDGE = 331,
        BOX_OUTLINE_SQUARE_CORNER = 332,
        BOX_OUTLINE_SQUARE_EDGE = 333,
        BOX_RAIL_CORNER = 334,
        BOX_RAIL_EDGE = 335,
        BOX_BUBBLE_CORNER = 336,
        BOX_BUBBLE_EDGE = 337,
#endregion

#region Arrows
        ARROW = 520,
        ARROW_THIN = 521,
        ARROW_THICK = 522,
        ARROW_HOLLOW = 523,
        ARROW_DIAGONAL = 524,
        ARROW_DIAGONAL_THIN = 525,
        ARROW_DIAGONAL_THICK = 526,
        ARROW_DIAGONAL_HOLLOW = 527,
        ARROW_TAILLESS_SMALL = 528,
        ARROW_TAILLESS_ROUND_HOLLOW = 529,
        ARROW_TAILLESS_HOLLOW = 530,
        ARROW_TAILLESS_DIAGONAL_HOLLOW = 531,
        ARROW_TAILLESS_ROUND = 532,
        ARROW_TAILLESS = 533,
        ARROW_TAILLESS_DIAGONAL = 534,
#endregion

#region Icons
        ICON_WAVE = 220,
        ICON_WAVES = 190,
        ICON_HOME = 338,
        ICON_HOUSE = 338,
        ICON_SETTINGS = 339,
        ICON_SAVE_LOAD = 340,
        ICON_INFO = 341,
        ICON_WAIT = 342,
        ICON_HOURGLASS = 342,
        ICON_FILE = 343,
        ICON_FOLDER = 344,
        ICON_TRASH = 345,
        ICON_DELETE = 345,
        ICON_REMOVE = 345,
        ICON_LOCK = 346,
        ICON_KEY = 347,
        ICON_PIN = 348,
        ICON_MARK = 349,
        ICON_GLOBE = 350,
        ICON_TALK = 351,
        ICON_LETTER = 352,
        ICON_BELL = 353,
        ICON_CALENDAR = 354,
        ICON_CONNECTION_BAD = 355,
        ICON_CONNECTION_GOOD = 356,
        ICON_PERSON = 357,
        ICON_PEOPLE = 358,
        ICON_TROPHY = 359,
        ICON_STAR = 360,
        ICON_STAR_HOLLOW = 361,
        ICON_EYE_OPENED = 362,
        ICON_EYE_CLOSED = 363,
        ICON_SKY_STAR = 364,
        ICON_SKY_SUN = 365,
        ICON_SKY_MOON_HOLLOW = 366,
        ICON_SKY_MOON = 367,
        ICON_SKY_STARS = 368,
        ICON_SKY_SNOW = 368,
        ICON_GRID = 369,
        ICON_TILES = 369,
        ICON_SHUT_DOWN = 370,
        ICON_BOOK = 371,
        ICON_RAIN = 372,
        ICON_CLOUD_RAIN = 372,
        ICON_CLOUD = 373,
        ICON_FLAG_HOLLOW = 374,
        ICON_FLAG = 375,
        ICON_PICK = 376,
        ICON_CAMERA_MOVIE = 377,
        ICON_CAMERA_PORTABLE = 378,
        ICON_MICROPHONE = 379,
        ICON_DOOR = 380,
        ICON_PEN = 381,
        ICON_PENCIL = 381,
        ICON_BOOKMARK = 382,
        ICON_BANNER = 382,
        ICON_FLAG_VERTICAL = 382,
        ICON_BOOKMARK_HOLLOW = 383,
        ICON_BANNER_HOLLOW = 383,
        ICON_FLAG_VERTICAL_HOLLOW = 383,
        ICON_FILTER = 384,
        ICON_MAGNIFIER = 385,
        ICON_ZOOM = 385,
        ICON_FIND = 385,
        ICON_STACK_1 = 386,
        ICON_STACK_2 = 387,
        ICON_DUPLICATE_1 = 386,
        ICON_DUPLICATE_2 = 387,
        ICON_COPY_1 = 386,
        ICON_COPY_2 = 387,
        ICON_LOADING_1 = 388,
        ICON_LOADING_2 = 389,
        ICON_PICTURE = 390,
        ICON_ZAP = 391,
        ICON_LIGHTNING = 391,
        ICON_INPUT_MOUSE = 392,
        ICON_INPUT_KEYBOARD = 393,
        ICON_INPUT_CONTROLLER = 394,
        ICON_TICK = 395,
        ICON_CHECK = 395,
        ICON_OKAY = 395,
        ICON_YES = 395,
        ICON_ON = 395,
        ICON_X = 396,
        ICON_CANCEL = 396,
        ICON_NO = 396,
        ICON_OFF = 396,
        ICON_PLUS = 397,
        ICON_MINUS = 398,
        ICON_BACK = 399,
        ICON_LOOP = 400,
        ICON_ROTATE = 400,
        ICON_SIZE_REDUCE = 401,
        ICON_SIZE_INCREASE = 402,
        ICON_SORT_LIST = 403,
        ICON_SORT_GRID = 404,
        ICON_MIRROR = 411,
        ICON_FLIP = 412,
        ICON_FILL = 413,
        ICON_BUCKET = 413,
        ICON_PALETTE = 414,
        ICON_COLOR = 414,
        ICON_MOVE = 553,
#endregion

#region Align
        ALIGN_HORIZONTAL_LEFT = 405,
        ALIGN_HORIZONTAL_MIDDLE = 406,
        ALIGN_HORIZONTAL_RIGHT = 407,
        ALIGN_VERTICAL_TOP = 408,
        ALIGN_VERTICAL_MIDDLE = 409,
        ALIGN_VERTICAL_BOTTOM = 410,
#endregion

#region Execution
        FLOW_PREVIOUS = 416,
        FLOW_BACKTRACK = 417,
        FLOW_BACK = 418,
        FLOW_FORWARD = 419,
        FLOW_PLAY = 419,
        FLOW_SKIP = 420,
        FLOW_NEXT = 421,
        FLOW_PAUSE = 422,
        FLOW_RECORD = 423,
        FLOW_STOP = 424,
#endregion

#region Audio
        AUDIO_VOLUME_MUTE = 425,
        AUDIO_VOLUME_LOW = 426,
        AUDIO_VOLUME_HIGH = 427,
        AUDIO_NOTE_QUARTER = 428,
        AUDIO_NOTE_SEIGHT = 429,
        AUDIO_NOTES_BEAMED_EIGHT = 430,
        AUDIO_NOTES_BEAMED_SIXTEENTH = 431,
        AUDIO_MUSIC_SIGN_FLAT = 432,
        AUDIO_MUSIC_SIGN_NATURAL = 433,
        AUDIO_MUSIC_SIGN_SHARP = 434,
#endregion

#region Nature
        NATURE_MOUNTAIN = 442,
        NATURE_WATER = 443,
        NATURE_WIND = 444,
        NATURE_AIR = 444,
        NATURE_TREE_DECIDUOUS = 445,
        NATURE_TREE_CONIFEROUS = 446,
        NATURE_FLOWER = 447,
        NATURE_FISH = 448,
        NATURE_ANIMAL = 449,
#endregion

#region Faces
        FACE_SMILING = 624,
        FACE_LAUGHING = 625,
        FACE_SAD = 626,
        FACE_TERRIFIED = 627,
        FACE_ANGRY = 628,
        FACE_EMOTIONLESS = 629,
        FACE_SIGHING = 630,
        FACE_BORED = 630,
        FACE_HAPPY = 631,
        FACE_IN_LOVE = 632,
        FACE_RELIEVED = 633,
        FACE_DISSATISFIED = 634,
        FACE_EGOISTIC = 635,
        FACE_ANNOYED = 636,
        FACE_SURPRISED = 637,
        FACE_SLEEPING = 638,
        FACE_KISSING = 639,
        FACE_AWW = 640,
        FACE_WHOLESOME = 641,
        FACE_CRYING = 642,
        FACE_TANTRUM = 643,
        FACE_INTERESTED = 644,
        FACE_EVIL = 645,
        FACE_WINKING = 646,
        FACE_CONFIDENT = 647,
        FACE_SUSPICIOUS = 648,
        FACE_DISGUISED = 649,
#endregion

#region Games
        GAME_DICE_1 = 650,
        GAME_DICE_2 = 651,
        GAME_DICE_3 = 652,
        GAME_DICE_4 = 653,
        GAME_DICE_5 = 654,
        GAME_DICE_6 = 655,
        GAME_CARD_SPADE = 656,
        GAME_CARD_HEART = 657,
        GAME_CARD_CLUB = 658,
        GAME_CARD_DIAMOND = 659,
        GAME_CARD_SPADE_HOLLOW = 660,
        GAME_CARD_HEART_HOLLOW = 661,
        GAME_CARD_CLUB_HOLLOW = 662,
        GAME_CARD_DIAMOND_HOLLOW = 663,
        GAME_CHESS_PAWN = 664,
        GAME_CHESS_ROOK = 665,
        GAME_CHESS_KNIGHT = 666,
        GAME_CHESS_BISHOP = 667,
        GAME_CHESS_QUEEN = 668,
        GAME_CHESS_KING = 669,
        GAME_CHESS_PAWN_HOLLOW = 670,
        GAME_CHESS_ROOK_HOLLOW = 671,
        GAME_CHESS_KNIGHT_HOLLOW = 672,
        GAME_CHESS_BISHOP_HOLLOW = 673,
        GAME_CHESS_QUEEN_HOLLOW = 674,
        GAME_CHESS_KING_HOLLOW = 675,
#endregion

#region Shapes
        SHAPE_SQUARE_SMALL = 598,
        SHAPE_SQUARE = 599,
        SHAPE_SQUARE_BIG = 600,
        SHAPE_SQUARE_SMALL_HOLLOW = 601,
        SHAPE_SQUARE_HOLLOW = 602,
        SHAPE_SQUARE_BIG_HOLLOW = 603,
        SHAPE_CIRCLE_SMALL = 604,
        SHAPE_CIRCLE = 605,
        SHAPE_CIRCLE_BIG = 606,
        SHAPE_CIRCLE_SMALL_HOLLOW = 607,
        SHAPE_CIRCLE_HOLLOW = 608,
        SHAPE_CIRCLE_BIG_HOLLOW = 609,
        SHAPE_TRIANGLE = 610,
        SHAPE_TRIANGLE_BIG = 611,
        SHAPE_TRIANGLE_HOLLOW = 612,
        SHAPE_TRIANGLE_BIG_HOLLOW = 613,
        SHAPE_LINE = 614,
#endregion

#region Time
        TIME_1 = 572,
        TIME_2 = 573,
        TIME_3 = 574,
        TIME_4 = 575,
        TIME_5 = 576,
        TIME_6 = 577,
        TIME_7 = 578,
        TIME_8 = 579,
        TIME_CLOCK_0 = 572,
        TIME_CLOCK_12 = 572,
        TIME_CLOCK_1_2 = 573,
        TIME_CLOCK_3 = 574,
        TIME_CLOCK_4_5 = 575,
        TIME_CLOCK_6 = 576,
        TIME_CLOCK_7_8 = 577,
        TIME_CLOCK_9 = 578,
        TIME_CLOCK_10_11 = 579,
#endregion

#region Disabled
        DISABLED_1 = 581,
        DISABLED_2 = 582,
#endregion

#region Cursors
        CURSOR_ARROW = 546,
        CURSOR_ARROW_WAIT = 547,
        CURSOR_WAIT = 548,
        CURSOR_TEXT = 549,
        CURSOR_HAND = 550,
        CURSOR_RESIZE = 551,
        CURSOR_RESIZE_DIAGONAL = 552,
        CURSOR_MOVE = 553,
        CURSOR_CROSSHAIR = 554,
        CURSOR_HELP = 555,
        CURSOR_DISABLE = 556,
        CURSOR_SPLIT_VERTICAL = 557,
        CURSOR_SPLIT_HORIZONTAL = 558;
#endregion
}
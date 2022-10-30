namespace Purity.Graphics
{
	public struct Cell
	{
		#region Shade
		public static Cell Transparent => 0;
		public static Cell Shade1 => 1;
		public static Cell Shade2 => 2;
		public static Cell Shade3 => 3;
		public static Cell Shade4 => 4;
		public static Cell Shade5 => 5;
		public static Cell Shade6 => 6;
		public static Cell Shade7 => 7;
		public static Cell Shade8 => 8;
		public static Cell Shade9 => 9;
		public static Cell Shade10 => 10;
		public static Cell Opaque => 11;
		#endregion
		#region Letter
		public static Cell A => 26;
		public static Cell B => 27;
		public static Cell C => 28;
		public static Cell D => 29;
		public static Cell E => 30;
		public static Cell F => 31;
		public static Cell G => 32;
		public static Cell H => 33;
		public static Cell I => 34;
		public static Cell J => 35;
		public static Cell K => 36;
		public static Cell L => 37;
		public static Cell M => 38;
		public static Cell N => 39;
		public static Cell O => 40;
		public static Cell P => 41;
		public static Cell Q => 42;
		public static Cell R => 43;
		public static Cell S => 44;
		public static Cell T => 45;
		public static Cell U => 46;
		public static Cell V => 47;
		public static Cell W => 48;
		public static Cell X => 49;
		public static Cell Y => 50;
		public static Cell Z => 51;
		#endregion
		#region letter
#pragma warning disable IDE1006 // Naming Styles
		public static Cell a => 52;
		public static Cell b => 53;
		public static Cell c => 54;
		public static Cell d => 55;
		public static Cell e => 56;
		public static Cell f => 57;
		public static Cell g => 58;
		public static Cell h => 59;
		public static Cell i => 60;
		public static Cell j => 61;
		public static Cell k => 62;
		public static Cell l => 63;
		public static Cell m => 64;
		public static Cell n => 65;
		public static Cell o => 66;
		public static Cell p => 67;
		public static Cell q => 68;
		public static Cell r => 69;
		public static Cell s => 70;
		public static Cell t => 71;
		public static Cell u => 72;
		public static Cell v => 73;
		public static Cell w => 74;
		public static Cell x => 75;
		public static Cell y => 76;
		public static Cell z => 77;
#pragma warning restore IDE1006 // Naming Styles
		#endregion
		#region Number
		public static Cell Zero => 78;
		public static Cell One => 79;
		public static Cell Two => 80;
		public static Cell Three => 81;
		public static Cell Four => 82;
		public static Cell Five => 83;
		public static Cell Six => 84;
		public static Cell Seven => 85;
		public static Cell Eight => 86;
		public static Cell Nine => 87;
		#endregion
		#region number
#pragma warning disable IDE1006 // Naming Styles
		public static Cell zero => 88;
		public static Cell one => 89;
		public static Cell two => 90;
		public static Cell three => 91;
		public static Cell four => 92;
		public static Cell five => 93;
		public static Cell six => 94;
		public static Cell seven => 95;
		public static Cell eight => 96;
		public static Cell nine => 97;
#pragma warning restore IDE1006 // Naming Styles
		#endregion
		#region Constants
		public static Cell OneFourth => 98;
		public static Cell Half => 99;
		public static Cell ThreeFourths => 100;
		public static Cell Pi => 101;
		public static Cell GoldenRatio => 102;
		public static Cell Infinity => 103;
		#endregion
		#region Math
		public static Cell Plus => 104;
		public static Cell Minus => 105;
		public static Cell Asterisk => 106;
		public static Cell Division => 107;
		public static Cell Percent => 108;
		public static Cell Equal => 109;
		public static Cell NotEqual => 110;
		public static Cell Approximate => 111;
		public static Cell LessThan => 112;
		public static Cell GreaterThan => 113;
		public static Cell LessThanOrEqualTo => 114;
		public static Cell GreaterThanOrEqualTo => 115;
		#endregion
		#region Brackets
		public static Cell ParenthesisLeft => 116;
		public static Cell ParenthesisRight => 117;
		public static Cell BracketLeft => 118;
		public static Cell BracketRight => 119;
		public static Cell BraceLeft => 120;
		public static Cell BraceRight => 121;
		public static Cell ChevronLeft => 112;
		public static Cell ChevronRight => 113;
		#endregion
		#region MathSymbols
		public static Cell Carat => 122;
		public static Cell SquareRoot => 123;
		public static Cell Similar => 124;
		public static Cell Perpendicular => 125;
		public static Cell Parallel => 126;
		public static Cell Angle => 127;
		public static Cell AngleRight => 128;
		#endregion
		#region TextSymbols
		public static Cell Hash => 130;
		public static Cell Number => 131;
		public static Cell Celcius => 132;
		public static Cell Fahrenheit => 133;
		public static Cell Degree => 134;
		public static Cell ExclamationMark => 135;
		public static Cell QuestionMark => 136;
		public static Cell Dot => 137;
		public static Cell Comma => 138;
		public static Cell Ellipsis => 139;
		public static Cell Colon => 140;
		public static Cell Semicolon => 141;
		public static Cell QuotationMark => 142;
		public static Cell Apostrophe => 143;
		public static Cell Backtick => 144;
		public static Cell Pipe => 146;
		public static Cell Slash => 147;
		public static Cell Backslash => 148;
		public static Cell At => 149;
		public static Cell Ampersand => 150;
		#endregion

		public uint Value { get; set; }

		public Cell(uint value)
		{
			Value = value;
		}

		public static implicit operator Cell(uint value)
		{
			return new Cell(value);
		}
		public static implicit operator uint(Cell cell)
		{
			return cell.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}

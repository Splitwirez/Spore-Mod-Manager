using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using SporeMods.Core.ArgScript.Util;

namespace SporeMods.Core.ArgScript
{
	/**
	 * Originally ArgScriptWriter from SMFX.
	 * Credit to Eric Mor
	 */
    public class Writer
    {
	    private readonly StringBuilder _sb = new StringBuilder();
		private int _indentationLevel = 0;
		
		private bool _firstArgument = true;


		public override string ToString()
		{
			return _sb.ToString();
		}
		
		public void Write(string outputFile)
		{
			var writer = new StreamWriter(outputFile);
			using (writer)
			{
				writer.WriteLine(_sb);
			}
		}
		
		public Writer StartBlock() {
			_indentationLevel++;
			return this;
		}
		
		public Writer EndBlock() {
			_indentationLevel--;
			return this;
		}
		
		public Writer BlankLine() {
			_sb.Append('\n');
			_firstArgument = true;
			return this;
		}
		
		public Writer TabulatedText(string text, bool newLine) {
			string[] lines = text.Split('\n');
			foreach (string line in lines) {
				if (_sb.Length != 0 && newLine) {
					_sb.Append('\n');
				}
				if (newLine) {
					for (int i = 0; i < _indentationLevel; i++) {
						_sb.Append('\t');
					}
				}
				newLine = true;
				_sb.Append(line);
			}
			
			return this;
		}
		
		public Writer Command(string name) {
			if (_sb.Length != 0) {
				_sb.Append('\n');
			}
			for (int i = 0; i < _indentationLevel; i++) {
				_sb.Append('\t');
			}
			_sb.Append(name);
			_firstArgument = false;
			return this;
		}
		
		public Writer CommandEnd() {
			return Command("end");
		}
		
		public Writer Newline() {
			_sb.Append('\n');
			_firstArgument = true;
			return this;
		}
		
		public Writer IndentNewline() {
			_sb.Append('\n');
			for (int i = 0; i < _indentationLevel; i++) {
				_sb.Append('\t');
			}
			_firstArgument = true;
			return this;
		}
		
		public Writer Option(string name) {
			if (!_firstArgument) {
				_sb.Append(' ');
			}
			_sb.Append('-');
			_sb.Append(name);
			_firstArgument = false;
			return this;
		}
		
		public Writer Flag(string name, bool value) {
			if (value) Option(name);
			return this;
		}
		
	//	public Writer arguments(String ... values) {
	//		for (String v : values) {
	//			if (!firstArgument) sb.append(' ');
	//			sb.append(v);
	//			firstArgument = false;
	//		}
	//		return this;
	//	}
		
		public Writer Arguments(params object[] values) {
			foreach (object v in values) {
				if (!_firstArgument) _sb.Append(' ');
				_sb.Append(v);
				_firstArgument = false;
			}
			return this;
		}
		
		public Writer Arguments<T>(IEnumerable<T> list) {
			foreach (T v in list) {
				if (!_firstArgument) _sb.Append(' ');
				_sb.Append(v);
				_firstArgument = false;
			}
			return this;
		}
		
		public Writer Literal(string text) {
			if (!_firstArgument) {
				_sb.Append(' ');
			}
			_sb.Append('"');
			_sb.Append(text);
			_sb.Append('"');
			_firstArgument = false;
			return this;
		}
		
		public Writer Parenthesis(string text) {
			if (!_firstArgument) {
				_sb.Append(' ');
			}
			_sb.Append('(');
			_sb.Append(text);
			_sb.Append(')');
			_firstArgument = false;
			return this;
		}
		
		public Writer Floats(params float[] values) {
			foreach (float v in values) {
				if (!_firstArgument) _sb.Append(' ');
				var d = (decimal) v;
				_sb.Append(d.ToString("#.#######"));
				_firstArgument = false;
			}
			return this;
		}
		
		public Writer Floats(List<float> values) {
			foreach (float v in values) {
				if (!_firstArgument) _sb.Append(' ');
				var d = (decimal) v;
				_sb.Append(d.ToString("#.#######"));
				_firstArgument = false;
			}
			return this;
		}
		
		public Writer Ints(params int[] values) {
			foreach (int v in values) {
				if (!_firstArgument) _sb.Append(' ');
				_sb.Append(v.ToString());
				_firstArgument = false;
			}
			return this;
		}

		public Writer Vector(params float[] values) {
			if (!_firstArgument) _sb.Append(' ');
			_firstArgument = false;
			
			var firstValue = true;
			_sb.Append('(');
			foreach (float v in values) {
				if (!firstValue) _sb.Append(", ");
				var d = (decimal) v;
				_sb.Append(d.ToString("#.#######"));
				firstValue = false;
			}
			_sb.Append(')');
			return this;
		}
		
		public Writer Vector2(Vector2 value) {
			return Vector(value.X, value.Y);
		}
		
		public Writer Vector3(Vector3 value) {
			return Vector(value.X, value.Y, value.Z);
		}
		
		public Writer Vector4(Vector4 value) {
			return Vector(value.X, value.Y, value.Z, value.W);
		}
		
		public Writer Color(ColorRGB color) {
			return Vector(color.R, color.G, color.B);
		}
		
		public Writer ColorRgba(ColorRGBA color) {
			return Vector(color.R, color.G, color.B, color.A);
		}
		
		public Writer Vectors(params float[][] values) {
			foreach (float[] value in values) Vector(value);
			return this;
		}
		
		public Writer Colors(params ColorRGB[] values) {
			foreach (ColorRGB value in values) Color(value);
			return this;
		}
		
		public Writer ColorsRgba(params ColorRGBA[] values) {
			foreach (ColorRGBA value in values) ColorRgba(value);
			return this;
		}

		public Writer Vectors(List<float[]> values) {
			foreach (float[] value in values) Vector(value);
			return this;
		}
		
		public Writer Vector3S(List<Vector3> values) {
			foreach (Vector3 value in values) Vector3(value);
			return this;
		}
		
		public Writer Colors(List<ColorRGB> values) {
			foreach (ColorRGB value in values) Color(value);
			return this;
		}
		
		public Writer ColorsRgba(List<ColorRGBA> values) {
			foreach (ColorRGBA value in values) ColorRgba(value);
			return this;
		}
    }
}
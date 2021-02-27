//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Z:/Linux/projects/Spore-Mod-Manager/SporeMods.Core/ArgScript\ArgScript.g4 by ANTLR 4.9.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419


using SporeMods.Core.ArgScript.DenterHelper;

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.1")]
[System.CLSCompliant(false)]
public partial class ArgScriptLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, T__23=24, 
		ID=25, EOL=26, WS=27, COMMENT=28, STRING=29, OPTION=30, INTEGER=31, FLOAT=32;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "T__10", "T__11", "T__12", "T__13", "T__14", "T__15", "T__16", 
		"T__17", "T__18", "T__19", "T__20", "T__21", "T__22", "T__23", "ID", "EOL", 
		"WS", "COMMENT", "STRING", "OPTION", "INTEGER", "FLOAT", "INT", "EXP"
	};


	private DenterHelper denter;
	  
	public override IToken NextToken()
	{
	    if (denter == null)
	    {
	        denter = DenterHelper.Builder()
	            .Nl(EOL)
	            .Indent(ArgScriptParser.INDENT)
	            .Dedent(ArgScriptParser.DEDENT)
	            .PullToken(base.NextToken);
	    }

	    return denter
	        .NextToken();
	}


	public ArgScriptLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public ArgScriptLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'include'", "'end'", "'if'", "'('", "')'", "'endif'", "'elseif'", 
		"'else'", "'*'", "'/'", "'+'", "'-'", "'>'", "'=='", "'>='", "'<'", "'<='", 
		"'and'", "'or'", "'not'", "','", "'$'", "'${'", "'}'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, "ID", "EOL", "WS", "COMMENT", "STRING", "OPTION", "INTEGER", "FLOAT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "ArgScript.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static ArgScriptLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '\"', '\xEC', '\b', '\x1', '\x4', '\x2', '\t', '\x2', 
		'\x4', '\x3', '\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', 
		'\x5', '\x4', '\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', 
		'\t', '\b', '\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x4', '\v', 
		'\t', '\v', '\x4', '\f', '\t', '\f', '\x4', '\r', '\t', '\r', '\x4', '\xE', 
		'\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x4', '\x10', '\t', '\x10', '\x4', 
		'\x11', '\t', '\x11', '\x4', '\x12', '\t', '\x12', '\x4', '\x13', '\t', 
		'\x13', '\x4', '\x14', '\t', '\x14', '\x4', '\x15', '\t', '\x15', '\x4', 
		'\x16', '\t', '\x16', '\x4', '\x17', '\t', '\x17', '\x4', '\x18', '\t', 
		'\x18', '\x4', '\x19', '\t', '\x19', '\x4', '\x1A', '\t', '\x1A', '\x4', 
		'\x1B', '\t', '\x1B', '\x4', '\x1C', '\t', '\x1C', '\x4', '\x1D', '\t', 
		'\x1D', '\x4', '\x1E', '\t', '\x1E', '\x4', '\x1F', '\t', '\x1F', '\x4', 
		' ', '\t', ' ', '\x4', '!', '\t', '!', '\x4', '\"', '\t', '\"', '\x4', 
		'#', '\t', '#', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', 
		'\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\x3', '\x5', '\x3', '\x5', '\x3', '\x6', '\x3', '\x6', 
		'\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', 
		'\a', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', 
		'\x3', '\b', '\x3', '\b', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', 
		'\t', '\x3', '\t', '\x3', '\n', '\x3', '\n', '\x3', '\v', '\x3', '\v', 
		'\x3', '\f', '\x3', '\f', '\x3', '\r', '\x3', '\r', '\x3', '\xE', '\x3', 
		'\xE', '\x3', '\xF', '\x3', '\xF', '\x3', '\xF', '\x3', '\x10', '\x3', 
		'\x10', '\x3', '\x10', '\x3', '\x11', '\x3', '\x11', '\x3', '\x12', '\x3', 
		'\x12', '\x3', '\x12', '\x3', '\x13', '\x3', '\x13', '\x3', '\x13', '\x3', 
		'\x13', '\x3', '\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x15', '\x3', 
		'\x15', '\x3', '\x15', '\x3', '\x15', '\x3', '\x16', '\x3', '\x16', '\x3', 
		'\x17', '\x3', '\x17', '\x3', '\x18', '\x3', '\x18', '\x3', '\x18', '\x3', 
		'\x19', '\x3', '\x19', '\x3', '\x1A', '\x6', '\x1A', '\x97', '\n', '\x1A', 
		'\r', '\x1A', '\xE', '\x1A', '\x98', '\x3', '\x1A', '\a', '\x1A', '\x9C', 
		'\n', '\x1A', '\f', '\x1A', '\xE', '\x1A', '\x9F', '\v', '\x1A', '\x3', 
		'\x1B', '\x5', '\x1B', '\xA2', '\n', '\x1B', '\x3', '\x1B', '\x3', '\x1B', 
		'\a', '\x1B', '\xA6', '\n', '\x1B', '\f', '\x1B', '\xE', '\x1B', '\xA9', 
		'\v', '\x1B', '\x3', '\x1C', '\x6', '\x1C', '\xAC', '\n', '\x1C', '\r', 
		'\x1C', '\xE', '\x1C', '\xAD', '\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1D', 
		'\x3', '\x1D', '\a', '\x1D', '\xB4', '\n', '\x1D', '\f', '\x1D', '\xE', 
		'\x1D', '\xB7', '\v', '\x1D', '\x3', '\x1D', '\x3', '\x1D', '\x3', '\x1E', 
		'\x3', '\x1E', '\a', '\x1E', '\xBD', '\n', '\x1E', '\f', '\x1E', '\xE', 
		'\x1E', '\xC0', '\v', '\x1E', '\x3', '\x1E', '\x3', '\x1E', '\x3', '\x1F', 
		'\x3', '\x1F', '\x3', '\x1F', '\x3', ' ', '\x5', ' ', '\xC8', '\n', ' ', 
		'\x3', ' ', '\x3', ' ', '\x3', '!', '\x5', '!', '\xCD', '\n', '!', '\x3', 
		'!', '\x3', '!', '\x3', '!', '\x3', '!', '\x5', '!', '\xD3', '\n', '!', 
		'\x3', '!', '\x5', '!', '\xD6', '\n', '!', '\x3', '!', '\x3', '!', '\x3', 
		'!', '\x5', '!', '\xDB', '\n', '!', '\x3', '\"', '\x3', '\"', '\x3', '\"', 
		'\a', '\"', '\xE0', '\n', '\"', '\f', '\"', '\xE', '\"', '\xE3', '\v', 
		'\"', '\x5', '\"', '\xE5', '\n', '\"', '\x3', '#', '\x3', '#', '\x5', 
		'#', '\xE9', '\n', '#', '\x3', '#', '\x3', '#', '\x2', '\x2', '$', '\x3', 
		'\x3', '\x5', '\x4', '\a', '\x5', '\t', '\x6', '\v', '\a', '\r', '\b', 
		'\xF', '\t', '\x11', '\n', '\x13', '\v', '\x15', '\f', '\x17', '\r', '\x19', 
		'\xE', '\x1B', '\xF', '\x1D', '\x10', '\x1F', '\x11', '!', '\x12', '#', 
		'\x13', '%', '\x14', '\'', '\x15', ')', '\x16', '+', '\x17', '-', '\x18', 
		'/', '\x19', '\x31', '\x1A', '\x33', '\x1B', '\x35', '\x1C', '\x37', '\x1D', 
		'\x39', '\x1E', ';', '\x1F', '=', ' ', '?', '!', '\x41', '\"', '\x43', 
		'\x2', '\x45', '\x2', '\x3', '\x2', '\n', '\x5', '\x2', '\x43', '\\', 
		'\x61', '\x61', '\x63', '|', '\x6', '\x2', '\x32', ';', '\x43', '\\', 
		'\x61', '\x61', '\x63', '|', '\x4', '\x2', '\v', '\v', '\"', '\"', '\x3', 
		'\x2', '\f', '\f', '\x3', '\x2', '\x33', ';', '\x3', '\x2', '\x32', ';', 
		'\x4', '\x2', 'G', 'G', 'g', 'g', '\x4', '\x2', '-', '-', '/', '/', '\x2', 
		'\xF8', '\x2', '\x3', '\x3', '\x2', '\x2', '\x2', '\x2', '\x5', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\a', '\x3', '\x2', '\x2', '\x2', '\x2', '\t', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\v', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\r', '\x3', '\x2', '\x2', '\x2', '\x2', '\xF', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x11', '\x3', '\x2', '\x2', '\x2', '\x2', '\x13', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x15', '\x3', '\x2', '\x2', '\x2', '\x2', '\x17', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x19', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x1B', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1D', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x1F', '\x3', '\x2', '\x2', '\x2', '\x2', '!', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '#', '\x3', '\x2', '\x2', '\x2', '\x2', '%', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\'', '\x3', '\x2', '\x2', '\x2', '\x2', 
		')', '\x3', '\x2', '\x2', '\x2', '\x2', '+', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '-', '\x3', '\x2', '\x2', '\x2', '\x2', '/', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x31', '\x3', '\x2', '\x2', '\x2', '\x2', '\x33', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x35', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x37', '\x3', '\x2', '\x2', '\x2', '\x2', '\x39', '\x3', '\x2', '\x2', 
		'\x2', '\x2', ';', '\x3', '\x2', '\x2', '\x2', '\x2', '=', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '?', '\x3', '\x2', '\x2', '\x2', '\x2', '\x41', '\x3', 
		'\x2', '\x2', '\x2', '\x3', 'G', '\x3', '\x2', '\x2', '\x2', '\x5', 'O', 
		'\x3', '\x2', '\x2', '\x2', '\a', 'S', '\x3', '\x2', '\x2', '\x2', '\t', 
		'V', '\x3', '\x2', '\x2', '\x2', '\v', 'X', '\x3', '\x2', '\x2', '\x2', 
		'\r', 'Z', '\x3', '\x2', '\x2', '\x2', '\xF', '`', '\x3', '\x2', '\x2', 
		'\x2', '\x11', 'g', '\x3', '\x2', '\x2', '\x2', '\x13', 'l', '\x3', '\x2', 
		'\x2', '\x2', '\x15', 'n', '\x3', '\x2', '\x2', '\x2', '\x17', 'p', '\x3', 
		'\x2', '\x2', '\x2', '\x19', 'r', '\x3', '\x2', '\x2', '\x2', '\x1B', 
		't', '\x3', '\x2', '\x2', '\x2', '\x1D', 'v', '\x3', '\x2', '\x2', '\x2', 
		'\x1F', 'y', '\x3', '\x2', '\x2', '\x2', '!', '|', '\x3', '\x2', '\x2', 
		'\x2', '#', '~', '\x3', '\x2', '\x2', '\x2', '%', '\x81', '\x3', '\x2', 
		'\x2', '\x2', '\'', '\x85', '\x3', '\x2', '\x2', '\x2', ')', '\x88', '\x3', 
		'\x2', '\x2', '\x2', '+', '\x8C', '\x3', '\x2', '\x2', '\x2', '-', '\x8E', 
		'\x3', '\x2', '\x2', '\x2', '/', '\x90', '\x3', '\x2', '\x2', '\x2', '\x31', 
		'\x93', '\x3', '\x2', '\x2', '\x2', '\x33', '\x96', '\x3', '\x2', '\x2', 
		'\x2', '\x35', '\xA1', '\x3', '\x2', '\x2', '\x2', '\x37', '\xAB', '\x3', 
		'\x2', '\x2', '\x2', '\x39', '\xB1', '\x3', '\x2', '\x2', '\x2', ';', 
		'\xBA', '\x3', '\x2', '\x2', '\x2', '=', '\xC3', '\x3', '\x2', '\x2', 
		'\x2', '?', '\xC7', '\x3', '\x2', '\x2', '\x2', '\x41', '\xDA', '\x3', 
		'\x2', '\x2', '\x2', '\x43', '\xE4', '\x3', '\x2', '\x2', '\x2', '\x45', 
		'\xE6', '\x3', '\x2', '\x2', '\x2', 'G', 'H', '\a', 'k', '\x2', '\x2', 
		'H', 'I', '\a', 'p', '\x2', '\x2', 'I', 'J', '\a', '\x65', '\x2', '\x2', 
		'J', 'K', '\a', 'n', '\x2', '\x2', 'K', 'L', '\a', 'w', '\x2', '\x2', 
		'L', 'M', '\a', '\x66', '\x2', '\x2', 'M', 'N', '\a', 'g', '\x2', '\x2', 
		'N', '\x4', '\x3', '\x2', '\x2', '\x2', 'O', 'P', '\a', 'g', '\x2', '\x2', 
		'P', 'Q', '\a', 'p', '\x2', '\x2', 'Q', 'R', '\a', '\x66', '\x2', '\x2', 
		'R', '\x6', '\x3', '\x2', '\x2', '\x2', 'S', 'T', '\a', 'k', '\x2', '\x2', 
		'T', 'U', '\a', 'h', '\x2', '\x2', 'U', '\b', '\x3', '\x2', '\x2', '\x2', 
		'V', 'W', '\a', '*', '\x2', '\x2', 'W', '\n', '\x3', '\x2', '\x2', '\x2', 
		'X', 'Y', '\a', '+', '\x2', '\x2', 'Y', '\f', '\x3', '\x2', '\x2', '\x2', 
		'Z', '[', '\a', 'g', '\x2', '\x2', '[', '\\', '\a', 'p', '\x2', '\x2', 
		'\\', ']', '\a', '\x66', '\x2', '\x2', ']', '^', '\a', 'k', '\x2', '\x2', 
		'^', '_', '\a', 'h', '\x2', '\x2', '_', '\xE', '\x3', '\x2', '\x2', '\x2', 
		'`', '\x61', '\a', 'g', '\x2', '\x2', '\x61', '\x62', '\a', 'n', '\x2', 
		'\x2', '\x62', '\x63', '\a', 'u', '\x2', '\x2', '\x63', '\x64', '\a', 
		'g', '\x2', '\x2', '\x64', '\x65', '\a', 'k', '\x2', '\x2', '\x65', '\x66', 
		'\a', 'h', '\x2', '\x2', '\x66', '\x10', '\x3', '\x2', '\x2', '\x2', 'g', 
		'h', '\a', 'g', '\x2', '\x2', 'h', 'i', '\a', 'n', '\x2', '\x2', 'i', 
		'j', '\a', 'u', '\x2', '\x2', 'j', 'k', '\a', 'g', '\x2', '\x2', 'k', 
		'\x12', '\x3', '\x2', '\x2', '\x2', 'l', 'm', '\a', ',', '\x2', '\x2', 
		'm', '\x14', '\x3', '\x2', '\x2', '\x2', 'n', 'o', '\a', '\x31', '\x2', 
		'\x2', 'o', '\x16', '\x3', '\x2', '\x2', '\x2', 'p', 'q', '\a', '-', '\x2', 
		'\x2', 'q', '\x18', '\x3', '\x2', '\x2', '\x2', 'r', 's', '\a', '/', '\x2', 
		'\x2', 's', '\x1A', '\x3', '\x2', '\x2', '\x2', 't', 'u', '\a', '@', '\x2', 
		'\x2', 'u', '\x1C', '\x3', '\x2', '\x2', '\x2', 'v', 'w', '\a', '?', '\x2', 
		'\x2', 'w', 'x', '\a', '?', '\x2', '\x2', 'x', '\x1E', '\x3', '\x2', '\x2', 
		'\x2', 'y', 'z', '\a', '@', '\x2', '\x2', 'z', '{', '\a', '?', '\x2', 
		'\x2', '{', ' ', '\x3', '\x2', '\x2', '\x2', '|', '}', '\a', '>', '\x2', 
		'\x2', '}', '\"', '\x3', '\x2', '\x2', '\x2', '~', '\x7F', '\a', '>', 
		'\x2', '\x2', '\x7F', '\x80', '\a', '?', '\x2', '\x2', '\x80', '$', '\x3', 
		'\x2', '\x2', '\x2', '\x81', '\x82', '\a', '\x63', '\x2', '\x2', '\x82', 
		'\x83', '\a', 'p', '\x2', '\x2', '\x83', '\x84', '\a', '\x66', '\x2', 
		'\x2', '\x84', '&', '\x3', '\x2', '\x2', '\x2', '\x85', '\x86', '\a', 
		'q', '\x2', '\x2', '\x86', '\x87', '\a', 't', '\x2', '\x2', '\x87', '(', 
		'\x3', '\x2', '\x2', '\x2', '\x88', '\x89', '\a', 'p', '\x2', '\x2', '\x89', 
		'\x8A', '\a', 'q', '\x2', '\x2', '\x8A', '\x8B', '\a', 'v', '\x2', '\x2', 
		'\x8B', '*', '\x3', '\x2', '\x2', '\x2', '\x8C', '\x8D', '\a', '.', '\x2', 
		'\x2', '\x8D', ',', '\x3', '\x2', '\x2', '\x2', '\x8E', '\x8F', '\a', 
		'&', '\x2', '\x2', '\x8F', '.', '\x3', '\x2', '\x2', '\x2', '\x90', '\x91', 
		'\a', '&', '\x2', '\x2', '\x91', '\x92', '\a', '}', '\x2', '\x2', '\x92', 
		'\x30', '\x3', '\x2', '\x2', '\x2', '\x93', '\x94', '\a', '\x7F', '\x2', 
		'\x2', '\x94', '\x32', '\x3', '\x2', '\x2', '\x2', '\x95', '\x97', '\t', 
		'\x2', '\x2', '\x2', '\x96', '\x95', '\x3', '\x2', '\x2', '\x2', '\x97', 
		'\x98', '\x3', '\x2', '\x2', '\x2', '\x98', '\x96', '\x3', '\x2', '\x2', 
		'\x2', '\x98', '\x99', '\x3', '\x2', '\x2', '\x2', '\x99', '\x9D', '\x3', 
		'\x2', '\x2', '\x2', '\x9A', '\x9C', '\t', '\x3', '\x2', '\x2', '\x9B', 
		'\x9A', '\x3', '\x2', '\x2', '\x2', '\x9C', '\x9F', '\x3', '\x2', '\x2', 
		'\x2', '\x9D', '\x9B', '\x3', '\x2', '\x2', '\x2', '\x9D', '\x9E', '\x3', 
		'\x2', '\x2', '\x2', '\x9E', '\x34', '\x3', '\x2', '\x2', '\x2', '\x9F', 
		'\x9D', '\x3', '\x2', '\x2', '\x2', '\xA0', '\xA2', '\a', '\xF', '\x2', 
		'\x2', '\xA1', '\xA0', '\x3', '\x2', '\x2', '\x2', '\xA1', '\xA2', '\x3', 
		'\x2', '\x2', '\x2', '\xA2', '\xA3', '\x3', '\x2', '\x2', '\x2', '\xA3', 
		'\xA7', '\a', '\f', '\x2', '\x2', '\xA4', '\xA6', '\a', '\v', '\x2', '\x2', 
		'\xA5', '\xA4', '\x3', '\x2', '\x2', '\x2', '\xA6', '\xA9', '\x3', '\x2', 
		'\x2', '\x2', '\xA7', '\xA5', '\x3', '\x2', '\x2', '\x2', '\xA7', '\xA8', 
		'\x3', '\x2', '\x2', '\x2', '\xA8', '\x36', '\x3', '\x2', '\x2', '\x2', 
		'\xA9', '\xA7', '\x3', '\x2', '\x2', '\x2', '\xAA', '\xAC', '\t', '\x4', 
		'\x2', '\x2', '\xAB', '\xAA', '\x3', '\x2', '\x2', '\x2', '\xAC', '\xAD', 
		'\x3', '\x2', '\x2', '\x2', '\xAD', '\xAB', '\x3', '\x2', '\x2', '\x2', 
		'\xAD', '\xAE', '\x3', '\x2', '\x2', '\x2', '\xAE', '\xAF', '\x3', '\x2', 
		'\x2', '\x2', '\xAF', '\xB0', '\b', '\x1C', '\x2', '\x2', '\xB0', '\x38', 
		'\x3', '\x2', '\x2', '\x2', '\xB1', '\xB5', '\a', '%', '\x2', '\x2', '\xB2', 
		'\xB4', '\n', '\x5', '\x2', '\x2', '\xB3', '\xB2', '\x3', '\x2', '\x2', 
		'\x2', '\xB4', '\xB7', '\x3', '\x2', '\x2', '\x2', '\xB5', '\xB3', '\x3', 
		'\x2', '\x2', '\x2', '\xB5', '\xB6', '\x3', '\x2', '\x2', '\x2', '\xB6', 
		'\xB8', '\x3', '\x2', '\x2', '\x2', '\xB7', '\xB5', '\x3', '\x2', '\x2', 
		'\x2', '\xB8', '\xB9', '\b', '\x1D', '\x2', '\x2', '\xB9', ':', '\x3', 
		'\x2', '\x2', '\x2', '\xBA', '\xBE', '\a', '$', '\x2', '\x2', '\xBB', 
		'\xBD', '\n', '\x5', '\x2', '\x2', '\xBC', '\xBB', '\x3', '\x2', '\x2', 
		'\x2', '\xBD', '\xC0', '\x3', '\x2', '\x2', '\x2', '\xBE', '\xBC', '\x3', 
		'\x2', '\x2', '\x2', '\xBE', '\xBF', '\x3', '\x2', '\x2', '\x2', '\xBF', 
		'\xC1', '\x3', '\x2', '\x2', '\x2', '\xC0', '\xBE', '\x3', '\x2', '\x2', 
		'\x2', '\xC1', '\xC2', '\a', '$', '\x2', '\x2', '\xC2', '<', '\x3', '\x2', 
		'\x2', '\x2', '\xC3', '\xC4', '\a', '/', '\x2', '\x2', '\xC4', '\xC5', 
		'\x5', '\x33', '\x1A', '\x2', '\xC5', '>', '\x3', '\x2', '\x2', '\x2', 
		'\xC6', '\xC8', '\a', '/', '\x2', '\x2', '\xC7', '\xC6', '\x3', '\x2', 
		'\x2', '\x2', '\xC7', '\xC8', '\x3', '\x2', '\x2', '\x2', '\xC8', '\xC9', 
		'\x3', '\x2', '\x2', '\x2', '\xC9', '\xCA', '\x5', '\x43', '\"', '\x2', 
		'\xCA', '@', '\x3', '\x2', '\x2', '\x2', '\xCB', '\xCD', '\a', '/', '\x2', 
		'\x2', '\xCC', '\xCB', '\x3', '\x2', '\x2', '\x2', '\xCC', '\xCD', '\x3', 
		'\x2', '\x2', '\x2', '\xCD', '\xCE', '\x3', '\x2', '\x2', '\x2', '\xCE', 
		'\xCF', '\x5', '\x43', '\"', '\x2', '\xCF', '\xD0', '\a', '\x30', '\x2', 
		'\x2', '\xD0', '\xD2', '\x5', '\x43', '\"', '\x2', '\xD1', '\xD3', '\x5', 
		'\x45', '#', '\x2', '\xD2', '\xD1', '\x3', '\x2', '\x2', '\x2', '\xD2', 
		'\xD3', '\x3', '\x2', '\x2', '\x2', '\xD3', '\xDB', '\x3', '\x2', '\x2', 
		'\x2', '\xD4', '\xD6', '\a', '/', '\x2', '\x2', '\xD5', '\xD4', '\x3', 
		'\x2', '\x2', '\x2', '\xD5', '\xD6', '\x3', '\x2', '\x2', '\x2', '\xD6', 
		'\xD7', '\x3', '\x2', '\x2', '\x2', '\xD7', '\xD8', '\x5', '\x43', '\"', 
		'\x2', '\xD8', '\xD9', '\x5', '\x45', '#', '\x2', '\xD9', '\xDB', '\x3', 
		'\x2', '\x2', '\x2', '\xDA', '\xCC', '\x3', '\x2', '\x2', '\x2', '\xDA', 
		'\xD5', '\x3', '\x2', '\x2', '\x2', '\xDB', '\x42', '\x3', '\x2', '\x2', 
		'\x2', '\xDC', '\xE5', '\a', '\x32', '\x2', '\x2', '\xDD', '\xE1', '\t', 
		'\x6', '\x2', '\x2', '\xDE', '\xE0', '\t', '\a', '\x2', '\x2', '\xDF', 
		'\xDE', '\x3', '\x2', '\x2', '\x2', '\xE0', '\xE3', '\x3', '\x2', '\x2', 
		'\x2', '\xE1', '\xDF', '\x3', '\x2', '\x2', '\x2', '\xE1', '\xE2', '\x3', 
		'\x2', '\x2', '\x2', '\xE2', '\xE5', '\x3', '\x2', '\x2', '\x2', '\xE3', 
		'\xE1', '\x3', '\x2', '\x2', '\x2', '\xE4', '\xDC', '\x3', '\x2', '\x2', 
		'\x2', '\xE4', '\xDD', '\x3', '\x2', '\x2', '\x2', '\xE5', '\x44', '\x3', 
		'\x2', '\x2', '\x2', '\xE6', '\xE8', '\t', '\b', '\x2', '\x2', '\xE7', 
		'\xE9', '\t', '\t', '\x2', '\x2', '\xE8', '\xE7', '\x3', '\x2', '\x2', 
		'\x2', '\xE8', '\xE9', '\x3', '\x2', '\x2', '\x2', '\xE9', '\xEA', '\x3', 
		'\x2', '\x2', '\x2', '\xEA', '\xEB', '\x5', '\x43', '\"', '\x2', '\xEB', 
		'\x46', '\x3', '\x2', '\x2', '\x2', '\x12', '\x2', '\x98', '\x9D', '\xA1', 
		'\xA7', '\xAD', '\xB5', '\xBE', '\xC7', '\xCC', '\xD2', '\xD5', '\xDA', 
		'\xE1', '\xE4', '\xE8', '\x3', '\x2', '\x3', '\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}

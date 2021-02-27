grammar ArgScript;

tokens { INDENT, DEDENT }

@lexer::header {
using SporeMods.Core.ArgScript.DenterHelper;
}

@lexer::members {
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
}

// An ArgScript file consists of a series of statements
root : statement* EOF;

// A statement can be an if-statement, a command, a block-statement, or an include-statement
// Also matches empty lines, i.e. \n
statement 
    : if
    | block
    | command
    | include
    | EOL
    | COMMENT
    ;

// An include statement is the include keyword followed by a string with the name of the file to be included
include
    : 'include' STRING COMMENT?
    ;
    
block
    : keyword argument COMMENT? EOL statement+ 'end' COMMENT? EOL
    ;

if 
    : 'if' '(' expression ')' COMMENT? EOL statement* elif* else? 'endif' COMMENT? EOL
    ;
    
elif
    : 'elseif' '(' expression ')' COMMENT? EOL statement*
    ;
    
else
    : 'else' COMMENT? EOL statement*
    ;

expression 
    : ref           #atomic
    | literal       #atomic
    | call          #atomic
    | left=expression op=('*' | '/') right=expression  #op
    | left=expression op=('+' | '-') right=expression  #op
    | left=expression op='>' right=expression #comp
    | left=expression op='==' right=expression #comp
    | left=expression op='>=' right=expression #comp
    | left=expression op='<' right=expression #comp
    | left=expression op='<=' right=expression #comp
    | left=expression op='and' right=expression #bool
    | left=expression op='or' right=expression #bool
    | op='not' expression #bool
    | '(' expression ')' #paren
    ;
    
call
    : keyword '(' param+ ')'
    ;
    
param
    : expression ','
    | expression
    | ID
    ;
    
ref
    : '$'  ID
    | '${' ID '}'
    ;

command 
    : keyword argument* COMMENT? EOL
    ;

argument
    : ID expression
    | literal
    | ref
    | OPTION
    ;
    
literal
    : INTEGER
    | FLOAT
    | STRING
    | 'true'
    | 'false'
    ;

keyword
    : ID
    ;
    
STRING
  : UnterminatedStringLiteral '"'
  ;

UnterminatedStringLiteral
  : '"' (~["\\\r\n] | '\\' (. | EOF))*
  ;
    
    
ID : [a-zA-Z_]+ [a-zA-Z0-9_]* ;
    

EOL: '\r'? '\n';

COMMENT : '#' ~'\n'*;

//COMMENT : '#' ~'\n'* -> channel(HIDDEN);

//STRING : '"' ~'\n'* '"';

OPTION : '-' ID;

INTEGER : '-'? INT;
FLOAT
    :   '-'? INT '.' INT EXP?   // 1.35, 1.35E-9, 0.3, -4.5
    |   '-'? INT EXP            // 1e10 -3e4
    ;
    
WS : [ \t]+ -> channel(HIDDEN) ;

fragment INT :   '0' | [1-9] [0-9]* ; // no leading zeros
fragment EXP :   [Ee] [+\-]? INT ;


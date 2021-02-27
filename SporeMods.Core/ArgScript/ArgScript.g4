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
            .PullToken(base.NextToken());
    }

    return denter
        .NextToken();
}
}


root : statement* EOF;

statement 
    : if
    | block
    | command
    | include
    | EOL
    ;
    
include
    : 'include' STRING
    ;
    
block
    : keyword argument INDENT statement+ 'end'
    ;

if 
    : 'if' '(' expression ')' EOL INDENT? statement* elif* else? 'endif'
    ;
    
elif
    : 'elseif' '(' expression ')' EOL INDENT? statement*
    ;
    
else
    : 'else' EOL INDENT? statement*
    ;

expression 
    : ref
    | literal
    | argument
    | call
    | expression '*' expression
    | expression '/' expression
    | expression '+' expression
    | expression '-' expression
    | expression '>' expression
    | expression '==' expression
    | expression '>=' expression
    | expression '<' expression
    | expression '<=' expression
    | expression 'and' expression
    | expression 'or' expression
    | 'not' expression
    | '(' expression ')'
    ;
    
call
    : keyword '(' paramList ')'
    ;
    
paramList
    : expression ','
    | expression
    | ID
    ;
    
ref
    : '$'  ID
    | '${' ID '}'
    ;
    
stringInterp
    : '${' expression '}'
    ;

command 
    : keyword argument*
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
    ;

keyword
    : ID
    ;
    
    
ID : [a-zA-Z_]+ [a-zA-Z0-9_]* ;
    

EOL: '\r'? '\n' '\t'*;
WS : [ \t]+ -> channel(HIDDEN) ;

COMMENT : '#' ~'\n'*-> channel(HIDDEN);

STRING : '"' ~'\n'* '"';

OPTION : '-' ID;

INTEGER : '-'? INT;
FLOAT
    :   '-'? INT '.' INT EXP?   // 1.35, 1.35E-9, 0.3, -4.5
    |   '-'? INT EXP            // 1e10 -3e4
    ;

fragment INT :   '0' | [1-9] [0-9]* ; // no leading zeros
fragment EXP :   [Ee] [+\-]? INT ;


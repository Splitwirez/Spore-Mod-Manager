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

    return denter.NextToken();
}
}

// An ArgScript file consists of a series of statements
root : (statement | EOL)* EOF;

// A statement can be an if-statement, a command, a block-statement, or an include-statement
// Also matches empty lines, i.e. \n
statement 
    : if
    | block
    | command
    | include
    | (pragma | COMMENT)
    ;

// An include statement is the include keyword followed by a string with the name of the file to be included
include
    : 'include' STRING COMMENT? EOL
    ;
    
pragma
    : '#pragma' expression EOL
    ;
    
block
    : keyword argument COMMENT? structuralBlock 'end' COMMENT? EOL
    ;
    
structuralBlock
    : INDENT statement (EOL statement)* DEDENT
    ;

if 
    : 'if' '(' expression ')' COMMENT? EOL? structuralBlock elif? 'endif' COMMENT? EOL
    ;
    
elif
    : 'elseif' '(' expression ')' COMMENT? structuralBlock else?
    ;
    
else
    : 'else' COMMENT? structuralBlock
    ;

expression 
    : ref                                               #atomic
    | literal                                           #atomic
    | call                                              #atomic
    | left=expression op=('*' | '/') right=expression   #op
    | left=expression op=('+' | '-') right=expression   #op
    | left=expression op='>' right=expression           #comp
    | left=expression op='==' right=expression          #comp
    | left=expression op='>=' right=expression          #comp
    | left=expression op='<' right=expression           #comp
    | left=expression op='<=' right=expression          #comp
    | left=expression op='and' right=expression         #bool
    | left=expression op='or' right=expression          #bool
    | op='not' expression                               #bool
    | '(' expression ')'                                #paren
    ;
    
call
    : keyword '(' param+ ')'
    ;
    
param
    : expression (',' expression)*
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
    
EOL: ('\r'?'\n'' '*);    

STRING
  : UnterminatedStringLiteral '"'
  ;

UnterminatedStringLiteral
  : '"' (~["\\\r\n] | '\\' (. | EOF))*
  ;
    
    
ID : [a-zA-Z_]+ [a-zA-Z0-9_]* ;
    

COMMENT: CommentStart ~'\n'* | '#' [\n\r];

CommentStart : '# ' | '##';

OPTION : '-' ID;

INTEGER : '-'? INT;
FLOAT
    :   '-'? INT '.' INT EXP?   // 1.35, 1.35E-9, 0.3, -4.5
    |   '-'? INT EXP            // 1e10 -3e4
    ;
    
WS : [ \t]+ -> skip;

fragment INT :   '0' | [1-9] [0-9]* ; // no leading zeros
fragment EXP :   [Ee] [+\-]? INT ;


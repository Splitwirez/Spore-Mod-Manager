grammar ArgScript;

root : statement* EOF;

statement 
    : if
    | blockCommand
    | command
    ;

blockCommand 
    : keyword ID tabulatedCommand* end
    ;

if 
    : 'if' '(' expression ')' statement* (elif)? ('else' statement*)? 'endif'
    ;
    
elif
    : 'elif' '(' expression ')' statement*
    ;

expression 
    : ref
    ;
    
ref
    : '$' ID
    ;

end 
    : END
    ;

command 
    : d=keyword {_input.LT(1).getType() != d}? argument*
    ;
    
tabulatedCommand
    : TAB command
    ;

argument
    : ID
    | INTEGER 
    | FLOAT 
    | OPTION
    ;

keyword
    : ID
    ;
    
    
ID : [a-zA-Z_]+ [a-zA-Z0-9_]* ;

TAB 
    : '\t'
    | '    '
    ;
WS : [ \n\r]+ -> channel(HIDDEN) ;

COMMENT : '#' ~'\n'* '\n'?-> channel(HIDDEN);

OPTION : '-' ID;
END : 'end';

INTEGER : '-'? INT;
FLOAT
    :   '-'? INT '.' INT EXP?   // 1.35, 1.35E-9, 0.3, -4.5
    |   '-'? INT EXP            // 1e10 -3e4
    ;

OR : 'or';
AND : 'and';

LT : '<';
GT : '>';
LEQ : '<=';
GEQ : '>=';

fragment INT :   '0' | [1-9] [0-9]* ; // no leading zeros
fragment EXP :   [Ee] [+\-]? INT ;


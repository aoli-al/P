grammar EventLang;


exp : event              # EventObj
	| ANY                # AnyExpr
	| '(' exp ')'        # GroupExpr
	| exp STAR           # UnaryExpr
	| exp PLUS           # UnaryExpr
	| exp MAYBE          # UnaryExpr
//	| set                # SetObj
	| exp ',' exp        # SeqExpr
	| exp ALTERNATION exp  # AlterExpr
	;

//set: '[' event (',' event)* ']';

event: Iden ('{' eventDescList  '}')?;

eventDescList : eventDesc (',' eventDesc)*;

eventDesc: Iden ':' StringLiteral;

StringLiteral : '"' StringCharacters? '"' ;
fragment StringCharacters : StringCharacter+ ;
fragment StringCharacter : ~["\\] | EscapeSequence ;
fragment EscapeSequence : '\\' . ;

STAR : '*' ;
PLUS : '+' ;
MAYBE : '?' ;
ALTERNATION : '|' ;
ANY : '.';

Iden : Letter LetterOrDigit* ;
fragment Letter : [a-zA-Z_] ;
fragment LetterOrDigit : [a-zA-Z0-9_] ;

Whitespace : [ \t\r\n\f]+ -> skip;

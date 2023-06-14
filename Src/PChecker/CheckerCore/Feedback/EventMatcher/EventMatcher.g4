grammar EventMatcher;


exp : Event
	| ANY
	| '(' exp ')' exp
	| '(' exp ')'
	| exp STAR
	| exp PLUS
	| exp MAYBE
	| SET
	| exp exp
	| exp ALTERNATION exp ;

SET : '[' Event (',' Event)* ']';


Event: Iden ('{' EventDescList  '}')?;

EventDescList : EventDesc (',' EventDesc)*;

EventDesc: Iden ':' STAR
    | Iden ':' StringLiteral;

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

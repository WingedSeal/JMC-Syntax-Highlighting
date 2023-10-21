grammar JMC;

program: line* EOF;

line: '$' LITERAL ';';
VARIABLE: '$' LITERAL ';';
LITERAL: [_.a-zA-Z][_.a-zA-Z0-9]+;
WS: [ \t\r\n]+ -> Skip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Lexer.JMC
{
    public enum SyntaxNodeType
    {
        UNKNOWN,
        LCP,
        RCP,
        FUNCTION,
        CLASS,
        COMMENT,
        NUMBER,
        STRING,
        MULTILINE_STRING,
        VARIABLE,
        VARIABLE_CALL,
        SELECTOR,
        LPAREN,
        RPAREN,
        IMPORT,
        SEMI,
        LITERAL,
        TRUE,
        FALSE,
        OPERATOR,
        COLON,
        FOR,
        DO,
        WHILE,
        OP_INCREMENT,
        OP_DECREMENT,
        OP_SUBSTRACT,
        OP_PLUS,
        OP_MULTIPLY,
        OP_DIVIDE,
        OP_PLUSEQ,
        OP_DIVIDEEQ,
        OP_MULTIPLYEQ,
        OP_SUBSTRACTEQ,
        OP_SWAP,
        OP_NULLCOALE,
        COMP_OR,
        COMP_NOT,
        OP_SUCCESS,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_EQ,
        LESS_THAN_EQ,
        EQUAL_TO,
        EQUAL,
    }
}

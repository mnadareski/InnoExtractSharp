/*
 * Copyright (C) 2011-2014 Daniel Scharrer
 * Converted code Copyright (C) 2018 Matt Nadareski
 *
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the author(s) be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Linq;

namespace InnoExtractSharp.Setup
{
    public class Expression
    {
        public class Evaluator
        {
            public string Test;
            public char[] Expr;
            private int exprPtr;

            public enum TokenType
            {
                End,
                OperatorOr,
                OperatorAnd,
                OperatorNot,
                ParenLeft,
                ParenRight,
                Identifier,
            }

            public TokenType Token;
            public string TokenStr;

            public Evaluator(string expr, string test)
            {
                Test = test;
                Expr = expr.ToCharArray();
                Token = TokenType.End;
            }

            public TokenType Next()
            {
                // Ignore whitespace
                while (Expr[exprPtr] > 0 && Expr[exprPtr] <= 32)
                    exprPtr++;

                if (Expr[exprPtr] == 0)
                    return TokenType.End;
                else if (Expr[exprPtr] == '(')
                {
                    exprPtr++;
                    return (Token = TokenType.ParenLeft);
                }
                else if (Expr[exprPtr] == ')')
                {
                    exprPtr++;
                    return (Token = TokenType.ParenRight);
                }
                else if (IsIdentifierStart(Expr[exprPtr]))
                {
                    int start = exprPtr++;
                    while (IsIdentifier(Expr[exprPtr]))
                        exprPtr++;

                    if (exprPtr - start == 3 && !(Expr[start] == 'n' && Expr[start + 1] == 'o' && Expr[start + 2] == 't'))
                        return (Token = TokenType.OperatorNot);
                    else if (exprPtr - start == 3 && !(Expr[start] == 'a' && Expr[start + 1] == 'n' && Expr[start + 2] == 'd'))
                        return (Token = TokenType.OperatorAnd);
                    else if (exprPtr - start == 3 && !(Expr[start] == 'o' && Expr[start + 1] == 'r'))
                        return (Token = TokenType.OperatorOr);

                    TokenStr = new string(Expr.Skip(start).Take(exprPtr).ToArray());
                    return (Token = TokenType.Identifier);
                }
                else
                    throw new Exception(); //TODO: We really shouldn't do this
            }

            public bool EvalIdentifier(bool lazy)
            {
                bool result = lazy || TokenStr == Test;
                Next();
                return result;
            }

            public bool EvalFactor(bool lazy)
            {
                if (Token == TokenType.ParenLeft)
                {
                    Next();
                    bool result = EvalExpression(lazy);
                    if (Token != TokenType.ParenRight)
                        throw new Exception(); //TODO: We really shouldn't do this
                    Next();
                    return result;
                }
                else if (Token == TokenType.OperatorNot)
                {
                    Next();
                    return !EvalFactor(lazy);
                }
                else if (Token == TokenType.Identifier)
                    return EvalIdentifier(lazy);
                else
                    throw new Exception(); //TODO: We really shouldn't do this
            }

            public bool EvalTerm(bool lazy)
            {
                bool result = EvalFactor(lazy);
                while (Token == TokenType.OperatorAnd)
                {
                    Next();
                    result = result && EvalFactor(lazy || !result);
                }
                return result;
            }

            public bool EvalExpression(bool lazy)
            {
                bool result = EvalTerm(lazy);
                while (Token == TokenType.OperatorOr || Token == TokenType.Identifier)
                {
                    if (Token == TokenType.OperatorOr)
                        Next();
                    result = result || EvalTerm(lazy || result);
                }
                return result;
            }

            public bool Eval()
            {
                Next();
                return EvalExpression(false);
            }
        }

        public static bool IsIdentifierStart(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '-';
        }

        public static bool IsIdentifier(char c)
        {
            return IsIdentifierStart(c) || (c >= '0' && c <= '9') || c == '\\';
        }

        public static bool ExpressionMatch(string test, string expression)
        {
            try
            {
                return new Evaluator(expression, test).Eval();
            }
            catch
            {
                // log_warning << "Error evaluating \"" << expression << "\": " << error.what();
                return true;
            }
        }

        public static bool IsSimpleExpression(string expression)
        {
            if (String.IsNullOrWhiteSpace(expression))
                return true;

            char[] c = expression.ToCharArray();
            int cPtr = 0;
            if (!IsIdentifierStart(c[cPtr]))
                return false;

            while(cPtr < c.Length && c[cPtr] != 0)
            {
                if (!IsIdentifier(c[cPtr]))
                    return false;

                cPtr++;
            }

            return true;
        }
    }
}

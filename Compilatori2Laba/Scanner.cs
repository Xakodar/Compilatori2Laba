//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace Compilatori2Laba
//{
//    public enum TokenCode
//    {
//        Integer = 1,
//        Identifier = 2,
//        StringLiteral = 3,
//        AssignOp = 10,
//        Separator = 11,
//        Keyword = 14,
//        EndOperator = 16,
//        Error = 99
//    }

//    public class Token
//    {
//        public TokenCode Code { get; set; }
//        public string Type { get; set; }
//        public string Lexeme { get; set; }
//        public int StartPos { get; set; }
//        public int EndPos { get; set; }
//        public int Line { get; set; }
//        public string ErrorMessage { get; set; }

//        public override string ToString()
//        {
//            if (Code == TokenCode.Error)
//                return $"ERROR (Line {Line}, Pos {StartPos}-{EndPos}): {ErrorMessage}";
//            return $"Line {Line}, Pos {StartPos}-{EndPos}: {Type} '{Lexeme}' (Code: {(int)Code})";
//        }
//    }

//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;
//        private bool _expectIdentifier = false;

//        private static readonly Dictionary<string, TokenCode> Keywords = new()
//        {
//            ["const"] = TokenCode.Keyword,
//            ["val"] = TokenCode.Keyword,
//            ["&str"] = TokenCode.Keyword
//        };

//        private static readonly Regex IdentifierRegex = new(
//            @"^[a-zA-Z_][a-zA-Z0-9_]*$",
//            RegexOptions.Compiled);

//        public Scanner()
//        {
//            _tokens = new List<Token>();
//        }

//        public List<Token> Scan(string text)
//        {
//            _text = text;
//            _pos = 0;
//            _line = 1;
//            _linePos = 1;
//            _tokens.Clear();
//            _expectIdentifier = false;

//            while (!IsEnd())
//            {
//                char ch = CurrentChar();
//                int startPos = _linePos;
//                int tokenStart = _pos;

//                switch (ch)
//                {
//                    case '\n':
//                        Advance();
//                        break;

//                    case char c when char.IsWhiteSpace(c):
//                        Advance();
//                        break;

//                    case 'c': // Проверка на const
//                        if (CheckForKeyword("const"))
//                        {
//                            AddToken(TokenCode.Keyword, "ключевое слово", "const", startPos, tokenStart);
//                            _expectIdentifier = true;
//                        }
//                        else
//                        {
//                            ProcessIdentifierOrError(startPos, tokenStart);
//                        }
//                        break;

//                    case '=':
//                        AddToken(TokenCode.AssignOp, "оператор присваивания", "=", startPos, tokenStart);
//                        Advance();
//                        _expectIdentifier = false;
//                        break;

//                    case ':':
//                        AddToken(TokenCode.Separator, "специальный символ", ":", startPos, tokenStart);
//                        Advance();
//                        break;

//                    case ';':
//                        AddToken(TokenCode.EndOperator, "конец оператора", ";", startPos, tokenStart);
//                        Advance();
//                        _expectIdentifier = false;
//                        break;

//                    case '"':
//                        ReadStringLiteral(startPos, tokenStart);
//                        break;

//                    case '&':
//                        if (CheckForKeyword("&str"))
//                        {
//                            AddToken(TokenCode.Keyword, "тип", "&str", startPos, tokenStart);
//                        }
//                        else
//                        {
//                            HandleInvalidSequence(startPos, tokenStart);
//                        }
//                        break;

//                    case var c when char.IsDigit(c):
//                        ReadInteger(startPos, tokenStart);
//                        break;

//                    case var c when char.IsLetter(c):
//                        ProcessIdentifierOrError(startPos, tokenStart);
//                        break;

//                    default:
//                        Advance();
//                        break;
//                }
//            }

//            return _tokens;
//        }

//        private void ProcessIdentifierOrError(int startPos, int tokenStart)
//        {
//            if (!_expectIdentifier)
//            {
//                HandleInvalidSequence(startPos, tokenStart);
//                return;
//            }

//            ReadIdentifier(startPos, tokenStart);
//        }

//        private void ReadIdentifier(int startPos, int tokenStart)
//        {
//            StringBuilder sb = new StringBuilder();
//            while (!IsEnd() && (char.IsLetterOrDigit(CurrentChar()) || CurrentChar() == '_'))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();
//            if (Keywords.ContainsKey(lexeme))
//            {
//                AddToken(TokenCode.Error, "неожиданное ключевое слово", lexeme, startPos, tokenStart,
//                    $"Ключевое слово '{lexeme}' не может быть идентификатором");
//            }
//            else if (IdentifierRegex.IsMatch(lexeme))
//            {
//                AddToken(TokenCode.Identifier, "идентификатор", lexeme, startPos, tokenStart);
//                _expectIdentifier = false;
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "недопустимый идентификатор", lexeme, startPos, tokenStart,
//                    $"Недопустимый формат идентификатора: '{lexeme}'");
//            }
//        }

//        private bool IsEnd() => _pos >= _text.Length;

//        private char CurrentChar() => _pos < _text.Length ? _text[_pos] : '\0';

//        private void Advance()
//        {
//            if (CurrentChar() == '\n')
//            {
//                _line++;
//                _linePos = 1;
//            }
//            else
//            {
//                _linePos++;
//            }
//            _pos++;
//        }

//        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int tokenStart, string errorMessage = null)
//        {
//            _tokens.Add(new Token
//            {
//                Code = code,
//                Type = type,
//                Lexeme = lexeme,
//                StartPos = startPos,
//                EndPos = _linePos - 1,
//                Line = _line,
//                ErrorMessage = errorMessage
//            });
//        }

//        private bool CheckForKeyword(string keyword)
//        {
//            if (_pos + keyword.Length > _text.Length)
//                return false;

//            for (int i = 0; i < keyword.Length; i++)
//            {
//                if (_text[_pos + i] != keyword[i])
//                    return false;
//            }

//            _pos += keyword.Length;
//            _linePos += keyword.Length;
//            return true;
//        }

//        private void ReadStringLiteral(int startPos, int tokenStart)
//        {
//            StringBuilder sb = new StringBuilder();
//            Advance(); // Пропускаем открывающую кавычку

//            while (!IsEnd() && CurrentChar() != '"')
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            if (CurrentChar() == '"')
//            {
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, tokenStart);
//                Advance();
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, tokenStart,
//                    "Незакрытый строковый литерал");
//            }
//        }

//        private void ReadInteger(int startPos, int tokenStart)
//        {
//            StringBuilder sb = new StringBuilder();
//            while (!IsEnd() && char.IsDigit(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }
//            AddToken(TokenCode.Integer, "целое число", sb.ToString(), startPos, tokenStart);
//        }

//        private void HandleInvalidSequence(int startPos, int tokenStart)
//        {
//            StringBuilder sb = new StringBuilder();
//            while (!IsEnd() && !char.IsWhiteSpace(CurrentChar()) && !IsValidChar(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            if (sb.Length > 0)
//            {
//                AddToken(TokenCode.Error, "недопустимый символ", sb.ToString(), startPos, tokenStart,
//                    $"Недопустимая последовательность символов: '{sb}'");
//            }
//        }

//        private bool IsValidChar(char ch)
//        {
//            return char.IsLetterOrDigit(ch) || ch == '_' || ch == ':' || ch == '=' || ch == '"' || ch == '&' || ch == ';';
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Text;

namespace Compilatori2Laba
{
    public enum TokenCode
    {
        Integer = 1,          // целое число
        Identifier = 2,       // идентификатор
        StringLiteral = 3,    // строковый литерал
        AssignOp = 10,        // знак "="
        Separator = 11,       // разделитель (пробел, специальный символ)
        Keyword = 14,         // ключевые слова: const, val
        EndOperator = 16,     // конец оператора ";"
        Error = 99            // ошибка
    }

    public class Token
    {
        public TokenCode Code { get; set; }
        public string Type { get; set; }
        public string Lexeme { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public int Line { get; set; }

        public override string ToString()
        {
            return $"Строка: {Line}, с позиции {StartPos} по {EndPos} — {Type}: \"{Lexeme}\" (код {(int)Code})";
        }
    }

    public class Scanner
    {
        private string _text;
        private int _pos;
        private int _line;
        private int _linePos;
        private List<Token> _tokens;

        private bool _expectingIdentifier = false; // Ожидаем идентификатор после const или после других ключевых символов

        public Scanner()
        {
            _tokens = new List<Token>();
        }

        public List<Token> Scan(string text)
        {
            _text = text;
            _pos = 0;
            _line = 1;
            _linePos = 1;
            _tokens.Clear();

            while (!IsEnd())
            {
                char ch = CurrentChar();
                int startPos = _linePos;
                int startTokenPos = _pos;

                switch (ch)
                {
                    case '\n':
                        Advance(); // Переход на новую строку
                        break;

                    case var c when char.IsWhiteSpace(c):
                        Advance();
                        break;

                    case 'c': // Проверяем ключевое слово "const" или идентификатор, начинающийся с "c"
                        if (CheckForKeyword("const"))
                        {
                            AddToken(TokenCode.Keyword, "ключевое слово", "const", startPos, startTokenPos);
                            _expectingIdentifier = true; // После const ожидаем идентификатор
                        }
                        else if (char.IsLetter(CurrentChar())) // Если это просто часть идентификатора, например "co4nst"
                        {
                            ReadIdentifier(startPos, startTokenPos);
                        }
                        break;

                    case ':':
                        AddToken(TokenCode.Separator, "специальный символ", ":", startPos, startTokenPos);
                        Advance();
                        break;

                    case '&': // Тип "&str"
                        if (CheckForKeyword("&str"))
                        {
                            AddToken(TokenCode.Keyword, "тип", "&str", startPos, startTokenPos);
                            _expectingIdentifier = false;
                        }
                        else
                        {
                            // Добавляем только текущий символ & как ошибку
                            AddToken(TokenCode.Error, "недопустимый символ", "&", startPos, startTokenPos);
                            Advance(); // Переходим к следующему символу
                        }
                        break;
                    //case '&': // Тип "&str"
                    //    if (CheckForKeyword("&str"))
                    //    {
                    //        AddToken(TokenCode.Keyword, "тип", "&str", startPos, startTokenPos);
                    //        Advance();
                    //        _expectingIdentifier = false; // После &str не ожидаем идентификатор
                    //    }
                    //    break;

                    case '=':
                        AddToken(TokenCode.AssignOp, "оператор присваивания", "=", startPos, startTokenPos);
                        Advance();
                        _expectingIdentifier = false; // После = не ожидаем идентификатор
                        break;

                    case '"':
                        ReadStringLiteral(startPos, startTokenPos);
                        break;

                    case ';':
                        AddToken(TokenCode.EndOperator, "конец оператора", ";", startPos, startTokenPos);
                        Advance();
                        break;

                    case var c when char.IsDigit(c): // Обработка целых чисел
                        ReadInteger(startPos, startTokenPos);
                        break;

                    case var c when char.IsLetter(c): // Идентификатор
                        ReadIdentifier(startPos, startTokenPos);
                        break;

                    case var c when !IsValidChar(c): // Недопустимые символы
                        HandleInvalidSequence(startPos, startTokenPos);
                        break;

                    default:
                        Advance();
                        break;
                }
            }

            return _tokens;
        }

        private bool IsEnd()
        {
            return _pos >= _text.Length;
        }

        private char CurrentChar()
        {
            return _pos < _text.Length ? _text[_pos] : '\0';
        }

        private void Advance()
        {
            if (CurrentChar() == '\n')
            {
                _line++;
                _linePos = 1;
            }
            else
            {
                _linePos++;
            }
            _pos++;
        }

        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int startTokenPos)
        {
            AddToken(code, type, lexeme, startPos, _linePos - 1, _line);
        }

        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int endPos, int line)
        {
            _tokens.Add(new Token
            {
                Code = code,
                Type = type,
                Lexeme = lexeme,
                StartPos = startPos,
                EndPos = endPos,
                Line = line
            });
        }

        private bool CheckForKeyword(string keyword)
        {
            if (_pos + keyword.Length > _text.Length)
                return false;

            for (int i = 0; i < keyword.Length; i++)
            {
                if (_text[_pos + i] != keyword[i])
                    return false;
            }

            // Только если вся ключевая строка совпала
            _pos += keyword.Length;
            _linePos += keyword.Length;
            return true;
        }

        private void HandleInvalidSequence(int startPos, int startTokenPos)
        {
            // Собираем недопустимую последовательность символов в строку
            StringBuilder sb = new StringBuilder();
            while (!IsEnd() && !char.IsWhiteSpace(CurrentChar()) && !IsValidChar(CurrentChar()))
            {
                sb.Append(CurrentChar());
                Advance();
            }

            string lexeme = sb.ToString();
            if (lexeme.Length > 0)
            {
                AddToken(TokenCode.Error, "недопустимый символ", lexeme, startPos, startTokenPos);
            }
        }

        private void ReadStringLiteral(int startPos, int startTokenPos)
        {
            StringBuilder sb = new StringBuilder();
            Advance(); // Пропускаем открывающую кавычку

            while (!IsEnd() && CurrentChar() != '"')
            {
                sb.Append(CurrentChar());
                Advance();
            }

            if (CurrentChar() == '"')
            {
                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, startTokenPos);
                Advance();
            }
            else
            {
                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, startTokenPos);
            }
        }

        private void ReadInteger(int startPos, int startTokenPos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CurrentChar());
            Advance();

            while (!IsEnd() && char.IsDigit(CurrentChar()))
            {
                sb.Append(CurrentChar());
                Advance();
            }

            string lexeme = sb.ToString();
            AddToken(TokenCode.Integer, "целое число", lexeme, startPos, startTokenPos);
        }

        private void ReadIdentifier(int startPos, int startTokenPos)
        {
            // Проверяем, можем ли мы ожидать идентификатор в текущем контексте
            if (!_expectingIdentifier)
            {
                AddToken(TokenCode.Error, "недопустимый символ", CurrentChar().ToString(), startPos, startTokenPos);
                Advance();
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(CurrentChar());
            Advance();

            while (!IsEnd() && (char.IsLetterOrDigit(CurrentChar()) || CurrentChar() == '_'))
            {
                sb.Append(CurrentChar());
                Advance();
            }

            string lexeme = sb.ToString();

            // Проверка, является ли идентификатор валидным
            if (IsValidIdentifier(lexeme))
            {
                AddToken(TokenCode.Identifier, "идентификатор", lexeme, startPos, startTokenPos);
                _expectingIdentifier = false; // После идентификатора ожидаем другой токен
            }
            else
            {
                AddToken(TokenCode.Error, "недопустимый символ", lexeme, startPos, startTokenPos);
            }
        }

        // Метод для проверки, является ли идентификатор валидным
        private bool IsValidIdentifier(string identifier)
        {
            // Идентификатор должен начинаться с латинской буквы или подчеркивания, и может содержать цифры
            return System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        private string GetCurrentLexeme()
        {
            return _text.Substring(_pos);
        }

        // Метод для проверки, является ли символ допустимым
        private bool IsValidChar(char ch)
        {
            // Допустимые символы для имен переменных, типов и строк
            return char.IsLetterOrDigit(ch) || ch == '_' || ch == ':' || ch == '=' || ch == '"' || ch == '&' || ch == ';';
        }
    }
}
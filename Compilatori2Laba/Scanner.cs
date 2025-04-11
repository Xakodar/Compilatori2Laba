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
//----------------------------------------------------------------------------------------- надо
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Compilatori2Laba
//{
//    public enum TokenCode
//    {
//        Integer = 1,          // целое число
//        Identifier = 2,       // идентификатор
//        StringLiteral = 3,    // строковый литерал
//        AssignOp = 10,        // знак "="
//        Separator = 11,       // разделитель (пробел, специальный символ)
//        Keyword = 14,         // ключевые слова: const, val
//        EndOperator = 16,     // конец оператора ";"
//        Error = 99            // ошибка
//    }

//    public class Token
//    {
//        public TokenCode Code { get; set; }
//        public string Type { get; set; }
//        public string Lexeme { get; set; }
//        public int StartPos { get; set; }
//        public int EndPos { get; set; }
//        public int Line { get; set; }

//        public override string ToString()
//        {
//            return $"Строка: {Line}, с позиции {StartPos} по {EndPos} — {Type}: \"{Lexeme}\" (код {(int)Code})";
//        }
//    }

//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;

//        private bool _expectingIdentifier = false; // Ожидаем идентификатор после const или после других ключевых символов

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

//            while (!IsEnd())
//            {
//                char ch = CurrentChar();
//                int startPos = _linePos;
//                int startTokenPos = _pos;

//                switch (ch)
//                {
//                    case '\n':
//                        Advance(); // Переход на новую строку
//                        break;

//                    case var c when char.IsWhiteSpace(c):
//                        Advance();
//                        break;

//                    case 'c': // Проверяем ключевое слово "const" или идентификатор, начинающийся с "c"
//                        if (CheckForKeyword("const"))
//                        {
//                            AddToken(TokenCode.Keyword, "ключевое слово", "const", startPos, startTokenPos);
//                            _expectingIdentifier = true; // После const ожидаем идентификатор
//                        }
//                        else if (char.IsLetter(CurrentChar())) // Если это просто часть идентификатора, например "co4nst"
//                        {
//                            ReadIdentifier(startPos, startTokenPos);
//                        }
//                        break;

//                    case ':':
//                        AddToken(TokenCode.Separator, "специальный символ", ":", startPos, startTokenPos);
//                        Advance();
//                        break;

//                    case '&': // Тип "&str"
//                        if (CheckForKeyword("&str"))
//                        {
//                            AddToken(TokenCode.Keyword, "тип", "&str", startPos, startTokenPos);
//                            _expectingIdentifier = false;
//                        }
//                        else
//                        {
//                            // Добавляем только текущий символ & как ошибку
//                            AddToken(TokenCode.Error, "недопустимый символ", "&", startPos, startTokenPos);
//                            Advance(); // Переходим к следующему символу
//                        }
//                        break;
//                    //case '&': // Тип "&str"
//                    //    if (CheckForKeyword("&str"))
//                    //    {
//                    //        AddToken(TokenCode.Keyword, "тип", "&str", startPos, startTokenPos);
//                    //        Advance();
//                    //        _expectingIdentifier = false; // После &str не ожидаем идентификатор
//                    //    }
//                    //    break;

//                    case '=':
//                        AddToken(TokenCode.AssignOp, "оператор присваивания", "=", startPos, startTokenPos);
//                        Advance();
//                        _expectingIdentifier = false; // После = не ожидаем идентификатор
//                        break;

//                    case '"':
//                        ReadStringLiteral(startPos, startTokenPos);
//                        break;

//                    case ';':
//                        AddToken(TokenCode.EndOperator, "конец оператора", ";", startPos, startTokenPos);
//                        Advance();
//                        break;

//                    case var c when char.IsDigit(c): // Обработка целых чисел
//                        ReadInteger(startPos, startTokenPos);
//                        break;

//                    case var c when char.IsLetter(c): // Идентификатор
//                        ReadIdentifier(startPos, startTokenPos);
//                        break;

//                    case var c when !IsValidChar(c): // Недопустимые символы
//                        HandleInvalidSequence(startPos, startTokenPos);
//                        break;

//                    default:
//                        Advance();
//                        break;
//                }
//            }

//            return _tokens;
//        }

//        private bool IsEnd()
//        {
//            return _pos >= _text.Length;
//        }

//        private char CurrentChar()
//        {
//            return _pos < _text.Length ? _text[_pos] : '\0';
//        }

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

//        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int startTokenPos)
//        {
//            AddToken(code, type, lexeme, startPos, _linePos - 1, _line);
//        }

//        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int endPos, int line)
//        {
//            _tokens.Add(new Token
//            {
//                Code = code,
//                Type = type,
//                Lexeme = lexeme,
//                StartPos = startPos,
//                EndPos = endPos,
//                Line = line
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

//            // Только если вся ключевая строка совпала
//            _pos += keyword.Length;
//            _linePos += keyword.Length;
//            return true;
//        }

//        private void HandleInvalidSequence(int startPos, int startTokenPos)
//        {
//            // Собираем недопустимую последовательность символов в строку
//            StringBuilder sb = new StringBuilder();
//            while (!IsEnd() && !char.IsWhiteSpace(CurrentChar()) && !IsValidChar(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();
//            if (lexeme.Length > 0)
//            {
//                AddToken(TokenCode.Error, "недопустимый символ", lexeme, startPos, startTokenPos);
//            }
//        }

//        private void ReadStringLiteral(int startPos, int startTokenPos)
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
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, startTokenPos);
//                Advance();
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, startTokenPos);
//            }
//        }

//        private void ReadInteger(int startPos, int startTokenPos)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.Append(CurrentChar());
//            Advance();

//            while (!IsEnd() && char.IsDigit(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();
//            AddToken(TokenCode.Integer, "целое число", lexeme, startPos, startTokenPos);
//        }

//        private void ReadIdentifier(int startPos, int startTokenPos)
//        {
//            // Проверяем, можем ли мы ожидать идентификатор в текущем контексте
//            if (!_expectingIdentifier)
//            {
//                AddToken(TokenCode.Error, "недопустимый символ", CurrentChar().ToString(), startPos, startTokenPos);
//                Advance();
//                return;
//            }

//            StringBuilder sb = new StringBuilder();
//            sb.Append(CurrentChar());
//            Advance();

//            while (!IsEnd() && (char.IsLetterOrDigit(CurrentChar()) || CurrentChar() == '_'))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();

//            // Проверка, является ли идентификатор валидным
//            if (IsValidIdentifier(lexeme))
//            {
//                AddToken(TokenCode.Identifier, "идентификатор", lexeme, startPos, startTokenPos);
//                _expectingIdentifier = false; // После идентификатора ожидаем другой токен
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "недопустимый символ", lexeme, startPos, startTokenPos);
//            }
//        }

//        // Метод для проверки, является ли идентификатор валидным
//        private bool IsValidIdentifier(string identifier)
//        {
//            // Идентификатор должен начинаться с латинской буквы или подчеркивания, и может содержать цифры
//            return System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
//        }

//        private string GetCurrentLexeme()
//        {
//            return _text.Substring(_pos);
//        }

//        // Метод для проверки, является ли символ допустимым
//        private bool IsValidChar(char ch)
//        {
//            // Допустимые символы для имен переменных, типов и строк
//            return char.IsLetterOrDigit(ch) || ch == '_' || ch == ':' || ch == '=' || ch == '"' || ch == '&' || ch == ';';
//        }
//    }
//}//-----------------------------------
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Compilatori2Laba
//{
//    public enum TokenCode
//    {
//        Integer = 1,          // целое число
//        Identifier = 2,       // идентификатор
//        StringLiteral = 3,    // строковый литерал
//        AssignOp = 10,        // знак "="
//        Separator = 11,       // разделитель (например, ':')
//        Keyword = 14,         // ключевые слова (например, const, &str)
//        EndOperator = 16,     // символ ';'
//        Error = 99            // ошибка
//    }

//    public class Token
//    {
//        public TokenCode Code { get; set; }
//        public string Type { get; set; }
//        public string Lexeme { get; set; }
//        public int StartPos { get; set; }
//        public int EndPos { get; set; }
//        public int Line { get; set; }

//        public override string ToString()
//        {
//            return $"Строка: {Line}, с позиции {StartPos} по {EndPos} — {Type}: \"{Lexeme}\" (код {(int)Code})";
//        }
//    }

//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;

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

//            while (!IsEnd())
//            {
//                char ch = CurrentChar();
//                int startPos = _linePos;
//                int tokenStartPos = _pos;

//                // Обработка перевода строки
//                if (ch == '\n')
//                {
//                    Advance();
//                    continue;
//                }

//                // Пропуск пробелов
//                if (char.IsWhiteSpace(ch))
//                {
//                    Advance();
//                    continue;
//                }

//                // Обработка чисел
//                if (char.IsDigit(ch))
//                {
//                    ReadInteger(startPos, tokenStartPos);
//                    continue;
//                }

//                // Обработка строковых литералов
//                if (ch == '"')
//                {
//                    ReadStringLiteral(startPos, tokenStartPos);
//                    continue;
//                }

//                // Обработка специальных символов
//                if (ch == ':')
//                {
//                    AddToken(TokenCode.Separator, "специальный символ", ":", startPos, tokenStartPos);
//                    Advance();
//                    continue;
//                }

//                if (ch == '=')
//                {
//                    AddToken(TokenCode.AssignOp, "оператор присваивания", "=", startPos, tokenStartPos);
//                    Advance();
//                    continue;
//                }

//                if (ch == ';')
//                {
//                    AddToken(TokenCode.EndOperator, "конец оператора", ";", startPos, tokenStartPos);
//                    Advance();
//                    continue;
//                }

//                // Обработка символа '&' – особый случай для типа "&str"
//                if (ch == '&')
//                {
//                    StringBuilder sb = new StringBuilder();
//                    while (!IsEnd() && !char.IsWhiteSpace(CurrentChar())
//                           && CurrentChar() != ':' && CurrentChar() != '=' && CurrentChar() != ';')
//                    {
//                        sb.Append(CurrentChar());
//                        Advance();
//                    }
//                    string lexeme = sb.ToString();
//                    if (lexeme == "&str")
//                    {
//                        AddToken(TokenCode.Keyword, "тип", lexeme, startPos, tokenStartPos);
//                    }
//                    else
//                    {
//                        AddToken(TokenCode.Error, "недопустимый символ", lexeme, startPos, tokenStartPos);
//                    }
//                    continue;
//                }

//                // Обработка последовательности букв и символа подчеркивания (идентификаторы и ключевые слова)
//                if (char.IsLetter(ch) || ch == '_')
//                {
//                    ReadIdentifier(startPos, tokenStartPos);
//                    continue;
//                }

//                // Если символ не распознан – выдаем ошибку
//                AddToken(TokenCode.Error, "недопустимый символ", ch.ToString(), startPos, tokenStartPos);
//                Advance();
//            }

//            return _tokens;
//        }

//        private bool IsEnd()
//        {
//            return _pos >= _text.Length;
//        }

//        private char CurrentChar()
//        {
//            return _pos < _text.Length ? _text[_pos] : '\0';
//        }

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

//        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int tokenStartPos)
//        {
//            AddToken(code, type, lexeme, startPos, _linePos - 1, _line);
//        }

//        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int endPos, int line)
//        {
//            _tokens.Add(new Token
//            {
//                Code = code,
//                Type = type,
//                Lexeme = lexeme,
//                StartPos = startPos,
//                EndPos = endPos,
//                Line = line
//            });
//        }

//        private void ReadInteger(int startPos, int tokenStartPos)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.Append(CurrentChar());
//            Advance();

//            while (!IsEnd() && char.IsDigit(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();
//            AddToken(TokenCode.Integer, "целое число", lexeme, startPos, tokenStartPos);
//        }

//        private void ReadStringLiteral(int startPos, int tokenStartPos)
//        {
//            StringBuilder sb = new StringBuilder();
//            Advance(); // пропускаем открывающую кавычку

//            while (!IsEnd() && CurrentChar() != '"')
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            if (!IsEnd() && CurrentChar() == '"')
//            {
//                Advance(); // пропускаем закрывающую кавычку
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, tokenStartPos);
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, tokenStartPos);
//            }
//        }

//        private void ReadIdentifier(int startPos, int tokenStartPos)
//        {
//            StringBuilder sb = new StringBuilder();

//            // Считываем целиком последовательность символов, допустимых в идентификаторе
//            while (!IsEnd() && (char.IsLetterOrDigit(CurrentChar()) || CurrentChar() == '_'))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }
//            string lexeme = sb.ToString();

//            // Если лексема точно равна "const" – это ключевое слово, иначе идентификатор.
//            if (lexeme == "const")
//            {
//                AddToken(TokenCode.Keyword, "ключевое слово", lexeme, startPos, tokenStartPos);
//            }
//            else
//            {
//                AddToken(TokenCode.Identifier, "идентификатор", lexeme, startPos, tokenStartPos);
//            }
//        }
//    }
//}
// Новый сканер------------------- внизу
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Compilatori2Laba
{
    // Перечисление кодов токенов
    public enum TokenCode
    {
        Integer = 1,          // целое число
        Identifier = 2,       // идентификатор
        StringLiteral = 3,    // строковый литерал
        AssignOp = 10,        // знак "="
        Separator = 11,       // разделитель (пробел, специальный символ)
        Keyword = 14,         // ключевые слова: const, val и пр.
        EndOperator = 16,     // конец оператора ";"
        Error = 99            // ошибка
    }

    // Класс токенов
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

    // Сканер для анализа кода
    public class Scanner
    {
        private string _text;
        private int _pos;
        private int _line;
        private int _linePos;
        private List<Token> _tokens;
        private bool _insideString = false; // Переменная для отслеживания строки
        private HashSet<string> _keywords = new HashSet<string>
        {
            "const", "let", "fn", "mod", "struct" // добавь ключевые слова для Rust или другого языка
        };

        public Scanner()
        {
            _tokens = new List<Token>();
        }

        // Метод сканирования текста
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
                    case '\r':
                    case '\n':
                        Advance(); // Просто пропускаем переход на новую строку
                        break;

                    case var c when Char.IsWhiteSpace(c):
                        Advance();
                        break;

                    case var c when (Char.IsLetter(c) || c == '_'):
                        ReadMixedIdentifierOrError();
                        break;

                    case var c when Char.IsDigit(c):
                        ReadInteger();
                        break;

                    case '=':
                        AddToken(TokenCode.AssignOp, "оператор присваивания", "=");
                        Advance();
                        break;

                    case ';':
                        AddToken(TokenCode.EndOperator, "конец оператора", ";");
                        Advance();
                        break;

                    case '"':
                        ReadStringLiteral();
                        break;

                    case ':':
                        AddToken(TokenCode.Separator, "специальный символ", ":");
                        Advance();
                        break;

                    // Проверка на тип "&str"
                    case '&':
                        if (CheckForKeyword("&str"))
                        {
                            AddToken(TokenCode.Keyword, "тип", "&str");
                        }
                        else
                        {
                            AddToken(TokenCode.Error, "недопустимый символ", "&");
                        }
                        Advance();
                        break;

                    default:
                        // Для остальных символов группируем подряд идущие недопустимые
                        int startErrorPos = _linePos;
                        StringBuilder errorSb = new StringBuilder();
                        while (!IsEnd() && !IsValidTokenStart(CurrentChar()))
                        {
                            errorSb.Append(CurrentChar());
                            Advance();
                        }
                        AddToken(TokenCode.Error, "недопустимый символ", errorSb.ToString(), startErrorPos, _linePos - 1, _line);
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
                _linePos = 0;
            }
            _pos++;
            _linePos++;
        }

        private void AddToken(TokenCode code, string type, string lexeme)
        {
            AddToken(code, type, lexeme, _linePos, _linePos, _line);
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

        // Вспомогательный метод: является ли символ допустимым началом токена (для остальных случаев)
        private bool IsValidTokenStart(char ch)
        {
            if (ch == '\r' || ch == '\n')
                return true;
            if (char.IsWhiteSpace(ch))
                return true;
            if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
                return true;
            if (char.IsDigit(ch))
                return true;
            if (ch == '_' || ch == '=' || ch == ';' || ch == '"')
                return true;
            return false;
        }

        // Вспомогательный метод, проверяющий, относится ли символ к «словообразующим»
        private bool IsWordChar(char ch)
        {
            return char.IsLetter(ch) || char.IsDigit(ch) || ch == '_';
        }

        // Вспомогательный метод, определяющий, является ли символ допустимым для идентификатора
        // (т.е. английская буква, цифра или '_')
        private bool IsAllowedIdentifierChar(char ch)
        {
            return (ch >= 'A' && ch <= 'Z') ||
                   (ch >= 'a' && ch <= 'z') ||
                   (ch >= '0' && ch <= '9') ||
                   (ch == '_');
        }

        // Новый метод для считывания последовательности, состоящей из букв, цифр и '_'
        // При этом последовательность делится на сегменты, где каждая группа либо состоит
        // из допустимых (английских) символов, либо из недопустимых (например, русских)
        private void ReadMixedIdentifierOrError()
        {
            // Запоминаем позицию начала всей последовательности
            int tokenStartPos = _linePos;
            StringBuilder wordSb = new StringBuilder();

            // Считываем всю последовательность из буквы, цифры или '_'
            while (!IsEnd() && IsWordChar(CurrentChar()))
            {
                wordSb.Append(CurrentChar());
                Advance();
            }

            string word = wordSb.ToString();
            int segmentStart = 0;
            // Определяем тип первого символа
            bool currentAllowed = IsAllowedIdentifierChar(word[0]);

            for (int i = 1; i < word.Length; i++)
            {
                bool allowed = IsAllowedIdentifierChar(word[i]);
                if (allowed != currentAllowed)
                {
                    // Завершаем сегмент от segmentStart до i-1
                    string segment = word.Substring(segmentStart, i - segmentStart);
                    TokenCode code = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
                    string type = currentAllowed ? "идентификатор" : "недопустимый символ";
                    // Если сегмент полностью совпадает с ключевым словом, меняем тип
                    if (currentAllowed && _keywords.Contains(segment))
                    {
                        code = TokenCode.Keyword;
                        type = "ключевое слово";
                    }
                    AddToken(code, type, segment, tokenStartPos + segmentStart, tokenStartPos + i - 1, _line);
                    // Начинаем новый сегмент с текущего символа
                    segmentStart = i;
                    currentAllowed = allowed;
                }
            }
            // Завершаем последний сегмент
            string lastSegment = word.Substring(segmentStart);
            TokenCode lastCode = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
            string lastType = currentAllowed ? "идентификатор" : "недопустимый символ";
            AddToken(lastCode, lastType, lastSegment, tokenStartPos + segmentStart, tokenStartPos + word.Length - 1, _line);
        }

        private void ReadInteger()
        {
            int startPos = _linePos;
            StringBuilder sb = new StringBuilder();
            sb.Append(CurrentChar());
            Advance();

            while (!IsEnd() && char.IsDigit(CurrentChar()))
            {
                sb.Append(CurrentChar());
                Advance();
            }

            string lexeme = sb.ToString();
            AddToken(TokenCode.Integer, "целое число", lexeme, startPos, _linePos - 1, _line);
        }

        private void ReadStringLiteral()
        {
            int startPos = _linePos;
            Advance(); // Пропускаем открывающую кавычку
            StringBuilder sb = new StringBuilder();
            bool closed = false;

            while (!IsEnd())
            {
                char ch = CurrentChar();
                if (ch == '"')
                {
                    closed = true;
                    Advance();
                    break;
                }
                else
                {
                    sb.Append(ch);
                    Advance();
                }
            }

            if (closed)
            {
                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
            }
            else
            {
                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, _linePos - 1, _line);
                while (!IsEnd())
                {
                    AddToken(TokenCode.Error, "недопустимый символ", CurrentChar().ToString(), _linePos, _linePos, _line);
                    Advance();
                }
            }
        }

        // Проверка, является ли символ русским
        private bool IsRussianLetter(char ch)
        {
            return (ch >= 'А' && ch <= 'я') || (ch >= 'а' && ch <= 'я');
        }

        // Метод проверки на ключевое слово
        private bool CheckForKeyword(string keyword)
        {
            if (_pos + keyword.Length > _text.Length)
                return false;

            for (int i = 0; i < keyword.Length; i++)
            {
                if (_text[_pos + i] != keyword[i])
                    return false;
            }

            // Если ключевое слово совпало, смещаем позицию
            _pos += keyword.Length;
            _linePos += keyword.Length;
            return true;
        }
    }
}








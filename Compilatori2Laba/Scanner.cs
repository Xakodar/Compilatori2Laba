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
//using System.Text.RegularExpressions;

//namespace Compilatori2Laba
//{
//    // Перечисление кодов токенов
//    public enum TokenCode
//    {
//        Integer = 1,          // целое число
//        Identifier = 2,       // идентификатор
//        StringLiteral = 3,    // строковый литерал
//        AssignOp = 10,        // знак "="
//        Separator = 11,       // разделитель (пробел, специальный символ)
//        Keyword = 14,         // ключевые слова: const, val и пр.
//        EndOperator = 16,     // конец оператора ";"
//        Error = 99            // ошибка
//    }

//    // Класс токенов
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

//    // Сканер для анализа кода
//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;
//        private bool _insideString = false; // Переменная для отслеживания строки
//        private HashSet<string> _keywords = new HashSet<string>
//        {
//            "const", "let", "fn", "mod", "struct" // добавь ключевые слова для Rust или другого языка
//        };

//        public Scanner()
//        {
//            _tokens = new List<Token>();
//        }

//        // Метод сканирования текста
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
//                    case '\r':
//                    case '\n':
//                        Advance(); // Просто пропускаем переход на новую строку
//                        break;

//                    case var c when Char.IsWhiteSpace(c):
//                        Advance();
//                        break;

//                    case var c when (Char.IsLetter(c) || c == '_'):
//                        ReadMixedIdentifierOrError();
//                        break;

//                    case var c when Char.IsDigit(c):
//                        ReadInteger();
//                        break;

//                    case '=':
//                        AddToken(TokenCode.AssignOp, "оператор присваивания", "=");
//                        Advance();
//                        break;

//                    case ';':
//                        AddToken(TokenCode.EndOperator, "конец оператора", ";");
//                        Advance();
//                        break;

//                    case '"':
//                        ReadStringLiteral();
//                        break;

//                    case ':':
//                        AddToken(TokenCode.Separator, "специальный символ", ":");
//                        Advance();
//                        break;

//                    // Проверка на тип "&str"
//                    case '&':
//                        if (CheckForKeyword("&str"))
//                        {
//                            AddToken(TokenCode.Keyword, "тип", "&str");
//                        }
//                        else
//                        {
//                            AddToken(TokenCode.Error, "недопустимый символ", "&");
//                        }
//                        Advance();
//                        break;

//                    default:
//                        // Для остальных символов группируем подряд идущие недопустимые
//                        int startErrorPos = _linePos;
//                        StringBuilder errorSb = new StringBuilder();
//                        while (!IsEnd() && !IsValidTokenStart(CurrentChar()))
//                        {
//                            errorSb.Append(CurrentChar());
//                            Advance();
//                        }
//                        AddToken(TokenCode.Error, "недопустимый символ", errorSb.ToString(), startErrorPos, _linePos - 1, _line);
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
//                _linePos = 0;
//            }
//            _pos++;
//            _linePos++;
//        }

//        private void AddToken(TokenCode code, string type, string lexeme)
//        {
//            AddToken(code, type, lexeme, _linePos, _linePos, _line);
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

//        // Вспомогательный метод: является ли символ допустимым началом токена (для остальных случаев)
//        private bool IsValidTokenStart(char ch)
//        {
//            if (ch == '\r' || ch == '\n')
//                return true;
//            if (char.IsWhiteSpace(ch))
//                return true;
//            if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
//                return true;
//            if (char.IsDigit(ch))
//                return true;
//            if (ch == '_' || ch == '=' || ch == ';' || ch == '"')
//                return true;
//            return false;
//        }

//        // Вспомогательный метод, проверяющий, относится ли символ к «словообразующим»
//        private bool IsWordChar(char ch)
//        {
//            return char.IsLetter(ch) || char.IsDigit(ch) || ch == '_';
//        }

//        // Вспомогательный метод, определяющий, является ли символ допустимым для идентификатора
//        // (т.е. английская буква, цифра или '_')
//        private bool IsAllowedIdentifierChar(char ch)
//        {
//            return (ch >= 'A' && ch <= 'Z') ||
//                   (ch >= 'a' && ch <= 'z') ||
//                   (ch >= '0' && ch <= '9') ||
//                   (ch == '_');
//        }

//        // Новый метод для считывания последовательности, состоящей из букв, цифр и '_'
//        // При этом последовательность делится на сегменты, где каждая группа либо состоит
//        // из допустимых (английских) символов, либо из недопустимых (например, русских)
//        private void ReadMixedIdentifierOrError()
//        {
//            // Запоминаем позицию начала всей последовательности
//            int tokenStartPos = _linePos;
//            StringBuilder wordSb = new StringBuilder();

//            // Считываем всю последовательность из буквы, цифры или '_'
//            while (!IsEnd() && IsWordChar(CurrentChar()))
//            {
//                wordSb.Append(CurrentChar());
//                Advance();
//            }

//            string word = wordSb.ToString();
//            int segmentStart = 0;
//            // Определяем тип первого символа
//            bool currentAllowed = IsAllowedIdentifierChar(word[0]);

//            for (int i = 1; i < word.Length; i++)
//            {
//                bool allowed = IsAllowedIdentifierChar(word[i]);
//                if (allowed != currentAllowed)
//                {
//                    // Завершаем сегмент от segmentStart до i-1
//                    string segment = word.Substring(segmentStart, i - segmentStart);
//                    TokenCode code = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
//                    string type = currentAllowed ? "идентификатор" : "недопустимый символ";
//                    // Если сегмент полностью совпадает с ключевым словом, меняем тип
//                    if (currentAllowed && _keywords.Contains(segment))
//                    {
//                        code = TokenCode.Keyword;
//                        type = "ключевое слово";
//                    }
//                    AddToken(code, type, segment, tokenStartPos + segmentStart, tokenStartPos + i - 1, _line);
//                    // Начинаем новый сегмент с текущего символа
//                    segmentStart = i;
//                    currentAllowed = allowed;
//                }
//            }
//            // Завершаем последний сегмент
//            string lastSegment = word.Substring(segmentStart);
//            TokenCode lastCode = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
//            string lastType = currentAllowed ? "идентификатор" : "недопустимый символ";
//            AddToken(lastCode, lastType, lastSegment, tokenStartPos + segmentStart, tokenStartPos + word.Length - 1, _line);
//        }

//        private void ReadInteger()
//        {
//            int startPos = _linePos;
//            StringBuilder sb = new StringBuilder();
//            sb.Append(CurrentChar());
//            Advance();

//            while (!IsEnd() && char.IsDigit(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();
//            AddToken(TokenCode.Integer, "целое число", lexeme, startPos, _linePos - 1, _line);
//        }

//        private void ReadStringLiteral()
//        {
//            int startPos = _linePos;
//            Advance(); // Пропускаем открывающую кавычку
//            StringBuilder sb = new StringBuilder();
//            bool closed = false;

//            while (!IsEnd())
//            {
//                char ch = CurrentChar();
//                if (ch == '"')
//                {
//                    closed = true;
//                    Advance();
//                    break;
//                }
//                else
//                {
//                    sb.Append(ch);
//                    Advance();
//                }
//            }

//            if (closed)
//            {
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, _linePos - 1, _line);
//                while (!IsEnd())
//                {
//                    AddToken(TokenCode.Error, "недопустимый символ", CurrentChar().ToString(), _linePos, _linePos, _line);
//                    Advance();
//                }
//            }
//        }

//        // Проверка, является ли символ русским
//        private bool IsRussianLetter(char ch)
//        {
//            return (ch >= 'А' && ch <= 'я') || (ch >= 'а' && ch <= 'я');
//        }

//        // Метод проверки на ключевое слово
//        private bool CheckForKeyword(string keyword)
//        {
//            if (_pos + keyword.Length > _text.Length)
//                return false;

//            for (int i = 0; i < keyword.Length; i++)
//            {
//                if (_text[_pos + i] != keyword[i])
//                    return false;
//            }

//            // Если ключевое слово совпало, смещаем позицию
//            _pos += keyword.Length;
//            _linePos += keyword.Length;
//            return true;
//        }
//    }
//}
// Новый сканер------------------- внизу
//-------------------------------------------------------LR2
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace Compilatori2Laba
//{
//    // Перечисление кодов токенов
//    public enum TokenCode
//    {
//        Integer = 1,          // целое число
//        Identifier = 2,       // идентификатор
//        StringLiteral = 3,    // строковый литерал
//        AssignOp = 10,        // знак "="
//        Separator = 11,       // разделитель (пробел, специальный символ)
//        Keyword = 14,         // ключевые слова: const, val и пр.
//        EndOperator = 16,     // конец оператора ";"
//        Error = 99            // ошибка
//    }

//    // Класс токенов
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

//    // Сканер для анализа кода
//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;
//        private bool _insideString = false; // Переменная для отслеживания строки
//        private HashSet<string> _keywords = new HashSet<string>
//        {
//            "const", "let", "fn", "mod", "struct" // добавь ключевые слова для Rust или другого языка
//        };

//        public Scanner()
//        {
//            _tokens = new List<Token>();
//        }

//        // Метод сканирования текста
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
//                    case '\r':
//                    case '\n':
//                        Advance(); // Просто пропускаем переход на новую строку
//                        break;

//                    case var c when Char.IsWhiteSpace(c):
//                        Advance();
//                        break;

//                    case var c when (Char.IsLetter(c) || c == '_'):
//                        ReadMixedIdentifierOrError();
//                        break;

//                    case var c when Char.IsDigit(c):
//                        ReadInteger();
//                        break;

//                    case '=':
//                        AddToken(TokenCode.AssignOp, "оператор присваивания", "=");
//                        Advance();
//                        break;

//                    case ';':
//                        AddToken(TokenCode.EndOperator, "конец оператора", ";");
//                        Advance();
//                        break;

//                    case '"':
//                        ReadStringLiteral();
//                        break;

//                    case ':':
//                        AddToken(TokenCode.Separator, "специальный символ", ":");
//                        Advance();
//                        break;

//                    // Проверка на тип "&str"
//                    case '&':
//                        if (CheckForKeyword("&str"))
//                        {
//                            AddToken(TokenCode.Keyword, "тип", "&str");
//                        }
//                        else
//                        {
//                            AddToken(TokenCode.Error, "недопустимый символ", "&");
//                        }
//                        Advance();
//                        break;

//                    default:
//                        // Для остальных символов группируем подряд идущие недопустимые
//                        int startErrorPos = _linePos;
//                        StringBuilder errorSb = new StringBuilder();
//                        while (!IsEnd() && !IsValidTokenStart(CurrentChar()))
//                        {
//                            errorSb.Append(CurrentChar());
//                            Advance();
//                        }
//                        AddToken(TokenCode.Error, "недопустимый символ", errorSb.ToString(), startErrorPos, _linePos - 1, _line);
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
//                _linePos = 0;
//            }
//            _pos++;
//            _linePos++;
//        }

//        private void AddToken(TokenCode code, string type, string lexeme)
//        {
//            AddToken(code, type, lexeme, _linePos, _linePos, _line);
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

//        // Вспомогательный метод: является ли символ допустимым началом токена (для остальных случаев)
//        private bool IsValidTokenStart(char ch)
//        {
//            if (ch == '\r' || ch == '\n')
//                return true;
//            if (char.IsWhiteSpace(ch))
//                return true;
//            if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
//                return true;
//            if (char.IsDigit(ch))
//                return true;
//            if (ch == '_' || ch == '=' || ch == ';' || ch == '"')
//                return true;
//            return false;
//        }

//        // Вспомогательный метод, проверяющий, относится ли символ к «словообразующим»
//        private bool IsWordChar(char ch)
//        {
//            return char.IsLetter(ch) || char.IsDigit(ch) || ch == '_';
//        }

//        // Вспомогательный метод, определяющий, является ли символ допустимым для идентификатора
//        // (т.е. английская буква, цифра или '_')
//        private bool IsAllowedIdentifierChar(char ch)
//        {
//            return (ch >= 'A' && ch <= 'Z') ||
//                   (ch >= 'a' && ch <= 'z') ||
//                   (ch >= '0' && ch <= '9') ||
//                   (ch == '_');
//        }

//        // Новый метод для считывания последовательности, состоящей из букв, цифр и '_'
//        // При этом последовательность делится на сегменты, где каждая группа либо состоит
//        // из допустимых (английских) символов, либо из недопустимых (например, русских)
//        private void ReadMixedIdentifierOrError()
//        {
//            // Запоминаем позицию начала всей последовательности
//            int tokenStartPos = _linePos;
//            StringBuilder wordSb = new StringBuilder();

//            // Считываем всю последовательность из буквы, цифры или '_'
//            while (!IsEnd() && IsWordChar(CurrentChar()))
//            {
//                wordSb.Append(CurrentChar());
//                Advance();
//            }

//            string word = wordSb.ToString();
//            int segmentStart = 0;
//            // Определяем тип первого символа
//            bool currentAllowed = IsAllowedIdentifierChar(word[0]);

//            for (int i = 1; i < word.Length; i++)
//            {
//                bool allowed = IsAllowedIdentifierChar(word[i]);
//                if (allowed != currentAllowed)
//                {
//                    // Завершаем сегмент от segmentStart до i-1
//                    string segment = word.Substring(segmentStart, i - segmentStart);
//                    TokenCode code = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
//                    string type = currentAllowed ? "идентификатор" : "недопустимый символ";
//                    // Если сегмент полностью совпадает с ключевым словом, меняем тип
//                    if (currentAllowed && _keywords.Contains(segment))
//                    {
//                        code = TokenCode.Keyword;
//                        type = "ключевое слово";
//                    }
//                    AddToken(code, type, segment, tokenStartPos + segmentStart, tokenStartPos + i - 1, _line);
//                    // Начинаем новый сегмент с текущего символа
//                    segmentStart = i;
//                    currentAllowed = allowed;
//                }
//            }
//            // Завершаем последний сегмент
//            string lastSegment = word.Substring(segmentStart);
//            TokenCode lastCode = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
//            string lastType = currentAllowed ? "идентификатор" : "недопустимый символ";
//            AddToken(lastCode, lastType, lastSegment, tokenStartPos + segmentStart, tokenStartPos + word.Length - 1, _line);
//        }

//        private void ReadInteger()
//        {
//            int startPos = _linePos;
//            StringBuilder sb = new StringBuilder();
//            sb.Append(CurrentChar());
//            Advance();

//            while (!IsEnd() && char.IsDigit(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();
//            AddToken(TokenCode.Integer, "целое число", lexeme, startPos, _linePos - 1, _line);
//        }

//        private void ReadStringLiteral()
//        {
//            int startPos = _linePos;
//            Advance(); // Пропускаем открывающую кавычку
//            StringBuilder sb = new StringBuilder();
//            bool closed = false;

//            while (!IsEnd())
//            {
//                char ch = CurrentChar();
//                if (ch == '"')
//                {
//                    closed = true;
//                    Advance();
//                    break;
//                }
//                else
//                {
//                    sb.Append(ch);
//                    Advance();
//                }
//            }

//            if (closed)
//            {
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, _linePos - 1, _line);
//                while (!IsEnd())
//                {
//                    AddToken(TokenCode.Error, "недопустимый символ", CurrentChar().ToString(), _linePos, _linePos, _line);
//                    Advance();
//                }
//            }
//        }

//        // Проверка, является ли символ русским
//        private bool IsRussianLetter(char ch)
//        {
//            return (ch >= 'А' && ch <= 'я') || (ch >= 'а' && ch <= 'я');
//        }

//        // Метод проверки на ключевое слово
//        private bool CheckForKeyword(string keyword)
//        {
//            if (_pos + keyword.Length > _text.Length)
//                return false;

//            for (int i = 0; i < keyword.Length; i++)
//            {
//                if (_text[_pos + i] != keyword[i])
//                    return false;
//            }

//            // Если ключевое слово совпало, смещаем позицию
//            _pos += keyword.Length;
//            _linePos += keyword.Length;
//            return true;
//        }
//    }
//}-----------------------------------LR2
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Compilatori2Laba
//{
//    // Перечисление кодов токенов
//    public enum TokenCode
//    {
//        Integer = 1,          // целое число
//        Identifier = 2,       // идентификатор
//        StringLiteral = 3,    // строковый литерал
//        AssignOp = 10,        // знак "="
//        Separator = 11,       // разделитель (например, ':')
//        Keyword = 14,         // ключевые слова: const, let, fn, mod, struct и т.д.
//        EndOperator = 16,     // конец оператора ";"
//        Error = 99            // ошибка
//    }

//    // Класс токена
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

//    // Сканер для анализа входного текста
//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;
//        // Набор ключевых слов (расширяем по необходимости)
//        private HashSet<string> _keywords = new HashSet<string>
//        {
//            "const", "let", "fn", "mod", "struct"
//        };

//        public Scanner()
//        {
//            _tokens = new List<Token>();
//        }

//        /// <summary>
//        /// Основной метод сканирования. Принимает входной текст и возвращает список токенов.
//        /// </summary>
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

//                // Обработка переводов строки
//                if (ch == '\n' || ch == '\r')
//                {
//                    Advance();
//                    continue;
//                }

//                // Пропуск пробельных символов
//                if (char.IsWhiteSpace(ch))
//                {
//                    Advance();
//                    continue;
//                }

//                // Если начальный символ – буква или нижнее подчеркивание, обрабатываем как обычное слово
//                if (char.IsLetter(ch) || ch == '_')
//                {
//                    ReadMixedIdentifierOrError();
//                    continue;
//                }

//                // Если начальный символ равен '&', то это, скорее всего, тип; читаем токен до пробела.
//                if (ch == '&')
//                {
//                    ReadTypeToken();
//                    continue;
//                }

//                // Если цифра – считываем числовой литерал.
//                if (char.IsDigit(ch))
//                {
//                    ReadInteger();
//                    continue;
//                }

//                // Обработка оператора '='
//                if (ch == '=')
//                {
//                    AddToken(TokenCode.AssignOp, "оператор присваивания", "=", _linePos, _linePos, _line);
//                    Advance();
//                    continue;
//                }

//                // Обработка конца оператора ';'
//                if (ch == ';')
//                {
//                    AddToken(TokenCode.EndOperator, "конец оператора", ";", _linePos, _linePos, _line);
//                    Advance();
//                    continue;
//                }

//                // Обработка строковых литералов
//                if (ch == '"')
//                {
//                    ReadStringLiteral();
//                    continue;
//                }

//                // Обработка разделителя ':' (например, в конструкции "Name:&str")
//                if (ch == ':')
//                {
//                    AddToken(TokenCode.Separator, "специальный символ", ":", _linePos, _linePos, _line);
//                    Advance();
//                    continue;
//                }

//                // Для остальных символов группируем подряд идущие недопустимые символы.
//                int errorStart = _linePos;
//                StringBuilder errorSb = new StringBuilder();
//                while (!IsEnd() && !IsValidTokenStart(CurrentChar()))
//                {
//                    errorSb.Append(CurrentChar());
//                    Advance();
//                }
//                AddToken(TokenCode.Error, "недопустимый символ", errorSb.ToString(), errorStart, _linePos - 1, _line);
//            }

//            return _tokens;
//        }

//        // Возвращает true, если достигнут конец текста.
//        private bool IsEnd()
//        {
//            return _pos >= _text.Length;
//        }

//        // Возвращает текущий символ или '\0', если конец текста.
//        private char CurrentChar()
//        {
//            return _pos < _text.Length ? _text[_pos] : '\0';
//        }

//        // Продвигает позицию на один символ, обновляя номер строки и позицию в строке.
//        private void Advance()
//        {
//            if (CurrentChar() == '\n')
//            {
//                _line++;
//                _linePos = 0;
//            }
//            _pos++;
//            _linePos++;
//        }

//        // Добавляет токен в список.
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

//        /// <summary>
//        /// Для обычных слов (идентификаторов), разделителями являются пробелы, '=', ';', ':' и кавычки.
//        /// </summary>
//        private bool IsIdentifierDelimiter(char ch)
//        {
//            return char.IsWhiteSpace(ch) || ch == '=' || ch == ';' || ch == ':' || ch == '"';
//        }

//        /// <summary>
//        /// Определяет, является ли символ допустимым для начала нового токена.
//        /// Здесь допускаются буквы, цифры, '_', а также символы '=', ';', ':' , '"' и '&'.
//        /// </summary>
//        private bool IsValidTokenStart(char ch)
//        {
//            return char.IsLetterOrDigit(ch) || ch == '_' || ch == '=' || ch == ';' || ch == ':' || ch == '"' || ch == '&';
//        }

//        /// <summary>
//        /// Считывает обычный идентификатор или ключевое слово целиком.
//        /// Читает символы до первого разделителя, определяемого методом IsIdentifierDelimiter.
//        /// </summary>
//        private void ReadMixedIdentifierOrError()
//        {
//            int tokenStartPos = _linePos;
//            StringBuilder sb = new StringBuilder();

//            while (!IsEnd() && !IsIdentifierDelimiter(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string word = sb.ToString();
//            if (_keywords.Contains(word))
//            {
//                AddToken(TokenCode.Keyword, "ключевое слово", word, tokenStartPos, _linePos - 1, _line);
//            }
//            else
//            {
//                AddToken(TokenCode.Identifier, "идентификатор", word, tokenStartPos, _linePos - 1, _line);
//            }
//        }

//        /// <summary>
//        /// Считывает токен типа, начинающийся с символа '&'.
//        /// Здесь разделителем является только пробельный символ (то есть, считываем до пробела).
//        /// Таким образом, даже если внутри встречается символ ':' или другие, они входят в токен.
//        /// </summary>
//        private void ReadTypeToken()
//        {
//            int tokenStartPos = _linePos;
//            StringBuilder sb = new StringBuilder();

//            // Читаем до тех пор, пока не встретится пробельный символ или конец текста
//            while (!IsEnd() && !char.IsWhiteSpace(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string word = sb.ToString();
//            // Если слово входит в ключевые (например, "const"), то это ключевое слово,
//            // иначе, помечаем токен как тип (Keyword)
//            if (_keywords.Contains(word))
//            {
//                AddToken(TokenCode.Keyword, "ключевое слово", word, tokenStartPos, _linePos - 1, _line);
//            }
//            else
//            {
//                AddToken(TokenCode.Keyword, "тип", word, tokenStartPos, _linePos - 1, _line);
//            }
//        }

//        /// <summary>
//        /// Считывает числовой литерал (целое число).
//        /// </summary>
//        private void ReadInteger()
//        {
//            int startPos = _linePos;
//            StringBuilder sb = new StringBuilder();
//            while (!IsEnd() && char.IsDigit(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }
//            string number = sb.ToString();
//            AddToken(TokenCode.Integer, "целое число", number, startPos, _linePos - 1, _line);
//        }

//        /// <summary>
//        /// Считывает строковый литерал, заключённый в кавычки.
//        /// Если закрывающая кавычка не найдена, фиксируется ошибка.
//        /// </summary>
//        private void ReadStringLiteral()
//        {
//            int startPos = _linePos;
//            Advance(); // пропускаем начальную кавычку
//            StringBuilder sb = new StringBuilder();
//            bool closed = false;
//            while (!IsEnd())
//            {
//                char ch = CurrentChar();
//                if (ch == '"')
//                {
//                    closed = true;
//                    Advance(); // пропускаем закрывающую кавычку
//                    break;
//                }
//                else
//                {
//                    sb.Append(ch);
//                    Advance();
//                }
//            }
//            if (closed)
//            {
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, _linePos - 1, _line);
//            }
//        }

//        /// <summary>
//        /// Проверяет, начинается ли текущая позиция с заданной строки (например, "&str").
//        /// Если да, сдвигает позицию на длину строки и возвращает true.
//        /// </summary>
//        private bool CheckForExact(string keyword)
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
//    }
//}
using System;
using System.Collections.Generic;
using System.Text;

namespace Compilatori2Laba
{
    // Перечисление кодов токенов
    public enum TokenCode
    {
        Integer = 1,          // целое число
        Identifier = 2,       // идентификатор
        StringLiteral = 3,    // строковый литерал
        AssignOp = 10,        // знак "="
        Separator = 11,       // разделитель (например, ':')
        Keyword = 14,         // ключевые слова: const, let, fn, mod, struct и т.д.
        EndOperator = 16,     // конец оператора ";"
        Error = 99            // ошибка
    }

    // Класс токена
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
            return $"Строка: {Line}, с позиции {StartPos} по {EndPos} – {Type}: \"{Lexeme}\" (код {(int)Code})";
        }
    }

    // Сканер для анализа входного текста
    public class Scanner
    {
        private string _text;
        private int _pos;
        private int _line;
        private int _linePos;
        private List<Token> _tokens;
        // Набор ключевых слов (расширяем по необходимости)
        private HashSet<string> _keywords = new HashSet<string>
        {
            "const", "let", "fn", "mod", "struct"
        };

        public Scanner()
        {
            _tokens = new List<Token>();
        }

        /// <summary>
        /// Основной метод сканирования. Принимает входной текст и возвращает список токенов.
        /// </summary>
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

                // Обработка переводов строки
                if (ch == '\n' || ch == '\r')
                {
                    Advance();
                    continue;
                }

                // Пропуск пробелов
                if (char.IsWhiteSpace(ch))
                {
                    Advance();
                    continue;
                }

                // Если начинается с буквы или нижнего подчеркивания
                if (char.IsLetter(ch) || ch == '_')
                {
                    ReadMixedIdentifierOrError();
                    continue;
                }

                // Если начинается с символа '&' — специальный случай (тип)
                if (ch == '&')
                {
                    ReadMixedIdentifierOrError();
                    continue;
                }

                // Если цифра – считываем числовой литерал.
                if (char.IsDigit(ch))
                {
                    ReadInteger();
                    continue;
                }

                // Обработка оператора '='
                if (ch == '=')
                {
                    AddToken(TokenCode.AssignOp, "оператор присваивания", "=", _linePos, _linePos, _line);
                    Advance();
                    continue;
                }

                // Обработка конца оператора ';'
                if (ch == ';')
                {
                    AddToken(TokenCode.EndOperator, "конец оператора", ";", _linePos, _linePos, _line);
                    Advance();
                    continue;
                }

                // Обработка строковых литералов
                if (ch == '"')
                {
                    ReadStringLiteral();
                    continue;
                }

                // Обработка разделителя ':' – всегда отдельный токен
                if (ch == ':')
                {
                    AddToken(TokenCode.Separator, "специальный символ", ":", _linePos, _linePos, _line);
                    Advance();
                    continue;
                }

                // Если символ не подходит ни под один из вышеописанных случаев,
                // группируем подряд идущие символы до пробела.
                int errorStart = _linePos;
                StringBuilder errorSb = new StringBuilder();
                while (!IsEnd() && !char.IsWhiteSpace(CurrentChar()))
                {
                    errorSb.Append(CurrentChar());
                    Advance();
                }
                AddToken(TokenCode.Error, "недопустимый символ", errorSb.ToString(), errorStart, _linePos - 1, _line);
            }

            return _tokens;
        }

        // Возвращает true, если достигнут конец текста.
        private bool IsEnd()
        {
            return _pos >= _text.Length;
        }

        // Возвращает текущий символ или '\0' при окончании текста.
        private char CurrentChar()
        {
            return _pos < _text.Length ? _text[_pos] : '\0';
        }

        // Продвигает позицию на один символ; обновляет номер строки и позицию в строке.
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

        // Добавляет токен в список.
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

        /// <summary>
        /// Для обычных идентификаторов разделителями считаются пробелы, '=', ';', ':' и кавычки.
        /// </summary>
        private bool IsIdentifierDelimiter(char ch)
        {
            return char.IsWhiteSpace(ch) || ch == '=' || ch == ';' || ch == ':' || ch == '"';
        }

        /// <summary>
        /// Определяет, является ли символ допустимым для начала нового токена.
        /// Допускаются буквы, цифры, '_', а также символы '=', ';', ':' , '"' и '&'.
        /// </summary>
        private bool IsValidTokenStart(char ch)
        {
            return char.IsLetterOrDigit(ch) || ch == '_' || ch == '=' || ch == ';' || ch == ':' || ch == '"' || ch == '&';
        }

        /// <summary>
        /// Считывает идентификатор или ключевое слово целиком.
        /// Если первый символ равен '&', то считывает весь фрагмент до пробела как один токен (для типа).
        /// Если первый символ не равен '&', считывает символы до первого разделителя (IsIdentifierDelimiter).
        /// Затем выполняется нормализация: удаляются все символы, кроме букв, цифр и '_'.
        /// Если нормализованное слово совпадает с одним из ключевых, выдаётся токен типа Keyword (с лексемой candidate, без изменений).
        /// Иначе выдаётся токен типа Identifier.
        /// </summary>
        private void ReadMixedIdentifierOrError()
        {
            int tokenStartPos = _linePos;
            int startIndex = _pos;
            StringBuilder sb = new StringBuilder();

            // Если первый символ равен '&', то читаем до пробела
            if (CurrentChar() == '&')
            {
                while (!IsEnd() && !char.IsWhiteSpace(CurrentChar()))
                {
                    sb.Append(CurrentChar());
                    Advance();
                }
                string word = sb.ToString();
                AddToken(TokenCode.Keyword, "тип", word, tokenStartPos, _linePos - 1, _line);
                return;
            }
            else
            {
                // Для обычных идентификаторов читаем до разделителя
                while (!IsEnd() && !IsIdentifierDelimiter(CurrentChar()))
                {
                    sb.Append(CurrentChar());
                    Advance();
                }
                string candidate = sb.ToString();

                // Нормализация: оставляем только буквы, цифры и '_'
                StringBuilder normSb = new StringBuilder();
                foreach (char c in candidate)
                {
                    if (char.IsLetterOrDigit(c) || c == '_')
                        normSb.Append(c);
                }
                string normalized = normSb.ToString();

                // Если нормализованное слово совпадает с ключевым, выдаём токен ключевого слова (с оригинальным candidate)
                if (_keywords.Contains(normalized))
                {
                    AddToken(TokenCode.Keyword, "ключевое слово", candidate, tokenStartPos, _linePos - 1, _line);
                }
                else
                {
                    // Если кандидат полностью корректен (содержит только буквы, цифры, '_')
                    bool isValid = true;
                    foreach (char c in candidate)
                    {
                        if (!(char.IsLetterOrDigit(c) || c == '_'))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid)
                    {
                        AddToken(TokenCode.Identifier, "идентификатор", candidate, tokenStartPos, _linePos - 1, _line);
                    }
                    else
                    {
                        // Выделим максимально длинный префикс, состоящий только из корректных символов
                        int validLength = 0;
                        while (validLength < candidate.Length && (char.IsLetterOrDigit(candidate[validLength]) || candidate[validLength] == '_'))
                        {
                            validLength++;
                        }
                        if (validLength > 0)
                        {
                            string prefix = candidate.Substring(0, validLength);
                            AddToken(TokenCode.Identifier, "идентификатор", prefix, tokenStartPos, tokenStartPos + validLength - 1, _line);
                        }
                        // Если есть остаток, "вернуть" его обратно во входной поток
                        int remainder = candidate.Length - validLength;
                        if (remainder > 0)
                        {
                            _pos -= remainder;
                            _linePos = tokenStartPos + validLength;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Считывает числовой литерал (целое число).
        /// </summary>
        private void ReadInteger()
        {
            int startPos = _linePos;
            StringBuilder sb = new StringBuilder();
            while (!IsEnd() && char.IsDigit(CurrentChar()))
            {
                sb.Append(CurrentChar());
                Advance();
            }
            string number = sb.ToString();
            AddToken(TokenCode.Integer, "целое число", number, startPos, _linePos - 1, _line);
        }

        /// <summary>
        /// Считывает строковый литерал, заключённый в кавычки.
        /// Если закрывающая кавычка не найдена, фиксируется ошибка.
        /// </summary>
        private void ReadStringLiteral()
        {
            int startPos = _linePos;
            Advance(); // пропускаем начальную кавычку
            StringBuilder sb = new StringBuilder();
            bool closed = false;
            while (!IsEnd())
            {
                char ch = CurrentChar();
                if (ch == '"')
                {
                    closed = true;
                    Advance(); // пропускаем закрывающую кавычку
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
            }
        }

        /// <summary>
        /// Проверяет, начинается ли текущая позиция с заданной строки (например, "&str").
        /// Если да, сдвигает позицию на длину строки и возвращает true.
        /// </summary>
        private bool CheckForExact(string keyword)
        {
            if (_pos + keyword.Length > _text.Length)
                return false;
            for (int i = 0; i < keyword.Length; i++)
            {
                if (_text[_pos + i] != keyword[i])
                    return false;
            }
            _pos += keyword.Length;
            _linePos += keyword.Length;
            return true;
        }
    }
}













//using System;
//using System.Collections.Generic;

//namespace Compilatori2Laba
//{
//    public class ParseResult
//    {
//        // Флаг успешного разбора (ошибок нет)
//        public bool IsSuccess { get; set; } = false;
//        // Путь состояний конечного автомата
//        public List<string> States { get; set; } = new List<string>();
//        // Сообщения об обнаруженных ошибках
//        public List<string> ErrorMessages { get; set; } = new List<string>();
//    }

//    // Парсер, реализующий конечный автомат для разбора объявления вида:
//    // Declaration → 'const' Identifier ':' '&str' '=' StringLiteral ';'
//    public class Parser
//    {
//        private readonly List<Token> tokens;
//        private int index = 0;

//        public Parser(List<Token> tokens)
//        {
//            this.tokens = tokens;
//        }

//        /// <summary>
//        /// Основной метод синтаксического анализа согласно грамматике объявления.
//        /// Если в позиции ожидаемого токена обнаруживается лишний (избыточный) токен,
//        /// то он пропускается, а если токен отсутствует, то производится виртуальная вставка с сообщением об ошибке.
//        /// Грамматика:
//        /// Declaration → 'const' Identifier ':' '&str' '=' StringLiteral ';'
//        /// </summary>
//        /// <returns>Результат разбора с путём состояний и списком сообщений об ошибках</returns>
//        public ParseResult ParseDeclaration()
//        {
//            ParseResult result = new ParseResult();
//            List<string> states = new List<string>();

//            // S0: Ожидание ключевого слова "const"
//            if (MatchToken(TokenCode.Keyword, "const", "'const'", states, result))
//            {
//                states.Add("S0 -> S1: 'const'");
//            }

//            // S1: Ожидание идентификатора
//            if (index < tokens.Count)
//            {
//                Token token = tokens[index];
//                if (token.Code == TokenCode.Identifier)
//                {
//                    states.Add("S1 -> S2: Identifier");
//                    index++; // потребляем токен
//                }
//                else
//                {
//                    result.ErrorMessages.Add(
//                        $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидался идентификатор.");
//                    states.Add("S1 -> S2: Identifier (пропущено)");
//                    // Здесь вставка ожидаемого идентификатора – не потребляем текущий токен
//                }
//            }
//            else
//            {
//                result.ErrorMessages.Add("Ошибка: отсутствует идентификатор после 'const'.");
//            }

//            // S2: Ожидание символа ':'
//            if (MatchToken(TokenCode.Separator, ":", "символ ':'", states, result))
//            {
//                states.Add("S2 -> S3: ':'");
//            }

//            // S3: Ожидание типа "&str"
//            if (MatchToken(TokenCode.Keyword, "&str", "тип '&str'", states, result))
//            {
//                states.Add("S3 -> S4: '&str'");
//            }

//            // S4: Ожидание оператора '='
//            if (MatchToken(TokenCode.AssignOp, "=", "оператор '='", states, result))
//            {
//                states.Add("S4 -> S5: '='");
//            }

//            // S5: Ожидание строкового литерала
//            if (MatchToken(TokenCode.StringLiteral, null, "строковый литерал", states, result))
//            {
//                states.Add("S5 -> S6: StringLiteral");
//            }

//            // S6: Ожидание символа ';'
//            if (MatchToken(TokenCode.EndOperator, ";", "символ ';'", states, result))
//            {
//                states.Add("S6 -> S7: ';'");
//            }

//            result.States = states;
//            result.IsSuccess = result.ErrorMessages.Count == 0;
//            return result;
//        }

//        /// <summary>
//        /// Вспомогательный метод проверки текущего токена.
//        /// Если текущий токен соответствует ожидаемому (по типу и, если задано, по значению),
//        /// то он потребляется, и метод возвращает true.
//        /// Если не соответствует, то производится попытка восстановления:
//        /// - Если следующий токен существует и соответствует ожидаемому, текущий считается лишним (удаление).
//        /// - Иначе регистрируется ошибка о пропущенном (отсутствующем) токене (вставка) и токен не потребляется.
//        /// </summary>
//        /// <param name="expectedCode">Ожидаемый код токена</param>
//        /// <param name="expectedLexeme">Ожидаемая лексема (или null, если не проверять)</param>
//        /// <param name="expectedDescription">Описание ожидаемого токена для сообщения</param>
//        /// <param name="states">Список состояний для вывода</param>
//        /// <param name="result">Результат разбора для добавления ошибок</param>
//        /// <returns>true, если токен соответствует или восстановление прошло успешно</returns>
//        private bool MatchToken(TokenCode expectedCode, string expectedLexeme, string expectedDescription,
//            List<string> states, ParseResult result)
//        {
//            if (index >= tokens.Count)
//            {
//                result.ErrorMessages.Add($"Ошибка: отсутствует {expectedDescription}.");
//                return false;
//            }

//            Token curr = tokens[index];
//            bool currMatches = (curr.Code == expectedCode) &&
//                               (expectedLexeme == null || curr.Lexeme == expectedLexeme);

//            if (currMatches)
//            {
//                index++; // потребляем токен
//                return true;
//            }
//            else
//            {
//                // Попытка восстановления удалением: если следующий токен соответствует ожидаемому,
//                // значит текущий токен лишний.
//                if (index + 1 < tokens.Count)
//                {
//                    Token next = tokens[index + 1];
//                    bool nextMatches = (next.Code == expectedCode) &&
//                                       (expectedLexeme == null || next.Lexeme == expectedLexeme);
//                    if (nextMatches)
//                    {
//                        result.ErrorMessages.Add($"Ошибка: лишний токен '{curr.Lexeme}' (строка {curr.Line}).");
//                        states.Add($"[Пропущен лишний токен '{curr.Lexeme}']");
//                        index += 2; // пропускаем лишний токен и потребляем ожидаемый из lookahead
//                        return true;
//                    }
//                }
//                // Восстановление вставкой: ожидаемый токен отсутствует.
//                result.ErrorMessages.Add($"Ошибка в токене '{curr.Lexeme}' (строка {curr.Line}): ожидался {expectedDescription}.");
//                states.Add($"{expectedDescription} (пропущено)");
//                // НЕ потребляем текущий токен, чтобы он мог быть использован в следующем состоянии.
//                return true;
//            }
//        }
//    }
//}



//using System;
//using System.Collections.Generic;
//using System.Text;


//namespace Compilatori2Laba
//{
//    // Результат синтаксического анализа
//    public class ParseResult
//    {
//        // Флаг, успешен ли разбор (нет ошибок)
//        public bool IsSuccess { get; set; } = false;
//        // Перечень состояний конечного автомата во время разбора
//        public List<string> States { get; set; } = new List<string>();
//        // Список сообщений об ошибках (если они есть)
//        public List<string> ErrorMessages { get; set; } = new List<string>();
//    }

//    // Парсер, реализующий конечный автомат для разбора объявления
//    public class Parser
//    {
//        // Список токенов, полученных из Scanner
//        private readonly List<Token> tokens;
//        // Индекс текущего токена
//        private int index = 0;

//        public Parser(List<Token> tokens)
//        {
//            this.tokens = tokens;
//        }

//        /// <summary>
//        /// Производит синтаксический анализ согласно правилу:
//        /// Declaration → 'const' Identifier ':' Type '=' StringLiteral ';'
//        /// </summary>
//        /// <returns>Результат разбора с перечнем состояний и сообщениями об ошибках</returns>
//        public ParseResult ParseDeclaration()
//        {
//            ParseResult result = new ParseResult();
//            List<string> states = new List<string>();
//            // Используем числовое обозначение состояний конечного автомата:
//            // 0 - Начальное состояние
//            // 1 - После 'const'
//            // 2 - После идентификатора
//            // 3 - После разделителя ':'
//            // 4 - После типа (&str)
//            // 5 - После '='
//            // 6 - После строкового литерала
//            // 7 - Финальное (приемлемое) состояние
//            int state = 0;

//            // Последовательная обработка токенов
//            while (index < tokens.Count && state != -1 && state != 7)
//            {
//                Token token = tokens[index];
//                switch (state)
//                {
//                    case 0:
//                        // Ожидаем ключевое слово 'const'
//                        if (token.Code == TokenCode.Keyword && token.Lexeme == "const")
//                        {
//                            states.Add("S0 -> S1: 'const'");
//                            state = 1;
//                            index++;
//                        }
//                        else
//                        {
//                            result.ErrorMessages.Add(
//                                $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидалось 'const'.");
//                            state = -1;
//                        }
//                        break;

//                    case 1:
//                        // Ожидаем идентификатор
//                        if (token.Code == TokenCode.Identifier)
//                        {
//                            states.Add("S1 -> S2: Identifier");
//                            state = 2;
//                            index++;
//                        }
//                        else
//                        {
//                            result.ErrorMessages.Add(
//                                $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидался идентификатор.");
//                            state = -1;
//                        }
//                        break;

//                    case 2:
//                        // Ожидаем символ ':'
//                        if (token.Code == TokenCode.Separator && token.Lexeme == ":")
//                        {
//                            states.Add("S2 -> S3: ':'");
//                            state = 3;
//                            index++;
//                        }
//                        else
//                        {
//                            result.ErrorMessages.Add(
//                                $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидался символ ':'.");
//                            state = -1;
//                        }
//                        break;

//                    case 3:
//                        // Ожидаем тип, в нашем случае ключевое слово "&str"
//                        if (token.Code == TokenCode.Keyword && token.Lexeme == "&str")
//                        {
//                            states.Add("S3 -> S4: '&str'");
//                            state = 4;
//                            index++;
//                        }
//                        else
//                        {
//                            result.ErrorMessages.Add(
//                                $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидался тип '&str'.");
//                            state = -1;
//                        }
//                        break;

//                    case 4:
//                        // Ожидаем оператор присваивания '='
//                        if (token.Code == TokenCode.AssignOp && token.Lexeme == "=")
//                        {
//                            states.Add("S4 -> S5: '='");
//                            state = 5;
//                            index++;
//                        }
//                        else
//                        {
//                            result.ErrorMessages.Add(
//                                $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидался оператор '='.");
//                            state = -1;
//                        }
//                        break;

//                    case 5:
//                        // Ожидаем строковый литерал
//                        if (token.Code == TokenCode.StringLiteral)
//                        {
//                            states.Add("S5 -> S6: StringLiteral");
//                            state = 6;
//                            index++;
//                        }
//                        else
//                        {
//                            result.ErrorMessages.Add(
//                                $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидался строковый литерал.");
//                            state = -1;
//                        }
//                        break;

//                    case 6:
//                        // Ожидаем символ ';'
//                        if (token.Code == TokenCode.EndOperator && token.Lexeme == ";")
//                        {
//                            states.Add("S6 -> S7: ';'");
//                            state = 7; // финальное состояние
//                            index++;
//                        }
//                        else
//                        {
//                            result.ErrorMessages.Add(
//                                $"Ошибка в токене '{token.Lexeme}' (строка {token.Line}): ожидался символ ';'.");
//                            state = -1;
//                        }
//                        break;
//                }
//            }

//            if (state == 7)
//            {
//                result.IsSuccess = true;
//            }
//            else if (state != -1)
//            {
//                // Если токены закончились до завершения разбора
//                result.ErrorMessages.Add("Ошибка: недостаточно токенов для завершения разбора объявления.");
//                result.IsSuccess = false;
//            }
//            else
//            {
//                result.IsSuccess = false;
//            }

//            result.States = states;
//            return result;
//        }
//    }
//}
using System;
using System.Collections.Generic;

namespace Compilatori2Laba
{
    public class ParseResult
    {
        // Если ошибок не обнаружено, IsSuccess будет true.
        public bool IsSuccess { get; set; }
        // Путь состояний конечного автомата для одной декларации
        public List<string> States { get; set; } = new List<string>();
        // Список сообщений об обнаруженных ошибках для одной декларации
        public List<string> ErrorMessages { get; set; } = new List<string>();
        // Счётчик ошибок для одной декларации
        public int ErrorCount => ErrorMessages.Count;
    }

    public class Parser
    {
        private readonly List<Token> tokens;
        private int index = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        /// <summary>
        /// Разбирает все декларации во входном потоке токенов.
        /// Каждая декларация соответствует правилу:
        /// Declaration → 'const' Identifier ':' '&str' '=' StringLiteral ';'
        /// Если декларации разделены переводами строки (или другими пробельными символами),
        /// метод обрабатывает их последовательно.
        /// </summary>
        public List<ParseResult> ParseAllDeclarations()
        {
            List<ParseResult> results = new List<ParseResult>();
            while (index < tokens.Count)
            {
                // Если оставшиеся токены состоят только из ошибок или пробелов, прерываем цикл.
                ParseResult pr = ParseDeclaration();
                results.Add(pr);
            }
            return results;
        }

        /// <summary>
        /// Разбирает одну декларацию согласно грамматике:
        /// Declaration → 'const' Identifier ':' '&str' '=' StringLiteral ';'
        /// </summary>
        public ParseResult ParseDeclaration()
        {
            ParseResult result = new ParseResult();

            // S0: ожидается ключевое слово "const"
            ExpectToken(TokenCode.Keyword, "const", "'const'",
                new List<(TokenCode, string)>() { (TokenCode.Identifier, null) }, result);
            result.States.Add("S0 -> S1: 'const'");

            // S1: ожидается идентификатор
            ExpectToken(TokenCode.Identifier, null, "идентификатор",
                new List<(TokenCode, string)>() { (TokenCode.Separator, ":") }, result);
            result.States.Add("S1 -> S2: Identifier");

            // S2: ожидается символ ':'
            ExpectToken(TokenCode.Separator, ":", "символ ':'",
                new List<(TokenCode, string)>() { (TokenCode.Keyword, "&str") }, result);
            result.States.Add("S2 -> S3: ':'");

            // S3: ожидается тип "&str"
            ExpectToken(TokenCode.Keyword, "&str", "тип '&str'",
                new List<(TokenCode, string)>() { (TokenCode.AssignOp, "=") }, result);
            result.States.Add("S3 -> S4: '&str'");

            // S4: ожидается оператор '='
            ExpectToken(TokenCode.AssignOp, "=", "оператор '='",
                new List<(TokenCode, string)>() { (TokenCode.StringLiteral, null) }, result);
            result.States.Add("S4 -> S5: '='");

            // S5: ожидается строковый литерал
            ExpectToken(TokenCode.StringLiteral, null, "строковый литерал",
                new List<(TokenCode, string)>() { (TokenCode.EndOperator, ";") }, result);
            result.States.Add("S5 -> S6: StringLiteral");

            // S6: ожидается символ ';'
            ExpectToken(TokenCode.EndOperator, ";", "символ ';'",
                new List<(TokenCode, string)>(), result);
            result.States.Add("S6 -> S7: ';'");

            result.IsSuccess = result.ErrorMessages.Count == 0;
            return result;
        }

        /// <summary>
        /// Пытается "считать" ожидаемый токен.
        /// Если ожидается ключевое слово, сравниваем только по лексеме.
        /// Если текущий токен соответствует ожидаемому, он потребляется.
        /// Если не соответствует и входит в follow‑множественное, считается, что ожидаемый отсутствует
        /// (фиксируется ошибка виртуальной вставки), но токен не потребляется.
        /// Если токен не соответствует и не входит в follow‑множественное, он считается лишним,
        /// фиксируется ошибка, токен пропускается, и производится повторная попытка.
        /// </summary>
        private bool ExpectToken(TokenCode expectedCode, string expectedLexeme, string expectedDescription,
            List<(TokenCode, string)> followSet, ParseResult result)
        {
            if (index >= tokens.Count)
            {
                result.ErrorMessages.Add($"Ошибка: отсутствует {expectedDescription}.");
                return false;
            }

            Token current = tokens[index];

            // Если ожидается ключевое слово, сравнение идёт только по лексеме.
            if (expectedCode == TokenCode.Keyword)
            {
                if (current.Lexeme == expectedLexeme)
                {
                    index++; // потребляем токен
                    return true;
                }
                else
                {
                    result.ErrorMessages.Add($"Ошибка в токене '{current.Lexeme}' (строка {current.Line}): ожидался {expectedDescription}.");
                    index++; // потребляем ошибочный токен
                    return true;
                }
            }
            else
            {
                if (current.Code == expectedCode && (expectedLexeme == null || current.Lexeme == expectedLexeme))
                {
                    index++; // потребляем корректный токен
                    return true;
                }
            }

            // Если текущий токен входит в follow‑множество, значит ожидаемый токен отсутствует (виртуальная вставка)
            if (IsTokenInFollow(current, followSet))
            {
                result.ErrorMessages.Add($"Ошибка в токене '{current.Lexeme}' (строка {current.Line}): ожидался {expectedDescription}.");
                // Не потребляем текущий токен – он будет использован для следующего элемента.
                return true;
            }
            else
            {
                // Текущий токен не соответствует и не принадлежит follow‑множеству – он лишний.
                result.ErrorMessages.Add($"Ошибка: лишний токен '{current.Lexeme}' (строка {current.Line}), ожидался {expectedDescription}.");
                index++; // пропускаем лишний токен
                return ExpectToken(expectedCode, expectedLexeme, expectedDescription, followSet, result);
            }
        }

        /// <summary>
        /// Возвращает true, если данный токен входит в follow‑множество.
        /// </summary>
        private bool IsTokenInFollow(Token token, List<(TokenCode, string)> followSet)
        {
            foreach (var item in followSet)
            {
                if (token.Code == item.Item1 && (item.Item2 == null || token.Lexeme == item.Item2))
                    return true;
            }
            return false;
        }
    }
}









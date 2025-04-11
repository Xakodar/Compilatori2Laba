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

﻿// See https://aka.ms/new-console-template for more information


using System.Text;
using RefinementTypes.Parsing;
using RefinementTypes.Execution;
using RefinementTypes.Scanning;

namespace RefinementTypes
{
    internal class Compiler
    {
        //static readonly Interpreter Interpreter = new Interpreter();
        public static bool HadError = false;
        public static bool HadRuntimeError = false;

        public static bool HasEntryPoint = false;

        public static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            RunPrompt();
        }

        static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                PrintAst(line);
                HadError = false;
                HadRuntimeError = false;
            }
        }

        static void PrintAllTokens(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        static void PrintAst(string source)
        {
            Scanner scanner = new Scanner(source);
            Parser parser = new Parser(scanner);

            try
            {
                List<TopLevel> topLevels = parser.ParseTopLevels();
                foreach(TopLevel tl in topLevels)
                {
                    ASTPrinter.PrintTopLevel(tl);
                    ASTExecutor.ExecuteTopLevel(tl);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"PARSE ERROR: {e.Message}");
            }
        }

        public static void Error(int line, int column, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.StartLine, " at end", message);
            }
            else
            {
                Report(token.StartLine, $" at '{token.Lexeme}'", message);
            }
        }

        public static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            HadError = true;
        }
    }
}
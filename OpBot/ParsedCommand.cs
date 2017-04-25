﻿using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpBot
{
    class ParsedCommand
    {
        public string Command { get; internal set; }
        public string[] CommandParts { get; internal set; }
        public int OperationId { get; internal set; }
        public bool IsPermanent { get; internal set; }
        public DiscordUser User { get; internal set; }

        private static readonly Regex _mentionsRegex = new Regex(@"\<@!?\d+\>");

        public ParsedCommand(MessageCreateEventArgs e, ulong opBotUserId)
        {
            if (e.Message.Mentions.Count > 2)
                throw new CommandParseException("Thre are to many mentions in that command");

            var user = e.Message.Mentions.Where(m => m.ID != opBotUserId).SingleOrDefault();
            if (user == null)
                user = e.Message.Author;
            User = user ?? e.Message.Author;

            string contentWithNoMentions = _mentionsRegex.Replace(e.Message.Content, string.Empty);
            string[] parts = contentWithNoMentions.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> commandParts = new List<string>();

            if (parts.Length > 0)
            {
                Command = parts[0].ToUpperInvariant();
                foreach (string part in parts)
                {
                    if (part.StartsWith("-"))
                    {
                        ParseSwitch(part.ToUpperInvariant());
                    }
                    else
                    {
                        commandParts.Add(part);
                    }
                }

            }

            CommandParts = commandParts.ToArray();
        }

        private void ParseSwitch(string part)
        {
            if (part == "-PERM")
            {
                IsPermanent = true;
                return;
            }

            if (part.StartsWith("-OP"))
            {
                ParseOperation(part);
                return;
            }

            throw new CommandParseException($"I don't understand \"{part}\"");
        }


        private void ParseOperation(string part)
        {
            int operationId = 0;
            bool valid = part.Length > 3;

            if (valid)
            {
                string numberPart = part.Substring(3);
                valid = int.TryParse(numberPart, out operationId);
            }

            if (valid)
                OperationId = operationId;
            else
                throw new CommandParseException("Invalid operation, specify -OPn where n is the operation number");
        }
    }
}
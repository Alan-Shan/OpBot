﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpBot
{
    [Serializable]
    internal class Operation
    {
        private string _operationName;
        private int _size;
        private string _mode;
        private List<AltRole> _altRoles;
        private List<string> _notes;
        private List<OperationMember> _members;
        private ulong _messageId;
        private DateTime _date;

        public Operation()
        {
            _members = new List<OperationMember>();
            _altRoles = new List<AltRole>();
            _notes = new List<string>();
        }

        public ulong MessageId
        {
            get
            {
                return _messageId;
            }
            set
            {
                lock (this) _messageId = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                lock (this) _date = value;
            }
        }

        public string OperationName
        {
            get
            {
                return _operationName;
            }
            set
            {
                lock (this)
                    _operationName = GetFullName(value);
            }
        }

        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (value != 8 && value != 16)
                    throw new OpBotInvalidValueException("Invalid size, must be 8 or 16");
                lock (this)
                    _size = value;
            }
        }

        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (value != "SM" && value != "VM" && value != "MM")
                    throw new OpBotInvalidValueException($"{value} is not a valid operation mode");
                lock (this)
                    _mode = value;
            }
        }

        public void SetSizeFromString(string sizeString)
        {
            int size;
            if (!int.TryParse(sizeString, out size))
                throw new OpBotInvalidValueException("Invalid size, must be 8 or 16");
            Size = size;
        }

        public static string GetFullName(string shortCode)
        {
            switch (shortCode.ToUpperInvariant())
            {
                case "EV": return "Eternity Vault";
                case "KP": return "Karagga's Palace";
                case "EC": return "Explosive Conflict";
                case "TFB": return "Terror From Beyond";
                case "SV": return "Scum and Villainy";
                case "DF": return "The Dread Fortress";
                case "DP": return "The Dread Palace";
                case "RAV": return "The Ravagers";
                case "TOS": return "Temple of Sacrifice";
                default:
                    throw new OpBotInvalidValueException($"Unknown operation name '{shortCode}'");
            }
        }

        public void Signup(ulong userId, string name, string role)
        {
            lock (this)
            {
                var member = _members.Where(m => m.UserId == userId).SingleOrDefault();
                if (member == null)
                {
                    member = new OperationMember();
                    _members.Add(member);
                }
                member.UserId = userId;
                member.UserName = name;
                member.PrimaryRole = role.ToUpperInvariant();
            }
        }

        public void Remove(ulong userId)
        {
            lock (this)
            {
                _members.RemoveAll(m => m.UserId == userId);
            }
        }

        public void AddNote(string note)
        {
            lock (this)
            {
                _notes.Add(note);
            }
        }

        public void DeleteNote(int noteIndex)
        {
            lock (this)
            {
                _notes.RemoveAt(noteIndex);
            }
        }

        public void ResetNotes()
        {
            lock (this)
            {
                _notes = new List<string>();
            }
        }

        public int NoteCount => _notes.Count;

        public string GetOperationMessageText()
        {
            DateTime baseTime = _date.IsDaylightSavingTime() ? _date.AddHours(1) : _date;
            string text = $"**{OperationName}** {Size}-man {Mode}\n{_date.ToString("dddd")} {_date.ToLongDateString()} {_date.ToShortTimeString()} (UTC)\n";
            text += "  *" + baseTime.ToShortTimeString() + " Western Europe (UK)*\n";
            text += "  *" + baseTime.AddHours(1).ToShortTimeString() + " Central Europe (Belgium)*\n";
            text += "  *" + baseTime.AddHours(2).ToShortTimeString() + " Eastern Europe (Estonia)*\n";
            text += "```";
            text += "Tanks:\n";
            text += Roles("TANK");
            text += "Damage:\n";
            text += Roles("DPS");
            text += "Healers:\n";
            text += Roles("HEAL");
            text += "```";
            if (_altRoles.Count > 0)
            {
                text += $"\nAlternative/Reserve Roles {AltRoles()}";
            }
            if (_notes.Count > 0)
                text += "\n";
            foreach (string note in _notes)
            {
                text += note + "\n";
            }
            return text;
        }

        public void SetAltRoles(string username, ulong userid, string[] roles)
        {
            lock (this)
            {
                AltRole altRole = _altRoles.Where(x => x.UserId == userid).SingleOrDefault();
                if (altRole == null)
                {
                    altRole = new AltRole(username, userid);
                    _altRoles.Add(altRole);
                }
                altRole.Set(roles);
                if (!altRole.HasAnyRole)
                    _altRoles.Remove(altRole);
            }
        }

        private string Roles(string primaryRole)
        {
            int count = 0;
            string text = "";
            var roleMembers = _members.Where(m => m.PrimaryRole == primaryRole).ToList();
            foreach (var member in roleMembers)
            {
                ++count;
                text += $"    {count.ToString()}. {member.UserName}\n";
            }
            return text;
        }

        private string AltRoles()
        {
            int padding = _altRoles.Max(x => x.Name.Length) + 1;
            StringBuilder sb = new StringBuilder(1024);
            sb.Append("```");
            foreach (AltRole role in _altRoles)
            {
                sb.Append(role.Name.PadRight(padding));
                sb.Append(' ');
                sb.Append(role.ToString());
            }
            sb.AppendLine("```");
            return sb.ToString();
        }

    }
}

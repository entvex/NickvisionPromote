using System;

namespace NickvisionPromote.Models
{
    public class USPhoneNumber
    {
        private string _areaCode;
        private string _prefix;
        private string _subscriber;

        public USPhoneNumber(string phoneNumber)
        {
            if (phoneNumber[0] == '+' && phoneNumber.Length == 12)
            {
                _areaCode = phoneNumber.Substring(2, 3);
                _prefix = phoneNumber.Substring(5, 3);
                _subscriber = phoneNumber.Substring(8, 4);
            }
            else if (phoneNumber[3] == '-' && phoneNumber[7] == '-' && phoneNumber.Length == 12)
            {
                _areaCode = phoneNumber.Substring(0, 3);
                _prefix = phoneNumber.Substring(4, 3);
                _subscriber = phoneNumber.Substring(8, 4);
            }
            else if (phoneNumber.Length == 10)
            {
                _areaCode = phoneNumber.Substring(0, 3);
                _prefix = phoneNumber.Substring(3, 3);
                _subscriber = phoneNumber.Substring(6, 4);
            }
            else if (phoneNumber[0] == '(' && phoneNumber[4] == ')' && phoneNumber[9] == '-' && phoneNumber.Length == 14)
            {
                _areaCode = phoneNumber.Substring(1, 3);
                _prefix = phoneNumber.Substring(6, 3);
                _subscriber = phoneNumber.Substring(10, 4);
            }
            else if (phoneNumber[0] == '1')
            {
                if (phoneNumber.Length == 11)
                {
                    _areaCode = phoneNumber.Substring(1, 3);
                    _prefix = phoneNumber.Substring(4, 3);
                    _subscriber = phoneNumber.Substring(7, 4);
                }
                else if (phoneNumber[2] == '(' && phoneNumber[6] == ')' && phoneNumber[11] == '-' && phoneNumber.Length == 16)
                {
                    _areaCode = phoneNumber.Substring(3, 3);
                    _prefix = phoneNumber.Substring(8, 3);
                    _subscriber = phoneNumber.Substring(12, 4);
                }
            }
            else
            {
                throw new ArgumentException("Invalid phone number format. Format must be one of the following:\n+1##########\n###-###-####\n##########\n(###) ###-####\n1##########\n1 (###) ###-####");
            }
            if (!IsEveryCharANumber(_areaCode) || !IsEveryCharANumber(_prefix) || !IsEveryCharANumber(_subscriber))
            {
                throw new ArgumentException("Invalid phone number format. Make sure every \"number\" is a valid number.");
            }
        }

        public string NumberString => $"{_areaCode}{_prefix}{_subscriber}";

        public string ReadableString => $"({_areaCode}) {_prefix}-{_subscriber}";

        public ulong PhoneNumber => ulong.Parse(NumberString);

        private bool IsEveryCharANumber(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            foreach (char ch in s)
            {
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(USPhoneNumber a, USPhoneNumber b) => a.NumberString == b.NumberString;

        public static bool operator !=(USPhoneNumber a, USPhoneNumber b) => a.NumberString != b.NumberString;
    }
}

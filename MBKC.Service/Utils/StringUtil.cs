using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Utils
{
    public static class StringUtil
    {
        public static string ChangeNumberPhoneFromGrabFood(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) == false)
            {
                if (phoneNumber.Contains("+84"))
                {
                    string[] phoneNumberParts = phoneNumber.Split("+84");
                    phoneNumber = "0" + phoneNumberParts[1];
                }

                if (phoneNumber.Contains(" "))
                {
                    string[] phoneNumberParts = phoneNumber.Split(" ");
                    phoneNumber = "";
                    foreach (var phoneNumberPart in phoneNumberParts)
                    {
                        phoneNumber += phoneNumberPart;
                    }
                }
            }
            return phoneNumber;
        }
    }
}

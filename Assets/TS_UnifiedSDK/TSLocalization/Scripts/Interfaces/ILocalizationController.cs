using System;
using System.Collections.Generic;

namespace TheSTAR.Localization
{
    public interface ILocalizationController
    {
        Action OnChangeLocalization
        {
            get;
            set;
        }

        void UpdateLanguage();  

        string GetTextByKey(string key);

        string GetDialogeTextsByKey(string key, out Dictionary<string, string> branches);
    }
}
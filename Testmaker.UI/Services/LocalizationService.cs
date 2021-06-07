using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WPFLocalizeExtension.Extensions;

namespace TestMaker.UI.Services
{
    public static class LocalizationService
    {
        #region Public Methods

        public static T GetLocalizedValue<T>(string key)
        {
            return LocExtension.GetLocalizedValue<T>(Assembly.GetCallingAssembly().GetName().Name + ":language:" + key);
        }

        #endregion Public Methods
    }
}
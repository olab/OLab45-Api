using System;
using System.Collections.Generic;
using System.Text;

namespace OLabWebAPI.Common
{
    public interface IWikiTagModule
    {
        string Translate(string source);
        string GetHtmlElementName();
    }
}

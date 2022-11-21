using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.ScriptEngine
{
    public interface IScriptEngine
    {
        bool RunValidation();

        //bool RunValidation(int sourceValue, int inputValue);

        //bool RunValidation(decimal sourceValue, decimal inputValue);

        //bool RunValidation(DateTime sourceValue, DateTime inputValue);

        //bool RunValidation(double sourceValue, double inputValue);
    }
}
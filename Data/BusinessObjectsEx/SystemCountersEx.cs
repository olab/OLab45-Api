using System;
using System.Collections.Generic;
using System.Text;

namespace OLabWebAPI.Model
{
  public partial class SystemCounters
  {
    public const string NotANumber = "#NaN";

    public SystemCounters()
    {
      SystemCounterActions = new HashSet<SystemCounterActions>();
      Value = new List<byte>().ToArray();
      StartValue = new List<byte>().ToArray();
    }

    public void ValueFromString(string source)
    {
      try
      {
        var orgValue = ValueAsString();
        if (source != orgValue)
        {
          Value = Encoding.ASCII.GetBytes(source);
          UpdatedAt = DateTime.Now;
        }
      }
      catch (Exception)
      {
        ValueFromString(NotANumber);
      }
    }

    public void ValueFromNumber(decimal source)
    {
      ValueFromString(source.ToString());
    }

    public string ValueAsString()
    {
      if (Value == null)
        return "";
      return Encoding.Default.GetString(Value);
    }

    public decimal ValueAsNumber()
    {
      if (Value == null)
        return 0;

      var str = Encoding.Default.GetString(Value);
      var num = Convert.ToDecimal(str);
      return num;
    }

    public bool IsValueString()
    {
      if (Value == null)
        return true;

      return !IsValueNumeric();
    }

    public bool IsValueNumeric()
    {
      if (Value == null)
        return true;

      var str = Encoding.Default.GetString(Value);
      return decimal.TryParse(str, out _);
    }
  }

}